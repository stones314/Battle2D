using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    bool inBattle = false;

    Shop shop;
    CursorControl cursor;
    Player player;
    Player opponent;

    bool waitForEnd = false;
    float countDownStart;
    
    [SerializeField]
    SaveSystem saveSystem;

    public float attackPeriod = 1.0f;
    float lastAttackTime;

    BattleController battleController = new BattleController();

    private void Awake()
    {
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
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
            if(Time.time - lastAttackTime > attackPeriod)
            {
                battleController.AttackNext();
                lastAttackTime = Time.time;
            }
            if (!opponent.HasShipsLeft() || !player.HasShipsLeft())
            {
                StartEndTimer();
            }
        }
        else if (waitForEnd && (Time.time - countDownStart) > 5)
        {
            if (!opponent.HasShipsLeft())
            {
                if (player.HasShipsLeft()) Debug.Log("YOU WON!");
                else Debug.Log("IT WAS A DRAW!");
                EndBattle();
            }
            else if (!player.HasShipsLeft())
            {
                if (opponent.HasShipsLeft())
                {
                    Debug.Log("YOU LOST!");
                    player.health -= opponent.level;
                }

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
        lastAttackTime = Time.time;
        inBattle = true;

        shop.SetEnableShop(false);
        cursor.SetEnableDrag(false);

        saveSystem.SavePlayer(player);
        
        opponent = GetOpponent();

        player.BattleStarted(opponent);
        opponent.BattleStarted(player);

        battleController.StartBattle(ref player, ref opponent);

        Debug.Log("StartBattle in " + SceneManager.GetActiveScene().name);
    }

    Player GetOpponent()
    {
        return saveSystem.LoadPlayer(player.round);
    }

    void EndBattle()
    {
        inBattle = false;
        waitForEnd = false;
        shop.SetEnableShop(true);
        //opponent.BattleEnded();
        Destroy(opponent.gameObject);
        player.BattleEnded();

        SceneManager.LoadScene("ShoppingScene");
    }

    void StartEndTimer()
    {
        inBattle = false;
        waitForEnd = true;
        countDownStart = Time.time;
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
