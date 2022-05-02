using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class TechTile : MonoBehaviour
{
    public bool singleUse = false;
    public string techName = "";
    public string description = "";
    public int cost = 1000;

    public abstract void GenerateTile();

    public abstract void BattleStarted(Player opponent);

    public abstract void BattleEnded();

    public virtual void PlacedOnShip(Slot slot)
    {
        ApplyBonusesToShip();
        if (singleUse)
        {
            slot.RemovedDraggable(this.transform);
            Destroy(this.gameObject);
        }
    }

    public abstract void ApplyBonusesToShip();

    public abstract void RemovedFromShip(Ship oldParent);

}
