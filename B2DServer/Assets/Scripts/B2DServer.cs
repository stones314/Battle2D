using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

public class B2DServer : MonoBehaviour
{
    string address = "127.0.0.1";
    ushort port = 50123;

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    const string PLAYER_PREFIX = "/p";
    const string COUNT_FILE = "/count";
    string storeDir = "players/";

    private void Awake()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-b2d-address")
                address = args[i + 1];
            else if (args[i] == "-b2d-port")
                port = ushort.Parse(args[i + 1]);
            else if (args[i] == "-b2d-store-dir")
                storeDir = args[i + 1];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.Parse(address, port);
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind port " + endpoint.Port + " at " + address);
        }
        else
        {
            m_Driver.Listen();
            Debug.Log("B2DServer listening on " + endpoint.Port + " at " + address);
        }

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        CleanUpUnusedConnections();
        AcceptNewConnections();
        HandleConnectionEvents();   //Incomming Data or Disconnects
    }

    void CleanUpUnusedConnections()
    {
        for (int i = m_Connections.Length - 1; i >= 0; i--)
            if (!m_Connections[i].IsCreated)
                m_Connections.RemoveAt(i);
    }

    void AcceptNewConnections()
    {
        NetworkConnection c;
        while((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted Connection");
        }
    }

    void HandleConnectionEvents()
    {
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    HandleMessage(m_Connections[i], stream);
                }
                else if(cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    void HandleMessage(NetworkConnection connection, DataStreamReader stream)
    {
        ushort msgId = stream.ReadUShort();
        Debug.Log("Server got Msg with ID " + msgId);
        if(msgId == B2DNetData.MSG_ID_SAVE_PLAYER_CMD) {
            SavePlayer(stream);
            SendSaveReply(connection);
        }
        else if(msgId == B2DNetData.MSG_ID_LOAD_PLAYER_CMD)
        {
            uint round = stream.ReadUInt();
            PlayerData data = LoadPlayer(round);
            SendPlayerToClient(data, connection);
        }
    }

    void SavePlayer(DataStreamReader stream) {

        PlayerData playerData = new PlayerData(stream);
        ushort round = playerData.roundsPlayed;
        Debug.Log("Save player for round " + round);

        string dir = GetDir(round);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        string json = JsonUtility.ToJson(playerData);
        
        string path = dir + PLAYER_PREFIX;
        string countPath = dir + COUNT_FILE;

        int savedPlayers = LoadPlayerCount(round);

        FileStream countStream = new FileStream(countPath, FileMode.Create);
        formatter.Serialize(countStream, savedPlayers + 1);
        countStream.Close();

        FileStream fileStream = new FileStream(path + savedPlayers + ".json", FileMode.Create);
        byte[] text = new UTF8Encoding(true).GetBytes(json);
        fileStream.Write(text, 0, text.Length);
        fileStream.Close();
    }

    void SendSaveReply(NetworkConnection connection)
    {
        m_Driver.BeginSend(connection, out var writer);
        writer.WriteUShort(B2DNetData.MSG_ID_SAVE_PLAYER_REP);
        m_Driver.EndSend(writer);
    }

    PlayerData LoadPlayer(uint round)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetDir(round) + PLAYER_PREFIX;

        int savedPlayers = LoadPlayerCount(round);

        if (savedPlayers < 1) return null;

        int triesLeft = 10;
        while (triesLeft > 0)
        {
            int x = (int)Random.Range(0, savedPlayers - 0.00001f);

            Debug.Log("Loading player " + x + " from round " + round);

            if (File.Exists(path + x + ".json"))
            {
                string json = File.ReadAllText(path + x + ".json");
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                return data;
            }
            triesLeft--;
        }
        return null;
    }

    void SendPlayerToClient(PlayerData playerData, NetworkConnection connection)
    {
        m_Driver.BeginSend(connection, out var writer);
        writer.WriteUShort(B2DNetData.MSG_ID_LOAD_PLAYER_REP);
        playerData.WriteTo(ref writer);
        m_Driver.EndSend(writer);
    }

    string GetDir(uint round)
    {
        return storeDir + "round" + round;
    }

    int LoadPlayerCount(uint round)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string countPath = GetDir(round) + COUNT_FILE;
        int savedPlayers = 0;

        if (File.Exists(countPath))
        {
            FileStream countStream = new FileStream(countPath, FileMode.Open);
            savedPlayers = (int)formatter.Deserialize(countStream);
            countStream.Close();
        }

        Debug.Log("Found " + savedPlayers + " saved players at round " + round);

        return savedPlayers;
    }


    private void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

}
