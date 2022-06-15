using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldGenerator : TechTile
{
    public float shieldStrength;
    public float rechargeTime;
    private GameSpeed.TimeInterval rechargeTimeGS;

    [SerializeField]
    Shield shield;
    private float strengthLeft;
    private bool battle = false;
    private float rechargeStartTime;

    Animator reloadAnimator;
    private float rescaleFactor = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        InitilaizeShield();
        InitilaizeReloadAnimator();
    }

    // Update is called once per frame
    void Update()
    {
        if (!battle) return;

        if (strengthLeft <= 0)
            Recharge();
    }

    public override void Clicked(Vector2 offset)
    {
        base.Clicked(offset);
        shield.transform.localScale /= 3;
        shield.transform.position = this.transform.position;
    }

    public override int Dropped(int playerMoney)
    {
        shield.transform.localScale *= 3;
        return base.Dropped(playerMoney);
    }

    public float ScaleStrengthLeft()
    {
        return strengthLeft / shieldStrength;
    }

    public void InitilaizeShield()
    {
        strengthLeft = shieldStrength;
        rechargeTimeGS = new GameSpeed.TimeInterval(rechargeTime);
        shield.SetGenerator(this);
        shield.SetStrengthLeft(ScaleStrengthLeft());
        
        if(!GetComponentInParent<Ship>())
            shield.gameObject.SetActive(false);
    }

    public void InitilaizeReloadAnimator()
    {
        if (reloadAnimator) return;
        reloadAnimator = GetComponentInChildren<Reload>().GetComponent<Animator>();
        reloadAnimator.SetBool("reloading", false);
        reloadAnimator.SetBool("active", false);
    }

    private void Recharge()
    {
        if (Time.time - rechargeStartTime >= rechargeTimeGS.interval)
        {
            strengthLeft = shieldStrength;
            reloadAnimator.SetBool("active", true);
            shield.gameObject.SetActive(true);
            shield.SetStrengthLeft(ScaleStrengthLeft());
        }
    }

    public override void ApplyBonusesToTarget(Slot slot)
    {
        Ship ship = GetComponentInParent<Ship>();
        ship.shieldCount++;
        shield.transform.position = ship.transform.position;
        rescaleFactor = 1.0f + 0.1f*(ship.shieldCount - 1);
        shield.transform.localScale *= rescaleFactor;
        shield.gameObject.SetActive(true);

    }

    public override void BattleEnded()
    {
        battle = false;
        strengthLeft = shieldStrength;
        shield.gameObject.SetActive(true);
        shield.SetStrengthLeft(ScaleStrengthLeft());

        reloadAnimator.SetBool("reloading", false);
        reloadAnimator.SetBool("active", false);
    }

    public override void BattleStarted(Player opponent)
    {
        battle = true;

        InitilaizeShield();         //in case this is the opponent it seems to not be initialized, so do it here
        InitilaizeReloadAnimator(); //in case this is the opponent it seems to not be initialized, so do it here
        
        reloadAnimator.speed = 2.083f / rechargeTimeGS.interval;  //Animation lasts 2 sec by default. We want it to last reloadTime instead
        reloadAnimator.SetBool("reloading", true);
        reloadAnimator.SetBool("active", true);
    }

    public override void GenerateTile()
    {
        
    }

    public override void RemovedFromShip(Ship oldParent)
    {
        shield.gameObject.SetActive(false);
        shield.transform.localScale /= rescaleFactor;
        oldParent.shieldCount--;
    }

    public override string GetHoverOverStats()
    {
        return
            "Shield Strength: " + shieldStrength.ToString("F2") +
            "\nRecharge Time: " + rechargeTimeGS.interval.ToString("F0") + " sec" +
            "\nSell Value:  " + cost / 3;
    }

    public void ShieldHitByProjectile(float damage)
    {
        strengthLeft -= damage;
        shield.SetStrengthLeft(ScaleStrengthLeft());
        if (strengthLeft <= 0)
        {
            rechargeStartTime = Time.time;
            shield.gameObject.SetActive(false);
            reloadAnimator.SetBool("active", false);
        }

    }

    public void AddRechargeBonus(int speedBonus)
    {
        rechargeTime *= (100f - (float)speedBonus) / 100f;
        rechargeTimeGS = new GameSpeed.TimeInterval(rechargeTime);
    }

    public override void PrepareCombatAction()
    {

    }

    public override void ExecuteCombatAction(Ship target)
    {

    }

}
