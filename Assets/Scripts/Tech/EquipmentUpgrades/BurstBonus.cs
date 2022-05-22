using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstBonus : TechTile
{
    public int burstBonus;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public override void ApplyBonusesToTarget(Slot slot)
    {
        FireUnit fu = slot.GetComponentInParent<FireUnit>();
        if (fu)
        {
            fu.AddBurstBonus(burstBonus);
        }
    }

    public override void BattleEnded()
    {

    }

    public override void BattleStarted(Player opponent)
    {

    }

    public override void GenerateTile()
    {
        GetComponentInChildren<ValueIndicator>().SetValue(burstBonus);
    }

    public override void RemovedFromShip(Ship oldParent)
    {

    }

    public override void Clicked(Vector2 offset)
    {
        base.Clicked(offset);
        transform.localScale /= 2f;
    }

    public override int Dropped(int playerMoney)
    {
        transform.localScale *= 2f;
        return base.Dropped(playerMoney);
    }

    public override string GetHoverOverStats()
    {
        return "Munition Damage: +" + burstBonus;
    }

    public override void PrepareAttack()
    {

    }

    public override void Attack()
    {

    }
}
