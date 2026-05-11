using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardClanPointsItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private Image _itemBackground;
    [field: SerializeField] public Button OpenProfileButton { get; private set; }

    public void Initialize(int rank, string name, int wins)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _pointsText.text = wins.ToString() + " pts";
    }

    public void RecolorBackground() //make list item bg white if the placement is the player's
    {
        _itemBackground.color = new Color(0, 0, 0, 0);
    }
}
