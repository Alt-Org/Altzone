using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class PlayerAvatar
    {
        public string Name { get; set; }
        public List<string> FeatureIds { get; set; }
        public string Color { get; set; }
        public Vector2 Scale { get; set; }

        public PlayerAvatar(AvatarDefaultReference.AvatarDefaultPartInfo featureIds)
        {
            FeatureIds = new List<string>
            {
                GetOrDefault(featureIds.HairId),
                GetOrDefault(featureIds.EyesId),
                GetOrDefault(featureIds.NoseId),
                GetOrDefault(featureIds.MouthId),
                GetOrDefault(featureIds.BodyId),
                GetOrDefault(featureIds.HandsId),
                GetOrDefault(featureIds.FeetId)
            };

            Name = string.Empty;
            Color = "#ffffff";
            Scale = Vector2.one;
        }

        public PlayerAvatar(string name, List<string> featuresIds, string color, Vector2 scale)
        {
            Name = name;
            FeatureIds = featuresIds ?? new List<string>();
            Color = color ?? "#ffffff";
            Scale = scale;
        }

        public PlayerAvatar(AvatarData data)
        {
            Name = data.Name ?? string.Empty;
            FeatureIds = new List<string>(data.FeatureIds ?? new List<string>());
            Color = data.Color ?? "#ffffff";
            Scale = new Vector2(data.ScaleX, data.ScaleY);
        }

        private static string GetOrDefault(string value) =>
            string.IsNullOrWhiteSpace(value) ? "0" : value;
    }
}
