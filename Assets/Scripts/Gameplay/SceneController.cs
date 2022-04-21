using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    float startTime;
    bool inBattle = false;

    ShopSlot shop;
    CursorControl cursor;
    Player player;
    Player opponent;
    
    [SerializeField]
    SaveSystem saveSystem;

    private void Awake()
    {
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<ShopSlot>();
        cursor = GameObject.FindGameObjectWithTag("Cursor").GetComponent<CursorControl>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();


        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.gameObject.tag);

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inBattle)
        {
            if (!opponent.HasShipsLeft())
            {
                if (player.HasShipsLeft()) Debug.Log("YOU WON!");
                else Debug.Log("IT WAS A DRAW!");
                EndBattle();
            }
            else if (!player.HasShipsLeft())
            {
                if (opponent.HasShipsLeft()) Debug.Log("YOU LOST!");
                EndBattle();
            }
        }
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded " + scene.name + ", mode = " + mode.ToString());

        if (scene.name == "BattleScene")
        {
            StartBattle();
        }
        else if (scene.name == "ShoppingScene")
        {
            StartShopping();
        }
    }

    void StartBattle()
    {
        startTime = Time.time;
        inBattle = true;

        shop.SetEnableShop(false);
        cursor.SetEnableDrag(false);

        saveSystem.SavePlayer(player);
        
        opponent = GetOpponent();

        player.BattleStarted(opponent);
        opponent.BattleStarted(player);

        Debug.Log("StartBattle in " + SceneManager.GetActiveScene().name);
    }

    Player GetOpponent()
    {
        return saveSystem.LoadPlayer(player.round);
    }

    void EndBattle()
    {
        inBattle = false;
        shop.SetEnableShop(true);
        opponent.BattleEnded();
        Destroy(opponent.gameObject);
        player.BattleEnded();
        player.round += 1;
        SceneManager.LoadScene("ShoppingScene");
    }

    void StartShopping()
    {
        cursor.SetEnableDrag(true);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
