using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class TechTile : Draggable
{
    public bool singleUse = false;

    public abstract void GenerateTile();

    public abstract void BattleStarted(Player opponent);

    public abstract void BattleEnded();

    public virtual void PlacedOnTarget(Slot slot)
    {
        ApplyBonusesToTarget(slot);
        if (singleUse)
        {
            slot.RemovedDraggable(this);
            Destroy(this.gameObject);
            slot.GetComponentInParent<Draggable>().cost += cost;
        }
    }

    public abstract void ApplyBonusesToTarget(Slot slot);

    public abstract void RemovedFromShip(Ship oldParent);

}
