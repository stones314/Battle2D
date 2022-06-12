using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void TechPlaced(TechTile tech, Slot slot);
    public static event TechPlaced OnTechPlaced;

    public delegate void PlayerLoaded(PlayerData player);
    public static event PlayerLoaded OnPlayerLoaded;

    public delegate void PlayerSaved();
    public static event PlayerSaved OnPlayerSaved;

    public delegate void ShipDestroyed(Ship ship);
    public static event ShipDestroyed OnShipDestroyed;

    public delegate void ShipSpawned(Ship ship);
    public static event ShipSpawned OnShipSpawned;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void NotifyTechPlaced(TechTile tech, Slot slot)
    {
        if(OnTechPlaced != null)
            OnTechPlaced(tech, slot);
    }

    public static void NotifyPlayerLoaded(PlayerData player)
    {
        if (OnPlayerLoaded != null)
            OnPlayerLoaded(player);
    }

    public static void NotifyPlayerSaved()
    {
        if (OnPlayerSaved != null)
            OnPlayerSaved();
    }

    public static void NotifyShipDestroyed(Ship ship)
    {
        if (OnShipDestroyed != null)
            OnShipDestroyed(ship);
    }

    public static void NotifyShipSpawned(Ship ship)
    {
        if (OnShipSpawned != null)
            OnShipSpawned(ship);
    }
}
