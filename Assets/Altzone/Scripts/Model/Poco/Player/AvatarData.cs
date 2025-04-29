using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Player
{
    [Serializable]
    public class AvatarData
    {
        public AvatarData(string name, List<string> featureIds, string color, Vector2 scale)
        {
            Name = (string)name.Clone();
            FeatureIds = new(featureIds);
            Color = new(color);
            ScaleX = scale.x;
            ScaleY = scale.y;
        }

        public AvatarData(string name, ServerAvatar data)
        {
            Name = (string)name.Clone();
            FeatureIds = new();
            FeatureIds.Add(data.hair.ToString());
            FeatureIds.Add(data.eyes.ToString());
            FeatureIds.Add(data.nose.ToString());
            FeatureIds.Add(data.mouth.ToString());
            FeatureIds.Add(data.clothes.ToString());
            FeatureIds.Add(data.feet.ToString());
            FeatureIds.Add(data.hands.ToString());
            Color = new(data.skinColor);
            ScaleX = 1;
            ScaleY = 1;
        }

        public string Name;
        public List<string> FeatureIds;
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
    }
}
