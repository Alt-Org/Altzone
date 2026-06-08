using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidLobbyClanListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clanNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private RectTransform progressFill;
    [SerializeField] private Image[] playerIcons;

    private RectTransform _rectTransform;

    private RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

    public void SetAnchors(float minY, float maxY)
    {
        RectTransform.anchorMin = new Vector2(0f, minY);
        RectTransform.anchorMax = new Vector2(1f, maxY);
        RectTransform.anchoredPosition = Vector2.zero;
        RectTransform.sizeDelta = Vector2.zero;
    }

    public void Configure(string clanName, int playerCount, int maxPlayersPerClan)
    {
        int clampedCount = Mathf.Clamp(playerCount, 0, Mathf.Max(0, maxPlayersPerClan));

        if (clanNameText != null)
        {
            clanNameText.text = string.IsNullOrWhiteSpace(clanName) ? "Klaani" : clanName;
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"Pelaajat {clampedCount}/{maxPlayersPerClan}";
        }

        if (progressFill != null)
        {
            float fill = maxPlayersPerClan <= 0 ? 0f : Mathf.Clamp01((float)clampedCount / maxPlayersPerClan);
            progressFill.anchorMax = new Vector2(fill, progressFill.anchorMax.y);
            progressFill.anchoredPosition = Vector2.zero;
            progressFill.sizeDelta = Vector2.zero;
        }

        if (playerIcons == null)
        {
            return;
        }

        for (int i = 0; i < playerIcons.Length; i++)
        {
            if (playerIcons[i] != null)
            {
                playerIcons[i].gameObject.SetActive(i < clampedCount);
            }
        }
    }
}
