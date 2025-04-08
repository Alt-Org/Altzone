using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Player
{
    [Serializable]
    public class AvatarData
    {
        public AvatarData(string name, List<string> featureIds, List<string> colors, Vector2 scale)
        {
            Name = (string)name.Clone();
            FeatureIds = new(featureIds);
            Colors = new(colors);
            ScaleX = scale.x;
            ScaleY = scale.y;
        }

        public string Name;
        public List<string> FeatureIds;
        public List<string> Colors;
        //public Vector2 Scale = new(ScaleX, ScaleY);
        public float ScaleX;
        public float ScaleY;

        public bool Validate()
        {
            if ((Name == null) ||
                (FeatureIds == null || FeatureIds.Count == 0) ||
                (Colors == null || Colors.Count == 0))
                return (false);

            return (true);
        }
    }
}
