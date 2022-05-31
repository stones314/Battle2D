using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplotionDestroy : MonoBehaviour
{

    public float destroyDelay = 2f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, destroyDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
