using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Munition : MonoBehaviour
{
    public float damage;
    public Vector3 speedVector;
    
    private Player owningPlayer;
    private bool isBattle = false;
    private bool hitShield = false;
    private Quaternion initialRotation;

    [SerializeField]
    GameObject explosionPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += speedVector * Time.deltaTime;
        if (!isBattle) transform.rotation = initialRotation;//lock rotation outside of battle
    }


    private void OnTriggerEnter(Collider other)
    {
        if (IgnoreThisCollision(other)) return;
        if (hitShield) return;


        if (other.gameObject.tag == "Shield")
            HitShield(other);
        else if (other.gameObject.tag == "Ship")
            HitShip(other);
    }

    private bool IgnoreThisCollision(Collider other)
    {
        //Ignore all collisions outside battle
        if (!isBattle) return true;

        Player targetOwner = other.gameObject.GetComponentInParent<Player>();

        //Ignore collision with targets that has no owner
        if (!targetOwner) return true;

        //Ignore friendly fire
        if (targetOwner == owningPlayer) return true;

        return false;
    }

    private void Explode()
    {
        Destroy(this.gameObject);

        if (explosionPrefab)
        {
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.transform.position = this.transform.position;
        }
    }

    private void HitShield(Collider other)
    {
        hitShield = true;

        Shield target = other.gameObject.GetComponent<Shield>();

        target.HitByProjectile(damage);

        Explode();
    }

    private void HitShip(Collider other)
    {
        Ship target = other.gameObject.GetComponent<Ship>();

        target.HitByProjectile(damage);

        Explode();
    }

    public void SetOwningPlayer(Player player)
    {
        owningPlayer = player;
    }

    public void SetBattle(bool battle)
    {
        isBattle = battle;
    }

    public void SetInitialRotation(Quaternion rotation)
    {
        initialRotation = rotation;
    }
}
