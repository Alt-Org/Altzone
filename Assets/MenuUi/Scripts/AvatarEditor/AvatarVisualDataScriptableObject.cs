using System.Collections.Generic;
using UnityEngine;
using Assets.Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.AvatarPartsInfo;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarVisualData")]
public class AvatarVisualDataScriptableObject : ScriptableObject
{
    [SerializeField] private AvatarVisualData data = new();

    public IReadOnlyDictionary<AvatarPiece, AvatarPartInfo> PartInfos => data.PartInfos;
    public Color SkinColor { get => data.SkinColor; set => data.SkinColor = value; }
    public Color ClassColor { get => data.ClassColor; set => data.ClassColor = value; }
    public Color HairColor { get => data.HairColor; set => data.HairColor = value; }
    public Color EyesColor { get => data.EyesColor; set => data.EyesColor = value; }
    public Color NoseColor { get => data.NoseColor; set => data.NoseColor = value; }
    public Color MouthColor { get => data.MouthColor; set => data.MouthColor = value; }
    public Color ClothesColor { get => data.ClothesColor; set => data.ClothesColor = value; }
    public Color FeetColor { get => data.FeetColor; set => data.FeetColor = value; }
    public Color HandsColor { get => data.HandsColor; set => data.HandsColor = value; }

    public AvatarPartInfo Hair { get => data.Hair; set => data.Hair = value; }
    public AvatarPartInfo Eyes { get => data.Eyes; set => data.Eyes = value; }
    public AvatarPartInfo Nose { get => data.Nose; set => data.Nose = value; }
    public AvatarPartInfo Mouth { get => data.Mouth; set => data.Mouth = value; }
    public AvatarPartInfo Clothes { get => data.Clothes; set => data.Clothes = value; }
    public AvatarPartInfo Feet { get => data.Feet; set => data.Feet = value; }
    public AvatarPartInfo Hands { get => data.Hands; set => data.Hands = value; }



    public void SetAvatarPiece(AvatarPiece piece, AvatarPartInfo partInfo) => data.SetAvatarPiece(piece, partInfo);
    public AvatarPartInfo GetAvatarPiece(AvatarPiece piece) => data.GetAvatarPiece(piece);
    public void SetColor(AvatarPiece piece, Color color) => data.SetColor(piece, color);
    public Color GetColor(AvatarPiece piece) => data.GetColor(piece);
}

[System.Serializable]
public class AvatarVisualData
{
    [SerializeField] private AvatarPartInfo hair;
    [SerializeField] private AvatarPartInfo eyes;
    [SerializeField] private AvatarPartInfo nose;
    [SerializeField] private AvatarPartInfo mouth;
    [SerializeField] private AvatarPartInfo clothes;
    [SerializeField] private AvatarPartInfo feet;
    [SerializeField] private AvatarPartInfo hands;

    [SerializeField] private Color skinColor = Color.white;
    [SerializeField] private Color classColor = Color.white;
    [SerializeField] private Color hairColor = Color.white;
    [SerializeField] private Color eyesColor = Color.white;
    [SerializeField] private Color noseColor = Color.white;
    [SerializeField] private Color mouthColor = Color.white;
    [SerializeField] private Color clothesColor = Color.white;
    [SerializeField] private Color feetColor = Color.white;
    [SerializeField] private Color handsColor = Color.white;

    private Dictionary<AvatarPiece, AvatarPartInfo> _partInfos;

    // 🔥 Public propertyt kaikille kentille
    public AvatarPartInfo Hair { get => hair; set => hair = value; }
    public AvatarPartInfo Eyes { get => eyes; set => eyes = value; }
    public AvatarPartInfo Nose { get => nose; set => nose = value; }
    public AvatarPartInfo Mouth { get => mouth; set => mouth = value; }
    public AvatarPartInfo Clothes { get => clothes; set => clothes = value; }
    public AvatarPartInfo Feet { get => feet; set => feet = value; }
    public AvatarPartInfo Hands { get => hands; set => hands = value; }

    public Color SkinColor { get => skinColor; set => skinColor = value; }
    public Color ClassColor { get => classColor; set => classColor = value; }
    public Color HairColor { get => hairColor; set => hairColor = value; }
    public Color EyesColor { get => eyesColor; set => eyesColor = value; }
    public Color NoseColor { get => noseColor; set => noseColor = value; }
    public Color MouthColor { get => mouthColor; set => mouthColor = value; }
    public Color ClothesColor { get => clothesColor; set => clothesColor = value; }
    public Color FeetColor { get => feetColor; set => feetColor = value; }
    public Color HandsColor { get => handsColor; set => handsColor = value; }

    public IReadOnlyDictionary<AvatarPiece, AvatarPartInfo> PartInfos
    {
        get
        {
            if (_partInfos == null)
            {
                _partInfos = new Dictionary<AvatarPiece, AvatarPartInfo>
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
            return _partInfos;
        }
    }

    public void SetAvatarPiece(AvatarPiece piece, AvatarPartInfo partInfo)
    {
        switch (piece)
        {
            case AvatarPiece.Hair: Hair = partInfo; break;
            case AvatarPiece.Eyes: Eyes = partInfo; break;
            case AvatarPiece.Nose: Nose = partInfo; break;
            case AvatarPiece.Mouth: Mouth = partInfo; break;
            case AvatarPiece.Clothes: Clothes = partInfo; break;
            case AvatarPiece.Feet: Feet = partInfo; break;
            case AvatarPiece.Hands: Hands = partInfo; break;
        }

        if (_partInfos != null) _partInfos[piece] = partInfo;
    }

    public AvatarPartInfo GetAvatarPiece(AvatarPiece piece) =>
        PartInfos.TryGetValue(piece, out var sprite) ? sprite : null;

    public void SetColor(AvatarPiece piece, Color color)
    {
        switch (piece)
        {
            case AvatarPiece.Hair: hairColor = color; break;
            case AvatarPiece.Eyes: eyesColor = color; break;
            case AvatarPiece.Nose: noseColor = color; break;
            case AvatarPiece.Mouth: mouthColor = color; break;
            case AvatarPiece.Clothes: clothesColor = color; break;
            case AvatarPiece.Feet: feetColor = color; break;
            case AvatarPiece.Hands: handsColor = color; break;
        }
    }

    public Color GetColor(AvatarPiece piece)
    {
        return piece switch
        {
            AvatarPiece.Hair => hairColor,
            AvatarPiece.Eyes => eyesColor,
            AvatarPiece.Nose => noseColor,
            AvatarPiece.Mouth => mouthColor,
            AvatarPiece.Clothes => clothesColor,
            AvatarPiece.Feet => feetColor,
            AvatarPiece.Hands => handsColor,
            _ => skinColor
        };
    }
}

