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

    //[Header("Secondary")]
    //[SerializeField] private GameObject _secondaryBase;
    //[Space]
    //[SerializeField] private Image _secondaryHead;
    //[SerializeField] private Image _secondaryHair;
    //[SerializeField] private Image _secondaryEyes;
    //[SerializeField] private Image _secondaryNose;
    //[SerializeField] private Image _secondaryMouth;
    //[SerializeField] private Image _secondaryBody;
    //[SerializeField] private Image _secondaryHands;
    //[SerializeField] private Image _secondaryFeet;

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

    //public void SetSecondaryCharacterImage(FeatureSlot feature, Sprite image)
    //{
    //    if (!_secondaryBase.activeSelf)
    //        _secondaryBase.SetActive(true);

    //    switch (feature)
    //    {
    //        case FeatureSlot.Hair: SetImage(_secondaryHair, image); break;
    //        case FeatureSlot.Eyes: SetImage(_secondaryEyes, image); break;
    //        case FeatureSlot.Nose: SetImage(_secondaryNose, image); break;
    //        case FeatureSlot.Mouth: SetImage(_secondaryMouth, image); break;
    //        case FeatureSlot.Body: SetImage(_secondaryBody, image); break;
    //        case FeatureSlot.Hands: SetImage(_secondaryHands, image); break;
    //        case FeatureSlot.Feet: SetImage(_secondaryFeet, image); break;
    //    }
    //}

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

        //if (_secondaryHead != null)
        //    _secondaryHead.color = color;
    }

    //public void SetSecondaryCharacterHidden()
    //{
    //    _secondaryBase.SetActive(false);
    //}
}
