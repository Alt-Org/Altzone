using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(menuName = "ALT-Zone/AvatarDefaultData")]
    public class AvatarDefaultReference : ScriptableObject
    {
        [SerializeField] private List<AvatarDefaultClassCategoryInfo> _info;

        public AvatarDefaultPartInfo Get(string Id)
        {
            try
            {
                string[] ids = Id.Split("-");
                var data = _info.Find(characterClass => characterClass.Id == ids[0]).
                    Characters.Find(character => character.Id == ids[2].Substring(0,2)).Variations;

                return (data[0]);
            }
            catch
            {
                Debug.LogError($"Could not find avatar part with ID: {Id}");
                return (null);
            }
        }

        public List<string> GetStringList(string Id)
        {
            try
            {
                List<string> list = new List<string>();

                string[] ids = Id.Split("-");
                var data = _info.Find(characterClass => characterClass.Id == ids[0]).
                    Characters.Find(character => character.Id == ids[1].Substring(0, 3)).Variations;

                list.Add(data[0].HairId);
                list.Add(data[0].EyesId);
                list.Add(data[0].NoseId);
                list.Add(data[0].MouthId);
                list.Add(data[0].BodyId);
                list.Add(data[0].HandsId);
                list.Add(data[0].FeetId);

                return (list);
            }
            catch
            {
                Debug.LogError($"Could not find avatar part with ID: {Id}");
                return (null);
            }
        }

        [System.Serializable]
        public class AvatarDefaultPartInfo
        {
            public string Name = "Default";
            [Space]
            public string HairId;
            public string EyesId;
            public string NoseId;
            public string MouthId;
            public string BodyId;
            public string HandsId;
            public string FeetId;
        }

        [System.Serializable]
        public class AvatarDefaultCharacterInfo
        {
            public string Name;
            public string Id;
            public List<AvatarDefaultPartInfo> Variations;
        }

        [System.Serializable]
        public class AvatarDefaultClassCategoryInfo
        {
            public string Name;
            public string Id;
            public List<AvatarDefaultCharacterInfo> Characters;
        }
    }
}
