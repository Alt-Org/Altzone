using MenuUi.Scripts.AvatarEditor;
using UnityEngine;
using UnityEngine.UI;

public class AvatarEditorCharacterHandle : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Image _mainHead;
    [SerializeField] private Image _mainHair;
    [SerializeField] private Image _mainEyes;
    [SerializeField] private Image _mainNose;
    [SerializeField] private Image _mainMouth;
    [SerializeField] private Image _mainBody;
    [SerializeField] private Image _mainHands;
    [SerializeField] private Image _mainFeet;

    private MaskImageHandler _maskImageHandler;

    private void Start()
    {
        TryGetComponent(out _maskImageHandler);
    }

    public void SetMainCharacterImage(FeatureSlot feature, Sprite image)
    {
        switch (feature)
        {
            case FeatureSlot.Hair: SetImage(_mainHair, image); break;
            case FeatureSlot.Eyes: SetImage(_mainEyes, image); break;
            case FeatureSlot.Nose: SetImage(_mainNose, image); break;
            case FeatureSlot.Mouth: SetImage(_mainMouth, image); break;
            case FeatureSlot.Body:
                {
                    SetImage(_mainBody, image);
                    if (_maskImageHandler != null)
                    {
                        _maskImageHandler.SetImage(image);
                    }
                    break;
                }
            case FeatureSlot.Hands: SetImage(_mainHands, image); break;
            case FeatureSlot.Feet: SetImage(_mainFeet, image); break;
        }
    }

    private void SetImage(Image imageComponent, Sprite image)
    {
        if (image == null)
        {
            imageComponent.enabled = false;
            return;
        }

        imageComponent.enabled = true;
        imageComponent.sprite = image;
    }

    public void SetHeadColor(Color color)
    {
        if (_mainHead != null)
            _mainHead.color = color;
    }
}
