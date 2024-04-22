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

    public void Addcoins()
    {
        moneyAmount += 10;
    }
}
