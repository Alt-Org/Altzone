using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                Color convertedColor; 
                if (ColorUtility.TryParseHtmlString(value, out convertedColor)) //If the colorcode provided is valid, assign it to the _color field
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

        public PlayerAvatar(string name, List<string> featuresIds, string color, Vector2 scale)
        {
            Name = name;
            foreach(string partId in featuresIds)
            {
                SortAndAssignByID(partId);
            }
            Color = color ?? "#ffffff";
            Scale = scale;
        }

        public PlayerAvatar(AvatarData data)
        {
            Name = data.Name ?? string.Empty;
            foreach(string partId in data.FeatureIds)
            {
                SortAndAssignByID(partId);
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

        public void SortAndAssignByID(string avatarPartId)
        {
            if(string.IsNullOrEmpty(avatarPartId)) {  return; }
            if(avatarPartId.Length  < 7) { return; }
            string identifier = avatarPartId.Substring(0, 2);
            switch(avatarPartId.Substring(0, 2)) //Check the first 2 characters from the partId
            {
                case "10":
                    HairId = avatarPartId;
                    break;
                case "21":
                    EyesId = avatarPartId;
                    break;
                case "22":
                    NoseId = avatarPartId;
                    break;
                case "23":
                    MouthId = avatarPartId;
                    break;
                case "31":
                    BodyId = avatarPartId;
                    break;
                case "32":
                    HandsId = avatarPartId;
                    break;
                case "33":
                    FeetId = avatarPartId;
                    break;
                default:
                    Debug.LogWarning($"Avatar part with id {avatarPartId} could not be sorted into any category!");
                    break;
            }
        }

        public string GetPartId(FeatureSlot feature)
        {
            switch (feature)
            {
                case FeatureSlot.Hair:
                    return HairId;
                case FeatureSlot.Eyes:
                    return EyesId;
                case FeatureSlot.Nose:
                    return NoseId;
                case FeatureSlot.Mouth:
                    return MouthId;
                case FeatureSlot.Body:
                    return BodyId;
                case FeatureSlot.Hands:
                    return HandsId;
                case FeatureSlot.Feet:
                    return FeetId;
                default:
                    return "";
            }
        }
    }
}
