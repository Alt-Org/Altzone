using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardActivityItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;

    public void Initialize(int rank, string name, int coins)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _coinsText.text = coins.ToString();
    }
}
