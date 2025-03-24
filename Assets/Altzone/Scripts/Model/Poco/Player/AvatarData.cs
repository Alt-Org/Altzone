using System.Collections.Generic;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Player
{
    public class AvatarData
    {
        public AvatarData(string name, List<string> featureIds, List<string> colors, Vector2 scale)
        {
            Name = (string)name.Clone();
            FeatureIds = new(featureIds);
            Colors = new(colors);
            Scale = new(scale.x, scale.y);
        }

        public string Name;
        public List<string> FeatureIds;
        public List<string> Colors;
        public Vector2 Scale;
    }
}
