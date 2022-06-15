using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fleet : MonoBehaviour
{
    Slot[] shipSlots = new Slot[5];
    short attackId = 0;
    Player enemyPlayer;

    // Start is called before the first frame update
    void Start()
    {
        Slot[] slots = GetComponentsInChildren<Slot>();
        int slotId = 0;
        foreach (var slot in slots)
        {
            if (slot.tag == "Ship Slot" && slotId < 5)
                shipSlots[slotId++] = slot;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public bool Attack()
    {
        int n = 0;
        while (n < 5)
        {
            if(!shipSlots[attackId])
            {
                Debug.Log("Fleet.Attack(): No shipSlots[attackId] for attckId = " + attackId);
            }
            else if (!shipSlots[attackId].GetComponentInChildren<Ship>())
            {
                //No ship in this slot
            }
            else if (!shipSlots[attackId].GetComponentInChildren<Ship>().HasCombatAction())
            {
                //Ship in this slot has no combat action
            }
            else
            {
                shipSlots[attackId].GetComponentInChildren<Ship>().Attack(GetTarget());
                attackId++;
                attackId %= 5;
                return true;
            }
            attackId++;
            attackId %= 5;
            n++;
        }
        return false;
    }

    private Ship GetTarget()
    {
        Ship[] ships = enemyPlayer.GetComponentsInChildren<Ship>();

        List<Ship> possibleTargets = new List<Ship>(ships.Length);
        bool hasDecoy = false;
        foreach (var ship in ships)
        {
            if (ship.GetComponentInChildren<Decoy>())
            {
                hasDecoy = true;
                break;
            }
        }
        foreach (var ship in ships)
        {
            if (!hasDecoy)
                possibleTargets.Add(ship);
            else if (ship.GetComponentInChildren<Decoy>())
                possibleTargets.Add(ship);

        }

        if (possibleTargets.Count == 0) return null;

        int x = (int)Random.Range(0f, possibleTargets.Count - 0.000001f);

        return possibleTargets[x];
    }

    public void InitCombat(Player opponent)
    {
        enemyPlayer = opponent;
        attackId = 0;
    }
}
