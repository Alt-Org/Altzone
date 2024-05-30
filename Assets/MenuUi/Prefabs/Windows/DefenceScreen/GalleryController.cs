using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalleryController : MonoBehaviour
{
    [SerializeField]
    private List <GameObject> _characterList;

    public int selectedCharacter { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        int characterInt = 0;
        foreach(GameObject character in _characterList)
        {
            Transform content = transform.Find("Content");
            Instantiate(character,content);
            if (character.GetComponent("CharacterGalleryIntComponent") == null)
            {
                character.AddComponent<CharacterGalleryIntComponent>().SetSelectedCharacterGalleryInt(characterInt);               
            }
            /*
            if (character.GetComponent("CharacterGalleryCharacterStatWindowToShowButton") == null)
            {
                character.AddComponent<CharacterGalleryCharacterStatWindowToShowButton>().CharacterStatWindowToShowValue = characterInt;
            }
            //character.GetComponentInChildren<Button>();
            */
            characterInt++;
            
        }

    }


}
