using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccuracyBonus : TechTile
{
    public int accuracyBonus;

    public Font font;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void GenerateTile()
    {
        GetComponentInChildren<ValueIndicator>().SetValue(accuracyBonus);
    }

    public override void BattleEnded()
    {
        
    }

    public override void BattleStarted(Player opponent)
    {
        
    }

    public override void ApplyBonusesToTarget(Slot slot)
    {
        
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
        return "Accuracy +" + accuracyBonus + "%";
    }

    public override void PrepareCombatAction()
    {

    }
    public override void ExecuteCombatAction()
    {

    }
}
