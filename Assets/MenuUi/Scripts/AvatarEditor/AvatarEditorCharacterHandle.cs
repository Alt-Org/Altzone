using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Player;
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
    [SerializeField] private Material _featureMaterial;
    [SerializeField] private Material _hairMaterial;

    private Color _skinColor = Color.white;
    private Color _classColor = Color.white;

    public void SetSkinColor(Color skinColor)
    {
        _skinColor = skinColor;
        UpdateColors();
    }

    public void SetClassColor(Color classColor)
    {
        _classColor = classColor;
        UpdateColors();
    }

    private void UpdateColors()
    {
        ApplyColors(_mainHair);
        ApplyColors(_mainEyes);
        ApplyColors(_mainNose);
        ApplyColors(_mainMouth);
        ApplyColors(_mainBody);
        ApplyColors(_mainHands);
        ApplyColors(_mainFeet);

        if (_mainHead != null)
        {
            _mainHead.color = _skinColor;
        }
    }

    public void SetPartColor(AvatarPiece slot, Color partColor)
    {
        switch (slot)
        {
            case AvatarPiece.Hair:
                _mainHair.material.SetColor("_SelectedColor", partColor);
                break;

            case AvatarPiece.Eyes:
                _mainEyes.material.SetColor("_SelectedColor", partColor);
                break;

            case AvatarPiece.Nose:
                _mainNose.material.SetColor("_SelectedColor", partColor);
                break;

            case AvatarPiece.Mouth:
                _mainMouth.material.SetColor("_SelectedColor", partColor);
                break;

            case AvatarPiece.Clothes:
                _mainBody.material.SetColor("_SelectedColor", partColor);
                break;

            case AvatarPiece.Feet:
                _mainFeet.material.SetColor("_SelectedColor", partColor);
                break;

            case AvatarPiece.Hands:
                _mainHands.material.SetColor("_SelectedColor", partColor);
                break;

            default:
                Debug.LogWarning($"Unhandled AvatarPiece in SetPartColor: {slot}");
                break;
        }
    }

    public IEnumerator SetMainCharacterImage(AvatarPiece feature, AvatarPartInfo partInfo, Color partColor)
    {
        Sprite image = partInfo ? partInfo.AvatarImage : null;
        Sprite mask = partInfo ? partInfo.MaskImage : null;
        bool isColorable = partInfo ? partInfo.IsColorable : false;

        switch (feature)
        {
            case AvatarPiece.Hair:
                SetImage(_mainHair, image);
                yield return (SetHairImage(_mainHair, mask, partColor, isColorable));
                break;

            case AvatarPiece.Eyes:
                SetImage(_mainEyes, image);
                yield return (SetMaskImage(_mainEyes, mask, partColor, isColorable));
                break;

            case AvatarPiece.Nose:
                SetImage(_mainNose, image);
                //SetMaskImage(_mainNose, mask, partColor);
                if (image != null)
                {
                    Texture2D noseMask = AvatarMaskUtility.GetSkinColorMask(image.texture);
                    EnsureMaterial(_mainNose);
                    _mainNose.material.SetTexture("_MaskTex", noseMask);
                    _mainNose.material.SetColor("_SkinColor", _skinColor);
                    _mainNose.material.SetColor("_SelectedColor", _skinColor);
                    _mainNose.material.SetColor("_ClassColor", _classColor);
                    _mainNose.material.SetFloat("_Colorable", 1f);
                }
                break;

            case AvatarPiece.Mouth:
                SetImage(_mainMouth, image);
                yield return (SetMaskImage(_mainMouth, mask, partColor, isColorable));
                break;

            case AvatarPiece.Clothes:
                SetImage(_mainBody, image);
                yield return (SetMaskImage(_mainBody, mask, partColor, isColorable));
                if (_mainHair != null && _mainHair.sprite != null)
                {
                    _mainHair.material.SetTexture("_BodyTex", _mainBody.sprite.texture);
                }
                break;

            case AvatarPiece.Hands:
                SetImage(_mainHands, image);
                yield return (SetMaskImage(_mainHands, mask, partColor, isColorable));
                break;

            case AvatarPiece.Feet:
                SetImage(_mainFeet, image);
                yield return (SetMaskImage(_mainFeet, mask, partColor, isColorable));
                break;
        }
    }

    private void SetImage(Image imageComponent, Sprite image)
    {
        if (image == null)
        {
            //imageComponent.enabled = false;
            return;
        }

        imageComponent.enabled = true;
        imageComponent.sprite = image;
    }

    private IEnumerator SetHairImage(Image avatarImage, Sprite maskSprite, Color selectedColor, bool isColorable)
    {
        // Hair uses a different shader & material to replace parts overlapping the body with transparency
        if (avatarImage.sprite == null)
        {
            yield break;
        }

        EnsureHairMaterial();

        Texture2D maskTex;

        if (maskSprite != null)
        {
            maskTex = maskSprite.texture;
        }
        else
        {
            //maskTex = AvatarMaskUtility.GetTransparentTexPixel(avatarImage.sprite.texture);
            //for testing, if maskimage doesn't exist lets you color the whole piece instead of preserving the default color
            maskTex = AvatarMaskUtility.GetSelectedColorMask(avatarImage.sprite.texture);
        }

        yield return new WaitForEndOfFrame();

        _mainHair.material.SetFloat("_Colorable", isColorable ? 1f : 0f);
        _mainHair.material.SetTexture("_MaskTex", maskTex);
        _mainHair.material.SetColor("_SkinColor", _skinColor);
        _mainHair.material.SetColor("_SelectedColor", selectedColor);
        _mainHair.material.SetColor("_ClassColor", _classColor);

        if (_mainBody != null && _mainBody.material != null && _mainBody.gameObject.activeInHierarchy && _mainBody.sprite != null)
        {
            _mainHair.material.SetTexture("_BodyTex", _mainBody.sprite.texture);
        }
        else
        {
            // Without this hair disappears if only showing the head image
            _mainHair.material.SetTexture("_BodyTex", AvatarMaskUtility.GetTransparentTexPixel);
        }
        _mainHair.gameObject.SetActive(false); // This is a little scuff, but the image doesn't update itself properly otherwise.
        //yield return null;
        _mainHair.gameObject.SetActive(true);
    }

    private void EnsureHairMaterial()
    {
        // Only 1 material made for hair
        if (!_mainHair.material.name.EndsWith("(Instance)"))
        {
            _mainHair.material = Instantiate(_hairMaterial);
        }
    }

    private IEnumerator SetMaskImage(Image avatarImage, Sprite maskSprite, Color selectedColor, bool isColorable)
    {
        if (avatarImage.sprite == null)
        {
            yield break;
        }

        EnsureMaterial(avatarImage);

        Texture2D maskTex;

        if (maskSprite != null)
        {
            maskTex = maskSprite.texture;
        }
        else
        {
            //maskTex = GetTransparentMask(avatarImage.sprite.texture);
            //for testing, if maskimage doesn't exist lets you color the whole piece instead of preserving the default color
            maskTex = AvatarMaskUtility.GetSelectedColorMask(avatarImage.sprite.texture);
        }

        yield return new WaitForEndOfFrame();

        avatarImage.material.SetFloat("_Colorable", isColorable ? 1f : 0f);
        avatarImage.material.SetTexture("_MaskTex", maskTex);
        avatarImage.material.SetColor("_SkinColor", _skinColor);
        avatarImage.material.SetColor("_SelectedColor", selectedColor);
        avatarImage.material.SetColor("_ClassColor", _classColor);

        avatarImage.gameObject.SetActive(false); // This is a little scuff, but the image doesn't update itself properly otherwise.
        //yield return null;
        avatarImage.gameObject.SetActive(true);
    }

    private void EnsureMaterial(Image image)
    {
        // Only 1 material made for each slot
        if (!image.material.name.EndsWith("(Instance)"))
        {
            image.material = Instantiate(_featureMaterial);
        }
    }

    private void ApplyColors(Image image)
    {
        if (image == null || image.material == null)
        {
            return;
        }

        image.material.SetColor("_SkinColor", _skinColor);
        image.material.SetColor("_ClassColor", _classColor);
    }
}
