using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Cannon,
    Laser,
    Missile
}

public class Weapon : TechTile
{
    public WeaponType weaponType;

    FireUnit fireUnit;

    // Start is called before the first frame update
    void Start()
    {
        fireUnit = GetComponentInChildren<FireUnit>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void GenerateTile()
    {

    }

    public override void BattleEnded()
    {
        this.gameObject.SetActive(true);
        FireUnit[] fireUnits = GetComponentsInChildren<FireUnit>(true);
        foreach (var fireUnit in fireUnits)
        {
            fireUnit.BattleEnded();
        }
    }

    public override void BattleStarted(Player opponent)
    {
        FireUnit[] fireUnits = GetComponentsInChildren<FireUnit>();
        foreach (var fireUnit in fireUnits)
        {
            fireUnit.BattleStarted(opponent);
        }

    }

    public override void ApplyBonusesToTarget(Slot slot)
    {
        
    }

    public override void RemovedFromShip(Ship oldParent)
    {
        
    }

    public override string GetHoverOverStats()
    {
        return fireUnit.StatsToString();
    }

    public float GetDamagePerAttack()
    {
        return fireUnit.GetDamagePerAttack();
    }

    public override bool HasCombatAction()
    {
        return true;
    }

    public override void PrepareCombatAction()
    {
        FireUnit[] fireUnits = GetComponentsInChildren<FireUnit>();
        foreach (var fireUnit in fireUnits)
        {
            fireUnit.PrepareAttack();
        }
    }

    public override void ExecuteCombatAction()
    {
        FireUnit[] fireUnits = GetComponentsInChildren<FireUnit>();
        foreach (var fireUnit in fireUnits)
        {
            fireUnit.Attack();
        }
    }
}
