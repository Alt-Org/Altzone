using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Addmoney : MonoBehaviour
{
    private Text money;
    private int moneyAmount;

    // Start is called before the first frame update
    void Start()
    {
        moneyAmount = 0;
        money = GetComponent<Text>();
    }

    private void Update()
    {
        money.text = moneyAmount.ToString();
    }

    // Method for adding 100 coins
    public void Add100Coins()
    {
        AddCoins(100);
    }

    // Method for adding 200 coins
    public void Add200Coins()
    {
        AddCoins(200);
    }

    // Method for adding 300 coins
    public void Add300Coins()
    {
        AddCoins(300);
    }
    // Method for adding 500 coins
    public void Add500Coins()
    {
        AddCoins(500);
    }
    // Method for adding 1000 coins
    public void Add1000Coins()
    {
        AddCoins(1000);
    }
    // Method for adding 2000 coins
    public void Add2000Coins()
    {
        AddCoins(2000);
    }

    // Method for adding a custom amount of coins
    public void AddCustomCoins(int amount)
    {
        AddCoins(amount);
    }

    // General method to add coins
    private void AddCoins(int amount)
    {
        moneyAmount += amount;
    }
}
