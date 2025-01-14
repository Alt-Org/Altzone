using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;

public class GameFurnitureVisualizer : MonoBehaviour
{
    [SerializeField] private TMP_Text _productText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _button;

    private GameObject _popUp;
    private GameFurniture _gameFurniture;

    public void Initialize(GameFurniture gameFurniture, GameObject correspondingPopUp)
    {
        _gameFurniture = gameFurniture;
        _productText.text = _gameFurniture.Name;
        _priceText.text = _gameFurniture.Value.ToString();

        _popUp = correspondingPopUp;
        _button.onClick.AddListener(() => _popUp.SetActive(true));
    }
}
