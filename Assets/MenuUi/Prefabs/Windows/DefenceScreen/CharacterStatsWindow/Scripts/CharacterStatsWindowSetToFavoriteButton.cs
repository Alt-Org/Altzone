using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatsWindowSetToFavoriteButton : MonoBehaviour
{
    private bool _favorite = false;
    [SerializeField] public Image FavoriteButton;

    private void Start()
    {
        FavoriteButton.color = new Color32(100, 100, 100, 255);
    }
    public void SetToFavorite()
    {
        if (_favorite)
        {
            _favorite = false;
            FavoriteButton.color = new Color32(100, 100, 100, 255);
        } else if (_favorite == false)
        {
            _favorite = true;
            FavoriteButton.color = new Color32(255, 255, 255, 255);
        }
    }
}
