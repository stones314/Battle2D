using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void TechPlaced(TechTile tech, Slot slot);
    public static event TechPlaced OnTechPlaced;

    public delegate void PlayerLoaded(PlayerData player);
    public static event PlayerLoaded OnPlayerLoaded;

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
}
