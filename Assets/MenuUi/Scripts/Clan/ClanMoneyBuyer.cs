using UnityEngine;
using MenuUi.Scripts.TopPanel;
using System.Collections.Generic;
using System;

[Serializable]
public struct ClanMoneyItemParameters
{
    public int grantedAmount;
    public float costEuro;
    public ClanMoneyItem clanMoneyItem;
}
public class ClanMoneyBuyer : MonoBehaviour
{
    [SerializeField] private ClanCoins _ClanCoins;
    [SerializeField] private List<ClanMoneyItemParameters> itemsWithParameters;

    private void Start()
    {
        foreach (var item in itemsWithParameters)
        {
            item.clanMoneyItem.Initialize(item.grantedAmount,item.costEuro);
            item.clanMoneyItem.Button.onClick.AddListener( () => BuyMoney(item.grantedAmount));
        }
    }
    public void BuyMoney(int amount)
    {
        _ClanCoins.AddCoins(amount);
    }
}
