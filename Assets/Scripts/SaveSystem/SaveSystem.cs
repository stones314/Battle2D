using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Grpc.Net.Client;

public class SaveSystem : MonoBehaviour
{
    [SerializeField]
    Player playerPrefab;

    GrpcNetworking.DataStorage.DataStorageClient client;
    const string PLAYER_PREFIX = "/player";
    const string COUNT_FILE = "/count";

    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.gameObject.tag);

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        if(client == null)
        {
            ConnectToServer(Constants.ServerAddress);
        }
    }

    public bool ConnectToServer(string address = "http://localhost:8001")
    {
        try
        {
            System.AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(new HttpClientHandler()),
            });

            client = new GrpcNetworking.DataStorage.DataStorageClient(channel);

            Debug.Log("Connected to " + address);
            return true;
        }
        catch
        {
            Debug.Log("Unable to onnected to " + address);
            return false;
        }
    }

    string GetDir(int round)
    {
        return Application.persistentDataPath + "/players/round" + round;
    }

    int LoadPlayerCount(int round)
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

    public void SavePlayer(Player player)
    {
        if (client != null)
        {
            Debug.Log("Saving player on net");
            SavePlayerOnNet(player);
        }
        else
        {
            Debug.Log("No gRPC Client, Saving player locally");
            SavePlayerLocaly(player);
        }
    }

    private void SavePlayerLocaly(Player player)
    {
        if (!Directory.Exists(GetDir(player.round)))
        {
            Directory.CreateDirectory(GetDir(player.round));
        }

        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetDir(player.round) + PLAYER_PREFIX;
        string countPath = GetDir(player.round) + COUNT_FILE;

        Debug.Log(path + "\n" + countPath);

        int savedPlayers = LoadPlayerCount(player.round);

        FileStream countStream = new FileStream(countPath, FileMode.Create);
        formatter.Serialize(countStream, savedPlayers + 1);
        countStream.Close();

        FileStream stream = new FileStream(path + savedPlayers, FileMode.Create);
        PlayerData data = new PlayerData(player);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    private void SavePlayerOnNet(Player player)
    {
        PlayerData data = new PlayerData(player);
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        formatter.Serialize(ms, data);

        var call = client.SavePlayer(new GrpcNetworking.PlayerData
        {
            Round = data.roundsPlayed,
            SerializedPlayerData = Google.Protobuf.ByteString.CopyFrom(ms.ToArray()),
        });
        ms.Close();
    }

    private Player LoadPlayerFromNet(int round)
    {
        var call = client.LoadPlayer(new GrpcNetworking.LoadRequest
        {
            Round = round,
        });

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        call.SerializedPlayerData.WriteTo(ms);
        PlayerData data = formatter.Deserialize(ms) as PlayerData;
        ms.Close();
        return CreatePlayer(data);
    }

    private Player LoadPlayerLocally(int round) {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetDir(round) + PLAYER_PREFIX;

        int savedPlayers = LoadPlayerCount(round);

        if (savedPlayers < 1) return null;

        int triesLeft = 10;
        while (triesLeft > 0)
        {
            int x = (int)Random.Range(0, savedPlayers - 0.00001f);

            Debug.Log("Loading player " + x + " from round " + round);

            if (File.Exists(path + x))
            {
                FileStream stream = new FileStream(path + x, FileMode.Open);
                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                return CreatePlayer(data);
            }
            triesLeft--;
        }
        return null;
    }

    public Player LoadPlayer(int round)
    {
        if(client != null)
        {
            Debug.Log("Loading player from net");
            return LoadPlayerFromNet(round);
        }
        else
        {
            Debug.Log("No gRPC Client, Loading player locally");
            return LoadPlayerLocally(round);
        }
    }

    public Player CreatePlayer(PlayerData data)
    {
        Player player = Instantiate(playerPrefab, new Vector3(), Quaternion.identity);
        player.level = data.level;
        player.money = data.money;
        player.round = data.roundsPlayed;
        player.tag = "Opponent";

        AddShips(player, data.fleet.ships);

        //player.transform.rotation = new Quaternion(0, 0, 180, 0);

        player.transform.position = new Vector3(10,0,0);

        return player;
    }

    void AddShips(Player player, ShipData[] ships)
    {
        Slot[] shipSlots = player.GetComponentsInChildren<Slot>();

        foreach (var shipData in ships)
        {
            Slot slot = GetSlotByName(shipData.slotName, shipSlots);
            if (!slot)
            {
                Debug.LogError("No slot named " + shipData.slotName);
                continue;
            }

            GameObject go = Instantiate(Resources.Load<GameObject>(shipData.prefabName));
            go.transform.localScale *= Constants.ShipScale;
            go.transform.parent = slot.transform;
            go.transform.position = slot.transform.position;
            Ship ship = go.GetComponent<Ship>();
            ship.hullLayers = shipData.hullLayers;
            ship.layerStrength = shipData.layerStrength;
            ship.Initiative = shipData.initiative;
            ship.Initialize();

            AddTechTiles(ship, shipData.techTiles);

            go.transform.rotation = new Quaternion(0, 0, 180, 0);
        }
    }

    void AddTechTiles(Ship ship, TechTileData[] tiles)
    {
        Slot[] techSlots = ship.GetComponentsInChildren<Slot>();

        foreach (var techData in tiles)
        {
            Slot slot = GetSlotByName(techData.slotName, techSlots);
            if (!slot)
            {
                Debug.LogError("No slot named " + techData.slotName);
                continue;
            }

            GameObject go = Instantiate(Resources.Load<GameObject>(techData.prefabName));
            go.transform.parent = slot.transform;
            go.transform.position = slot.transform.position;
            go.transform.localScale *= Constants.EquipmentScale;

            FireUnit fu = go.GetComponentInChildren<FireUnit>();
            if (fu)
            {
                fu.burstSize = techData.burstSize;
                fu.SetMunitionDamage(techData.munitionDamage);
            }

            ShieldGenerator sg = go.GetComponentInChildren<ShieldGenerator>();
            if (sg)
            {
                sg.shieldStrength = techData.shieldStrength;
                sg.rechargeTime = techData.rechargeTime;
                sg.PlacedOnTarget(slot, true);
            }
        }
    }

    Slot GetSlotByName(string name, Slot[] slots)
    {
        foreach(var slot in slots)
        {
            if (slot.name == name) return slot;
        }
        return null;
    }

}
