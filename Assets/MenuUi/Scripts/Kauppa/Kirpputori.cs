using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Kirpputori : MonoBehaviour
{
    //Currently works as a place to instantiate desiarable amounts of "ads".
    //To do:
    //Make it a place to Instantiate and store information about real-time available "ads".

    [Header("Instantiation Parameters")]
    [SerializeField] private int _AdsAmount;
    [SerializeField] private GameObject _AdPrefab;

    private List<GameObject> _adsInScene;

    [Header("Rect Transforms")]
    [SerializeField] private RectTransform _AdsGroup;
    [SerializeField] private RectTransform _Content;

    private void Awake()
    {
        _adsInScene = new();

        for (int i = 0; i < _AdsAmount; i++)
        {
            var newAd = Instantiate(_AdPrefab, _AdsGroup);
            _adsInScene.Add(newAd);
        }

        //Force content to rebild to avoid UI issues
        LayoutRebuilder.ForceRebuildLayoutImmediate(_Content);
    }
}
