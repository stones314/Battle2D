using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reassemble : MonoBehaviour
{
    [Tooltip("Level of new ship")]
    public ushort shipLevel = 1;
    
    [Tooltip("How many equipments that will be added (if ship has enough slots)")]
    public ushort maxEquipCount = 1;

    [Tooltip("Level of attached equipments")]
    public ushort equipLevel = 1;

    private void OnEnable()
    {
        EventManager.OnShipDestroyed += OnShipDestruction;
    }

    private void OnDisable()
    {
        EventManager.OnShipDestroyed -= OnShipDestruction;
    }

    private void OnShipDestruction(Ship destroyedShip)
    {
        if (!GetComponentInParent<Player>()) return;

        Ship myShip = GetComponentInParent<Ship>();
        if (!myShip) return;
        if (myShip != destroyedShip) return;

        Slot slot = myShip.GetCurrentSlot();
        if (!slot) return;
        slot.GetComponent<ShipSpawner>().Activate(shipLevel, equipLevel, maxEquipCount, 1);
    }
}
