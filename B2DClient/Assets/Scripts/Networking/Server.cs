using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Server : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    //
    public static readonly uint MSG_ID_SAVE_PLAYER_CMD = 1;
    public static readonly uint MSG_ID_SAVE_PLAYER_REP = 2;
    public static readonly uint MSG_ID_LOAD_PLAYER_CMD = 3;
    public static readonly uint MSG_ID_LOAD_PLAYER_REP = 4;

    const string PLAYER_PREFIX = "/player";
    const string COUNT_FILE = "/count";

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 50123;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind port " + endpoint.Port);
        else
            m_Driver.Listen();

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
        uint msgId = stream.ReadUInt();
        if(msgId == MSG_ID_SAVE_PLAYER_CMD) {
            SavePlayer(stream);
        }
        else if(msgId == MSG_ID_LOAD_PLAYER_CMD)
        {
            SendPlayerToClient(LoadPlayer(stream), connection);
        }
    }

    void SavePlayer(DataStreamReader stream) {
        uint round = stream.ReadUInt();

        int n_bytes = stream.ReadInt();
        NativeArray<byte> data = new NativeArray<byte>(n_bytes, Allocator.Temp);
        stream.ReadBytes(data);

        string dir = GetDir(round);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        string path = dir + PLAYER_PREFIX;
        string countPath = dir + COUNT_FILE;

        Debug.Log(path + "\n" + countPath);

        int savedPlayers = LoadPlayerCount(round);

        FileStream countStream = new FileStream(countPath, FileMode.Create);
        formatter.Serialize(countStream, savedPlayers + 1);
        countStream.Close();

        FileStream fileStream = new FileStream(path + savedPlayers, FileMode.Create);
        fileStream.Write(data.ToArray(), 0, n_bytes);
        fileStream.Close();
    }

    byte[] LoadPlayer(DataStreamReader inStream)
    {
        uint round = inStream.ReadUInt();

        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetDir(round) + PLAYER_PREFIX;

        int savedPlayers = LoadPlayerCount(round);

        if (savedPlayers < 1) return new byte[0];

        int triesLeft = 10;
        while (triesLeft > 0)
        {
            int x = (int)Random.Range(0, savedPlayers - 0.00001f);

            Debug.Log("Loading player " + x + " from round " + round);

            if (File.Exists(path + x))
            {
                return File.ReadAllBytes(path + x);
            }
            triesLeft--;
        }
        return new byte[0];
    }

    void SendPlayerToClient(byte[] playerData, NetworkConnection connection)
    {
        m_Driver.BeginSend(connection, out var writer);
        writer.WriteUInt(MSG_ID_LOAD_PLAYER_REP);
        writer.WriteInt(playerData.Length);
        foreach (var b in playerData)
        {
            writer.WriteByte(b);
        }
        m_Driver.EndSend(writer);
    }

    string GetDir(uint round)
    {
        return "players/round" + round;
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
