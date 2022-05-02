using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType
{
    Tech,
    Ship,
    Shop,
    Inventory
}

public class Slot : MonoBehaviour
{
    public int maxItems;
    public SlotType slotType;
    protected Draggable dragged;
    protected Color defaultColor;
    protected Player player;
    public int itemCount;
    public float animateSpeed = 1f;
    public bool alignVertical = false;

    Animator m_Animator;
    Sprite m_initial_Sprite;

    // Start is called before the first frame update
    void Start()
    {
        BaseStart();
    }

    protected void BaseStart()//Add this so that I can override and call base.BaseStart in inherited classes
    {
        defaultColor = GetComponent<SpriteRenderer>().color;
        FetchPlayer();
        m_initial_Sprite = GetComponent<SpriteRenderer>().sprite;

        m_Animator = GetComponent<Animator>();
        if (m_Animator)
        {
            m_Animator.speed = animateSpeed;
            m_Animator.enabled = false;
        }
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
        if (!ValidCollision(collision)) return;

        dragged.OverSlot(this.transform);

        if (dragged.dropCost <= player.money)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
            //StartAnimation();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!ValidCollision(collision)) return;

        dragged.LeftSlot();

        if (GetComponent<SpriteRenderer>().color == Color.green && m_Animator)
            StopAnimation();

        GetComponent<SpriteRenderer>().color = defaultColor;
    }

    protected virtual bool ValidCollision(Collider2D collision)
    {
        if (IsFilled()) return false;
        if (collision.gameObject.tag != "Draggable Centre") return false;
        
        dragged = collision.gameObject.GetComponentInParent<Draggable>();

        if (!dragged) return false;
        if (dragged.transform.parent == this.transform) return false;
        if (!dragged.isDragged) return false;
        if (slotType != SlotType.Shop
            && slotType != SlotType.Inventory
            && dragged.slotsInto != slotType) return false;

        return true;
    }

    private void StartAnimation()
    {
        if (!m_Animator) return;
        m_Animator.enabled = true;
    }

    private void StopAnimation()
    {
        if (!m_Animator) return;
        m_Animator.enabled = false;
        GetComponent<SpriteRenderer>().sprite = m_initial_Sprite;

    }

    protected void AlignItems()
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

    public virtual void PlaceDraggable(Transform dragged)
    {
        itemCount++;
        Slot oldParent = dragged.GetComponentInParent<Slot>();
        if (oldParent) oldParent.RemovedDraggable(dragged);
        dragged.parent = this.transform;
        GetComponent<SpriteRenderer>().color = defaultColor;
        AlignItems();

        if (slotType == SlotType.Tech)
            dragged.GetComponent<TechTile>().PlacedOnShip(this);

        if (itemCount > 0)
            StartAnimation();
    }

    public virtual void RemovedDraggable(Transform dragged)
    {
        itemCount--;
        GetComponent<SpriteRenderer>().color = defaultColor;

        if (slotType == SlotType.Tech)
            dragged.GetComponent<TechTile>().RemovedFromShip(GetComponentInParent<Ship>());

        if (itemCount == 0)
            StopAnimation();
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
