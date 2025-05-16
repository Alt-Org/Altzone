using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardActivityItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [field: SerializeField] public Button OpenProfileButton { get; private set; }

    public void Initialize(int rank, string name, int coins)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _coinsText.text = coins.ToString();
    }
}
