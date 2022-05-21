using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldGenerator : TechTile
{
    public float shieldStrength;
    public float rechargeTime;

    [SerializeField]
    Shield shield;
    private float strengthLeft;
    private bool battle = false;
    private float rechargeStartTime;

    Animator reloadAnimator;

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

    public float ScaleStrengthLeft()
    {
        return strengthLeft / shieldStrength;
    }

    public void InitilaizeShield()
    {
        strengthLeft = shieldStrength;
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
        if (Time.time - rechargeStartTime >= rechargeTime)
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
        shield.transform.position = ship.transform.position;
        shield.gameObject.SetActive(true);
        //shield.transform.localScale *= 3;
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
        
        reloadAnimator.speed = 2.083f / rechargeTime;  //Animation lasts 2 sec by default. We want it to last reloadTime instead
        reloadAnimator.SetBool("reloading", true);
        reloadAnimator.SetBool("active", true);
    }

    public override void GenerateTile()
    {
        
    }

    public override void RemovedFromShip(Ship oldParent)
    {
        //shield.transform.localScale /= 3;
        shield.gameObject.SetActive(false);
    }

    public override string GetHoverOverStats()
    {
        return
            "Shield Strength: " + shieldStrength.ToString("F2") +
            "Recharge Time: " + rechargeTime.ToString("F0") + " sec\n" +
            "Sell Value:  " + cost / 3;
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

    public void AddSpeedBonus(int speedBonus)
    {
        rechargeTime *= (100f - (float)speedBonus) / 100f;
    }

    public override void PrepareAttack()
    {

    }

    public override void Attack()
    {

    }

}
