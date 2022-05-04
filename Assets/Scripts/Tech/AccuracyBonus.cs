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
        GetComponentInChildren<Accuracy>().SetAccuracy(accuracyBonus);
    }

    public override void BattleEnded()
    {
        
    }

    public override void BattleStarted(Player opponent)
    {
        
    }

    public override void ApplyBonusesToShip()
    {
        //GetComponentInParent<Ship>().AddAccuracy(accuracyBonus);
    }

    public override void RemovedFromShip(Ship oldParent)
    {
        
    }

}
