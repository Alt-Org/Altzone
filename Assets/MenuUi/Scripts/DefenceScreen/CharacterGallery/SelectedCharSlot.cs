using UnityEngine.UI;
using UnityEngine;
using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
using TMPro;

public class SelectedCharSlot : MonoBehaviour
{
    [SerializeField] private Image _characterHeadImage;
    [SerializeField] private Image _lockImage;
    [SerializeField] private TMP_Text _className;

    private StatsWindowController _controller;
    void OnEnable()
    {
        if (_controller == null) _controller = FindObjectOfType<StatsWindowController>();

        SetCharacterHeadImage();
        SetClassName();
    }
    private void SetCharacterHeadImage()
    {
        Sprite sprite = _controller.GetCurrentCharacterSprite();

        if (sprite != null)
        {
            _characterHeadImage.sprite = sprite;
        }
    }
    private void SetClassName()
    {
        _className.text = _controller.GetCurrentCharacterClassName();
    }
}
