using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
//using Grpc.Net.Client.Web;
using System.Net.Http;
//using Grpc.Net.Client;

public class SaveSystem : MonoBehaviour
{
    [SerializeField]
    Player playerPrefab;

    Client client;

    ShopPool shipPool;
    ShopPool equipmentPool;

    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.gameObject.tag);

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        client = GetComponent<Client>();
        shipPool = GameObject.FindGameObjectWithTag("Ship Pool").GetComponent<ShopPool>();
        equipmentPool = GameObject.FindGameObjectWithTag("Equipment Pool").GetComponent<ShopPool>();


    }

    public void SavePlayer(Player player)
    {
        if (client)
        {
            //Debug.Log("Saving player on net");
            SavePlayerOnNet(player);
        }
        else
        {
            Debug.Log("No Client, unable to save player");
        }
    }

    private void SavePlayerOnNet(Player player)
    {
        client.SavePlayer(player);
    }

    private void LoadPlayerFromNet(int round)
    {
        client.BeginLoadPlayer((uint)round);
    }


    public void BeginLoadOpponent(int round)
    {
        if(client)
        {
            //Debug.Log("Loading player from net");
            LoadPlayerFromNet(round);
        }
        else
        {
            Debug.Log("No Client, unable to load player");
        }
    }

    public Player CreatePlayer(PlayerData data)
    {
        Player player = Instantiate(playerPrefab, new Vector3(), Quaternion.identity);
        player.level = data.level;
        player.SetBalance(data.money);
        player.SetRound(data.roundsPlayed);
        //data.health;
        player.tag = "Opponent";

        AddShips(player, data.ships);

        //player.transform.rotation = new Quaternion(0, 0, 180, 0);

        player.transform.position = new Vector3(10,0,0);

        return player;
    }

    void AddShips(Player player, ShipData[] ships)
    {
        Slot[] shipSlots = player.GetComponentsInChildren<Slot>();

        foreach (var shipData in ships)
        {
            Slot slot = GetSlotById(shipData.slotId, shipSlots);
            if (!slot)
            {
                Debug.LogError("No slot with Id " + shipData.slotId);
                continue;
            }

            GameObject go = Instantiate(Resources.Load<GameObject>(shipPool.GetPrefabPathFromId(shipData.prefabId)));
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
            Slot slot = GetSlotById(techData.slotId, techSlots);
            if (!slot)
            {
                Debug.LogError("No slot with Id " + techData.slotId);
                continue;
            }

            GameObject go = Instantiate(Resources.Load<GameObject>(equipmentPool.GetPrefabPathFromId(techData.prefabId)));
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
                sg.rechargeTime = techData.rechargeTime;
                sg.PlacedOnTarget(slot, true);
                sg.InitSG();
            }
        }
    }

    Slot GetSlotById(ushort id, Slot[] slots)
    {
        foreach(var slot in slots)
        {
            if (slot.slotId == id) return slot;
        }
        return null;
    }

}
