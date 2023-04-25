using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{

    private float rechargeTime;

    [SerializeField]
    SpriteRenderer spriteRenderer;

    float currentRescale = 1;
    bool recharge = false;
    //private bool battle = false;
    private float rechargeStartTime;

    Animator reloadAnimator;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    public void RechargeIfNeeded()
    {
        if (recharge) Recharge();
    }

    public void HitByProjectile(float damage)
    {
        recharge = true;
        rechargeStartTime = Time.time;
        gameObject.SetActive(false);
        reloadAnimator.SetBool("active", false);
    }

    public void SetRechargeTime(float time)
    {
        rechargeTime = time;
    }

    public void InitilaizeShield(float rchgTime)
    {
        rechargeTime = rchgTime;

        if (!GetComponentInParent<Ship>())
            gameObject.SetActive(false);
    }

    public void InitilaizeReloadAnimator()
    {
        if (reloadAnimator) return;
        reloadAnimator = transform.parent.GetComponentInChildren<Reload>().GetComponent<Animator>();
        reloadAnimator.SetBool("reloading", false);
        reloadAnimator.SetBool("active", false);
    }

    private void Recharge()
    {
        if (Time.time - rechargeStartTime >= rechargeTime)
        {
            recharge = false;
            Debug.Log("BS: Shield Recharged!");
            reloadAnimator.SetBool("active", true);
            gameObject.SetActive(true);
        }
    }

    public void Rescale(float scale)
    {
        transform.localScale /= currentRescale;
        transform.localScale *= scale;
        currentRescale = scale;
    }

    public void BattleEnded()
    {
        //battle = false;
        gameObject.SetActive(true);

        reloadAnimator.SetBool("reloading", false);
        reloadAnimator.SetBool("active", false);
    }

    public void BattleStarted()
    {
        //battle = true;

        InitilaizeShield(rechargeTime); //in case this is the opponent it seems to not be initialized, so do it here
        InitilaizeReloadAnimator(); //in case this is the opponent it seems to not be initialized, so do it here

        Debug.Log("BS: Shield: recharge time = " + rechargeTime.ToString("F2"));

        reloadAnimator.speed = 2.083f / rechargeTime;  //Animation lasts 2 sec by default. We want it to last reloadTime instead
        reloadAnimator.SetBool("reloading", true);
        reloadAnimator.SetBool("active", true);
    }
}
