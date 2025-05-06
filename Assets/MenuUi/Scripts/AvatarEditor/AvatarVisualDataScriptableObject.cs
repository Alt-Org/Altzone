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
            list.Add(Hair);
            list.Add(Eyes);
            list.Add(Nose);
            list.Add(Mouth);
            list.Add(Clothes);
            list.Add(Feet);
            list.Add(Hands);
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
