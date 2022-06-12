using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    Shop shop;
    CursorControl cursor;
    Player player;
    Player opponent;

    float countDownStart;
    
    [SerializeField]
    SaveSystem saveSystem;

    public float attackPeriod = 1.0f;
    float lastAttackTime;

    float endOfBattleTime = 5.0f;
    float msgSendTime;
    int msgRetryCount;

    BattleController battleController = new BattleController();

    enum SceneState
    {
        LobbyScene,
        ShoppingScene,
        SavingPlayer,
        LoadingOpponent,
        InBattle,
        EndBattle
    }
    SceneState sceneState;

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
        EventManager.OnPlayerLoaded += OnOpponentDataLoaded;
        EventManager.OnPlayerSaved += OnPlayerSaved;
        attackPeriod /= Constants.GameSpeed;
        endOfBattleTime /= Constants.GameSpeed;
        sceneState = SceneState.ShoppingScene;
    }

    // Update is called once per frame
    void Update()
    {
        switch (sceneState)
        {
            case SceneState.LobbyScene:
            case SceneState.ShoppingScene:
                break;
            case SceneState.SavingPlayer:
                if (msgRetryCount > 3)
                {
                    GoToShoping();
                }
                else if (Time.time >= msgSendTime + 3)
                {
                    //Try again
                    saveSystem.SavePlayer(player);
                    msgSendTime = Time.time;
                    msgRetryCount++;
                }
                break;
            case SceneState.LoadingOpponent:
                if(msgRetryCount > 3)
                {
                    GoToShoping();
                }
                else if (Time.time >= msgSendTime + 3)
                {

                    //Try again
                    saveSystem.BeginLoadOpponent(player.round);
                    msgSendTime = Time.time;
                    msgRetryCount++;
                }
                break;
            case SceneState.InBattle:
                if (!opponent.HasShipsLeft() || !player.HasShipsLeft())
                {
                    GoToEndBattle();
                }
                else if (Time.time - lastAttackTime > attackPeriod)
                {
                    battleController.AttackNext();
                    lastAttackTime = Time.time;
                }
                break;
            case SceneState.EndBattle:
                if ((Time.time - countDownStart) > endOfBattleTime)
                {
                    if (!opponent.HasShipsLeft())
                    {
                        /*
                        if (player.HasShipsLeft()) Debug.Log("YOU WON!");
                        else Debug.Log("IT WAS A DRAW!");
                        */
                        GoToShoping();
                    }
                    else if (!player.HasShipsLeft())
                    {
                        if (opponent.HasShipsLeft())
                        {
                            //Debug.Log("YOU LOST!");
                            player.health -= opponent.level;
                        }

                        GoToShoping();
                    }
                }
                break;
        }
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("OnSceneLoaded " + scene.name + ", mode = " + mode.ToString());

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
        sceneState = SceneState.SavingPlayer;
        //Debug.Log("Saving Player");

        shop.SetEnableShop(false);
        cursor.SetEnableDrag(false);

        saveSystem.SavePlayer(player);
        msgSendTime = Time.time;
        msgRetryCount = 0;
    }

    void OnPlayerSaved()
    {
        //Load Opponent:
        //Debug.Log("Player Saved. Loading Opponent");
        saveSystem.BeginLoadOpponent(player.round);
        sceneState = SceneState.LoadingOpponent;
        msgSendTime = Time.time;
        msgRetryCount = 0;
    }

    void OnOpponentDataLoaded(PlayerData data)
    {
        //Debug.Log("Opponent Loaded, Start Fighting");
        lastAttackTime = Time.time;

        opponent = saveSystem.CreatePlayer(data);

        player.BattleStarted(opponent);
        opponent.BattleStarted(player);

        battleController.StartBattle(ref player, ref opponent);

        sceneState = SceneState.InBattle;
    }

    void GoToShoping()
    {
        //Debug.Log("Victory/Loss thing complete. Go back to shop.");
        shop.SetEnableShop(true);

        if(opponent) Destroy(opponent.gameObject);

        player.BattleEnded();

        sceneState = SceneState.ShoppingScene;

        SceneManager.LoadScene("ShoppingScene");
    }

    void GoToEndBattle()
    {
        //Debug.Log("Fighting Ended, Display Victory/Loss thing (TODO");
        sceneState = SceneState.EndBattle;
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
