using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts;

public class LeaderboardWinsItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _winsText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;
    [field: SerializeField] public Button OpenProfileButton { get; private set; }

    private string _clanId;

    private string TruncateName(string name, int maxLength = 24)
    {
        if (string.IsNullOrEmpty(name)) return "";
        if (name.Length <= maxLength) return name;
        return name.Substring(0, maxLength - 3) + "...";
    }

    public void Initialize(int rank, string name, int wins, AvatarVisualData avatarVisualData, string clanId = null)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = TruncateName(name);
        _winsText.text = wins.ToString();
        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }

    }

    public void Initialize(int rank, PlayerLeaderboard ranking)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = ranking.Player.Name;
        _winsText.text = ranking.WonBattles.ToString();
        if(ranking.Player != null)
        {
            AvatarVisualData avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(ranking.Player);
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }
        if (ranking.Clanlogo != null) _clanHeart.SetHeartColors(ranking.Clanlogo);
    }
}
