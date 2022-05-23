using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RechargeBonus : TechTile
{
    public int rechageBonus;

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
        ShieldGenerator sg = slot.GetComponentInParent<ShieldGenerator>();
        if (sg)
        {
            sg.AddRechargeBonus(rechageBonus);
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
        GetComponentInChildren<ValueIndicator>().SetValue(rechageBonus);
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
        return "Recharge Time: -" +rechageBonus + "%";
    }

    public override void PrepareCombatAction()
    {

    }

    public override void ExecuteCombatAction()
    {

    }
}
