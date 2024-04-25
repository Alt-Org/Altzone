using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public TMP_Text moneyText;
    public int moneyAmountToAdd;
    public int moneyAmountToReduce;

    private int currentMoney = 0;

    private void Start()
    {
        UpdateMoneyText();
    }

    public void AddMoney()
    {
        currentMoney += moneyAmountToAdd;
        UpdateMoneyText();
    }

    public void ReduceMoney()
    {
        int newMoney = currentMoney - moneyAmountToReduce;
        if (newMoney >= 0)
        {
            currentMoney = newMoney;
            UpdateMoneyText();
        }
    }

    private void UpdateMoneyText()
    {
        moneyText.text = "" + currentMoney.ToString();
    }
}
