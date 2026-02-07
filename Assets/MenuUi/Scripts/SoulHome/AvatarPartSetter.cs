using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.U2D.Animation;
using static MenuUI.Scripts.SoulHome.SoulHomeLoad;

namespace MenuUI.Scripts.SoulHome
{
    public static class AvatarPartSetter
    {
        private const string DefaultLabel = "0000000";
        private static readonly MaterialPropertyBlock s_materialPropertyBlock = new();
        private static Texture2D s_transparentTex;
        private static Texture2D TransparentTex
        {
            get
            {
                if (s_transparentTex == null)
                {
                    s_transparentTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    s_transparentTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
                    s_transparentTex.Apply();
                }
                return s_transparentTex;
            }
        }

        public struct AvatarResolverStruct
        {
            public Func<AvatarData, int?> _dataGetter;
            public string _category;
            public string _suffix;
        }

        public static void AssignAvatarPart(SpriteResolver resolver, AvatarResolverStruct resolverStruct, PlayerData playerData, AvatarPartsReference avatarPartsReference)
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
            SetMask(renderer, label, playerData, avatarPartsReference);
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

        private static void SetMask(SpriteRenderer renderer, string label, PlayerData playerData, AvatarPartsReference partsReference)
        {
            if (renderer == null)
            {
                return;
            }

            s_materialPropertyBlock.Clear();
            renderer.GetPropertyBlock(s_materialPropertyBlock);

            if (playerData?.AvatarData?.Color != null)
            {
                if (ColorUtility.TryParseHtmlString(playerData.AvatarData.Color, out Color skinColor))
                {
                    s_materialPropertyBlock.SetColor("_SkinColor", skinColor);
                }
            }
            else
            {
                s_materialPropertyBlock.SetColor("_SkinColor", Color.white);
            }

            int? id = playerData?.SelectedCharacterId;
            Color classColor = Color.white;

            if (id.HasValue && ClassReference.Instance != null)
            {
                CharacterClassType classType = BaseCharacter.GetClass((CharacterID)id.Value);
                classColor = ClassReference.Instance.GetColor(classType);
            }
            s_materialPropertyBlock.SetColor("_ClassColor", classColor);

            Color selectedColor = Color.white;
            // add real selectedcolor here



            AvatarPartInfo part = partsReference.GetAvatarPartById(label.Substring(0, 7));
            Texture2D mask;

            if (part != null && part.MaskImage != null)
            {
                mask = part.MaskImage.texture;
                s_materialPropertyBlock.SetTexture("_MaskTex", mask);
            }
            else
            {
                s_materialPropertyBlock.SetTexture("_MaskTex", TransparentTex);
            }

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
    }
}
