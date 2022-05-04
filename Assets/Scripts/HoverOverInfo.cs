using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverOverInfo : MonoBehaviour
{

    [SerializeField]
    UnityEngine.UI.Text description;
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    Transform icons;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDescription(string text)
    {
        description.text = text;
    }
}
