using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    ShopPool shipPool;
    ShopPool equipmentPool;
    Slot slot;
    float spawnTime = 1;
    ushort shipLevel = 1;
    ushort maxEquipCount = 1;
    ushort equipLevel = 1;
    bool doSpawn = false;
    string enemyTag;

    // Start is called before the first frame update
    void Start()
    {
        slot = GetComponent<Slot>();
        Player owner = GetComponentInParent<Player>();
        if (owner.tag == "Opponent")
        {
            shipPool = owner.GetEnemy().GetShipPool();
            shipPool = owner.GetEnemy().GetEquipPool();
            enemyTag = "Player";
        }
        else
        {
            shipPool = GameObject.FindGameObjectWithTag("ShipShopSlot").GetComponent<ShopPool>();
            equipmentPool = GameObject.FindGameObjectWithTag("EquipShopSlot").GetComponent<ShopPool>();
            enemyTag = "Opponent";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!doSpawn) return;
        if (Time.time < spawnTime) return;

        SpawnShip();
        doSpawn = false;
    }

    public bool IsSpawning()
    {
        return doSpawn;
    }

    public ShopPool GetShipPool()
    {
        return shipPool;
    }
    public ShopPool GetEquipPool()
    {
        return equipmentPool;
    }

    public void Activate(ushort shipLevel_, ushort equipLevel_, ushort equipCount_, float delay_)
    {
        spawnTime = Time.time + delay_;
        shipLevel = shipLevel_;
        equipLevel = equipLevel_;
        maxEquipCount = equipCount_;
        doSpawn = true;
    }

    private void SpawnShip()
    {
        GameObject go = Instantiate(Resources.Load<GameObject>(shipPool.GetRandomItemAtLevel(shipLevel-1, shipLevel-1).path));
        go.transform.localScale *= Constants.ShipScale;
        go.transform.parent = slot.transform;
        go.transform.position = slot.transform.position;
        Ship ship = go.GetComponent<Ship>();
        ship.Initialize(true);

        AddEquipment(ship);

        ship.BattleStarted(GameObject.FindGameObjectWithTag(enemyTag).GetComponent<Player>());
        EventManager.NotifyShipSpawned(ship);
    }

    private void AddEquipment(Ship ship)
    {
        Slot[] techSlots = ship.GetComponentsInChildren<Slot>();
        if (techSlots.Length < maxEquipCount)
            maxEquipCount = (ushort)techSlots.Length;
        
        for (int t = 0; t < maxEquipCount; t++)
        {
            if (techSlots[t].gameObject.tag != "Tech Slot") continue;
            GameObject go = Instantiate(Resources.Load<GameObject>(equipmentPool.GetRandomItemAtLevel(0, equipLevel-1).path));
            go.transform.parent = techSlots[t].transform;
            go.transform.position = techSlots[t].transform.position;
            go.transform.localScale *= Constants.EquipmentScale;

            TechTile tech = go.GetComponent<TechTile>();
            tech.PlacedOnTarget(techSlots[t], true);
        }
    }
}
