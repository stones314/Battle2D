using NaughtyAttributes;
using UnityEngine;

public class DeathTrigger : ShipAbility
{
    public enum DeathAction
    {
        GiveReassemble,
        SelfReassemble,
        GiveHullOne,
        GiveHullAll,
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

    private bool IsGiveHull()
    {
        return deathAction == DeathAction.GiveHullOne
            || deathAction == DeathAction.GiveHullAll;
    }
    [ShowIf("IsGiveHull")]
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
            case DeathAction.GiveHullOne:
                DoGiveHullOne();
                break;
            case DeathAction.GiveHullAll:
                DoGiveHullAll();
                break;
        }
    }

    private Ship GetRandomOtherShip()
    {
        Fleet fleet = GetComponentInParent<Fleet>();
        if (!fleet) return null;

        Ship[] targets = fleet.GetComponentsInChildren<Ship>();


        if (targets.Length == 0) return null;

        int x = Random.Range(0, targets.Length - 1);

        return targets[x];
    }

    private void DoGiveReassemble()
    {
        Ship target = GetRandomOtherShip();
        if (!target) return;

        DeathTrigger nt = target.gameObject.AddComponent<DeathTrigger>();
        nt.deathAction = DeathAction.SelfReassemble;
        nt.shipLevel = shipLevel;
        nt.equipLevel = equipLevel;
        nt.equipCount = equipCount;
    }


    private void DoGiveHullOne()
    {
        Ship target = GetRandomOtherShip();
        if (!target) return;

        target.AddBonusLayers(hullCount);
    }

    private void DoGiveHullAll()
    {
        Fleet fleet = GetComponentInParent<Fleet>();
        if (!fleet) return;

        Ship[] targets = fleet.GetComponentsInChildren<Ship>();

        foreach (var target in targets)
        {
            target.AddBonusLayers(hullCount);
        }
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
