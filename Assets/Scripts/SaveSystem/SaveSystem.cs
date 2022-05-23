using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    [SerializeField]
    Player playerPrefab;

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

    public Player LoadPlayer(int round)
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
            Ship ship = go.GetComponent<Ship>();
            go.transform.parent = slot.transform;
            go.transform.localScale *= 1.8f;
            go.transform.position = slot.transform.position;
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
            go.transform.localScale *= 2.2f;

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
