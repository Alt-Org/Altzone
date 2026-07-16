using UnityEngine;

public static class ThreeColorGradient
{
    private const int textureHeight = 256;

    public static Sprite GenerateGradient(Color topColor, Color midColor, Color botColor)
    {
        Texture2D tex = new Texture2D(1, textureHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < textureHeight; y++)
        {
            float t = y / (float)(textureHeight - 1);

            Color c;

            if (t < 0.5f)
            {
                // Bottom -> Middle
                c = Color.Lerp(botColor, midColor, t * 2f);
            }
            else
            {
                // Middle -> Top
                c = Color.Lerp(midColor, topColor, (t - 0.5f) * 2f);
            }

            tex.SetPixel(0, y, c);
        }

        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
