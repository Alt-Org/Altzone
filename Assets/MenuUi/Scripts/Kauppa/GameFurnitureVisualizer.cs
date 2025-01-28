using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using MenuUI.Scripts;

public class GameFurnitureVisualizer : MonoBehaviour
{
    [SerializeField] private Image _contentImage;
    [SerializeField] private TMP_Text _productText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _button;
    private GameFurniture _gameFurniture;
    private DrivenRectTransformTracker m_Tracker;

    public void Initialize(GameFurniture gameFurniture)
    {
        _gameFurniture = gameFurniture;
        _productText.text = _gameFurniture.Name;
        _priceText.text = _gameFurniture.Value.ToString();
        _contentImage.sprite = _gameFurniture.FurnitureInfo.PosterImage? _gameFurniture.FurnitureInfo.PosterImage : _gameFurniture.FurnitureInfo.Image;
        gameObject.GetComponent<GameFurniturePasser>().SetGameFurniture(gameFurniture);
    }
}
