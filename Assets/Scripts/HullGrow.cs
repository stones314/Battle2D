using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HullGrow : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        EventManager.OnTechPlaced += TriggerHullGrow;
    }

    // Update is called once per frame
    private void OnDisable()
    {
        EventManager.OnTechPlaced -= TriggerHullGrow;
    }

    public void TriggerHullGrow()
    {
        Ship ship = GetComponentInParent<Ship>();
        if (ship)
        {
            ship.AddBonusLayers(1);
        }
    }

}
