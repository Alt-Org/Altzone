using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGalleryIntComponent : MonoBehaviour
{
    [SerializeField]
    private int selectedCharacter;
    public void SetSelectedCharacterGalleryInt(int value)
    {
        selectedCharacter = value;
    }
    public int GetSelectedCharacterGalleryInt()
    {
        return selectedCharacter;
    }
}
