using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType
{
    Equipment,
    EquipmentUpgrade,
    Ship,
    ShipUpgrade,
    Shop,
    Inventory
}

public class Slot : MonoBehaviour
{
    public int maxItems;
    public SlotType slotType;
    protected Draggable m_dragged;
    protected Color defaultColor = Color.white;
    protected Player player;
    public int itemCount;
    public bool alignVertical = false;

    // Start is called before the first frame update
    void Start()
    {
        BaseStart();
    }

    protected void BaseStart()//Add this so that I can override and call base.BaseStart() in inherited classes
    {
        defaultColor = GetComponentInParent<SpriteRenderer>().color;
        FetchPlayer();
    }

    protected virtual void FetchPlayer()
    {
        player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!ValidCollision(collision.gameObject)) return;

        m_dragged = collision.gameObject.GetComponentInParent<Draggable>();
        m_dragged.OverSlot(this);

        if (m_dragged.cost <= player.money)
        {
            SetIndication();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!ValidCollision(collision.gameObject)) return;

        RemoveIndication();

        m_dragged.LeftSlot();
        m_dragged = null;
    }

    protected virtual void SetIndication()
    {
        GetComponentInParent<SpriteRenderer>().color = Color.green;
    }

    protected virtual void RemoveIndication()
    {
        GetComponentInParent<SpriteRenderer>().color = defaultColor;
    }

    protected virtual bool ValidCollision(GameObject other)
    {
        if (IsFilled()) return false;
        if (other.tag != "Draggable Centre") return false;
        
        Draggable dragged = other.GetComponentInParent<Draggable>();

        //only a draggable dragged over slot can slot into it:
        if (!dragged) return false;
        if (!dragged.IsDragged()) return false;

        //must move to a new slot:
        if (dragged.GetCurrentSlot() == this) return false;

        //Can not move from shop to shop:
        if (dragged.GetCurrentSlot().slotType == SlotType.Shop && slotType == SlotType.Shop) return false;

        //check that the draggable can slot into this type of slot:
        //everything matches Shop and Inventory:
        if (slotType == SlotType.Shop) return true;
        if (slotType == SlotType.Inventory) return true;

        //Can not move onto an item in the shop:
        if (GetComponentInParent<Shop>()) return false;

        //Not shop or inventory: must match exactly:
        if (dragged.slotsInto != slotType) return false;

        return true;
    }

    protected virtual void AlignItems()
    {
        float slotDimension = 1.0f / maxItems;
        float nextPos = -0.5f + slotDimension / 2;
        Draggable[] items = GetComponentsInChildren<Draggable>();
        foreach (Draggable d in items)
        {
            if (d.transform.parent == this.transform)
            {
                if(alignVertical)
                    d.transform.localPosition = new Vector3(0, nextPos, 0.0f);
                else
                    d.transform.localPosition = new Vector3(nextPos, 0.0f, 0.0f);
                nextPos += slotDimension;
            }
        }
    }

    protected virtual Transform GetNewParent()
    {
        return this.transform;
    }

    public virtual void PlaceDraggable(Draggable dragged)
    {
        itemCount++;

        Slot oldSlot = dragged.GetCurrentSlot();
        if (oldSlot) oldSlot.RemovedDraggable(dragged);

        dragged.transform.parent = GetNewParent();

        RemoveIndication();
        AlignItems();
        
        if (dragged.GetNewSlot().slotType == SlotType.EquipmentUpgrade || dragged.GetNewSlot().slotType == SlotType.ShipUpgrade || dragged.GetNewSlot().slotType == SlotType.Equipment)
            dragged.GetComponent<TechTile>().PlacedOnTarget(this);
    }

    public virtual void RemovedDraggable(Draggable dragged)
    {
        itemCount--;
        RemoveIndication();

        if (slotType == SlotType.Equipment)
            dragged.GetComponent<TechTile>().RemovedFromShip(GetComponentInParent<Ship>());
    }

    public virtual bool IsFilled()
    {
        return itemCount >= maxItems;
    }

    public int AvailableSlots()
    {
        return maxItems - itemCount;
    }
}
