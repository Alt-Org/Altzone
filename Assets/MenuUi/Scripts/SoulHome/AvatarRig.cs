using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using MenuUI.Scripts.SoulHome;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class AvatarRig : MonoBehaviour
{
    private Dictionary<AvatarPart, SpriteResolver> _resolvers;

    public Dictionary<AvatarPart, SpriteResolver> Resolvers { get { return _resolvers; } }

    private readonly Dictionary<AvatarPart, AvatarPartSetter.AvatarResolverStruct> _resolverDictionary = new()
        {
            {
                AvatarPart.Body,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Clothes,
                    _category = "Body",
                    _suffix = "",
                }
            },
            {
                AvatarPart.R_Hand,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Hands,
                    _category = "Hands",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.L_Hand,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Hands,
                    _category = "Hands",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.L_Eyebrow,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyebrows",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.L_Eye,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category ="Eyes",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.R_Eyebrow,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyebrows",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.R_Eye,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyes",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.Nose,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Nose,
                    _category = "Nose",
                    _suffix = ""
                }
            },
            {
                AvatarPart.Mouth,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Mouth,
                    _category = "Mouth",
                    _suffix = ""
                }
            },
            {
                AvatarPart.R_Leg,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Feet,
                    _category = "Legs",
                    _suffix= "R"
                }
            },
            {
                AvatarPart.L_Leg,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Feet,
                    _category = "Legs",
                    _suffix = "L"
                }
            },
        };

    private void Awake()
    {
        _resolvers = new Dictionary<AvatarPart, SpriteResolver>();

        foreach (var part in GetComponentsInChildren<AvatarPartResolver>(true))
        {
            _resolvers[part.Part] = part.Resolver;
        }
    }

    private SpriteResolver GetRenderer(AvatarPart part)
    {
        return _resolvers.TryGetValue(part, out SpriteResolver resolver) ? resolver : null;
    }

    public void ApplyAvatarToRig(PlayerData playerData)
    {
        foreach ((AvatarPart part, SpriteResolver resolver) in _resolvers)
        {
            if (!_resolverDictionary.TryGetValue(part, out AvatarPartSetter.AvatarResolverStruct resolverStruct))
            {
                continue;
            }

            AvatarPartSetter.AssignAvatarPart(resolver, resolverStruct, playerData, part);
        }

        SpriteRenderer headSpriteRenderer = GetRenderer(AvatarPart.Head).GetComponent<SpriteRenderer>();
        AvatarPartSetter.SetHeadColor(headSpriteRenderer, playerData);
    }
}
