using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HullGrow : MonoBehaviour
{
    public enum HullGrowCondition
    {
        HullUpgraded,
        InitiativeUpgraded,
        ShipUpgraded,
        ShipEquipped,
        FireUnitUpgraded,
        ShieldUpgraded,
        EquipmentUpgraded,
    }

    public int growth = 1;
    public HullGrowCondition condition = HullGrowCondition.ShipUpgraded;
    
    private void OnEnable()
    {
        EventManager.OnTechPlaced += TriggerHullGrow;
    }

    
    private void OnDisable()
    {
        EventManager.OnTechPlaced -= TriggerHullGrow;
    }

    public void TriggerHullGrow(TechTile tech, Slot slot)
    {
        if (!GetComponentInParent<Player>()) return;

        switch (condition) {
            case HullGrowCondition.HullUpgraded:
                if (!tech.GetComponentInChildren<HullBonus>()) return;
                break;
            case HullGrowCondition.InitiativeUpgraded:
                if (!tech.GetComponentInChildren<InitiativeBonus>()) return;
                break;
            case HullGrowCondition.ShipUpgraded:
                if (slot.slotType != SlotType.ShipUpgrade) return;
                break;
            case HullGrowCondition.ShipEquipped:
                if (slot.slotType != SlotType.Equipment) return;
                break;
            case HullGrowCondition.EquipmentUpgraded:
                if (slot.slotType != SlotType.EquipmentUpgrade) return;
                break;
        }

        Ship ship = GetComponentInParent<Ship>();
        if (ship)
        {
            ship.AddBonusLayers(growth);
        }
    }

}
