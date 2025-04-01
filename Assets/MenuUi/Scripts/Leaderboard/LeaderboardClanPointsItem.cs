using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardClanPointsItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _pointsText;

    public void Initialize(int rank, string name, int wins)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _pointsText.text = wins.ToString();
    }
}
