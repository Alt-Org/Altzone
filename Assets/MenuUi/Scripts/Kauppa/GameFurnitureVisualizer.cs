using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using MenuUI.Scripts;
using Altzone.Scripts.AvatarPartsInfo;
public class GameFurnitureVisualizer : MonoBehaviour
{
    [SerializeField] private Image _contentImage;
    [SerializeField] private TMP_Text _productText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _button;
    private GameFurniture _gameFurniture;
    private AvatarPartInfo _avatarPart;
    private DrivenRectTransformTracker m_Tracker;

    public void Initialize(GameFurniture gameFurniture, GameObject confirmationPopUp)
    {
        _gameFurniture = gameFurniture;
        _productText.text = _gameFurniture.Name;
        _priceText.text = _gameFurniture.Value.ToString();
        _contentImage.sprite = _gameFurniture.FurnitureInfo.RibbonImage? _gameFurniture.FurnitureInfo.RibbonImage : _gameFurniture.FurnitureInfo.Image;
        gameObject.GetComponent<GameFurniturePasser>().SetGameFurniture(gameFurniture);
        _button.onClick.AddListener(() => confirmationPopUp.SetActive(true));
        _button.onClick.AddListener(() => gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1"));
    }

    public void Initialize(AvatarPartInfo avatarPart, GameObject confirmationPopUp)
    {
        _avatarPart = avatarPart;
        _productText.text = string.IsNullOrWhiteSpace(_avatarPart.VisibleName) ? _avatarPart.Name : _avatarPart.VisibleName;
        _priceText.text = "100"; //_avatarPart.Value.ToString();
        _contentImage.sprite = _avatarPart.IconImage ? _avatarPart.IconImage : _avatarPart.AvatarImage;
        gameObject.GetComponent<GameFurniturePasser>().SetAvatarPart(_avatarPart);
        _button.onClick.AddListener(() => confirmationPopUp.SetActive(true));
        _button.onClick.AddListener(() => gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1"));
    }
}
