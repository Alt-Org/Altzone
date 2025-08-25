using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class PlayerAvatar
    {
        private string _characterName;
        private List<string> _features;
        private string _color;
        private Vector2 _scale;

        public PlayerAvatar(AvatarDefaultReference.AvatarDefaultPartInfo featureIds)
        {
            _features = new List<string>();
            _features.Add(!string.IsNullOrWhiteSpace(featureIds.HairId) ? featureIds.HairId : "0");
            _features.Add(!string.IsNullOrWhiteSpace(featureIds.EyesId) ? featureIds.EyesId : "0");
            _features.Add(!string.IsNullOrWhiteSpace(featureIds.NoseId) ? featureIds.NoseId : "0");
            _features.Add(!string.IsNullOrWhiteSpace(featureIds.MouthId) ? featureIds.MouthId : "0");
            _features.Add(!string.IsNullOrWhiteSpace(featureIds.BodyId) ? featureIds.BodyId : "0");
            _features.Add(!string.IsNullOrWhiteSpace(featureIds.HandsId) ? featureIds.HandsId : "0");
            _features.Add(!string.IsNullOrWhiteSpace(featureIds.FeetId) ? featureIds.FeetId : "0");

            _characterName = "";
            _color = new string("#ffffff");
            _scale = Vector2.one;
        }

        public PlayerAvatar(string name, List<string> featuresIds, string color, Vector2 scale)
        {
            _characterName = name;
            _features = featuresIds;
            _color = color;
            _scale = scale;
        }

        public PlayerAvatar(AvatarData data)
        {
            //Debug.LogError(data == null);
            //Debug.LogError(data.Name == null);
            _characterName = (string)data.Name.Clone();
            _features = new(data.FeatureIds);
            _color = new(data.Color);
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
        public string Color{
            get => _color;
            set => _color = value;
        }
        public Vector2 Scale{
            get => _scale;
            set => _scale = value;
        }
    }
}
