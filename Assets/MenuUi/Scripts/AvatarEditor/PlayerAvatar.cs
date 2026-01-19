using System;
using System.Linq;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class PlayerAvatar
    {
        private string _name;
        public string Name {
            get { return _name; }

            set
            {
                //Only assign the value if the string contains only numbers and letters and if the current value is null or empty
                if (!HasSpecialCharacters(value))
                {_name = value;}
                else if (string.IsNullOrEmpty(_name))
                { _name = ""; }
            }
        }
        public string HairId { get; set; }
        public string EyesId { get; set; }
        public string NoseId { get; set; }
        public string MouthId { get; set; }
        public string BodyId { get; set; }
        public string HandsId { get; set; }
        public string FeetId { get; set; }

        private string _color;
        public string Color {
            get { return _color; }

            set
            {
                string correctedValue = value;
                if (!correctedValue.StartsWith("#"))
                {
                    correctedValue = "#" + correctedValue; ;
                }

                Color convertedColor; 
                if (ColorUtility.TryParseHtmlString(correctedValue, out convertedColor)) //If the colorcode provided is valid, assign it to the _color field
                {
                    _color = ColorUtility.ToHtmlStringRGBA(convertedColor);
                }
                else
                {
                    Debug.LogWarning($"Submitted color {value}could not be parsed, using white");
                    convertedColor = new Color(1,1,1,1); //values for solid white
                    _color = ColorUtility.ToHtmlStringRGBA(convertedColor);
                }
            }
        }

        private Vector2 _scale;
        public Vector2 Scale {
            get { return _scale; }

            set
            {
                if (value.x >= 0.8f && value.y >= 0.8f)
                {
                    _scale = value;
                }
                else
                {
                    //Minimum scale is 0.8, so we assign that if the input value is less.
                    _scale = new Vector2( 0.8f, 0.8f);
                }
            }
        }

        public PlayerAvatar(AvatarDefaultReference.AvatarDefaultPartInfo featureIds)
        {
            
            HairId = GetOrDefault(featureIds.HairId);
            EyesId = GetOrDefault(featureIds.EyesId);
            NoseId = GetOrDefault(featureIds.NoseId);
            MouthId = GetOrDefault(featureIds.MouthId);
            BodyId = GetOrDefault(featureIds.BodyId);
            HandsId = GetOrDefault(featureIds.HandsId);
            FeetId = GetOrDefault(featureIds.FeetId);
            

            Name = string.Empty;
            Color = "#ffffff";
            Scale = Vector2.one;
        }

        public PlayerAvatar(AvatarData data)
        {
            Name = data.Name ?? string.Empty;

            foreach (AvatarPiece piece in Enum.GetValues(typeof(AvatarPiece)))
            {
                int id = data.GetPieceID(piece);

                if (id > 0)
                {
                    SetPart(piece, id.ToString());
                }
            }

            Color = data.Color ?? "#ffffff";
            Scale = new Vector2(data.ScaleX, data.ScaleY);
        }
        
        private static string GetOrDefault(string value) =>
            string.IsNullOrWhiteSpace(value) ? "0" : value;

        private bool HasSpecialCharacters(string input)
        {
            return input.Any(ch => !char.IsLetterOrDigit(ch));
        }

        public void SetPart(AvatarPiece slot, string partId)
        {
            switch (slot)
            {
                case AvatarPiece.Hair:
                    HairId = partId;
                    break;

                case AvatarPiece.Eyes:
                    EyesId = partId;
                    break;

                case AvatarPiece.Nose:
                    NoseId = partId;
                    break;

                case AvatarPiece.Mouth:
                    MouthId = partId;
                    break;

                case AvatarPiece.Clothes:
                    BodyId = partId;
                    break;

                case AvatarPiece.Hands:
                    HandsId = partId;
                    break;

                case AvatarPiece.Feet:
                    FeetId = partId;
                    break;

                default:
                    Debug.LogWarning($"Invalid AvatarPiece slot {slot}");
                    break;
            }
        }

        public string GetPartId(AvatarPiece feature)
        {
            switch (feature)
            {
                case AvatarPiece.Hair:
                    return HairId;
                case AvatarPiece.Eyes:
                    return EyesId;
                case AvatarPiece.Nose:
                    return NoseId;
                case AvatarPiece.Mouth:
                    return MouthId;
                case AvatarPiece.Clothes:
                    return BodyId;
                case AvatarPiece.Hands:
                    return HandsId;
                case AvatarPiece.Feet:
                    return FeetId;
                default:
                    return "";
            }
        }
    }
}
