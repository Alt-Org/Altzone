using UnityEngine.UI;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Button))]
public class ClanMoneyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _grantedAmountText;
    [SerializeField] private TMP_Text _costText;
    public Button Button;

    private int _grantedAmount;
    public int GrantedAmount
    {
        get
        {
            return _grantedAmount;
        }

        set
        {
            _grantedAmount = value;
            UpdateText(_grantedAmountText, _grantedAmount, "");
        }
    }


    private float _cost;
    public float Cost
    {
        get
        {
            return _cost;
        }

        set
        {
            _cost = value;
            UpdateText(_costText, _cost, "ˆ");
        }
    }

    public void Initialize(int grantedAmount, float cost)
    {
        GrantedAmount = grantedAmount;
        Cost = cost;
    }

    private void UpdateText(TMP_Text tmpText, float newAmount, string endSymbol)
    {
        tmpText.text = newAmount + endSymbol; 
    }
}
