using UnityEngine;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection = default(NetworkConnection);

    enum LoadState {LOAD_COMMANDED, AWAITING_SERVER, LOAD_COMPLETE, NO_LOAD};

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_Connection.IsCreated) return;

        m_Driver = NetworkDriver.Create();
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
        ushort msgId = stream.ReadUShort();
        if (msgId == B2DNetData.MSG_ID_SAVE_PLAYER_REP)
        {
            EventManager.NotifyPlayerSaved();
        }
        else if (msgId == B2DNetData.MSG_ID_LOAD_PLAYER_REP)
        {
            PlayerData playerLoaded = new PlayerData(ref stream);
            //Debug.Log("Client: Player Loaded, playerData = \n" + playerLoaded.GetString());
            EventManager.NotifyPlayerLoaded(playerLoaded);
        }
    }

    public void SavePlayer(Player player)
    {
        if (!IsConnected()) return;

        PlayerData playerData = player.ToPlayerData();

        m_Driver.BeginSend(m_Connection, out var writer);
        writer.WriteUShort(B2DNetData.MSG_ID_SAVE_PLAYER_CMD);
        playerData.WriteTo(ref writer);
        var x = m_Driver.EndSend(writer);

        //Debug.Log("Client.SavePlayer() completed " + x + " bytes sent, playerData = \n" + playerData.GetString());
    }

    public bool BeginLoadPlayer(uint round)
    {
        if (!IsConnected()) return false;

        m_Driver.BeginSend(m_Connection, out var writer);
        writer.WriteUShort(B2DNetData.MSG_ID_LOAD_PLAYER_CMD);
        writer.WriteUInt(round);
        m_Driver.EndSend(writer);

        return true;
    }

}
