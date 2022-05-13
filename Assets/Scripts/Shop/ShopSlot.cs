using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSlot : Slot
{

    ShopPool pool;
    int pendingFetch = 0;

    bool frozen = false;

    public float techScale = 1.0f;
    public float shipScale = 1.0f;

    Color originalDefault;

    Color m_draggedDefaultColor = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        base.BaseStart();

        pool = GetComponent<ShopPool>();
        originalDefault = defaultColor;

        maxItems = pool.GetShopSize(1);

        Roll();
    }

    // Update is called once per frame
    void Update()
    {
        if (pendingFetch > 0)
        {
            pendingFetch--;
        }
        if (pendingFetch == 1)
        {
            FetchNewItems();
        }
    }

    protected override void FetchPlayer()
    {
        player = FindObjectOfType<Player>();
    }

    protected override void SetIndication()
    {
        if (!m_dragged) return;

        if(m_dragged.GetComponentInChildren<SpriteRenderer>().color == Color.red)
        {
            //this means that we got Enter call in new shop slot before Exit call in old shop slot
            //we ignore it here, and tell dragged item do not remove indication in Exit call:
            m_dragged.SetIgnoreNextIndicationReset();
        }
        else
        {
            m_draggedDefaultColor = m_dragged.GetComponentInChildren<SpriteRenderer>().color;
            m_dragged.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
    }

    protected override void RemoveIndication()
    {
        if (!m_dragged) return;

        if(!m_dragged.GetIgnoreNextIndicationReset())
            m_dragged.GetComponentInChildren<SpriteRenderer>().color = m_draggedDefaultColor;
    }

    public void ReturnUnboughtItems()
    {
        Draggable[] items = GetComponentsInChildren<Draggable>();
        foreach (var item in items)
        {
            if (item.transform.parent == this.transform)
            {
                RemovedDraggable(item);
                Object.Destroy(item.gameObject);
            }
        }

        foreach (var infoItem in GetComponentsInChildren<ShopItemInfo>())
        {
            Object.Destroy(infoItem.gameObject);
        }
    }

    public void FetchNewItems()
    {
        List<PoolItem> items = pool.GetRandomItems(player.level);

        foreach (var item in items)
        {
            GameObject gameObject = null;
            ShopItemInfo itemInfo = null;

            //Fetch Holder:
            try
            {
                if (item.type == SlotType.Ship)
                    itemInfo = Instantiate(Resources.Load<ShopItemInfo>("Prefabs/Ships/ShipInfo"));
                else
                    itemInfo = Instantiate(Resources.Load<ShopItemInfo>("Prefabs/Tech/TechInfo"));
                itemInfo.transform.parent = this.transform;
            }
            catch
            {
                Debug.LogError("Null when instantiating?");
            }

            try
            {
                gameObject = Instantiate(Resources.Load<GameObject>(item.path));
                gameObject.transform.parent = this.transform;
            }
            catch
            {
                Debug.LogError("Null when instantiating?");
            }

            if (item.type == SlotType.Equipment || item.type == SlotType.ShipUpgrade || item.type == SlotType.EquipmentUpgrade)
            {
                gameObject.transform.localScale *= techScale;
                gameObject.GetComponent<TechTile>().GenerateTile();
            }

            if (item.type == SlotType.Ship)
            {
                gameObject.transform.localScale *= shipScale;
                gameObject.GetComponent<Ship>().GenerateHullMeter();//Must be done after rescale
            }

            Draggable dragged = gameObject.GetComponent<Draggable>();
            dragged.Initialize(this, item);

            PlaceDraggable(dragged);

            itemInfo.transform.position = dragged.transform.position;
            itemInfo.SetCost(dragged.cost);
            itemInfo.SetDisplayName(dragged.displayName);
        }
        AlignItems();
    }

    public void Roll()
    {
        ReturnUnboughtItems();
        pendingFetch = 5;//
        if (frozen) ToggleFreeze();
    }

    public void ToggleFreeze()
    {
        frozen = !frozen;

        if(frozen)
            defaultColor = Color.cyan;
        else
            defaultColor = originalDefault;
        GetComponent<SpriteRenderer>().color = defaultColor;
    }

    public void Sell(Transform item)
    {
        Object.Destroy(item.gameObject);

        //player.IncreaseBalance(1000);
    }

    public override void PlaceDraggable(Draggable dragged)
    {
        if (pendingFetch > 0)
        {
            base.PlaceDraggable(dragged);
        }
        else
        {
            Slot oldSlot = dragged.GetCurrentSlot();
            if (oldSlot) oldSlot.RemovedDraggable(dragged);
            RemoveIndication();
            Sell(dragged.transform);
        }
    }

    public override void RemovedDraggable(Draggable dragged)
    {
        base.RemovedDraggable(dragged);

        if(dragged.GetNewSlot().slotType != SlotType.Shop)
            Destroy(GetComponentInChildren<ShopItemInfo>().gameObject);
    }

    public override bool IsFilled()
    {
        return false;
    }

    public void SetEnableShop(bool enable)
    {
        this.gameObject.SetActive(enable);

        if (enable)
        {
            if (frozen)
                ToggleFreeze();
            else
                Roll();
        }
    }
}
