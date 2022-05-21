using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBonus : TechTile
{
    public int speedBonus;

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
            fu.AddSpeedBonus(speedBonus);
        }
        ShieldGenerator sg = slot.GetComponentInParent<ShieldGenerator>();
        if (sg)
        {
            sg.AddSpeedBonus(speedBonus);
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
        GetComponentInChildren<Accuracy>().SetAccuracy(speedBonus);
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
        return "Reload/Recharge Time: -" +speedBonus + "%";
    }

    public override void Attack()
    {

    }
}
