using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Player
{
    public enum AvatarPiece
    {
        Hair,
        Eyes,
        Nose,
        Mouth,
        Clothes,
        Feet,
        Hands
    }

    [Serializable]
    public class AvatarData
    {
        public AvatarData(string name, List<string> featureIds, string color, Vector2 scale)
        {
            Name = (string)name.Clone();
            Hair = int.Parse(featureIds[0]);
            Eyes = int.Parse(featureIds[1]);
            Nose = int.Parse(featureIds[2]);
            Mouth = int.Parse(featureIds[3]);
            Clothes = int.Parse(featureIds[4]);
            Feet = int.Parse(featureIds[5]);
            Hands = int.Parse(featureIds[6]);
            Color = new(color);
            ScaleX = scale.x;
            ScaleY = scale.y;
        }

        public AvatarData(string name, ServerAvatar data)
        {
            Name = (string)name.Clone();
            Hair = data.hair;
            Eyes = data.eyes;
            Nose = data.nose;
            Mouth = data.mouth;
            Clothes = data.clothes;
            Feet = data.feet;
            Hands = data.hands;
            Color = new(data.skinColor);
            ScaleX = 1;
            ScaleY = 1;
        }

        public string Name;
        public List<string> FeatureIds
        {
            get
            {
                List<string> list = new();
                list.Add(Hair.ToString());
                list.Add(Eyes.ToString());
                list.Add(Nose.ToString());
                list.Add(Mouth.ToString());
                list.Add(Clothes.ToString());
                list.Add(Feet.ToString());
                list.Add(Hands.ToString());
                return list;
            }
        }
        public int Hair;
        public int Eyes;
        public int Nose;
        public int Mouth;
        public int Clothes;
        public int Feet;
        public int Hands;
        public string Color;
        //public Vector2 Scale = new(ScaleX, ScaleY);
        public float ScaleX;
        public float ScaleY;

        public bool Validate()
        {
            if ((Name == null) ||
                (FeatureIds == null || FeatureIds.Count == 0) ||
                (string.IsNullOrWhiteSpace(Color)))
                return (false);

            return (true);
        }

        public int GetPieceID(AvatarPiece piece)
        {
            return piece switch
            {
                AvatarPiece.Hair => Hair,
                AvatarPiece.Eyes => Eyes,
                AvatarPiece.Nose => Nose,
                AvatarPiece.Mouth => Mouth,
                AvatarPiece.Clothes => Clothes,
                AvatarPiece.Feet => Feet,
                AvatarPiece.Hands => Hands,
                _ => -1,
            };
        }
    }
}
