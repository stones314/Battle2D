using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSlot : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ship Centre")
        {
            Draggable cursorDrag = collision.gameObject.GetComponentInParent<Draggable>();
            cursorDrag.OverSlot(this.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ship Centre")
        {
            Draggable cursorDrag = collision.gameObject.GetComponentInParent<Draggable>();
            cursorDrag.LeftSlot();
        }
    }

}
