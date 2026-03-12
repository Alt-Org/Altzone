using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskInfoFiller : MonoBehaviour
{


    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _type;
    [SerializeField] private TextMeshProUGUI[] _description;
    [SerializeField] private TextMeshProUGUI _giftPrize;
    [SerializeField] private TextMeshProUGUI _coinPrize;
    
    public void FillInfo(Image image, string title, EducationCategoryType type, string[] description, int giftPrize, int coinPrize)
    {
        _image = image;
        _title.text = title;
        for (int i = 0; i < _description.Length; i++)
        {
            // If there is too much text to insert, stop the loop
            if (i > description.Length) break;

            _description[i].text = description[i];
        }
        _giftPrize.text = "+" + giftPrize;
        _coinPrize.text = "+" + coinPrize;
    }
}
