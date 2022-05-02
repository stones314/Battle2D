using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{

    public SlotType slotsInto;
    public SlotType currentSlot;
    Transform newSlot;
    public bool isDragged;
    public int dropCost;
    Vector3 initialPosition;
    Vector2 clickOffset;
    int initialSortingOrder;

    public PoolItem prefabInfo;

    // Start is called before the first frame update
    void Start()
    {
        newSlot = this.transform.parent;
        currentSlot = GetComponentInParent<Slot>().slotType;
        isDragged = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OverSlot(Transform slot) {
        newSlot = slot;
        dropCost = CalculateDropCost();
    }

    public void LeftSlot()
    {
        newSlot = this.transform.parent;
        dropCost = 0;
    }

    public Transform GetNewSlot()
    {
        return newSlot;
    }

    public SlotType CurrentSlot()
    {
        return currentSlot;
    }

    public void HoverOverEnter()
    {
        transform.localScale *= 1.1f;
    }

    public void HoverOverExit()
    {
        transform.localScale /= 1.1f;
    }

    public void Clicked(Vector2 offset)
    {
        isDragged = true;
        currentSlot = GetComponentInParent<Slot>().slotType;
        initialPosition = transform.position;
        clickOffset = offset;
        dropCost = 0;

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

        if (dropCost <= playerMoney && transform.parent != newSlot)
        {
            newSlot.GetComponent<Slot>().PlaceDraggable(transform);
            return dropCost;
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
        if (currentSlot == SlotType.Shop)
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
}
