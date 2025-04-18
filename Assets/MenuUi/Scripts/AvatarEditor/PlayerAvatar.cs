using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class PlayerAvatar
    {
        private string _characterName;
        private List<string> _features;
        private List<string> _colors;
        private Vector2 _scale;

        public PlayerAvatar(AvatarDefaultReference.AvatarDefaultPartInfo featureIds)
        {
            _features = new List<string>();
            _features.Add(featureIds.HairId);
            _features.Add(featureIds.EyesId);
            _features.Add(featureIds.NoseId);
            _features.Add(featureIds.MouthId);
            _features.Add(featureIds.BodyId);
            _features.Add(featureIds.HandsId);
            _features.Add(featureIds.FeetId);

            _characterName = "";
            _colors = new List<string>() { "#ffffff" };
            _scale = Vector2.one;
        }

        public PlayerAvatar(string name, List<string> featuresIds, List<string> colors, Vector2 scale)
        {
            _characterName = name;
            _features = featuresIds;
            _colors = colors;
            _scale = scale;
        }

        public PlayerAvatar(AvatarData data)
        {
            //Debug.LogError(data == null);
            //Debug.LogError(data.Name == null);
            _characterName = (string)data.Name.Clone();
            _features = new(data.FeatureIds);
            _colors = new(data.Colors);
            _scale = new(data.ScaleX, data.ScaleY);
        }

        public string Name
        {
            get => _characterName;
            set => _characterName = value;
        }
        public List<string> FeatureIds{
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
