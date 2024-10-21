using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class PlayerAvatar
    {
        private string _characterName;
        private List<FeatureID> _features;
        private List<FeatureColor> _colors;
        private Vector2 _scale;

        public PlayerAvatar(string name, List<FeatureID> features, List<FeatureColor> colors, Vector2 scale)
        {
            _characterName = name;
            _features = features;
            _colors = colors;
            _scale = scale;
        }
        public string Name
        {
            get => _characterName;
            set => _characterName = value;
        }
        public List<FeatureID> Features{
            get => _features;
            set => _features = value;
        }
        public List<FeatureColor> Colors{
            get => _colors;
            set => _colors = value;
        }
        public Vector2 Scale{
            get => _scale;
            set => _scale = value;
        }
    }
}
