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
    [SerializeField] private Image _itemBackground;
    [field: SerializeField] public Button OpenProfileButton { get; private set; }

    public void Initialize(int rank, string name, int wins, AvatarVisualData avatarVisualData)
    {
        _rankText.text = rank.ToString() + ".";
        _nameText.text = name;
        _winsText.text = wins.ToString() + " pts";
        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }
    }

    public void Initialize(int rank, PlayerLeaderboard ranking)
    {

        _rankText.text = rank.ToString() + ".";
        _nameText.text = ranking.Player.Name;
        _winsText.text = ranking.WonBattles.ToString() + " pts";
        if(ranking.Player != null)
        {
            AvatarVisualData avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(ranking.Player);
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }
        if (ranking.Clanlogo != null) _clanHeart.SetHeartColors(ranking.Clanlogo);
    }

    public void RecolorBackground() //make list item bg white if the placement is the player's, also set the avatar to player avatar
    {
        _itemBackground.color = new Color(0, 0, 0, 0);
        _avatarFaceLoader.SetUseOwnAvatarVisuals(true);
    }
}
