using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    bool inBattle = false;
    bool loadingOpponent = false;

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
        EventManager.OnPlayerLoaded += OpponentDataLoaded;
        attackPeriod /= Constants.GameSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (inBattle)
        {
            if (loadingOpponent) return;
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
        inBattle = true;

        shop.SetEnableShop(false);
        cursor.SetEnableDrag(false);

        saveSystem.SavePlayer(player);
        
        //Load Opponent:
        saveSystem.BeginLoadOpponent(player.round);
        loadingOpponent = true;

        Debug.Log("StartBattle in " + SceneManager.GetActiveScene().name);
    }

    void OpponentDataLoaded(PlayerData data)
    {
        lastAttackTime = Time.time;

        opponent = saveSystem.CreatePlayer(data);

        player.BattleStarted(opponent);
        opponent.BattleStarted(player);

        battleController.StartBattle(ref player, ref opponent);

        loadingOpponent = false;
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
