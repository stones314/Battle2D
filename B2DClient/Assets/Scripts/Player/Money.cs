using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetBalance()
    {
        return int.Parse(GetComponent<UnityEngine.UI.Text>().text.Substring(1));
    }

    public void SetBalance(int balance)
    {
        GetComponent<UnityEngine.UI.Text>().text = "$" + balance;
    }
}
