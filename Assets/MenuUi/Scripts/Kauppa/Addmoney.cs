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

    // Method for adding 10 coins
    public void Add10Coins()
    {
        AddCoins(10);
    }

    // Method for adding 20 coins
    public void Add20Coins()
    {
        AddCoins(20);
    }

    // Method for adding 30 coins
    public void Add30Coins()
    {
        AddCoins(30);
    }
    // Method for adding 40 coins
    public void Add40Coins()
    {
        AddCoins(40);
    }
    // Method for adding 50 coins
    public void Add50Coins()
    {
        AddCoins(50);
    }
    // Method for adding 60 coins
    public void Add60Coins()
    {
        AddCoins(60);
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
