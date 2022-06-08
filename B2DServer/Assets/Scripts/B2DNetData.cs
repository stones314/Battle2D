using Unity.Networking.Transport;

public static class B2DNetData
{
    public static readonly ushort MSG_ID_SAVE_PLAYER_CMD = 1;
    public static readonly ushort MSG_ID_SAVE_PLAYER_REP = 2;
    public static readonly ushort MSG_ID_LOAD_PLAYER_CMD = 3;
    public static readonly ushort MSG_ID_LOAD_PLAYER_REP = 4;
}

[System.Serializable]
public class PlayerData
{
    public ushort roundsPlayed;    //how many rounds this player has played
    public ushort level;           //what level the player is at
    public ushort money;           //how much money the player has
    public short health;           //how much health the player has
    public ushort numShips;        //number of ships in fleet
    public ShipData[] ships;       //list of ships in fleet

    public PlayerData()
    {
    }

    public PlayerData(DataStreamReader stream)
    {
        roundsPlayed = stream.ReadUShort();
        level = stream.ReadUShort();
        money = stream.ReadUShort();
        health = stream.ReadShort();
        numShips = stream.ReadUShort();

        ships = new ShipData[numShips];
        for (int i = 0; i < numShips; i++)
        {
            ships[i] = new ShipData(stream);
        }
    }

    public void WriteTo(ref DataStreamWriter writer)
    {
        writer.WriteUShort(roundsPlayed);
        writer.WriteUShort(level);
        writer.WriteUShort(money);
        writer.WriteShort(health);
        writer.WriteUShort(numShips);
        for (int i = 0; i < numShips; i++)
        {
            ships[i].WriteTo(ref writer);
        }
    }
}

[System.Serializable]
public class ShipData
{
    public ushort prefabId;       //name of ship prefab
    public ushort slotId;            //slot where ship is placed
    public ushort hullLayers;          //number of hull layers
    public ushort layerStrength;       //strenght of each layer
    public ushort initiative;          //initiative of ship
    public ushort numTechTiles;          //number of equiped tech tiles
    public TechTileData[] techTiles;  //list of tech tiles attached

    public ShipData()
    {

    }

    public ShipData(DataStreamReader stream)
    {
        prefabId = stream.ReadUShort();
        slotId = stream.ReadUShort();
        hullLayers = stream.ReadUShort();
        layerStrength = stream.ReadUShort();
        initiative = stream.ReadUShort();
        numTechTiles = stream.ReadUShort();

        techTiles = new TechTileData[numTechTiles];
        for (int i = 0; i < numTechTiles; i++)
        {
            techTiles[i] = new TechTileData(stream);
        }
    }

    public void WriteTo(ref DataStreamWriter writer)
    {
        writer.WriteUShort(prefabId);
        writer.WriteUShort(slotId);
        writer.WriteUShort(hullLayers);
        writer.WriteUShort(layerStrength);
        writer.WriteUShort(initiative);
        writer.WriteUShort(numTechTiles);

        for (int i = 0; i < numTechTiles; i++)
        {
            techTiles[i].WriteTo(ref writer);
        }
    }
}

[System.Serializable]
public class TechTileData
{
    public ushort prefabId;       //name of tech prefab
    public ushort slotId;         //id of slot where tech is attached

    //fire unit params:
    public ushort burstSize;        //burst size of fire unit
    public float munitionDamage;    //damage of a single munition

    //shield generator params
    public float shieldStrength;    //strength of shield
    public float rechargeTime;      //recharge time of shield

    public TechTileData()
    {

    }

    public TechTileData(DataStreamReader stream)
    {
        prefabId = stream.ReadUShort();
        slotId = stream.ReadUShort();
        burstSize = stream.ReadUShort();
        munitionDamage = stream.ReadFloat();
        shieldStrength = stream.ReadFloat();
        rechargeTime = stream.ReadFloat();
    }

    public void WriteTo(ref DataStreamWriter writer)
    {
        writer.WriteUShort(prefabId);
        writer.WriteUShort(slotId);
        writer.WriteUShort(burstSize);
        writer.WriteFloat(munitionDamage);
        writer.WriteFloat(shieldStrength);
        writer.WriteFloat(rechargeTime);
    }
}