using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int round = 0;
    public int level = 1;
    public int money;
    public int health;

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
    }

    // Update is called once per frame
    void Update()
    {
        
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
        Ship[] ships = GetComponentsInChildren<Ship>();
        foreach(var ship in ships)
        {
            ship.BattleStarted(opponent);
        }
    }

    public bool HasShipsLeft()
    {
        return GetComponentsInChildren<FireUnit>().Length > 0;
    }

    public void BattleEnded()
    {
        Ship[] ships = GetComponentsInChildren<Ship>(true);
        foreach (var ship in ships)
        {
            ship.BattleEnded();
        }
    }
}
