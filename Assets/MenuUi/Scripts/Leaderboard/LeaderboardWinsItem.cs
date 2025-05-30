using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardWinsItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _winsText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;
    [field: SerializeField] public Button OpenProfileButton { get; private set; }

    public void Initialize(int rank, string name, int wins, AvatarVisualData avatarVisualData)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _winsText.text = wins.ToString();
        if(avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }
    }
}
