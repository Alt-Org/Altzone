using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardWinsItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _winsText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;

    public void Initialize(int rank, string name, int wins)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _winsText.text = wins.ToString();
    }
}
