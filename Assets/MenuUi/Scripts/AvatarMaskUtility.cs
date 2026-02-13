using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMaskUtility : MonoBehaviour
{
    private static Texture2D s_transparentTexPixel;
    // I assume HideFlags.HideAndDontSave fixed the problem, but if the mask colors
    // behave unexpectedly, it is almost certainly because the Texture2Ds in the dictionaries get destroyed
    private static readonly Dictionary<Vector2Int, Texture2D> s_selectedColorMaskCache = new();
    private static readonly Dictionary<Vector2Int, Texture2D> s_skinColorMaskCache = new();
    private static readonly Dictionary<Vector2Int, Texture2D> s_transparentMaskCache = new();

    public static Texture2D GetTransparentTexPixel
    {
        get
        {
            if (s_transparentTexPixel == null)
            {
                s_transparentTexPixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                s_transparentTexPixel.SetPixel(0, 0, new Color(0, 0, 0, 0));
                s_transparentTexPixel.Apply();

                s_transparentTexPixel.hideFlags = HideFlags.HideAndDontSave;
            }

            return s_transparentTexPixel;
        }
    }

    public static Texture2D GetSelectedColorMask(Texture2D reference)
    {
        if (reference == null)
        {
            Debug.LogWarning($"Reference texture {reference.name} is null");
            return s_transparentTexPixel;
        }
            

        Vector2Int size = new(reference.width, reference.height);

        if (s_selectedColorMaskCache.TryGetValue(size, out var tex))
        {
            // I have no idea why tex can be null
            if (tex != null)
                return tex;

            s_selectedColorMaskCache.Remove(size);
        }

        Texture2D mask = new Texture2D(
            reference.width,
            reference.height,
            TextureFormat.RGBA32,
            false
        );

        mask.filterMode = reference.filterMode;
        mask.wrapMode = TextureWrapMode.Clamp;
        mask.hideFlags = HideFlags.HideAndDontSave;

        Color fill = new(0, 1, 0, 1);
        Color[] pixels = new Color[reference.width * reference.height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = fill;

        mask.SetPixels(pixels);
        mask.Apply();

        s_selectedColorMaskCache[size] = mask;
        return mask;
    }

    public static Texture2D GetTransparentMask(Texture2D reference)
    {
        if (reference == null)
        {
            Debug.LogWarning($"Reference texture {reference.name} is null");
            return GetTransparentTexPixel;
        }

        // Creates a transparent mask if maskimage doesn't exist,
        // and uses already made one if mask for that size of image has
        // already been made. Shader needs the maskimage and avatarimage
        // to be the same size, and if maskimage is null the sprite
        // will be black
        Vector2Int size = new(reference.width, reference.height);

        if (s_transparentMaskCache.TryGetValue(size, out var tex))
            return tex;

        Texture2D mask = new Texture2D(
            reference.width,
            reference.height,
            TextureFormat.RGBA32,
            false
        );

        mask.filterMode = reference.filterMode;
        mask.wrapMode = TextureWrapMode.Clamp;
        mask.hideFlags = HideFlags.HideAndDontSave;

        Color clear = new(0, 0, 0, 0);
        Color[] pixels = new Color[reference.width * reference.height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        mask.SetPixels(pixels);
        mask.Apply();

        s_transparentMaskCache[size] = mask;
        return mask;
    }

    public static Texture2D GetSkinColorMask(Texture2D reference)
    {
        if (reference == null)
        {
            Debug.LogWarning($"Reference texture {reference.name} is null");
            return GetTransparentTexPixel;
        }

        Vector2Int size = new(reference.width, reference.height);

        if (s_skinColorMaskCache.TryGetValue(size, out var tex))
            return tex;

        Texture2D mask = new Texture2D(
            reference.width,
            reference.height,
            TextureFormat.RGBA32,
            false
        );

        mask.filterMode = reference.filterMode;
        mask.wrapMode = TextureWrapMode.Clamp;
        mask.hideFlags = HideFlags.HideAndDontSave;

        Color skinMask = new(1, 0, 0, 1);
        Color[] pixels = new Color[reference.width * reference.height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = skinMask;

        mask.SetPixels(pixels);
        mask.Apply();

        s_skinColorMaskCache[size] = mask;
        return mask;
    }
}

