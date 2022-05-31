using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemInfo : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Text m_displayName;
    [SerializeField]
    UnityEngine.UI.Text m_cost;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCost(int cost)
    {
        m_cost.text = "$" + cost.ToString();
    }

    public void SetDisplayName(string name)
    {
        m_displayName.text = name;
    }
}
