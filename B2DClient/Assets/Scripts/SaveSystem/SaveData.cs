using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData
{

}

[System.Serializable]
public class PlayerData
{
    public int roundsPlayed;    //how many rounds this player has played
    public int level;           //what level the player is at
    public int money;           //how much money the player has
    public int health;          //how much health the player has
    public FleetData fleet;     //the current fleet the player has
    public int numTechInventory;//how many tech player has in inventory
    public TechTileData[] techInventory;//the techs in inventory

    public PlayerData(Player player)
    {
        roundsPlayed = 0;
        level = player.level;
        money = player.money;
        health = 1;
        fleet = new FleetData(player.GetComponentInChildren<Fleet>());

        numTechInventory = 0;
        for (int i = 0; i < numTechInventory; i++)
        {
            //techInventory[i]
        }
    }
}

[System.Serializable]
public class FleetData
{
    public int numShips;            //number of ships in fleet
    public ShipData[] ships;        //list of ships

    public FleetData(Fleet fleet)
    {
        Ship[] ships_ = fleet.GetComponentsInChildren<Ship>();
        numShips = ships_.Length;
        ships = new ShipData[numShips];
        for (int i = 0; i < numShips; i++)
        {
            ships[i] = new ShipData(ships_[i]);
        }
    }
}

[System.Serializable]
public class ShipData
{
    public string prefabName;       //name of ship prefab
    public string slotName;         //slot where ship is placed
    public int hullLayers;          //number of hull layers
    public int layerStrength;       //strenght of each layer
    public int initiative;          //initiative of ship
    public TechTileData[] techTiles;//list of tech tiles attached

    public ShipData(Ship ship)
    {
        prefabName = ship.GetPrefabPath();
        slotName = ship.GetComponentInParent<Slot>().name;
        hullLayers = ship.hullLayers;
        layerStrength = (int)ship.layerStrength;
        initiative = ship.Initiative;

        TechTile[] shipTech = ship.GetComponentsInChildren<TechTile>();
        int numTechTiles = shipTech.Length;
        techTiles = new TechTileData[numTechTiles];

        for (int i = 0; i < numTechTiles; i++)
        {
            techTiles[i] = new TechTileData(shipTech[i]);
        }
    }
}

[System.Serializable]
public class TechTileData
{
    public string prefabName;       //name of tech prefab
    public string slotName;         //name of slot where tech is attached

    //fire unit params:
    public int burstSize;           //burst size of fire unit
    public float munitionDamage;    //damage of a single munition

    //shield generator params
    public float shieldStrength;    //strength of shield
    public float rechargeTime;      //recharge time of shield

    public TechTileData(TechTile techTile)
    {
        prefabName = techTile.GetPrefabPath();
        slotName = techTile.GetComponentInParent<Slot>().name;

        FireUnit fu = techTile.GetComponentInChildren<FireUnit>();
        if (fu)
        {
            burstSize = fu.burstSize;
            munitionDamage = fu.GetMunitionDamage();
        }

        ShieldGenerator sg = techTile.GetComponentInChildren<ShieldGenerator>();
        if (sg)
        {
            shieldStrength = sg.shieldStrength;
            rechargeTime = sg.rechargeTime;
        }
    }
}