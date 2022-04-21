using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireUnit : MonoBehaviour
{

    [SerializeField]
    Transform muzzel;
    public float rotationSpeed = 40f;   //Degees turned per second
    public float reloadTime = 5f;       //Time between each fire
    public string munitionPrefab;       //The munition fired
    public int burstSize = 1;           //Each Fire will be a burst of this many munitions fired
    public float burstDeltaTime = 0.2f; //Time between each munition in a burst
    public float munitionSpeed = 5f;    //How fast the mnition flies
    public float accuracy = 0.3f;       //Probablity of hitting the target
    public bool lockTarget = false;     //If fireUnit can lock on to a target

    public Player enemyPlayer;

    private Quaternion wantedRotation;
    Ship targetShip;
    bool hasTarget;
    bool battle = false;
    float lastFireTime;
    float lastBurstTime;
    int burstCounter;
    Vector3 targetDirection;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponentInParent<Player>().tag == "Opponent")
            battle = battle;//For ability to break - sometimes enemy ship does not fire back

        if (!battle) return;
        if (!enemyPlayer) return;

        GetTarget();
        RotateTowardsTarget();
        FireWhenReady();
    }

    public void BattleStarted(Player opponent)
    {
        battle = true;
        hasTarget = false;
        enemyPlayer = opponent;
        lastFireTime = Time.time;
        burstCounter = 0;
        lastBurstTime = Time.time;
        accuracy = ((float)GetComponentInParent<Ship>().GetAccuracy())/100f;
    }

    public void BattleEnded()
    {
        this.gameObject.SetActive(true);
        battle = false;
        enemyPlayer = null;
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
    }

    private void FireNextInBurst()
    {
        if (burstCounter > 0 && Time.time - lastBurstTime < burstDeltaTime) return;

        lastBurstTime = Time.time;
        burstCounter++;

        GameObject projectile = Instantiate(Resources.Load<GameObject>(munitionPrefab));
        Munition m = projectile.GetComponent<Munition>();
        m.speedVector = new Vector3(targetDirection.x, targetDirection.y, 0f) * munitionSpeed;
        m.owningPlayer = this.GetComponentInParent<Player>();
        projectile.transform.rotation = this.transform.rotation;
        projectile.transform.position = muzzel.transform.position;
        projectile.transform.localScale *= 0.4f;

        if (Random.value > accuracy) projectile.transform.position += 50 * Vector3.back;
    }
}
