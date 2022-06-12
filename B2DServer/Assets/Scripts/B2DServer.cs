using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

public class B2DServer : MonoBehaviour
{
    ushort port = 50123;

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    const string PLAYER_PREFIX = "/p";
    const string PLAYER_POSTIX = ".json";
    const string COUNT_FILE = "/count";
    string storeDir = "./players/";

    private void Awake()
    {
        // Had trouble with running server on lightsail, it was using 130% cpu
        // Seems it tries to maximize framerate, and can go pretty fast when the server has no UI-stuff
        // Explicit set target framerate to 60 => it uses about 2% cpu
        // 60 frames per second might be a bit slow(?), will have to evaluate
        Application.targetFrameRate = 60;

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-b2d-port")
                port = ushort.Parse(args[i + 1]);
            else if (args[i] == "-b2d-store-dir")
                storeDir = args[i + 1];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind port " + endpoint.Port + " at " + endpoint.Address);
        }
        else
        {
            m_Driver.Listen();
            Debug.Log("B2DServer listening on " + endpoint.Port + " at " + endpoint.Address);
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
            //Debug.Log("Loaded player from file, playerData = \n" + data.GetString());
            SendPlayerToClient(data, connection);
        }
    }

    void SavePlayer(DataStreamReader stream) {

        PlayerData playerData = new PlayerData(ref stream);
        ushort round = playerData.roundsPlayed;
        //Debug.Log("Save player for round " + round + ", playerData = \n" + playerData.GetString());

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

        FileStream fileStream = new FileStream(path + savedPlayers + PLAYER_POSTIX, FileMode.Create);
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
        string path = GetDir(round) + PLAYER_PREFIX;

        int savedPlayers = LoadPlayerCount(round);

        if (savedPlayers < 1) return null;

        int triesLeft = 10;
        while (triesLeft > 0)
        {
            int x = (int)Random.Range(0, savedPlayers - 0.00001f);

            //Debug.Log("Loading player " + x + " from round " + round);
            string fileName = path + x + PLAYER_POSTIX;

            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
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

        //Debug.Log("Found " + savedPlayers + " saved players at round " + round);

        return savedPlayers;
    }


    private void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

}
