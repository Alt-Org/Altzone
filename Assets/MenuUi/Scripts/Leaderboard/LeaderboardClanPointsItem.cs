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
    [field: SerializeField] public Button OpenProfileButton { get; private set; }

    public void Initialize(int rank, string name, int wins)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _pointsText.text = wins.ToString();
    }
}
