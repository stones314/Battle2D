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

    [SerializeField]
    HoverOverInfo hoverOverInfo;

    Draggable draggable;

    bool _enableDrag;

    Ray mouseRay;

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
        HoverOver();

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

    private void HoverOver()
    {
        //Skip this if we are dragging an object, as it is not needed in that case,
        //and fast drag would make the ray miss and we "drop" the dragged object
        if (draggable && draggable.IsDragged()) return;

        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!_enableDrag)//For now this means we are in battle!
        {

        }
        else if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, 300.0f, LayerMask.GetMask("Tech Tile")))
        {
            HoverOverDraggable(hit.collider.transform);
        }
        else if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, 300.0f, LayerMask.GetMask("Ship")))
        {
            HoverOverDraggable(hit.collider.transform);
        }
        else if (draggable)
        {
            draggable.HoverOverExit();
            draggable = null;
            HideHoverInfo();
        }
    }

    private void HoverOverDraggable(Transform target)
    {
        if (!draggable)
        {
            draggable = target.GetComponent<Draggable>();
            draggable.HoverOverEnter();
            ShowHoverInfo();
        }
        else if (draggable.transform != target)
        {
            draggable.HoverOverExit();
            draggable = target.GetComponent<Draggable>();
            draggable.HoverOverEnter();
            ShowHoverInfo();
        }
    }

    private void ShowHoverInfo()
    {
        hoverOverInfo.gameObject.SetActive(true);
        hoverOverInfo.SetDescription(draggable.GetDescription());
        hoverOverInfo.SetStats(draggable.GetHoverOverStats());
    }

    private void HideHoverInfo()
    {
        hoverOverInfo.gameObject.SetActive(false);
    }

    private void Click()
    {
        RaycastHit hit;
        if (!_enableDrag)//For now this means we are in battle!
        {
            
        }
        else if (draggable)
        {
            DraggableClicked();
        }
        else if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, 300.0f, LayerMask.GetMask("UI Button")))
        {
            ButtonClicked(hit.collider.transform);
        }
    }

    private void DraggableClicked()
    {
        draggable.Clicked(draggable.transform.position - mouseRay.origin);
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
        if (button.tag == "Level Up")
        {
            if (player.money >= player.levelUpCost)
                player.LevelUp();
        }
    }

    private void Drag()
    {
        if (draggable && draggable.IsDragged())
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
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
