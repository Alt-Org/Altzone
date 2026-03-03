using System;
using System.Linq;
using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
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

        private string _skinColor;
        private string _classColor;
        private string _hairColor;
        private string _eyesColor;
        private string _noseColor;
        private string _mouthColor;
        private string _clothesColor;
        private string _handsColor;
        private string _feetColor;
        public string SkinColor { get => _skinColor; set => _skinColor = CorrectColor(value); }
        public string ClassColor { get => _classColor; set => _classColor = CorrectColor(value); }
        public string HairColor { get => _hairColor; set => _hairColor = CorrectColor(value); }
        public string EyesColor { get => _eyesColor; set => _eyesColor = CorrectColor(value); }
        public string NoseColor { get => _noseColor; set => _noseColor = CorrectColor(value); }
        public string MouthColor { get => _mouthColor; set => _mouthColor = CorrectColor(value); }
        public string ClothesColor { get => _clothesColor; set => _clothesColor = CorrectColor(value); }
        public string HandsColor { get => _handsColor; set => _handsColor = CorrectColor(value); }
        public string FeetColor { get => _feetColor; set => _feetColor = CorrectColor(value); }

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
            SkinColor = "#ffffff";
            ClassColor = "#ffffff";
            HairColor = "#ffffff";
            EyesColor = "#ffffff";
            NoseColor = "#ffffff";
            MouthColor = "#ffffff";
            ClothesColor = "#ffffff";
            HandsColor = "#ffffff";
            FeetColor = "#ffffff";
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

            SkinColor = data.Color;
            ClassColor = GetClassColor();
            HairColor = data.HairColor;
            EyesColor = data.EyesColor;
            NoseColor = data.NoseColor;
            MouthColor = data.MouthColor;
            ClothesColor = data.ClothesColor;
            HandsColor = data.HandsColor;
            FeetColor = data.FeetColor;
            Scale = new Vector2(data.ScaleX, data.ScaleY);
        }

        private string GetClassColor()
        {
            CharacterClassType classId = BaseCharacter.GetClass((CharacterID)ServerManager.Instance.Player.currentAvatarId);
            Color classColor = ClassReference.Instance.GetColor(classId);
            return ColorUtility.ToHtmlStringRGBA(classColor);
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

        public void SetPartColor(AvatarPiece piece, string color)
        {
            switch (piece)
            {
                case AvatarPiece.Hair: HairColor = color; break;
                case AvatarPiece.Eyes: EyesColor = color; break;
                case AvatarPiece.Nose: NoseColor = color; break;
                case AvatarPiece.Mouth: MouthColor = color; break;
                case AvatarPiece.Clothes: ClothesColor = color; break;
                case AvatarPiece.Hands: HandsColor = color; break;
                case AvatarPiece.Feet: FeetColor = color; break;
                default: Debug.LogWarning($"Invalid AvatarPiece: {piece}"); break;
            }
        }

        public string GetPartColor(AvatarPiece piece)
        {
            switch (piece)
            {
                case AvatarPiece.Hair:
                    return HairColor;

                case AvatarPiece.Eyes:
                    return EyesColor;

                case AvatarPiece.Nose:
                    return NoseColor;

                case AvatarPiece.Mouth:
                    return MouthColor;

                case AvatarPiece.Clothes:
                    return ClothesColor;

                case AvatarPiece.Hands:
                    return HandsColor;

                case AvatarPiece.Feet:
                    return FeetColor;

                default:
                    Debug.LogError($"Invalid AvatarPiece: {piece}");
                    return string.Empty;
            }
        }

        public static string CorrectColor(string value)
        {
            string correctedValue = value ?? string.Empty;
            if (!correctedValue.StartsWith("#"))
            {
                correctedValue = "#" + correctedValue; ;
            }

            if (ColorUtility.TryParseHtmlString(correctedValue, out Color convertedColor)) //If the colorcode provided is valid, assign it to the _color field
            {
                return "#" + ColorUtility.ToHtmlStringRGBA(convertedColor);
            }
            else
            {
                Debug.LogWarning($"Submitted color {value}could not be parsed, using white");
                convertedColor = new Color(1, 1, 1, 1); //values for solid white
                return "#" + ColorUtility.ToHtmlStringRGBA(convertedColor);
            }
        }
    }
}
