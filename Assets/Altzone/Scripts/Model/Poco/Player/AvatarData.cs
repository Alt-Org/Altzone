﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Player
{
    public class AvatarData
    {
        public AvatarData(string name, List<int> features, List<string> colors, Vector2 scale)
        {
            Name = name;
            Features = features;
            Colors = colors;
            Scale = scale;
        }

        public string Name;
        public List<int> Features;
        public List<string> Colors;
        public Vector2 Scale;
    }
}
