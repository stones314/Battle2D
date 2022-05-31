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

public enum ItemType
{
    Upgrades,
    Ships,
    Equipments
}

public class ShopPool : MonoBehaviour
{

    [Tooltip("How many items appear at each level")]
    public int[] itemsPerLevel = { 3, 4, 4, 5, 5, 6 };

    [Tooltip("What kind of item this shop slot sells")]
    public ItemType itemType;

    List<List<PoolItem>> itemPool = new List<List<PoolItem>>();

    // Start is called before the first frame update
    void Start()
    {
        for(int level = 1; level < 7; level++)
        {
            List<PoolItem> items = new List<PoolItem>();

            DirectoryInfo itemsPath = new DirectoryInfo(Application.dataPath + "/Resources/" + GetSubDir() + "/Level" + level + "/");
            FileInfo[] itemFiles = itemsPath.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);
            foreach(var file in itemFiles)
            {
                print("Found: " + file.FullName);
                for (int i = 0; i < itemsPerLevel[level - 1]; i++)
                {
                    PoolItem item = new PoolItem(
                        level,
                        GetSlotType(),
                        GetSubDir() + "/Level" + level + "/" + file.Name.Substring(0, file.Name.Length - 7));
                    items.Add(item);
                }
            }

            itemPool.Add(items);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string GetSubDir()
    {
        if (itemType == ItemType.Ships)
            return "Prefabs/Ships";
        if (itemType == ItemType.Equipments)
            return "Prefabs/Tech/Equipments";
        if (itemType == ItemType.Upgrades)
            return "Prefabs/Tech/Upgrades";
        return "";
    }

    SlotType GetSlotType()
    {
        if (itemType == ItemType.Ships)
            return SlotType.Ship;
        return SlotType.Equipment;
    }

    public int GetShopSize(int level)
    {
        return itemsPerLevel[level];
    }

    public List<PoolItem> GetRandomItems(int maxLevel)
    {
        List<PoolItem> items = new List<PoolItem>();

        for(int i = 0; i < itemsPerLevel[maxLevel-1]; i++)
        {
            int level = GetRandomLevel(maxLevel, itemPool);
            items.Add(GetRandomItemAtLevel(level, itemPool));
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
        return pool[level][x];
    }

}
