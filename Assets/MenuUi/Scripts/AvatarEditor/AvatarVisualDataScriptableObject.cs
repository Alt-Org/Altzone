using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ALT-Zone/AvatarVisualData")]
public class AvatarVisualDataScriptableObject : ScriptableObject
{
    [SerializeField] private AvatarVisualData data = new();

    public IReadOnlyDictionary<AvatarPiece, Sprite> Sprites => data.Sprites;
    public Color Color { get => data.Color; set => data.Color = value; }

    public void SetAvatarPiece(AvatarPiece piece, Sprite sprite) => data.SetAvatarPiece(piece, sprite);
    public Sprite GetAvatarPiece(AvatarPiece piece) => data.GetAvatarPiece(piece);
}

[System.Serializable]
public class AvatarVisualData
{
    [SerializeField] private Sprite hair;
    [SerializeField] private Sprite eyes;
    [SerializeField] private Sprite nose;
    [SerializeField] private Sprite mouth;
    [SerializeField] private Sprite clothes;
    [SerializeField] private Sprite feet;
    [SerializeField] private Sprite hands;
    [SerializeField] private Color color = Color.white;

    private Dictionary<AvatarPiece, Sprite> _sprites;

    public IReadOnlyDictionary<AvatarPiece, Sprite> Sprites
    {
        get
        {
            if (_sprites == null)
            {
                _sprites = new Dictionary<AvatarPiece, Sprite>
                {
                    { AvatarPiece.Hair, hair },
                    { AvatarPiece.Eyes, eyes },
                    { AvatarPiece.Nose, nose },
                    { AvatarPiece.Mouth, mouth },
                    { AvatarPiece.Clothes, clothes },
                    { AvatarPiece.Feet, feet },
                    { AvatarPiece.Hands, hands },
                };
            }
            return _sprites;
        }
    }

    public Color Color { get => color; set => color = value; }

    public void SetAvatarPiece(AvatarPiece piece, Sprite sprite)
    {
        switch (piece)
        {
            case AvatarPiece.Hair: hair = sprite; break;
            case AvatarPiece.Eyes: eyes = sprite; break;
            case AvatarPiece.Nose: nose = sprite; break;
            case AvatarPiece.Mouth: mouth = sprite; break;
            case AvatarPiece.Clothes: clothes = sprite; break;
            case AvatarPiece.Feet: feet = sprite; break;
            case AvatarPiece.Hands: hands = sprite; break;
        }

       
        if (_sprites != null) _sprites[piece] = sprite;
    }

    public Sprite GetAvatarPiece(AvatarPiece piece) =>
        Sprites.TryGetValue(piece, out var sprite) ? sprite : null;
}
