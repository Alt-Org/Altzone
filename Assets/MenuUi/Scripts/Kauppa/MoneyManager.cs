using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public TMP_Text moneyText;
    public int moneyAmountToAdd;
    public int moneyAmountToReduce;


    //alkurahan m‰‰r‰
    private int currentMoney = 0;

    private void Start()
    {
        UpdateMoneyText();
    }

    //lis‰‰ rahaa kun painaa nappia
    public void AddMoney()
    {
        currentMoney += moneyAmountToAdd;
        UpdateMoneyText();
    }

    //v‰hent‰‰ rahaa kun painaa nappia
    public void ReduceMoney()
    {
        int newMoney = currentMoney - moneyAmountToReduce;
        if (newMoney >= 0)
        {
            currentMoney = newMoney;
            UpdateMoneyText();
        }
    }

    //P‰ivitt‰‰ teksti‰ jossa lukee rahan m‰‰r‰
    private void UpdateMoneyText()
    {
        moneyText.text = "" + currentMoney.ToString();
    }
}
