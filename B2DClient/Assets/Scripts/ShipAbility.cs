using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShipAbility : MonoBehaviour
{
    bool removeAfterCombat = true;

    public void BattleEnded()
    {
        if (removeAfterCombat)
        {
            Destroy(this);
        }
    }

    public virtual void BattleStarted()
    {
        removeAfterCombat = false;
    }

    public void MakePermanent()
    {
        removeAfterCombat = false;
    }
}
