using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Name;
        public int Hair;
        public int Eyes;
        public int Nose;
        public int Mouth;
        public int Clothes;
        public int Feet;
        public int Hands;
        public string Color;
        public Vector2 Scale = Vector2.one;

        // Default constructor
        public AvatarData() { }

        // Constructor from feature IDs
        public AvatarData(string name, List<string> featureIds, string color, Vector2 scale)
        {
            if (featureIds != null && featureIds?.Count != 7)
                throw new ArgumentException("FeatureIds must contain exactly 7 elements, provided list contains " + featureIds.Count.ToString());

            Name = name;
            if (featureIds != null)
            {
                Hair = int.Parse(featureIds[0]);
                Eyes = int.Parse(featureIds[1]);
                Nose = int.Parse(featureIds[2]);
                Mouth = int.Parse(featureIds[3]);
                Clothes = int.Parse(featureIds[4]);
                Feet = int.Parse(featureIds[5]);
                Hands = int.Parse(featureIds[6]);
            }
            Color = color;
            Scale = scale;
        }


        // Constructor from server data
        public AvatarData(string name, ServerAvatar serverData)
        {
            Name = name;
            Hair = serverData.hair;
            Eyes = serverData.eyes;
            Nose = serverData.nose;
            Mouth = serverData.mouth;
            Clothes = serverData.clothes;
            Feet = serverData.feet;
            Hands = serverData.hands;
            Color = serverData.skinColor;
            Scale = Vector2.one;
        }

        // Properties
        public List<string> FeatureIds => new[] { Hair, Eyes, Nose, Mouth, Clothes, Feet, Hands }
                                          .Select(id => id.ToString()).ToList();

        public float ScaleX 
        { 
            get => Scale.x; 
            set => Scale = new Vector2(value, Scale.y); 
        }
        
        public float ScaleY 
        { 
            get => Scale.y; 
            set => Scale = new Vector2(Scale.x, value); 
        }

        // Methods
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Color);
        
        public bool Validate() => IsValid;

        public int GetPieceID(AvatarPiece piece) => piece switch
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

        public void SetPieceID(AvatarPiece piece, int id)
        {
            switch (piece)
            {
                case AvatarPiece.Hair: Hair = id; break;
                case AvatarPiece.Eyes: Eyes = id; break;
                case AvatarPiece.Nose: Nose = id; break;
                case AvatarPiece.Mouth: Mouth = id; break;
                case AvatarPiece.Clothes: Clothes = id; break;
                case AvatarPiece.Feet: Feet = id; break;
                case AvatarPiece.Hands: Hands = id; break;
            }
        }
    }
}

