using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class AvatarRig : MonoBehaviour
{
    private Dictionary<AvatarPart, SpriteRenderer> _renderers;

    private void Awake()
    {
        _renderers = new Dictionary<AvatarPart, SpriteRenderer>();

        foreach (var part in GetComponentsInChildren<AvatarPartRenderer>(true))
        {
            _renderers[part.Part] = part.SpriteRenderer;
        }
    }

    public SpriteRenderer GetRenderer(AvatarPart part)
    {
        return _renderers.TryGetValue(part, out SpriteRenderer renderer) ? renderer : null;
    }
}
