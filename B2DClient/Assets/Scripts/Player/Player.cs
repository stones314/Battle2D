using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int round = 0;
    public int level = 1;
    public int money;
    public int health;

    public int levelUpCost { get; private set; } = 8000;
    public int income { get; private set; } = 2000;

    [SerializeField]
    UnityEngine.UI.Text RoundText;
    [SerializeField]
    UnityEngine.UI.Text LevelUpCostText;
    [SerializeField]
    UnityEngine.UI.Text LevelText;

    Player enemy;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(this.gameObject.tag);

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        SetBalance(money);
        SetRound(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRound(int r)
    {
        round = r;
        RoundText.text = "Round\n" + (round + 1);
    }

    public void SetBalance(int balance)
    {
        money = balance;
        GetComponentInChildren<Money>().SetBalance(money);
    }

    public int GetBalance()
    {
        return money;
    }

    public void IncreaseBalance(int sum)
    {
        money += sum;
        GetComponentInChildren<Money>().SetBalance(money);
    }

    public bool TryDecreeseBalance(int sum)
    {
        if (money < sum) return false;
        money -= sum;
        GetComponentInChildren<Money>().SetBalance(money);
        return true;
    }

    public void BattleStarted(Player opponent)
    {
        enemy = opponent;
        Ship[] ships = GetComponentsInChildren<Ship>();
        foreach(var ship in ships)
        {
            ship.BattleStarted(opponent);
        }

        GetComponentInChildren<Fleet>().InitCombat(opponent);
    }

    public bool HasShipsLeft()
    {
        if (GetComponentInChildren<Fleet>().GetComponentsInChildren<FireUnit>().Length > 0) return true;

        ShipSpawner[] spawners = GetComponentsInChildren<ShipSpawner>();
        foreach (var spawner in spawners)
        {
            if (spawner.IsSpawning()) return true;
        }

        return false;
    }

    public void BattleEnded()
    {
        HandleIncome();

        Ship[] ships = GetComponentsInChildren<Ship>(true);
        foreach (var ship in ships)
        {
            ship.BattleEnded();
        }
    }

    private void HandleIncome()
    {
        SetRound(++round);

        if (income < 10000)
            income += 1000;
        IncreaseBalance(income);

        levelUpCost -= 1000;
        if (levelUpCost < 0) levelUpCost = 0;

        LevelUpCostText.text = "Level Up\n$" + levelUpCost;
    }

    public void LevelUp()
    {
        TryDecreeseBalance(levelUpCost);
        level += 1;
        levelUpCost += 8000;
        LevelUpCostText.text = "Level Up\n$" + levelUpCost;
        LevelText.text = "Level " + level;
    }

    public PlayerData ToPlayerData()
    {
        PlayerData pd = new PlayerData();
        pd.roundsPlayed = (ushort)round;
        pd.level = (ushort)level;
        pd.money = (ushort)money;
        pd.health = (short)health;
        
        Ship[] ships_ = GetComponentInChildren<Fleet>().GetComponentsInChildren<Ship>();
        pd.numShips = (ushort)ships_.Length;
        pd.ships = new ShipData[pd.numShips];
        for (int s = 0; s < pd.numShips; s++)
        {
            pd.ships[s] = ships_[s].ToShipData();
        }

        return pd;
    }

    public ShopPool GetShipPool()
    {
        return GetComponentInChildren<ShipSpawner>().GetShipPool();
    }
    public ShopPool GetEquipPool()
    {
        return GetComponentInChildren<ShipSpawner>().GetEquipPool();
    }
    public Player GetEnemy()
    {
        return enemy;
    }
}
