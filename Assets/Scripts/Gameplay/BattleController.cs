using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController
{
    private Player player;
    private Player opponent;
    private List<Ship> attackOrder;
    private int nextAttacker;

    public BattleController(ref Player _player, ref Player _opponent)
    {
        player = _player;
        opponent = _opponent;
        InitAttackOrder();
        nextAttacker = 0;
    }

    private void InitAttackOrder()
    {
        attackOrder = new List<Ship>(20);
        foreach (var ship in player.GetComponentsInChildren<Ship>())
        {
            InsertIntoAttackOrder(ship);
        }
        foreach (var ship in opponent.GetComponentsInChildren<Ship>())
        {
            InsertIntoAttackOrder(ship);
        }
    }

    private void InsertIntoAttackOrder(Ship newShip)
    {
        int pos = 0;
        int opts = 1;
        foreach (var sortedShip in attackOrder)
        {
            if (newShip.Speed < sortedShip.Speed)
            {
                break;
            }
            else if (newShip.Speed == sortedShip.Speed)
            {
                opts++;
            }
            else
            {
                pos++;
            }
        }

        pos = Random.Range(pos, pos + opts);
        attackOrder.Insert(pos, newShip);
    }

    private void IncreaseNextAttacker()
    {
        nextAttacker++;
        nextAttacker %= attackOrder.Count;
    }

    private void FindNextAttacker()
    {
        IncreaseNextAttacker();
        while (!attackOrder[nextAttacker].gameObject.activeInHierarchy)
        {
            IncreaseNextAttacker();
        }
    }

    public void AttackNext()
    {
        attackOrder[nextAttacker].Attack();
        FindNextAttacker();
    }
}