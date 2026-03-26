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
        public string Color; // Skin Color
        public string HairColor;
        public string EyesColor;
        public string NoseColor;
        public string MouthColor;
        public string ClothesColor;
        public string FeetColor;
        public string HandsColor;
        public Vector2 Scale = Vector2.one;

        // Default constructor
        public AvatarData() { }

        // Constructor from feature IDs
        public AvatarData(string name, List<string> featureIds, string skinColor, Vector2 scale)
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
            Color = skinColor;
            Scale = scale;
        }

        public AvatarData(string name, List<string> featureIds, string skinColor, List<string> featureColors, Vector2 scale)
        {
            if (featureIds != null && featureIds?.Count != 7)
                throw new ArgumentException("FeatureIds must contain exactly 7 elements, provided list contains " + featureIds.Count.ToString());

            if (featureColors != null && featureColors?.Count != 7)
                throw new ArgumentException("FeatureColors must contain exactly 7 elements, provided list contains " + featureColors.Count.ToString());

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
            Color = skinColor;
            if (featureColors != null)
            {
                HairColor = featureColors[0];
                EyesColor = featureColors[1];
                NoseColor = featureColors[2];
                MouthColor = featureColors[3];
                ClothesColor = featureColors[4];
                FeetColor = featureColors[5];
                HandsColor = featureColors[6];
            }
            Scale = scale;
        }

        // Constructor from server data
        public AvatarData(string name, ServerAvatar serverData)
        {
            Name = name;
            if (serverData.hair != null)
            {
                Hair = serverData.hair.id;
                HairColor = serverData.hair.color;
            }
            if (serverData.eyes != null)
            {
                Eyes = serverData.eyes.id;
                EyesColor = serverData.eyes.color;
            }
            if (serverData.nose != null)
            {
                Nose = serverData.nose.id;
                NoseColor = serverData.nose.color;
            }
            if (serverData.mouth != null)
            {
                Mouth = serverData.mouth.id;
                MouthColor = serverData.mouth.color;
            }
            if (serverData.clothes != null)
            {
                Clothes = serverData.clothes.id;
                ClothesColor = serverData.clothes.color;
            }
            if (serverData.feet != null)
            {
                Feet = serverData.feet.id;
                FeetColor = serverData.feet.color;
            }
            if (serverData.hands != null)
            {
                Hands = serverData.hands.id;
                HandsColor = serverData.hands.color;
            }
            Color = serverData.skinColor;
            Scale = Vector2.one;
        }

        public AvatarData(AvatarDefault serverData)
        {
            Name = serverData.CharacterName;
            if (serverData.HairId != null)
            {
                int.TryParse(serverData.HairId, out Hair);
                HairColor = "#FFFFFF";
            }
            if (serverData.EyesId != null)
            {
                int.TryParse(serverData.EyesId, out Eyes);
                EyesColor = "#FFFFFF";
            }
            if (serverData.NoseId != null)
            {
                int.TryParse(serverData.NoseId, out Nose);
                NoseColor = "#FFFFFF";
            }
            if (serverData.MouthId != null)
            {
                int.TryParse(serverData.MouthId, out Mouth);
                MouthColor = "#FFFFFF";
            }
            if (serverData.BodyId != null)
            {
                int.TryParse(serverData.BodyId, out Clothes);
                ClothesColor = "#FFFFFF";
            }
            if (serverData.FeetId != null)
            {
                int.TryParse(serverData.FeetId, out Feet);
                FeetColor = "#FFFFFF";
            }
            if (serverData.HandsId != null)
            {
                int.TryParse(serverData.HandsId, out Hands);
                HandsColor = "#FFFFFF";
            }
            Color = "#FFFFFF";
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
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Color) && Hair != 0 && Eyes != 0 && Nose != 0 && Mouth != 0 && Clothes != 0 && Feet != 0 && Hands != 0;
        
        public bool Validate() => IsValid;

        public bool ValidateAvatarPiece(AvatarPiece piece, AvatarPartsReference partsReference)
        {
            int pieceId = GetPieceID(piece);
            string pieceIdString = pieceId.ToString();

            return partsReference.GetAvatarPartById(pieceIdString) != null;
        }

        public List<AvatarPiece> GetInvalidAvatarPieces()
        {
            List<AvatarPiece> invalidPieces = new();
            AvatarPartsReference partsReference = AvatarPartsReference.Instance;
            foreach (AvatarPiece piece in Enum.GetValues(typeof(AvatarPiece)))
            {
                if (!ValidateAvatarPiece(piece, partsReference))
                {
                    invalidPieces.Add(piece);
                }
            }
            return invalidPieces;
        }

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

        public string GetPieceColor(AvatarPiece piece) => piece switch
        {
            AvatarPiece.Hair => HairColor,
            AvatarPiece.Eyes => EyesColor,
            AvatarPiece.Nose => NoseColor,
            AvatarPiece.Mouth => MouthColor,
            AvatarPiece.Clothes => ClothesColor,
            AvatarPiece.Feet => FeetColor,
            AvatarPiece.Hands => HandsColor,
            _ => Color
        };

        public void SetPieceColor(AvatarPiece piece, string color)
        {
            switch (piece)
            {
                case AvatarPiece.Hair: HairColor = color; break;
                case AvatarPiece.Eyes: EyesColor = color; break;
                case AvatarPiece.Nose: NoseColor = color; break;
                case AvatarPiece.Mouth: MouthColor = color; break;
                case AvatarPiece.Clothes: ClothesColor = color; break;
                case AvatarPiece.Feet: FeetColor = color; break;
                case AvatarPiece.Hands: HandsColor = color; break;
            }
        }
    }
}

