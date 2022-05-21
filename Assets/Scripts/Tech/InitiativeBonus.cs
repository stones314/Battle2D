using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitiativeBonus : TechTile
{

    [Tooltip("How many are added")]
    public int initiativeBonus;      //number of added layers

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
        for(int i = 0; i < initiativeBonus; i++)
        {
            AddInitiativeBonus(i);
        }
    }

    private void AddInitiativeBonus(int index)
    {
        int col = index % 3;
        float r = index / 3f;
        int row = (int)r;
        GameObject initiative = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Ships/InitiativeElement"));
        initiative.transform.SetParent(this.transform);
        initiative.transform.localPosition = new Vector3(-initiative.transform.localScale.x * 1.28f, 0.128f, 0);
        initiative.transform.position += Vector3.right * initiative.transform.lossyScale.x * col * 1.28f;
        initiative.transform.position += Vector3.down * initiative.transform.lossyScale.y * row * 1.28f;
    }

    public override void BattleStarted(Player opponent)
    {
        
    }

    public override void BattleEnded()
    {
        
    }

    public override void ApplyBonusesToTarget(Slot slot)
    {
        GetComponentInParent<Ship>().AddBonusInitiative(initiativeBonus);
    }

    public override void RemovedFromShip(Ship oldParent)
    {
        
    }

    public override string GetHoverOverStats()
    {
        return "+" + initiativeBonus + " Initiative";
    }

    public override void Attack()
    {

    }
}
