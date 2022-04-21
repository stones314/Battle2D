using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HullElement : MonoBehaviour
{

    public float strength = 0f;
    public float maxStrength = 10f;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitHull(float maxStrength_)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxStrength = maxStrength_;
        strength = maxStrength;
        UpdateColor();
    }

    public void StrengthenHull(float percentage)
    {
        maxStrength += maxStrength * percentage;
        strength = maxStrength;
    }

    public void DamageHull(float damage)
    {
        strength -= damage;

        if(strength <= 0)
        {
            this.gameObject.SetActive(false);
        }

        UpdateColor();
    }

    public void RegenerateHull(float addStrength)
    {
        strength += addStrength;
        UpdateColor();
    }

    public void RestoreHull()
    {
        this.gameObject.SetActive(true);
        strength = maxStrength;
        UpdateColor();
    }

    private void UpdateColor()
    {
        float hue = strength/maxStrength*0.33f;
        spriteRenderer.color = Color.HSVToRGB(hue, 1, 1);

    }

}
