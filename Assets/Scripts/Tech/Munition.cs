using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Munition : MonoBehaviour
{
    public float damage;
    public Vector3 speedVector;
    
    private Player owningPlayer;
    private bool isBattle = false;
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
        if (!isBattle) return;
        if (other.gameObject.tag != "Ship") return;

        Ship target = other.gameObject.GetComponent<Ship>();
        Player targetOwner = target.GetComponentInParent<Player>();

        if (targetOwner == owningPlayer) return;

        target.HitByProjectile(damage);
        Destroy(this.gameObject);

        //Explosion:
        if (explosionPrefab)
        {
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.transform.position = this.transform.position;
        }
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
