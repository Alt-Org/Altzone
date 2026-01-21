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

    private void Start()
    {
        TryGetComponent(out _maskImageHandler);
    }

    public void SetMainCharacterImage(AvatarPiece feature, AvatarPartInfo partInfo)
    {
        Sprite image = partInfo.AvatarImage;
        Sprite mask = partInfo.MaskImage;

        switch (feature)
        {
            case AvatarPiece.Hair:
                SetImage(_mainHair, image);
                SetMaskImage(mask, _mainHair);
                break;

            case AvatarPiece.Eyes:
                SetImage(_mainEyes, image);
                SetMaskImage(mask, _mainEyes);
                break;

            case AvatarPiece.Nose:
                SetImage(_mainNose, image);
                SetMaskImage(mask, _mainNose);
                break;

            case AvatarPiece.Mouth:
                SetImage(_mainMouth, image);
                SetMaskImage(mask, _mainMouth);
                break;

            case AvatarPiece.Clothes:
                SetImage(_mainBody, image);
                SetMaskImage(mask, _mainBody);
                if (_maskImageHandler != null)
                {
                    _maskImageHandler.SetImage(image);
                }
                break;

            case AvatarPiece.Hands:
                SetImage(_mainHands, image);
                SetMaskImage(mask, _mainHands);
                break;

            case AvatarPiece.Feet:
                SetImage(_mainFeet, image);
                SetMaskImage(mask, _mainFeet);
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

    private void SetMaskImage(Sprite maskSprite, Image avatarImage)
    {
        EnsureMaterial(avatarImage);

        Texture2D maskTex;

        if (maskSprite != null)
        {
            maskTex = maskSprite.texture;
        }
        else
        {
            maskTex = GetTransparentMask(avatarImage.sprite.texture);
        }

        avatarImage.material.SetTexture("_MaskTex", maskTex);
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

    public void SetHeadColor(Color color)
    {
        if (_mainHead != null)
            _mainHead.color = color;
    }
}
