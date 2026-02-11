using System.Collections;
using System.Collections.Generic;
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

    public SpriteResolver GetRenderer(AvatarPart part)
    {
        return _resolvers.TryGetValue(part, out SpriteResolver resolver) ? resolver : null;
    }
}
