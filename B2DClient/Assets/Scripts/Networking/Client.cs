using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Client : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;

    private PlayerData playerToSave;
    private bool pendingSave;

    enum LoadState {LOAD_COMMANDED, AWAITING_SERVER, LOAD_COMPLETE, NO_LOAD};

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 50123;
        m_Connection = m_Driver.Connect(endpoint);
    }

    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!IsConnected()) return;

        HandleConnectionEvents();
    }

    private void OnDestroy()
    {
        m_Driver.Dispose();
    }

    public bool IsConnected()
    {
        if (!m_Connection.IsCreated)
        {
            if (!Done)
                Debug.Log("Something went wrong during connect");
            return false;
        }
        return true;
    }

    void HandleConnectionEvents()
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                HandleMessage(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }

    void HandleMessage(DataStreamReader stream)
    {
        uint msgId = stream.ReadUInt();
        if (msgId == Server.MSG_ID_SAVE_PLAYER_REP)
        {
            Debug.Log("Save Player Reply Id = " + stream.ReadUInt());
        }
        else if (msgId == Server.MSG_ID_LOAD_PLAYER_REP)
        {
            PlayerData playerLoaded = DezerializePlayerData(stream);
            EventManager.NotifyPlayerLoaded(playerLoaded);
        }
    }

    PlayerData DezerializePlayerData(DataStreamReader stream)
    {
        int n_bytes = stream.ReadInt();
        NativeArray<byte> pd = new NativeArray<byte>(n_bytes, Allocator.Temp);
        stream.ReadBytes(pd);

        MemoryStream memStream = new MemoryStream();
        foreach (var b in pd.ToArray())
        {
            memStream.WriteByte(b);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        memStream.Position = 0; //For some reason I need to do this before deserialization
        PlayerData data = formatter.Deserialize(memStream) as PlayerData;
        memStream.Close();
        
        return data;
    }

    public void SavePlayer(Player player)
    {
        if (!IsConnected()) return;

        BinaryFormatter formatter = new BinaryFormatter();

        MemoryStream stream = new MemoryStream();
        PlayerData data = new PlayerData(player);

        formatter.Serialize(stream, data);

        m_Driver.BeginSend(m_Connection, out var writer);
        writer.WriteUInt(Server.MSG_ID_SAVE_PLAYER_CMD);
        writer.WriteUInt((uint)player.round);
        writer.WriteInt((int)stream.Length);
        foreach (var b in stream.ToArray())
        {
            writer.WriteByte(b);
        } 
        m_Driver.EndSend(writer);

        stream.Close();
    }

    public bool BeginLoadPlayer(uint round)
    {
        if (!IsConnected()) return false;

        m_Driver.BeginSend(m_Connection, out var writer);
        writer.WriteUInt(Server.MSG_ID_LOAD_PLAYER_CMD);
        writer.WriteUInt(round);
        m_Driver.EndSend(writer);

        return true;
    }

}
