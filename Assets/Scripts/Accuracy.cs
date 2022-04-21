using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Accuracy : MonoBehaviour
{
    private int accuracy;

    UnityEngine.UI.Text textField;

    // Start is called before the first frame update
    void Start()
    {
        SetAccuracy(accuracy);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetAccuracy(int newAcc)
    {
        if(!textField) textField = GetComponentInChildren<UnityEngine.UI.Text>();
        accuracy = newAcc;
        textField.text = accuracy.ToString();
    }

    public void AddAccuracy(int addAcc)
    {
        accuracy += addAcc;
        textField.text = accuracy.ToString();
    }

    public int GetAccuracy()
    {
        return accuracy;
    }
}
