using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Munition : MonoBehaviour
{
    public float damage;
    public Player owningPlayer;

    public Vector3 speedVector;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += speedVector * Time.deltaTime;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Ship") return;

        Ship target = other.gameObject.GetComponent<Ship>();
        Player targetOwner = target.GetComponentInParent<Player>();

        if (targetOwner == owningPlayer) return;

        target.HitByProjectile(damage);
        Destroy(this.gameObject);
    }

}
