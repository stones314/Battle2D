using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    ShieldGenerator generator;
    
    [SerializeField]
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGenerator(ShieldGenerator generatedBy)
    {
        generator = generatedBy;
    }

    public void HitByProjectile(float damage)
    {
        generator.ShieldHitByProjectile(damage);
    }

    public void SetStrengthLeft(float strengthScale)
    {
        Color color = spriteRenderer.color;
        color.a = strengthScale / 2; //Max alpha is 0.5, which happens at strengthScale = 1
        if (color.a < 0.05f) color.a = 0.05f;
        spriteRenderer.color = color;
    }
}
