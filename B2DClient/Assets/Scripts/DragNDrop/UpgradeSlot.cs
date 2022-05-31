using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSlot : Slot
{

    // Start is called before the first frame update
    void Start()
    {
        base.BaseStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected override Transform GetNewParent()
    {
        return this.transform.parent;
    }

}
