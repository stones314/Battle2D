using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : ShipAbility
{
    public enum DeathAction
    {
        GiveReassemble,
        SelfReassemble,
        GiveHull
    }
    public DeathAction deathAction = DeathAction.SelfReassemble;

    private bool IsReassemble()
    {
        return deathAction == DeathAction.GiveReassemble
            || deathAction == DeathAction.SelfReassemble;
    }
    [ShowIf("IsReassemble")]
    public ushort shipLevel;
    [ShowIf("IsReassemble")]
    public ushort equipLevel;
    [ShowIf("IsReassemble")]
    public ushort equipCount;

    [ShowIf("deathAction", DeathAction.GiveHull)]
    public ushort hullCount;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate()
    {
        switch (deathAction)
        {
            case DeathAction.GiveReassemble:
                DoGiveReassemble();
                break;
            case DeathAction.SelfReassemble:
                DoSelfReassemble();
                break;
        }
    }

    private void DoGiveReassemble()
    {
        Fleet fleet = GetComponentInParent<Fleet>();
        if (!fleet) return;

        Ship myShip = GetComponent<Ship>();
        if (!myShip)
        {
            return;
        }

        Ship[] targets = fleet.GetComponentsInChildren<Ship>();


        if (targets.Length == 0) return;

        int x = Random.Range(0, targets.Length - 1);

        DeathTrigger nt = targets[x].gameObject.AddComponent<DeathTrigger>();
        nt.deathAction = DeathAction.SelfReassemble;
        nt.shipLevel = shipLevel;
        nt.equipLevel = equipLevel;
        nt.equipCount = equipCount;
    }

    private void DoSelfReassemble()
    {
        if (!GetComponentInParent<Player>()) return;

        Ship myShip = GetComponent<Ship>();
        if (!myShip)
        {
            return;
        }

        Slot slot = myShip.GetComponentInParent<Slot>();
        if (!slot)
        {
            Debug.Log("Reassemble: No slot");
            return;
        }
        slot.GetComponent<ShipSpawner>().AddSpawn(shipLevel, equipLevel, equipCount);

    }
}
