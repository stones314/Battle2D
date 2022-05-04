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

        Roll(false);
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

        m_draggedDefaultColor = m_dragged.GetComponentInChildren<SpriteRenderer>().color;
        m_dragged.GetComponentInChildren<SpriteRenderer>().color = Color.red;
    }

    protected override void RemoveIndication()
    {
        if (!m_dragged) return;

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
    }

    public void FetchNewItems()
    {
        List<PoolItem> items = pool.GetRandomItems(player.level);

        foreach (var item in items)
        {
            GameObject gameObject = null;
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
            dragged.Initialize(this);

            PlaceDraggable(dragged);
        }
        AlignItems();
    }

    public void Roll(bool doPay = true)
    {
        if(doPay) player.TryDecreeseBalance(1000);
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

        player.IncreaseBalance(1000);
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
                Roll(false);
        }
    }
}
