using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.gameObject.tag);

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Roll()
    {
        foreach(var shopSlot in GetComponentsInChildren<ShopSlot>())
        {
            shopSlot.Roll();
        }
        FindObjectOfType<Player>().TryDecreeseBalance(1000);
    }
    public void ToggleFreeze()
    {
        foreach (var shopSlot in GetComponentsInChildren<ShopSlot>())
        {
            shopSlot.ToggleFreeze();
        }
    }

    public void SetEnableShop(bool enable)
    {
        this.gameObject.SetActive(enable);

        foreach (var shopSlot in GetComponentsInChildren<ShopSlot>(true))
        {
            shopSlot.SetEnableShop(enable);
        }
    }

}
