using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : Draggable
{
    public int hullLayers = 1;
    public float layerStrength = 1;

    public int shieldCount = 0;
    
    public int Initiative = 1;

    [SerializeField]
    GameObject explosionPrefab;
    [SerializeField]
    Transform hullArea;
    [SerializeField]
    Transform initiativeArea;

    List<HullElement> hullElements = new List<HullElement>();
    List<GameObject> initiativeElements = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Here?");
    }

    public void BattleStarted(Player opponent)
    {
        TechTile[] techs = GetComponentsInChildren<TechTile>();
        foreach (var tech in techs)
        {
            tech.BattleStarted(opponent);
        }

        GetComponent<CapsuleCollider>().radius /= 3f;
    }

    public void BattleEnded()
    {
        this.gameObject.SetActive(true);
        GetComponentInParent<Slot>().GetComponent<SpriteRenderer>().color = Color.white;
        TechTile[] techs = GetComponentsInChildren<TechTile>(true);
        foreach (var tech in techs)
        {
            tech.BattleEnded();
        }
        RestoreHull();

        GetComponent<CapsuleCollider>().radius *= 3f;
    }

    public bool HasCombatAction()
    {
        foreach (var tech in GetComponentsInChildren<TechTile>())
        {
            if(tech.HasCombatAction()) return true;
        }
        return false;
    }

    public void PrepareAttack()
    {
        TechTile[] techs = GetComponentsInChildren<TechTile>();
        foreach (var tech in techs)
        {
            tech.PrepareCombatAction();
        }
        GetComponentInParent<Slot>().GetComponent<SpriteRenderer>().color = Color.green;
    }

    public void Attack()
    {
        TechTile[] techs = GetComponentsInChildren<TechTile>();
        foreach (var tech in techs)
        {
            tech.ExecuteCombatAction();
        }
        GetComponentInParent<Slot>().GetComponent<SpriteRenderer>().color = Color.white;
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
                DestroyShip();

            if(remainingDamage <= 0) break;
        }
    }

    public void DestroyShip()
    {
        this.gameObject.SetActive(false);
        GetComponentInParent<Slot>().GetComponent<SpriteRenderer>().color = Color.white;

        if (!explosionPrefab) return;

        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = this.transform.position;
        explosion.transform.localScale *= 4;
    }

    public void GenerateHullMeter()
    {
        for (int l = 0; l < hullLayers; l++)
        {
            AddHullLayer(l);
        }
    }

    public void Initialize()
    {
        GenerateHullMeter();
        GenerateInitiativeMeter();
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
        int row = layerNumber % 10;
        float c = layerNumber / 10f;
        int col = (int)c;
        HullElement hullElement = Instantiate<HullElement>(Resources.Load<HullElement>("Prefabs/Ships/HullElement"));
        hullElement.transform.SetParent(hullArea);
        hullElement.transform.localPosition = new Vector3(0.25f - 0.25f*col, -0.45f + 0.1f*row, 0)*2.56f;
        hullElement.transform.localScale = new Vector3(0.25f, 0.1f, 0);
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


    public void GenerateInitiativeMeter()
    {
        for (int i = 0; i < Initiative; i++)
        {
            AddInitiativeIndicator(i);
        }

    }

    public void AddBonusInitiative(int number)
    {
        for (int i = Initiative; i < Initiative + number; i++)
        {
            AddInitiativeIndicator(i);
        }
        Initiative += number;
    }

    public void RemoveBonusInitiative(int number)
    {
        Initiative -= number;

        for (int i = initiativeElements.Count - 1; i >= Initiative; i--)
        {
            Destroy(initiativeElements[i]);
            initiativeElements.RemoveAt(i);
        }
    }

    private void AddInitiativeIndicator(int index)
    {
        int row = index % 10;
        float c = index / 10f;
        int col = (int)c;
        GameObject initiativeElement = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Ships/InitiativeElement"));
        initiativeElement.transform.SetParent(initiativeArea);
        initiativeElement.transform.rotation = initiativeArea.rotation;
        initiativeElement.transform.localPosition = new Vector3(0.25f - 0.25f * col, -0.45f + 0.1f * row, 0) * 1.28f;
        initiativeElement.transform.localScale = new Vector3(0.25f, 0.1f, 0);
        initiativeElements.Add(initiativeElement);
    }


    public float GetDamagePerAttack()
    {
        float ds = 0.0f;
        foreach(var weapon in GetComponentsInChildren<Weapon>())
        {
            ds += weapon.GetDamagePerAttack();
        }
        return ds;
    }

    public override string GetHoverOverStats()
    {
        int c = 0;
        foreach(var d in GetComponentsInChildren<Draggable>())
        {
            c += d.cost;
        }

        string str =
            "Hull:    " + hullLayers + "\n" +
            "Initiative: " + Initiative;
        if (shieldCount > 0) str += "\nShields: " + shieldCount;
        
        float dmg = GetDamagePerAttack();
        if (dmg > 0) str += "\nDamage/attack: " + dmg;

        str += "\nSell Value:  " + c / 3;
        
        return str;
    }

    public ShipData ToShipData()
    {
        ShipData sd = new ShipData();
        sd.prefabId = GetPrefabId();
        sd.slotId = GetComponentInParent<Slot>().slotId;
        sd.hullLayers = (ushort)hullLayers;
        sd.layerStrength = (ushort)layerStrength;
        sd.initiative = (ushort)Initiative;

        TechTile[] shipTech = GetComponentsInChildren<TechTile>();
        sd.numTechTiles = (ushort)shipTech.Length;
        sd.techTiles = new TechTileData[sd.numTechTiles];

        for (int t = 0; t < sd.numTechTiles; t++)
        {
            sd.techTiles[t] = shipTech[t].ToTechTileData();
        }

        return sd;
    }
}
