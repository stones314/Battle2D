using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Text pendingSpawnsInfo;

    ShopPool shipPool;
    ShopPool equipmentPool;
    Slot slot;
    struct SpawnInfo
    {
        public float spawnTime;
        public ushort shipLevel;
        public ushort maxEquipCount;
        public ushort equipLevel;
    }
    Queue<SpawnInfo> spawnQueue = new Queue<SpawnInfo>();
    float spawnTime;
    bool spawnTimeSet = false;
    string enemyTag;

    // Start is called before the first frame update
    void Start()
    {
        pendingSpawnsInfo.text = "";
        slot = GetComponent<Slot>();
        Player owner = GetComponentInParent<Player>();
        shipPool = GameObject.FindGameObjectWithTag("Ship Pool").GetComponent<ShopPool>();
        equipmentPool = GameObject.FindGameObjectWithTag("Equipment Pool").GetComponent<ShopPool>();
        if (owner.tag == "Opponent")
        {
            enemyTag = "Player";
        }
        else
        {
            enemyTag = "Opponent";
        }
    }

    // Update is called once per frame
    void Update()
    {
        SetInfoCount();

        if (spawnQueue.Count == 0) return;                  //nothing to spawn
        if (slot.GetComponentInChildren<Ship>()) return;    //no room to spawn

        //Spawn a new ship! But add a short delay
        //Initialize the delay if it has not already been done: 
        if (!spawnTimeSet)
        {
            spawnTime = Time.time + 1 / Constants.GameSpeed;
            spawnTimeSet = true;
        }
        if (Time.time < spawnTime) return;

        SpawnShip();
        spawnTimeSet = false;
    }

    public bool IsSpawning()
    {
        return spawnQueue.Count > 0;
    }

    public ShopPool GetShipPool()
    {
        return shipPool;
    }
    public ShopPool GetEquipPool()
    {
        return equipmentPool;
    }

    public void AddSpawn(ushort shipLevel_, ushort equipLevel_, ushort equipCount_)
    {
        spawnQueue.Enqueue(new SpawnInfo
        {
            shipLevel = shipLevel_,
            equipLevel = equipLevel_,
            maxEquipCount = equipCount_
        });
        pendingSpawnsInfo.text = spawnQueue.Count.ToString();
    }

    private void SpawnShip()
    {
        SpawnInfo si = spawnQueue.Dequeue();
        GameObject go = Instantiate(Resources.Load<GameObject>(shipPool.GetRandomItemAtLevel(si.shipLevel-1, si.shipLevel -1).path));
        go.transform.localScale *= Constants.ShipScale;
        go.transform.parent = slot.transform;
        go.transform.position = slot.transform.position;
        Ship ship = go.GetComponent<Ship>();
        ship.Initialize(true);

        AddEquipment(ship, si);

        if(enemyTag == "Player")
            go.transform.rotation = new Quaternion(0, 0, 180, 0);

        ship.BattleStarted(GameObject.FindGameObjectWithTag(enemyTag).GetComponent<Player>());
        EventManager.NotifyShipSpawned(ship);
    }

    private void AddEquipment(Ship ship, SpawnInfo si)
    {
        Slot[] techSlots = ship.GetComponentsInChildren<Slot>();
        if (techSlots.Length < si.maxEquipCount)
            si.maxEquipCount = (ushort)techSlots.Length;
        
        for (int t = 0; t < si.maxEquipCount; t++)
        {
            if (techSlots[t].gameObject.tag != "Tech Slot") continue;
            GameObject go = Instantiate(Resources.Load<GameObject>(equipmentPool.GetRandomItemAtLevel(0, si.equipLevel -1).path));
            go.transform.parent = techSlots[t].transform;
            go.transform.position = techSlots[t].transform.position;
            go.transform.localScale *= Constants.EquipmentScale;

            TechTile tech = go.GetComponent<TechTile>();
            tech.PlacedOnTarget(techSlots[t], true);
        }
    }

    private int GetShipSpawnCount()
    {
        Ship ship = GetComponentInChildren<Ship>();
        if (!ship) return 0;

        int c = 0;
        foreach (var dt in ship.GetComponents<DeathTrigger>())
        {
            if (dt.deathAction == DeathTrigger.DeathAction.SelfReassemble)
            {
                c++;
            }

        }
        return c;
    }

    private void SetInfoCount()
    {
        string info = "";
        int count = GetShipSpawnCount() + spawnQueue.Count;
        if (count > 0) info = count.ToString();
        pendingSpawnsInfo.text = info;
    }
}
