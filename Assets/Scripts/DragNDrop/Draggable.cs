using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public string displayName;
    public string description;
    public SlotType slotsInto;
    public int cost;
    Transform newSlot;
    private bool isDragged;
    Vector3 initialPosition;
    Vector2 clickOffset;

    public PoolItem prefabInfo;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Initialize(Slot startSlot)
    {
        newSlot = startSlot.transform;
        isDragged = false;
    }

    public void OverSlot(Transform slot) {
        newSlot = slot;
        cost = CalculateDropCost();
    }

    public void LeftSlot()
    {
        newSlot = this.transform.parent;
        cost = 0;
    }

    public Transform GetNewSlot()
    {
        return newSlot;
    }

    public Slot GetCurrentSlot()
    {
        return transform.parent.GetComponent<Slot>();
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

    public void Clicked(Vector2 offset)
    {
        isDragged = true;
        initialPosition = transform.position;
        clickOffset = offset;
        cost = 0;

        ChangeSortOrder(20);
    }

    public void DragTo(Vector3 mousePos)
    {
        transform.position = mousePos + (Vector3)clickOffset;
    }

    public int Dropped(int playerMoney)
    {
        isDragged = false;

        ChangeSortOrder(-20);

        if (cost <= playerMoney && transform.parent != newSlot)
        {
            newSlot.GetComponent<Slot>().PlaceDraggable(this);
            return cost;
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
        SlotType toSlot = newSlot.GetComponent<Slot>().slotType;

        //Sell is free:
        if (toSlot == SlotType.Shop)
        {
            return 0;
        }

        //Buy from shop costs 3000 (regardless of where it is placed):
        if (CurrentSlotType() == SlotType.Shop)
        {
            return 3000;
        }

        //Move to inventory is free:
        if (toSlot == SlotType.Inventory)
        {
            return 0;
        }

        //Remaining option is to reconfigurie battlefield, which costs 500:
        return 500;
    }

    public bool IsDragged()
    {
        return isDragged;
    }
}
