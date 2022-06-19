using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class TechTile : Draggable
{
    public bool singleUse = false;
    private bool firstPlacement = true;    //So that tech placement triggers only once for equipments

    /**
     * Shoping Related Methods:
     */

    public abstract void GenerateTile();

    public virtual void PlacedOnTarget(Slot slot, bool atLoad = false)
    {
        if (atLoad) firstPlacement = false;
        if (firstPlacement)
        {
            EventManager.NotifyTechPlaced(this, slot);
            firstPlacement = false;
        }
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

    /**
     * Battle Related Methods:
     */

    public abstract void BattleStarted(Player opponent);

    public abstract void BattleEnded();

    public virtual bool HasCombatAction() { return false; }
    public abstract void PrepareCombatAction();
    public abstract void ExecuteCombatAction(Ship target);

    public virtual TechTileData ToTechTileData()
    {
        TechTileData ttd = new TechTileData();
        ttd.prefabId = GetPrefabId();
        ttd.slotId = GetComponentInParent<Slot>().slotId;

        FireUnit fu = GetComponentInChildren<FireUnit>();
        if (fu)
        {
            ttd.burstSize = (ushort)fu.burstSize;
            ttd.munitionDamage = fu.GetMunitionDamage();
        }

        ShieldGenerator sg = GetComponentInChildren<ShieldGenerator>();
        if (sg)
        {
            ttd.shieldStrength = 0;
            ttd.rechargeTime = sg.rechargeTime;
        }

        return ttd;
    }
}
