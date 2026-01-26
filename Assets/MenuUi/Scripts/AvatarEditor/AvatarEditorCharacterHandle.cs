using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Assets.Altzone.Scripts.Model.Poco.Player;
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

    private MaskImageHandler _maskImageHandler;
    private readonly Dictionary<Vector2Int, Texture2D> _transparentMaskCache = new();
    private readonly Dictionary<Vector2Int, Texture2D> _skinColorMaskCache = new();
    private readonly Dictionary<Vector2Int, Texture2D> _selectedColorMaskCache = new();
    private Color _skinColor = Color.white;
    private Color _classColor = Color.white;

    private void Start()
    {
        TryGetComponent(out _maskImageHandler);
    }

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

    public void SetMainCharacterImage(AvatarPiece feature, AvatarPartInfo partInfo, Color partColor)
    {
        Sprite image = partInfo ? partInfo.AvatarImage : null;
        Sprite mask = partInfo ? partInfo.MaskImage : null;

        switch (feature)
        {
            case AvatarPiece.Hair:
                SetImage(_mainHair, image);
                SetMaskImage(_mainHair, mask, partColor);
                break;

            case AvatarPiece.Eyes:
                SetImage(_mainEyes, image);
                SetMaskImage(_mainEyes, mask, partColor);
                break;

            case AvatarPiece.Nose:
                SetImage(_mainNose, image);
                //SetMaskImage(_mainNose, mask, partColor);
                Texture2D noseMask = GetSkinColorMask(image.texture);
                EnsureMaterial(_mainNose);
                _mainNose.material.SetTexture("_MaskTex", noseMask);
                _mainNose.material.SetColor("_SkinColor", _skinColor);
                _mainNose.material.SetColor("_SelectedColor", _skinColor);
                _mainNose.material.SetColor("_ClassColor", _classColor);
                break;

            case AvatarPiece.Mouth:
                SetImage(_mainMouth, image);
                SetMaskImage(_mainMouth, mask, partColor);
                break;

            case AvatarPiece.Clothes:
                SetImage(_mainBody, image);
                SetMaskImage(_mainBody, mask, partColor);
                if (_maskImageHandler != null)
                {
                    _maskImageHandler.SetImage(image);
                }
                break;

            case AvatarPiece.Hands:
                SetImage(_mainHands, image);
                SetMaskImage(_mainHands, mask, partColor);
                break;

            case AvatarPiece.Feet:
                SetImage(_mainFeet, image);
                SetMaskImage(_mainFeet, mask, partColor);
                break;
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

    private void SetMaskImage(Image avatarImage, Sprite maskSprite, Color selectedColor)
    {
        if (avatarImage.sprite == null)
        {
            return;
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
            maskTex = GetSelectedColorMask(avatarImage.sprite.texture);
        }

        avatarImage.material.SetTexture("_MaskTex", maskTex);
        avatarImage.material.SetColor("_SkinColor", _skinColor);
        avatarImage.material.SetColor("_SelectedColor", selectedColor);
        avatarImage.material.SetColor("_ClassColor", _classColor);
    }

    private void EnsureMaterial(Image image)
    {
        // Only 1 material made for each slot
        if (!image.material.name.EndsWith("(Instance)"))
        {
            image.material = Instantiate(_featureMaterial);
        }
    }

    private Texture2D GetTransparentMask(Texture2D reference)
    {
        // Creates a transparent mask if maskimage doesn't exist,
        // and uses already made one if mask for that size of image has
        // already been made. Shader needs the maskimage and avatarimage
        // to be the same size, and if maskimage is null the sprite
        // will be black
        Vector2Int size = new(reference.width, reference.height);

        if (_transparentMaskCache.TryGetValue(size, out var tex))
            return tex;

        Texture2D mask = new Texture2D(
            reference.width,
            reference.height,
            TextureFormat.RGBA32,
            false
        );

        mask.filterMode = reference.filterMode;
        mask.wrapMode = TextureWrapMode.Clamp;

        Color clear = new(0, 0, 0, 0);
        Color[] pixels = new Color[reference.width * reference.height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        mask.SetPixels(pixels);
        mask.Apply();

        _transparentMaskCache[size] = mask;
        return mask;
    }

    private Texture2D GetSelectedColorMask(Texture2D reference)
    {
        // For testing
        Vector2Int size = new(reference.width, reference.height);

        if (_selectedColorMaskCache.TryGetValue(size, out var tex))
            return tex;

        Texture2D mask = new Texture2D(
            reference.width,
            reference.height,
            TextureFormat.RGBA32,
            false
        );

        mask.filterMode = reference.filterMode;
        mask.wrapMode = TextureWrapMode.Clamp;

        Color skinMask = new(0, 1, 0, 1);
        Color[] pixels = new Color[reference.width * reference.height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = skinMask;

        mask.SetPixels(pixels);
        mask.Apply();

        _selectedColorMaskCache[size] = mask;
        return mask;
    }

    private Texture2D GetSkinColorMask(Texture2D reference)
    {
        // Same as above but for skincolor instead of transparent
        Vector2Int size = new(reference.width, reference.height);

        if (_skinColorMaskCache.TryGetValue(size, out var tex))
            return tex;

        Texture2D mask = new Texture2D(
            reference.width,
            reference.height,
            TextureFormat.RGBA32,
            false
        );

        mask.filterMode = reference.filterMode;
        mask.wrapMode = TextureWrapMode.Clamp;

        Color skinMask = new(1, 0, 0, 1);
        Color[] pixels = new Color[reference.width * reference.height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = skinMask;

        mask.SetPixels(pixels);
        mask.Apply();

        _skinColorMaskCache[size] = mask;
        return mask;
    }

    private void ApplyColors(Image image)
    {
        if (image == null || image.material == null)
        {
            return;
        }

        image.material.SetColor("_SkinColor", _skinColor);
        image.material.SetColor("_ClassColor", _classColor);
        _mainHead.color = _skinColor;
    }
}
