using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cost : MonoBehaviour
{
    public int cost;

    UnityEngine.UI.Text textField;

    // Start is called before the first frame update
    void Start()
    {
        textField = GetComponentInChildren<UnityEngine.UI.Text>();
        SetCost(cost);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCost(int newCost)
    {
        cost = newCost;
        textField.text = "$" + cost;
    }

    public void AddCost(int addCost)
    {
        cost += addCost;
        textField.text = "$" + cost;
    }
}
