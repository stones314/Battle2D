using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public string displayName;
    public string description;
    public SlotType slotsInto;
    public int cost;
    Slot newSlot;
    Slot currentSlot;
    private bool isDragged;
    Vector3 initialPosition;
    Vector2 clickOffset;

    PoolItem prefabInfo;
    public ShopItemInfo itemInfoObj;

    private bool ignoreNextIndicationReset = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Initialize(Slot startSlot, PoolItem item)
    {
        newSlot = startSlot;
        currentSlot = startSlot;
        isDragged = false;
        prefabInfo = item;
    }

    public void DestroyShopInfo()
    {
        Debug.Log("Try destroy shop info");
        if(itemInfoObj)
        {
            Debug.Log("Destroy shop info");
            Destroy(itemInfoObj.gameObject);
        }
    }

    public string GetPrefabPath()
    {
        return prefabInfo.path;
    }

    public void OverSlot(Slot slot) {
        newSlot = slot;
    }

    public void LeftSlot()
    {
        newSlot = currentSlot;
    }

    public Slot GetNewSlot()
    {
        return newSlot;
    }

    public Slot GetCurrentSlot()
    {
        return currentSlot;
    }

    public SlotType CurrentSlotType()
    {
        return GetCurrentSlot().slotType;
    }

    public void HoverOverEnter()
    {
        //transform.localScale *= 1.1f;
    }

    public void HoverOverExit()
    {
        //transform.localScale /= 1.1f;
    }

    public virtual void Clicked(Vector2 offset)
    {
        isDragged = true;
        initialPosition = transform.position;
        clickOffset = offset;

        ChangeSortOrder(20);
    }

    public void DragTo(Vector3 mousePos)
    {
        transform.position = mousePos + (Vector3)clickOffset;
    }

    public virtual int Dropped(int playerMoney)
    {
        isDragged = false;

        ChangeSortOrder(-20);

        int dc = CalculateDropCost();

        if (dc <= playerMoney && currentSlot != newSlot)
        {
            newSlot.PlaceDraggable(this);
            currentSlot = newSlot;
            return dc;
        }
        
        transform.position = initialPosition;
        return 0;
    }

    private void ChangeSortOrder(int amount)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.sortingOrder += amount;
        }
        foreach (var c in GetComponentsInChildren<Canvas>())
        {
            c.sortingOrder += amount;
        }
    }

    private int CalculateDropCost()
    {
        SlotType toSlot = newSlot.slotType;

        //Sell is free:
        if (toSlot == SlotType.Shop)
        {
            int c = 0;
            foreach(var d in GetComponentsInChildren<Draggable>())
            {
                c += d.cost;
            }

            return -c/3;
        }

        //Buy from shop costs 3000 (regardless of where it is placed):
        if (CurrentSlotType() == SlotType.Shop)
        {
            return cost;
        }

        //Move to inventory is free:
        if (toSlot == SlotType.Inventory)
        {
            return 0;
        }

        //Remaining option is to reconfigurie battlefield, which costs 0:
        return 0;
    }

    public bool IsDragged()
    {
        return isDragged;
    }

    public void SetIgnoreNextIndicationReset()
    {
        ignoreNextIndicationReset = true;
    }
    public bool GetIgnoreNextIndicationReset()
    {
        bool doit = ignoreNextIndicationReset;
        ignoreNextIndicationReset = false;
        return doit;
    }

    public virtual string GetDescription()
    {
        return description;
    }

    public virtual string GetHoverOverStats()
    {
        return "";
    }
}
