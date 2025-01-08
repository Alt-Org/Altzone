using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGalleryIntComponent : MonoBehaviour
{
    [SerializeField]
    private int characterInt;
    public void SetSelectedCharacterGalleryInt(int value)
    {
        characterInt = value;
    }
    public int GetSelectedCharacterGalleryInt()
    {
        return characterInt;
    }
}
