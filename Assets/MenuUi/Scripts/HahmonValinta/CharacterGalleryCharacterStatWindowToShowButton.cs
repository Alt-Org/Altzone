using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CharacterGalleryCharacterStatWindowToShowButton : MonoBehaviour
{
    public int CharacterStatWindowToShowValue;

    private Button _characterGalleryCharacterButton;

    private void Awake()
    {
        _characterGalleryCharacterButton = GetComponent<Button>();
        _characterGalleryCharacterButton.onClick.AddListener(_characterGalleryCharacterButtonOnClick);
    }

    private void _characterGalleryCharacterButtonOnClick()
    {
        SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow = CharacterStatWindowToShowValue;
        Debug.Log("changed to " + CharacterStatWindowToShowValue);
    }
}
