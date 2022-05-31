using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverOverInfo : MonoBehaviour
{

    [SerializeField]
    UnityEngine.UI.Text description;
    [SerializeField]
    UnityEngine.UI.Text stats;

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

    public void SetStats(string text)
    {
        stats.text = text;
    }
}
