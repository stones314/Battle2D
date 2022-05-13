using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HullBonus : TechTile
{

    [Tooltip("How many layers are added")]
    public int layerBonus;      //number of added layers

    [Tooltip("How much is the strengh of layers increased (0-100%)")]
    public int strengthBonus;   //percentage, effects all existing layers

    //TODO: script creating of image:
    // show x hullElements where x is layer-boonus
    // show percentage as text

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
        for(int i = 0; i < layerBonus; i++)
        {
            AddLayerBonus(i);
        }
    }

    private void AddLayerBonus(int layerNumber)
    {
        int col = layerNumber % 3;
        float r = layerNumber / 3f;
        int row = (int)r;
        HullElement hullElement = Instantiate<HullElement>(Resources.Load<HullElement>("Prefabs/Ships/HullElement"));
        hullElement.transform.SetParent(this.transform);
        hullElement.transform.localPosition = new Vector3(-hullElement.transform.localScale.x * 2.56f, 0.256f, 0);
        hullElement.transform.position += Vector3.right * hullElement.transform.lossyScale.x * col * 2.56f;
        hullElement.transform.position += Vector3.down * hullElement.transform.lossyScale.y * row * 2.56f;
        hullElement.InitHull(10);
    }

    public override void BattleStarted(Player opponent)
    {
        
    }

    public override void BattleEnded()
    {
        
    }

    public override void ApplyBonusesToTarget(Slot slot)
    {
        GetComponentInParent<Ship>().AddBonusLayers(layerBonus);
    }

    public override void RemovedFromShip(Ship oldParent)
    {
        
    }
    public override string GetHoverOverStats()
    {
        return "+" + layerBonus + " Hull";
    }
}
