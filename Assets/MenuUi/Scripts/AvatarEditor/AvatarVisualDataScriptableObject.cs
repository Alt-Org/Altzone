using System.Collections.Generic;
using UnityEngine;
using Assets.Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.AvatarPartsInfo;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarVisualData")]
public class AvatarVisualDataScriptableObject : ScriptableObject
{
    [SerializeField] private AvatarVisualData data = new();

    public IReadOnlyDictionary<AvatarPiece, AvatarPartInfo> PartInfos => data.PartInfos;
    public Color Color { get => data.Color; set => data.Color = value; }

    public AvatarPartInfo Hair { get => data.Hair; set => data.Hair = value; }
    public AvatarPartInfo Eyes { get => data.Eyes; set => data.Eyes = value; }
    public AvatarPartInfo Nose { get => data.Nose; set => data.Nose = value; }
    public AvatarPartInfo Mouth { get => data.Mouth; set => data.Mouth = value; }
    public AvatarPartInfo Clothes { get => data.Clothes; set => data.Clothes = value; }
    public AvatarPartInfo Feet { get => data.Feet; set => data.Feet = value; }
    public AvatarPartInfo Hands { get => data.Hands; set => data.Hands = value; }

    public void SetAvatarPiece(AvatarPiece piece, AvatarPartInfo partInfo) => data.SetAvatarPiece(piece, partInfo);
    public AvatarPartInfo GetAvatarPiece(AvatarPiece piece) => data.GetAvatarPiece(piece);
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
    [SerializeField] private Color color = Color.white;

    private Dictionary<AvatarPiece, AvatarPartInfo> _partInfos;

    // 🔥 Public propertyt kaikille kentille
    public AvatarPartInfo Hair { get => hair; set => hair = value; }
    public AvatarPartInfo Eyes { get => eyes; set => eyes = value; }
    public AvatarPartInfo Nose { get => nose; set => nose = value; }
    public AvatarPartInfo Mouth { get => mouth; set => mouth = value; }
    public AvatarPartInfo Clothes { get => clothes; set => clothes = value; }
    public AvatarPartInfo Feet { get => feet; set => feet = value; }
    public AvatarPartInfo Hands { get => hands; set => hands = value; }

    public Color Color { get => color; set => color = value; }

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
}

