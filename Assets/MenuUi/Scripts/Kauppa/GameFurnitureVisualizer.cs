using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;

public class GameFurnitureVisualizer : MonoBehaviour
{
    [SerializeField] private TMP_Text _productText;
    [SerializeField] private TMP_Text _priceText;
    private GameFurniture _gameFurniture;
    public void Initialize(GameFurniture gameFurniture)
    {
        _gameFurniture = gameFurniture;
        _productText.text = _gameFurniture.Name;
        _priceText.text = _gameFurniture.Value.ToString();

        gameObject.GetComponent<GameFurniturePasser>().SetGameFurniture(gameFurniture);
    }
}
