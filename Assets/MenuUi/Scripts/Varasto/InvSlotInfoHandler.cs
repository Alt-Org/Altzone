using MenuUi.Scripts.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvSlotInfoHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _topText;
    [SerializeField] private TMP_Text _bottomText;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _commonBoxAttachmentBase;
    [SerializeField] private GameObject _inSoulHomePanel;
    [SerializeField] private TMP_Text _inSoulHomeText;
    [SerializeField] private GameObject _markedForSellingPanel;
    [SerializeField] private TMP_Text _markedForSellingText;
    [SerializeField] private GameObject _inVotingPanel;
    [SerializeField] private TMP_Text _inVotingText;
    [SerializeField] private GameObject _coin;

    [Header("Rarity Color")]
    [SerializeField] private Color _commonColor;
    [SerializeField] private Color _rareColor;
    [SerializeField] private Color _epicColor;
    [SerializeField] private Color _antiqueColor;

    public Image Icon { get => _icon;}

    public void SetSlotInfo(StorageFurniture furn, int sortingBy)
    {
        // Set color based on rarity
        _commonBoxAttachmentBase.color = GetColorByRarity(furn.Rarity.ToString());

        // Icon
        _icon.sprite = furn.Info.RibbonImage;
        if (_icon.sprite == null) _icon.sprite = furn.Sprite;

        // Name
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
        {
            _topText.text = furn.VisibleName;
        }
        else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
        {
            _topText.text = furn.Info != null ? furn.Info.EnglishName : furn.VisibleName;
        }
        else
        {
            _topText.text = furn.VisibleName; // Fallback if the language is null
        }


        // Weight
        switch (sortingBy)
        {
            case 0:
                _bottomText.text = furn.Value.ToString();
                break;
            case 1:
                _bottomText.text = furn.Value.ToString();
                break;
            case 2:
                _bottomText.text = furn.Weight + " KG";
                break;
            case 3:
                _bottomText.text = furn.Rarity.ToString();
                break;
            case 4:
                _bottomText.text = furn.SetName;
                break;
        }

        // In Soul Home panel text
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
        {
            _inSoulHomeText.text = "Sielunkodissa";
        }
        else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
        {
            _inSoulHomeText.text = "In Soul Home";
        }
        else
        {
            _inSoulHomeText.text = "In Soul Home"; // Default fallback if the language is null
        }

        // Show/hide In Soul Home panel (original if/else)
        if (furn.Position == new Vector2Int(-1, -1))
        {
            _inSoulHomePanel.SetActive(false);
        }
        else
        {
            _inSoulHomePanel.SetActive(true);
        }

        // Marked for Selling panel text
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
        {
            _markedForSellingText.text = "Myynnissä";
        }
        else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
        {
            _markedForSellingText.text = "In Selling";
        }
        else
        {
            _markedForSellingText.text = "In Selling"; // Default fallback if the language is null
        }
        _markedForSellingPanel.SetActive(furn.ClanFurniture.VotedToSell);

        // In Voting panel text
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.Finnish)
        {
            _inVotingText.text = "Äänestyksessä";
        }
        else if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
        {
            _inVotingText.text = "In Voting";
        }
        else
        {
            _inVotingText.text = "In Voting"; // Default fallback if the language is null
        }
        _inVotingPanel.SetActive(furn.ClanFurniture.InVoting);

        // Coin
        if (sortingBy == 0 || sortingBy == 1) _coin.SetActive(true);
        else _coin.SetActive(false);
    }


    private Color GetColorByRarity(string rarity)
    {
        return rarity switch
        {
            "Common" => _commonColor,
            "Rare" => _rareColor,
            "Epic" => _epicColor,
            "Antique" => _antiqueColor,
            _ => _commonColor, // Default to common color
        };
    }
}
