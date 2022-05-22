using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ValueIndicator : MonoBehaviour
{
    private int value;

    UnityEngine.UI.Text textField;

    // Start is called before the first frame update
    void Start()
    {
        SetValue(value);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetValue(int newVal)
    {
        if(!textField) textField = GetComponentInChildren<UnityEngine.UI.Text>();
        value = newVal;
        textField.text = value.ToString();
    }

    public void AddValue(int addVal)
    {
        value += addVal;
        textField.text = value.ToString();
    }

    public int GetValue()
    {
        return value;
    }
}
