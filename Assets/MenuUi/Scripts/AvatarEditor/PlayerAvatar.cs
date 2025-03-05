using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class PlayerAvatar
    {
        private string _characterName;
        private List<FeatureID> _features;
        private List<string> _colors;
        private Vector2 _scale;

        public PlayerAvatar(List<FeatureID> featureIds)
        {
            _characterName = "";
            _features = featureIds;
            _colors = new List<string>();
            _scale = Vector2.one;
        }

        public PlayerAvatar(string name, List<FeatureID> features, List<string> colors, Vector2 scale)
        {
            _characterName = name;
            _features = features;
            _colors = colors;
            _scale = scale;
        }

        public PlayerAvatar(AvatarData data)
        {
            _characterName = data.Name;
            _features = ToFeaturesListEnum(data.Features);
            _colors = data.Colors;
            _scale = data.Scale;
        }

        private List<FeatureID> ToFeaturesListEnum(List<int> indexes)
        {
            List<FeatureID> tempList = new List<FeatureID>();

            foreach (var index in indexes)
                tempList.Add((FeatureID)index);

            return (tempList);
        }

        public List<int> ToFeaturesListInt(List<FeatureID> featureIds)
        {
            List<int> tempList = new List<int>();

            foreach (var featureId in featureIds)
                tempList.Add((int)featureId);

            return (tempList);
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
        public List<string> Colors{
            get => _colors;
            set => _colors = value;
        }
        public Vector2 Scale{
            get => _scale;
            set => _scale = value;
        }
    }
}
