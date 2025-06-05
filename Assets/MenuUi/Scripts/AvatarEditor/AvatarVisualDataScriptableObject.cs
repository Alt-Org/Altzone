using System.Collections;
using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarVisualData")]
public class AvatarVisualDataScriptableObject : ScriptableObject
{
     
    public List<Sprite> sprites
    {
        get
        {
            var list = new List<Sprite>();
            list.Add(Data.Hair);
            list.Add(Data.Eyes);
            list.Add(Data.Nose);
            list.Add(Data.Mouth);
            list.Add(Data.Clothes);
            list.Add(Data.Feet);
            list.Add(Data.Hands);
            return list;
        }
        set
        {
            Hair = value[0];
            Eyes = value[1];
            Nose = value[2];
            Mouth = value[3];
            Clothes = value[4];
            Feet = value[5];
            Hands = value[6];
        }
    }
    public AvatarVisualData Data = new ();
    public Sprite Hair { get => Data.Hair; set => Data.Hair = value; }
    public Sprite Eyes { get => Data.Eyes; set => Data.Eyes = value; }
    public Sprite Nose { get => Data.Nose; set => Data.Nose = value; }
    public Sprite Mouth { get => Data.Mouth; set => Data.Mouth = value; }
    public Sprite Clothes { get => Data.Clothes; set => Data.Clothes = value; }
    public Sprite Feet { get => Data.Feet; set => Data.Feet = value; }
    public Sprite Hands { get => Data.Hands; set => Data.Hands = value; }
    public Color color { get => Data.color; set => Data.color = value; }



    public void SetAvatarPiece(AvatarPiece piece, Sprite id) => Data.SetAvatarPiece(piece, id);
    /*{
        switch (piece)
        {
            case AvatarPiece.Hair:
                Data.Hair = id;
                break;
            case AvatarPiece.Eyes:
                Data.Eyes = id;
                break;
            case AvatarPiece.Nose:
                Data.Nose = id;
                break;
            case AvatarPiece.Mouth:
                Data.Mouth = id;
                break;
            case AvatarPiece.Clothes:
                Data.Clothes = id;
                break;
            case AvatarPiece.Feet:
                Data.Feet = id;
                break;
            case AvatarPiece.Hands:
                Data.Hands = id;
                break;
        }
    }*/
}

public class AvatarVisualData
{
    public Sprite Hair;
    public Sprite Eyes;
    public Sprite Nose;
    public Sprite Mouth;
    public Sprite Clothes;
    public Sprite Feet;
    public Sprite Hands;
    public Color color = new();

    public void SetAvatarPiece(AvatarPiece piece, Sprite id)
    {
        switch (piece)
        {
            case AvatarPiece.Hair:
                Hair = id;
                break;
            case AvatarPiece.Eyes:
                Eyes = id;
                break;
            case AvatarPiece.Nose:
                Nose = id;
                break;
            case AvatarPiece.Mouth:
                Mouth = id;
                break;
            case AvatarPiece.Clothes:
                Clothes = id;
                break;
            case AvatarPiece.Feet:
                Feet = id;
                break;
            case AvatarPiece.Hands:
                Hands = id;
                break;
        }
    }
}
