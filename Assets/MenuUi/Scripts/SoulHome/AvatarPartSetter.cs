using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace MenuUI.Scripts.SoulHome
{
    public static class AvatarPartSetter
    {
        private const string DefaultLabel = "0000000";
        private static readonly MaterialPropertyBlock s_materialPropertyBlock = new();

        public struct AvatarResolverStruct
        {
            public Func<AvatarData, int?> _dataGetter;
            public string _category;
            public string _suffix;
        }

        public static void AssignAvatarPart(SpriteResolver resolver, AvatarResolverStruct resolverStruct, PlayerData playerData, AvatarPart part)
        {
            SpriteLibraryAsset library = resolver.spriteLibrary.spriteLibraryAsset;
            SpriteRenderer spriteRenderer = resolver.GetComponent<SpriteRenderer>();

            string label = ResolveLabel(resolverStruct, playerData);

            bool labelExists = library.GetCategoryLabelNames(resolverStruct._category).Contains(label);

            if (!labelExists)
            {
                label = DefaultLabel + resolverStruct._suffix;
            }
            else
            {
                Sprite sprite = library.GetSprite(resolverStruct._category, label);
                if (sprite == null)
                {
                    spriteRenderer.enabled = false;
                    return;
                }
            }

            spriteRenderer.enabled = true;
            resolver.SetCategoryAndLabel(resolverStruct._category, label);
            resolver.ResolveSpriteToSpriteRenderer();
            SpriteRenderer renderer = resolver.GetComponent<SpriteRenderer>();
            SetMask(renderer, label, playerData, part);
        }

        private static string ResolveLabel(AvatarResolverStruct resolverStruct, PlayerData playerData)
        {
            if (playerData?.AvatarData == null || resolverStruct._dataGetter == null)
            {
                return DefaultLabel + resolverStruct._suffix;
            }

            int? id = resolverStruct._dataGetter(playerData.AvatarData);

            if (!id.HasValue)
            {
                return DefaultLabel + resolverStruct._suffix;
            }

            string idString = id.Value.ToString();

            if (idString.Length != 7)
            {
                return DefaultLabel + resolverStruct._suffix;
            }

            return idString + resolverStruct._suffix;
        }

        private static void SetMask(SpriteRenderer renderer, string label, PlayerData playerData, AvatarPart part)
        {
            if (renderer == null)
            {
                return;
            }

            s_materialPropertyBlock.Clear();
            renderer.GetPropertyBlock(s_materialPropertyBlock);

            Color skinColor = Color.white;
            if (playerData?.AvatarData?.Color != null && ColorUtility.TryParseHtmlString(playerData.AvatarData.Color, out Color parsedSkinColor))
            {
                skinColor = parsedSkinColor;
            }

            s_materialPropertyBlock.SetColor("_SkinColor", skinColor);

            int? id = playerData?.SelectedCharacterId;
            Color classColor = Color.white;

            if (id.HasValue && ClassReference.Instance != null)
            {
                CharacterClassType classType = BaseCharacter.GetClass((CharacterID)id.Value);
                classColor = ClassReference.Instance.GetColor(classType);
            }

            s_materialPropertyBlock.SetColor("_ClassColor", classColor);

            Color selectedColor = GetSelectedColor(playerData, part);
            s_materialPropertyBlock.SetColor("_SelectedColor", selectedColor);

            AvatarPartInfo partInfo = AvatarPartsReference.Instance.GetAvatarPartById(label.Substring(0, 7));
            Texture2D mask;

            // Nose is always skin color
            if (part == AvatarPart.Nose)
            {
                mask = AvatarMaskUtility.GetSkinColorMask(renderer.sprite.texture);
            }
            else if (partInfo != null && partInfo.MaskImage != null)
            {
                mask = partInfo.MaskImage.texture;;
            }
            // If maskimage does not exist color whole part
            else if (partInfo != null && partInfo.MaskImage == null)
            {
                Texture2D referenceTexture = partInfo.AvatarImage.texture;
                mask = AvatarMaskUtility.GetSelectedColorMask(referenceTexture);
            }
            // optionally don't color at all
            else
            {
                mask = AvatarMaskUtility.GetTransparentTexPixel;
            }

            s_materialPropertyBlock.SetTexture("_MaskTex", mask);

            renderer.SetPropertyBlock(s_materialPropertyBlock);
        }

        public static void SetHeadColor(SpriteRenderer headSpriteRenderer, PlayerData playerData)
        {
            if (playerData?.AvatarData?.Color != null && ColorUtility.TryParseHtmlString(playerData.AvatarData.Color, out Color color))
            {
                headSpriteRenderer.color = color;
            }
            else
            {
                headSpriteRenderer.color = Color.white;
            }
        }

        private static Color GetSelectedColor(PlayerData playerData, AvatarPart part)
        {
            // this should work when part colors are added to database
            if (playerData?.AvatarData == null)
                return Color.white;

            string colorString = null;

            switch(part)
            {
                case AvatarPart.R_Hand:
                case AvatarPart.L_Hand:
                    colorString = playerData.AvatarData.HandsColor;
                    break;
                case AvatarPart.L_Eyebrow:
                case AvatarPart.L_Eye:
                case AvatarPart.R_Eyebrow:
                case AvatarPart.R_Eye:
                    colorString = playerData.AvatarData.EyesColor;
                    break;
                case AvatarPart.Nose:
                    //colorString = playerData.AvatarData.NoseColor;
                    // Skin color
                    colorString = playerData.AvatarData.Color;
                    break;
                case AvatarPart.Mouth:
                    colorString = playerData.AvatarData.MouthColor;
                    break;
                case AvatarPart.Body:
                    colorString = playerData.AvatarData.ClothesColor;
                    break;
                case AvatarPart.R_Leg:
                case AvatarPart.L_Leg:
                    colorString = playerData.AvatarData.FeetColor;
                    break;
            }

            if (colorString != null && !colorString.StartsWith("#"))
            {
                colorString = "#" + colorString;
            }

            if (ColorUtility.TryParseHtmlString(colorString, out Color color))
            {
                return color;
            }

            return Color.white;
        }
    }
}
