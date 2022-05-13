using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireUnit : MonoBehaviour
{

    [Tooltip("")]
    [SerializeField]
    Transform muzzel;

    [Tooltip("Degees turned per second")]
    public float rotationSpeed = 40f;

    [Tooltip("Time between each fire")]
    public float reloadTime = 5f;

    [Tooltip("The munition fired")]
    [SerializeField]
    GameObject munitionPrefab;

    [Tooltip("Each Fire will be a burst of this many munitions fired")]
    public int burstSize = 1;

    [Tooltip("Time between each munition in a burst")]
    public float burstDeltaTime = 0.2f;

    [Tooltip("How fast the mnition flies")]
    public float munitionSpeed = 5f;

    [Tooltip("Probablity of hitting the target")]
    public int m_accuracy = 30;

    [Tooltip("If fireUnit can lock on to a target")]
    public bool lockTarget = false;

    [Tooltip("Prefab for accuracy indicator")]
    [SerializeField]
    GameObject accuracyIndicatorPrefab;

    Animator reloadAnimator;
    //Sprite reloadStartSprite;
    //SpriteRenderer reloadRenderer;

    private Player enemyPlayer;

    private Quaternion wantedRotation;
    Ship targetShip;
    bool hasTarget;
    bool battle = false;
    float lastFireTime;
    float lastBurstTime;
    int burstCounter;
    Vector3 targetDirection;

    private Text munitionIndicatorText;

    // Start is called before the first frame update
    void Start()
    {
        AddMunitionIndicator();
        AddAccuracyIndicator();
        InitilaizeReloadAnimator();
    }

    // Update is called once per frame
    void Update()
    {
        if (!battle) return;
        if (!enemyPlayer) return;

        GetTarget();
        RotateTowardsTarget();
        FireWhenReady();
    }

    public void InitilaizeReloadAnimator()
    {
        if (reloadAnimator) return;
        reloadAnimator = GetComponentInChildren<Reload>().GetComponent<Animator>();
        reloadAnimator.SetBool("reloading", false);
        reloadAnimator.SetBool("active", false);
    }

    public void BattleStarted(Player opponent)
    {
        battle = true;
        hasTarget = false;
        enemyPlayer = opponent;
        lastFireTime = Time.time;
        burstCounter = 0;
        lastBurstTime = Time.time;
        
        InitilaizeReloadAnimator();//in case this is the opponent it seems to not be initialized, so do it here
        reloadAnimator.speed = 2.083f / reloadTime;  //Animation lasts 2 sec by default. We want it to last reloadTime instead
        reloadAnimator.SetBool("reloading", true);
    }

    public void BattleEnded()
    {
        this.gameObject.SetActive(true);
        battle = false;
        enemyPlayer = null;
        reloadAnimator.SetBool("reloading", false);
    }

    private void GetTarget()
    {
        if (hasTarget && targetShip && targetShip.gameObject.activeSelf) return;

        Ship[] possibleTargets = enemyPlayer.GetComponentsInChildren<Ship>();

        if (possibleTargets.Length == 0) return;

        int x = (int)Random.Range(0f, possibleTargets.Length - 0.000001f);

        SetShipTarget(possibleTargets[x]);
    }

    public void SetShipTarget(Ship target)
    {
        targetShip = target;
        SetTargetPos(target.transform.position);
    }

    public void SetTargetPos(Vector3 target)
    {
        targetDirection = (target - transform.position).normalized;

        float angleToTarget = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        wantedRotation = Quaternion.AngleAxis(angleToTarget, Vector3.forward);
        hasTarget = true;
    }

    private void RotateTowardsTarget()
    {
        if (!hasTarget) return;
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, wantedRotation, Time.deltaTime * rotationSpeed);
    }

    public void FireWhenReady()
    {
        if (!hasTarget) return;
        float angleDiff = Quaternion.Angle(transform.rotation, wantedRotation);
        if (angleDiff > 0.01f) return;
        if (Time.time - lastFireTime < reloadTime) return;

        //ready to fire:
        reloadAnimator.SetBool("active", true);

        if (burstCounter < burstSize)
            FireNextInBurst();
        else
            EndBurstFire();
    }

    private void EndBurstFire()
    {
        lastFireTime = Time.time;
        hasTarget = lockTarget;
        burstCounter = 0;
        reloadAnimator.SetBool("active", false);
    }

    private void FireNextInBurst()
    {
        if (burstCounter > 0 && Time.time - lastBurstTime < burstDeltaTime) return;

        lastBurstTime = Time.time;
        burstCounter++;

        GameObject projectile = Instantiate(munitionPrefab);
        Quaternion rot = this.transform.rotation;
        rot.eulerAngles -= new Vector3(0,0,90);
        projectile.transform.rotation = rot;
        projectile.transform.position = muzzel.transform.position;
        projectile.transform.localScale *= 0.4f;
        Munition m = projectile.GetComponent<Munition>();
        m.speedVector = new Vector3(targetDirection.x, targetDirection.y, 0f) * munitionSpeed;
        m.SetOwningPlayer(this.GetComponentInParent<Player>());
        m.SetBattle(true);

        if (Random.value > (float)m_accuracy/100f) projectile.transform.position += 50 * Vector3.back;
    }

    public void AddMunitionIndicator()
    {
        //Create Munition Game Object
        GameObject munitionObj = Instantiate(munitionPrefab);
        munitionObj.transform.parent = this.transform;
        munitionObj.transform.rotation = this.transform.rotation;
        munitionObj.transform.localPosition = new Vector3(-0.8f, -0.8f, 0);
        munitionObj.transform.localScale *= 1f;
        munitionObj.GetComponent<Rigidbody>().freezeRotation = true;
        Munition munition = munitionObj.GetComponent<Munition>();
        munition.SetBattle(false);
        munition.SetInitialRotation(munitionObj.transform.rotation);

        // Load the Arial font from the Unity Resources folder.
        Font arial;
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        // Create Canvas GameObject.
        GameObject canvasGO = new GameObject();
        canvasGO.transform.parent = munitionObj.transform;
        canvasGO.name = "Canvas";
        canvasGO.AddComponent<Canvas>();
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform CrectTransform;
        CrectTransform = canvasGO.GetComponent<RectTransform>();
        CrectTransform.localPosition = new Vector3(0, 0, 0);
        CrectTransform.sizeDelta = new Vector2(100, 100);
        CrectTransform.localScale = new Vector3(0.01f, 0.01f, 1f);

        // Get canvas from the GameObject.
        Canvas canvas;
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 11;


        // Create the Text GameObject.
        GameObject textGO = new GameObject();
        textGO.transform.parent = canvasGO.transform;
        textGO.name = "Damage";
        textGO.AddComponent<Text>();

        // Set Text component properties.
        munitionIndicatorText = textGO.GetComponent<Text>();
        munitionIndicatorText.font = arial;
        munitionIndicatorText.text = "" + munition.damage;
        munitionIndicatorText.fontSize = 80;
        munitionIndicatorText.alignment = TextAnchor.LowerCenter;
        munitionIndicatorText.color = Color.black;

        // Provide Text position and size using RectTransform.
        RectTransform rectTransform;
        rectTransform = munitionIndicatorText.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, 0, 0);
        rectTransform.sizeDelta = new Vector2(100, 100);
        rectTransform.localScale = new Vector3(1, 1, 1);
    }

    public void AddAccuracyIndicator()
    {
        //Create Accuracy Indicator Game Object
        GameObject accuracyIndicatorObj = Instantiate(accuracyIndicatorPrefab);
        accuracyIndicatorObj.transform.parent = this.transform;
        accuracyIndicatorObj.transform.rotation = this.transform.rotation;
        accuracyIndicatorObj.transform.localPosition = new Vector3(-1.5f, -0.8f, 0);
        accuracyIndicatorObj.transform.localScale *= 0.4f;
        Accuracy acc = accuracyIndicatorObj.GetComponent<Accuracy>();
        acc.SetAccuracy(m_accuracy);
    }

    public void AddAccuracy(int accuracy)
    {
        m_accuracy += accuracy;
        GetComponentInChildren<Accuracy>().SetAccuracy(m_accuracy);
    }

    public void AddSpeedBonus(int speedBonus)
    {
        reloadTime *= (100f - (float)speedBonus) / 100f;
    }

    public float GetDamagePerSec()
    {
        float ds = munitionPrefab.GetComponent<Munition>().damage;
        float acc = (float)m_accuracy/100f;
        ds *= acc;
        ds *= burstSize;
        ds /= (reloadTime + burstDeltaTime*(burstSize-1));
        return ds;
    }

    public string StatsToString()
    {
        Munition m = munitionPrefab.GetComponent<Munition>();

        return
            "Reload Time: " + reloadTime + " sec\n" +
            "Accuracy:    " + m_accuracy + " %\n" +
            "Burst Size:  " + burstSize + "\n" +
            "Munition:    " + m.name + " (" + m.damage +" damage)\n" +
            "Sell Value:  " + GetComponentInParent<Draggable>().cost/3;
    }
}
