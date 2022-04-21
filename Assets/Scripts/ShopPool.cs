using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public int level;
    public SlotType type;
    public string path;

    public PoolItem(int _level, SlotType _slotType, string _path)
    {
        level = _level;
        type = _slotType;
        path = _path;
    }
}

public class ShopPool : MonoBehaviour
{

    [Tooltip("How many of each tech there are at each level")]
    public int[] techsPerLevel = { 30, 25, 20, 15, 10, 5 };

    [Tooltip("How many shop slots there are for tech tiles at each level")]
    public int[] techSlotsPerLevel = { 3, 4, 4, 5, 5, 6 };

    [Tooltip("How many of each ship there are at each level")]
    public int[] shipssPerLevel = { 30, 25, 20, 15, 10, 5 };

    [Tooltip("How many shop slots there are for ships at each level")]
    public int[] shipSlotsPerLevel = { 1, 1, 1, 2, 2, 2 };

    List<List<PoolItem>> techPool = new List<List<PoolItem>>();
    List<List<PoolItem>> shipPool = new List<List<PoolItem>>();

    // Start is called before the first frame update
    void Start()
    {
        for(int level = 1; level < 7; level++)
        {
            List<PoolItem> techs = new List<PoolItem>();

            DirectoryInfo techPath = new DirectoryInfo(Application.dataPath + "/Resources/Prefabs/TechTiles/Level" + level + "/");
            FileInfo[] techTiles = techPath.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);
            foreach(var tile in techTiles)
            {
                print("Found: " + tile.FullName);
                for (int i = 0; i < techsPerLevel[level - 1]; i++)
                {
                    PoolItem item = new PoolItem(
                        level,
                        SlotType.Tech,
                        "Prefabs/TechTiles/Level" + level + "/" + tile.Name.Substring(0, tile.Name.Length - 7));
                    techs.Add(item);
                }
            }

            techPool.Add(techs);

            List<PoolItem> ships = new List<PoolItem>();

            DirectoryInfo shipPath = new DirectoryInfo(Application.dataPath + "/Resources/Prefabs/Ships/Level" + level + "/");
            FileInfo[] shipTiles = shipPath.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);
            foreach (var ship in shipTiles)
            {
                print("Found: " + ship.FullName);
                for (int i = 0; i < techsPerLevel[level - 1]; i++)
                {
                    PoolItem item = new PoolItem(
                        level,
                        SlotType.Ship,
                        "Prefabs/Ships/Level" + level + "/" + ship.Name.Substring(0, ship.Name.Length - 7));
                    ships.Add(item);
                }
            }

            shipPool.Add(ships);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetShopSize(int level)
    {
        return techSlotsPerLevel[level] + shipSlotsPerLevel[level];
    }

    public List<PoolItem> GetRandomItems(int maxLevel)
    {
        List<PoolItem> items = new List<PoolItem>();

        for(int i = 0; i < techSlotsPerLevel[maxLevel-1]; i++)
        {
            int level = GetRandomLevel(maxLevel, techPool);
            items.Add(GetRandomItemAtLevel(level, techPool));
        }

        for (int i = 0; i < shipSlotsPerLevel[maxLevel - 1]; i++)
        {
            int level = GetRandomLevel(maxLevel, shipPool);
            items.Add(GetRandomItemAtLevel(level, shipPool));
        }

        return items;
    }

    private int GetRandomLevel(int maxLevel, List<List<PoolItem>> pool)
    {
        int opts = 0;
        for (int l = 0; l < maxLevel; l++)
        {
            opts += pool[l].Count;
        }
        int x = (int)Random.Range(0f, opts - 0.000001f);
        int skipped = 0;
        for (int l = 0; l < maxLevel; l++)
        {
            if (x < pool[l].Count + skipped) return l;
            skipped += pool[l].Count;
        }
        return 0;
    }

    private PoolItem GetRandomItemAtLevel(int level, List<List<PoolItem>> pool)
    {
        int x = (int)Random.Range(0f, pool[level].Count - 0.000001f);
        PoolItem item = pool[level][x];
        pool[level].RemoveAt(x);
        return item;
    }

    public void ReturnItems(List<PoolItem> items)
    {
        for(int i = 0; i <  items.Count; i++)
        {
            if (items[i].type == SlotType.Tech)
            {
                try
                {
                    techPool[items[i].level - 1].Add(items[i]);
                }
                catch
                {
                    Debug.LogError("i = " + i + "\ntechPool.Count = " + techPool.Count + "\nitem = " + items[i].ToString());
                }
            }
            if (items[i].type == SlotType.Ship)
                shipPool[items[i].level - 1].Add(items[i]);
        }
    }
}
