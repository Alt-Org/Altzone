using System.Collections.Generic;
using UnityEngine;
using Assets.Altzone.Scripts.Model.Poco.Player;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarVisualData")]
public class AvatarVisualDataScriptableObject : ScriptableObject
{
    [SerializeField] private AvatarVisualData data = new();

    public IReadOnlyDictionary<AvatarPiece, Sprite> Sprites => data.Sprites;
    public Color Color { get => data.Color; set => data.Color = value; }

    public Sprite Hair { get => data.Hair; set => data.Hair = value; }
    public Sprite Eyes { get => data.Eyes; set => data.Eyes = value; }
    public Sprite Nose { get => data.Nose; set => data.Nose = value; }
    public Sprite Mouth { get => data.Mouth; set => data.Mouth = value; }
    public Sprite Clothes { get => data.Clothes; set => data.Clothes = value; }
    public Sprite Feet { get => data.Feet; set => data.Feet = value; }
    public Sprite Hands { get => data.Hands; set => data.Hands = value; }

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

    // 🔥 Public propertyt kaikille kentille
    public Sprite Hair { get => hair; set => hair = value; }
    public Sprite Eyes { get => eyes; set => eyes = value; }
    public Sprite Nose { get => nose; set => nose = value; }
    public Sprite Mouth { get => mouth; set => mouth = value; }
    public Sprite Clothes { get => clothes; set => clothes = value; }
    public Sprite Feet { get => feet; set => feet = value; }
    public Sprite Hands { get => hands; set => hands = value; }

    public Color Color { get => color; set => color = value; }

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

    public void SetAvatarPiece(AvatarPiece piece, Sprite sprite)
    {
        switch (piece)
        {
            case AvatarPiece.Hair: Hair = sprite; break;
            case AvatarPiece.Eyes: Eyes = sprite; break;
            case AvatarPiece.Nose: Nose = sprite; break;
            case AvatarPiece.Mouth: Mouth = sprite; break;
            case AvatarPiece.Clothes: Clothes = sprite; break;
            case AvatarPiece.Feet: Feet = sprite; break;
            case AvatarPiece.Hands: Hands = sprite; break;
        }

        if (_sprites != null) _sprites[piece] = sprite;
    }

    public Sprite GetAvatarPiece(AvatarPiece piece) =>
        Sprites.TryGetValue(piece, out var sprite) ? sprite : null;
}

