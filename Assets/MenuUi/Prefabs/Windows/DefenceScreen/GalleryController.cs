using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalleryController : MonoBehaviour
{
    [SerializeField]
    private List <GameObject> _characterList;
    // Start is called before the first frame update
    void Start()
    {
        int selectedCharacter = 0;
        foreach(GameObject character in _characterList)
        {
            Transform content = transform.Find("Content");
            Instantiate(character,content);
            character.AddComponent<CharacterGalleryIntComponent>().SetSelectedCharacterGalleryInt(selectedCharacter);
            selectedCharacter++;
        }

    }
}