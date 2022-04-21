using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int hullLayers = 1;
    public float layerStrength = 10;

    public float shieldStrength = 0;

    public int startAccuracy;

    List<HullElement> hullElements = new List<HullElement>();

    Accuracy accuracyIndicator;

    // Start is called before the first frame update
    void Start()
    {
        if (!accuracyIndicator)
        {
            accuracyIndicator = GetComponentInChildren<Accuracy>();
            accuracyIndicator.SetAccuracy(startAccuracy);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BattleStarted(Player opponent)
    {
        TechTile[] techs = GetComponentsInChildren<TechTile>();
        foreach (var tech in techs)
        {
            tech.BattleStarted(opponent);
        }
    }

    public void BattleEnded()
    {
        this.gameObject.SetActive(true);
        TechTile[] techs = GetComponentsInChildren<TechTile>(true);
        foreach (var tech in techs)
        {
            tech.BattleEnded();
        }
        RestoreHull();
    }

    public void HitByProjectile(float damage)
    {
        float remainingDamage = damage;
        for(int i = hullElements.Count -1; i >= 0; i--)
        {
            if (hullElements[i].strength <= 0) continue;

            float damageToElement = Mathf.Min(hullElements[i].strength, remainingDamage);
            remainingDamage -= damageToElement;
            hullElements[i].DamageHull(damageToElement);

            if (i == 0 && hullElements[i].strength <= 0)
                this.gameObject.SetActive(false);

            if(remainingDamage <= 0) break;
        }
    }

    public void GenerateHullMeter()
    {
        for (int l = 0; l < hullLayers; l++)
        {
            AddHullLayer(l);
        }
    }

    public void AddBonusLayers(int numLayers)
    {
        for (int l = hullLayers; l < hullLayers + numLayers; l++)
        {
            AddHullLayer(l);
        }
        hullLayers += numLayers;
    }

    public void RemoveBonusLayers(int numLayers)
    {
        hullLayers -= numLayers;

        for(int i = hullElements.Count - 1; i >= hullLayers; i--)
        {
            Destroy(hullElements[i].gameObject);
            hullElements.RemoveAt(i);
        }
    }

    private void AddHullLayer(int layerNumber)
    {
        int col = layerNumber % 10;
        float r = layerNumber / 10f;
        int row = (int)r;
        HullElement hullElement = Instantiate<HullElement>(Resources.Load<HullElement>("Prefabs/Ships/HullElement"));
        hullElement.transform.SetParent(this.transform);
        hullElement.transform.localPosition = new Vector3(-hullElement.transform.localScale.x * 4.5f * 2.56f, -1.0f, 0);
        hullElement.transform.position += Vector3.right * hullElement.transform.lossyScale.x * col * 2.56f;
        hullElement.transform.position += Vector3.down * 3.1f + Vector3.down * hullElement.transform.lossyScale.y * row * 2.56f;
        hullElement.InitHull(layerStrength);
        hullElements.Add(hullElement);

    }

    public void RestoreHull()
    {
        foreach(var hullElement in hullElements)
        {
            hullElement.RestoreHull();
        }
    }

    public void AddAccuracy(int accuracy)
    {
        accuracyIndicator.AddAccuracy(accuracy);
    }

    public void SetAccuracy(int accuracy)
    {
        if (!accuracyIndicator)
        {
            accuracyIndicator = GetComponentInChildren<Accuracy>();
        }
        accuracyIndicator.SetAccuracy(accuracy);
    }

    public void RemoveAccuracy(int accuracy)
    {
        accuracyIndicator.AddAccuracy(-accuracy);
    }

    public int GetAccuracy()
    {
        return accuracyIndicator.GetAccuracy();
    }
}
