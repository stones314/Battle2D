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

    public void ReturnUnboughtItems()
    {
        Draggable[] items = GetComponentsInChildren<Draggable>();
        foreach (var item in items)
        {
            if (item.transform.parent == this.transform)
            {
                RemovedDraggable(item.transform);
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
            }
            catch
            {
                Debug.LogError("Null when instantiating?");
            }
            

            if (item.type == SlotType.Tech)
            {
                gameObject.transform.localScale *= techScale;
                gameObject.GetComponent<TechTile>().GenerateTile();
            }

            if (item.type == SlotType.Ship)
            {
                gameObject.transform.localScale *= shipScale;
                gameObject.GetComponent<Ship>().GenerateHullMeter();//Must be done after rescale
            }

            PlaceDraggable(gameObject.transform);
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

    public override void PlaceDraggable(Transform dragged)
    {
        if (pendingFetch > 0)
        {
            base.PlaceDraggable(dragged);
        }
        else
        {
            Slot oldParent = dragged.GetComponentInParent<Slot>();
            if (oldParent) oldParent.RemovedDraggable(dragged);
            GetComponent<SpriteRenderer>().color = defaultColor;
            Sell(dragged);
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
