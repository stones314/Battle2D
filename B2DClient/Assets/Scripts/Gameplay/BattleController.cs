using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController
{
    private Player player;
    private Player opponent;
    private List<Ship> attackOrder;
    private int nextAttacker;
    private int numActive;
    private bool shipBasedAttacks = false;


    enum Attacker { PLAYER = 0, OPPONENT = 1 }
    private Attacker attacker;

    public BattleController()
    {
        attackOrder = new List<Ship>(20);
        EventManager.OnShipSpawned += OnShipSpawned;
    }

    public void StartBattle(ref Player _player, ref Player _opponent)
    {
        player = _player;
        opponent = _opponent;

        if (shipBasedAttacks)
            InitShipBasedAttacks();
        else
            InitSlotBasedAttacks();

    }
    void InitShipBasedAttacks()
    {
        attackOrder.Clear();
        nextAttacker = 0;
        foreach (var ship in player.GetComponentsInChildren<Ship>())
        {
            if (ship.HasCombatAction())
                InsertIntoAttackOrder(ship);
        }
        foreach (var ship in opponent.GetComponentsInChildren<Ship>())
        {
            if (ship.HasCombatAction())
                InsertIntoAttackOrder(ship);
        }
        numActive = attackOrder.Count;
        if (numActive > 0)
            attackOrder[nextAttacker].PrepareAttack();
    }

    void InitSlotBasedAttacks()
    {
        if (Random.Range(0, 1) == 0)
            attacker = Attacker.OPPONENT;
        else
            attacker = Attacker.PLAYER;
    }

    private void InsertIntoAttackOrder(Ship newShip)
    {
        int pos = 0;
        int opts = 1;
        foreach (var sortedShip in attackOrder)
        {
            if (newShip.Initiative > sortedShip.Initiative)
            {
                break;
            }
            else if (newShip.Initiative == sortedShip.Initiative)
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

    private void MarkSecondAttacker()
    {
        int secondAttacker = nextAttacker + 1;
        secondAttacker %= attackOrder.Count;
        while (!attackOrder[secondAttacker] || (!attackOrder[secondAttacker].gameObject.activeInHierarchy && secondAttacker != nextAttacker))
        {
            secondAttacker++;
            secondAttacker %= attackOrder.Count;
        }
        if(secondAttacker != nextAttacker)
        {
            attackOrder[secondAttacker].GetComponentInParent<Slot>().GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }

    private void FindFirstAttacker()
    {
        IncreaseNextAttacker();
        while (!attackOrder[nextAttacker] || !attackOrder[nextAttacker].gameObject.activeInHierarchy)
        {
            IncreaseNextAttacker();
        }
        attackOrder[nextAttacker].PrepareAttack();
    }

    public void AttackNext()
    {
        if (shipBasedAttacks)
        {
            if (!attackOrder[nextAttacker]) return;
            attackOrder[nextAttacker].Attack(null);
            FindFirstAttacker();
            MarkSecondAttacker();
        }
        else
        {
            if (attacker == Attacker.PLAYER)
            {
                if(player.GetComponentInChildren<Fleet>().Attack())
                    attacker = Attacker.OPPONENT;
            }
            else
            {
                if(opponent.GetComponentInChildren<Fleet>().Attack())
                    attacker = Attacker.PLAYER;
            }
        }
    }

    private void OnShipSpawned(Ship ship)
    {
        if (shipBasedAttacks)
        {
            if (ship.HasCombatAction())
                InsertIntoAttackOrder(ship);
        }
    }
}