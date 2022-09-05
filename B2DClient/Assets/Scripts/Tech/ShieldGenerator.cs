using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldGenerator : TechTile
{
    public float rechargeTime;
    private GameSpeed.TimeInterval rechargeTimeGS;

    Shield[] shields;
    bool battle = false;

    private void Awake()
    {
        shields = GetComponentsInChildren<Shield>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitSG();
    }

    // Update is called once per frame
    void Update()
    {
        if (!battle) return;

        foreach (var shield in shields)
        {
            shield.RechargeIfNeeded();
        }
    }

    public override void Clicked(Vector2 offset)
    {
        base.Clicked(offset);

        foreach (var shield in shields)
        {
            shield.transform.position = this.transform.position;
        }
    }

    public override int Dropped(int playerMoney)
    {
        int cost = base.Dropped(playerMoney);

        Ship ship = GetComponentInParent<Ship>();
        if (ship)
        {
            foreach (var shield in shields)
            {
                shield.transform.position = ship.transform.position;
            }
        }

        return cost;
    }

    public void InitSG()
    {
        rechargeTimeGS = new GameSpeed.TimeInterval(rechargeTime);
        foreach (var shield in shields)
        {
            shield.InitilaizeShield(rechargeTimeGS.interval);
            shield.InitilaizeReloadAnimator();
        }
    }


    public override void BattleEnded()
    {
        battle = false;

        foreach (var shield in shields)
        {
            shield.BattleEnded();
        }
    }

    public override void BattleStarted(Player opponent)
    {
        battle = true;

        foreach (var shield in shields)
        {
            shield.BattleStarted();
        }
    }

    public override void GenerateTile()
    {
        
    }

    public override void ApplyBonusesToTarget(Slot slot)
    {
        Ship ship = GetComponentInParent<Ship>();
        foreach (var shield in shields)
        {
            ship.shieldCount++;
            shield.gameObject.SetActive(true);
            shield.transform.position = ship.transform.position;
        }
        RescaleShipShields(ship);
    }

    public override void RemovedFromShip(Ship oldParent)
    {
        foreach (var shield in shields)
        {
            shield.gameObject.SetActive(false);
            oldParent.shieldCount--;
        }
        RescaleShipShields(oldParent);
    }

    private void RescaleShipShields(Ship ship)
    {
        Shield[] shipShields = ship.GetComponentsInChildren<Shield>();
        float scale = Constants.ShieldScale;
        foreach (var shipShield in shipShields)
        {
            shipShield.Rescale(scale);
            scale += Constants.ShieldDeltaScale;
        }
    }

    public override string GetHoverOverStats()
    {
        return
            "Shield Count: " + shields.Length +
            "\nRecharge Time: " + rechargeTimeGS.interval.ToString("F1") + " sec" +
            "\nSell Value:  " + cost / 3;
    }

    public void AddRechargeBonus(int speedBonus)
    {
        rechargeTime *= (100f - (float)speedBonus) / 100f;
        rechargeTimeGS = new GameSpeed.TimeInterval(rechargeTime);
        foreach (var shield in shields)
        {
            shield.SetRechargeTime(rechargeTimeGS.interval);
        }
    }

    public override void PrepareCombatAction()
    {

    }

    public override void ExecuteCombatAction(Ship target)
    {

    }

}
