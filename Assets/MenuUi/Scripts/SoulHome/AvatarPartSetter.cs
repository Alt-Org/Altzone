using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.U2D.Animation;
using static MenuUI.Scripts.SoulHome.SoulHomeLoad;

namespace MenuUI.Scripts.SoulHome
{
    public static class AvatarPartSetter
    {
        private const string _defaultLabel = "0000000";

        public struct AvatarResolverStruct
        {
            public Func<AvatarData, int?> _dataGetter;
            public string _category;
            public string _suffix;
        }

        public static void AssignAvatarPart(SpriteResolver resolver, AvatarResolverStruct resolverStruct, PlayerData playerData)
        {
            SpriteLibraryAsset library = resolver.spriteLibrary.spriteLibraryAsset;
            SpriteRenderer spriteRenderer = resolver.GetComponent<SpriteRenderer>();
            string label = ResolveLabel(resolverStruct, playerData);

            bool labelExists = library.GetCategoryLabelNames(resolverStruct._category).Contains(label);

            if (!labelExists)
            {
                label = _defaultLabel + resolverStruct._suffix;
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
        }

        private static string ResolveLabel(AvatarResolverStruct resolverStruct, PlayerData playerData)
        {
            if (playerData?.AvatarData == null || resolverStruct._dataGetter == null)
            {
                return _defaultLabel + resolverStruct._suffix;
            }

            int? id = resolverStruct._dataGetter(playerData.AvatarData);

            if (!id.HasValue)
            {
                return _defaultLabel + resolverStruct._suffix;
            }

            string idString = id.Value.ToString();

            if (idString.Length != 7)
            {
                return _defaultLabel + resolverStruct._suffix;
            }

            return idString + resolverStruct._suffix;
        }
    }
}
