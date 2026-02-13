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
            if (!AvatarResolverDictionary.Instance.TryGetValue(part, out AvatarPartSetter.AvatarResolverStruct resolverStruct))
            {
                continue;
            }

            AvatarPartSetter.AssignAvatarPart(resolver, resolverStruct, playerData, part);
        }

        SpriteRenderer headSpriteRenderer = GetRenderer(AvatarPart.Head).GetComponent<SpriteRenderer>();
        AvatarPartSetter.SetHeadColor(headSpriteRenderer, playerData);
    }
}
