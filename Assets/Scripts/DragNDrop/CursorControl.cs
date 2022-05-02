using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorControl : MonoBehaviour
{

    //public variables:
    public Texture2D mouseCursor;

    //private variables
    Vector2 moveInput = new Vector2(0, 0);
    CursorMode cursorMode = CursorMode.Auto;

    [SerializeField]
    Player player;

    [SerializeField]
    Shop shop;

    Draggable draggable;

    bool _enableDrag;

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
        Cursor.SetCursor(mouseCursor, moveInput, cursorMode);
        _enableDrag = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Fire1"))
        {
            Click();
        }

        if (_enableDrag)
        {
            Drag();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            Drop();
        }
    }

    private void Click()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!_enableDrag)//For now this means we are in battle!
        {
            
        }
        else if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000.0f, LayerMask.GetMask("Tech Tile")))
        {
            TechClicked(hit.collider.transform, ray);
        }
        else if (Physics.Raycast(ray.origin, ray.direction, out hit, 500.0f, LayerMask.GetMask("Ship")))
        {
            ShipClicked(hit.collider.transform, ray);
        }
        else if (Physics.Raycast(ray.origin, ray.direction, out hit, 500.0f, LayerMask.GetMask("UI Button")))
        {
            ButtonClicked(hit.collider.transform);
        }
        else if (draggable)
        {
            draggable = null;
        }
    }

    private void TechClicked(Transform tech, Ray ray)
    {
        DraggableClicked(tech, ray);
    }

    private void ShipClicked(Transform ship, Ray ray)
    {
        DraggableClicked(ship, ray);
    }

    private void DraggableClicked(Transform target, Ray ray)
    {
        draggable = target.GetComponent<Draggable>();
        draggable.Clicked(target.position - ray.origin);
    }


    private void ButtonClicked(Transform button)
    {
        draggable = null;
        if (button.tag == "Roll")
        {
            if(player.money >= 1000)
                shop.Roll();
        }
        if (button.tag == "Freeze")
        {
            shop.ToggleFreeze();
        }
        if (button.tag == "Start Battle")
        {
            SceneManager.LoadScene("BattleScene");
        }
    }

    private void Drag()
    {
        if (draggable && draggable.isDragged)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = -10;
            draggable.DragTo(mousePos);
        }
    }

    private void Drop()
    {
        if (!draggable) return;
        
        int cost = draggable.Dropped(player.money);
        player.TryDecreeseBalance(cost);
    }

    public void SetEnableDrag(bool enable)
    {
        _enableDrag = enable;
    }
}
