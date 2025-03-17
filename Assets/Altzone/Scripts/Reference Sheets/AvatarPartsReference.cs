using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{

    [CreateAssetMenu(menuName = "ALT-Zone/AvatarPartsData")]
    public class AvatarPartsReference : ScriptableObject
    {
        [SerializeField] private Sprite _head;
        [SerializeField] private List<AvatarPartCategoryInfo> _info;

        public int GetCategoryCount()
        {
            return (_info.Count);
        }

        public AvatarPartInfo GetAvatarPartById(string Id)
        {
            string[] ids = Id.Split("-");
            try
            {
                var data = _info.Find(category => category.Id == ids[0]).
                    AvatarCategories.Find(character => character.Id == ids[1]).
                    Parts.Find(part => part.Id == Id);

                return (data);
            }
            catch
            {
                Debug.LogError($"Could not find avatar part with ID: {Id}");
                return (null);
            }
        }

        public AvatarPartCategoryInfo GetPackedAvatarPartsByCategory(string Id)
        {
            string[] ids = Id.Split("-");

            var data = _info.Find(category => category.Id == ids[0]);

            if (data == null)
                Debug.LogError($"Could not find avatar parts category with ID: {Id}");

            return (data);
        }

        public List<AvatarPartInfo> GetUnpackedAvatarPartsByCategory(string Id)
        {
            string[] ids = Id.Split("-");

            var dataPacked = _info.Find(category => category.Id == ids[0]);

            if (dataPacked == null)
            {
                Debug.LogError($"Could not find avatar parts category with ID: {Id}");
                return (null);
            }

            List<AvatarPartInfo> avatarParts = new List<AvatarPartInfo>();

            foreach (AvatarClassCategoryInfo acci in dataPacked.AvatarCategories)
                foreach (AvatarPartInfo part in acci.Parts)
                    avatarParts.Add(part);

            return (avatarParts);
        }

        [System.Serializable]
        public class AvatarPartInfo
        {
            public string Name;
            public string Id;
            public string VisibleName;
            public Sprite AvatarImage;
            public Sprite IconImage;
        }

        [System.Serializable]
        public class AvatarClassCategoryInfo
        {
            public string Name;
            public string Id;
            public List<AvatarPartInfo> Parts;
        }

        [System.Serializable]
        public class AvatarPartCategoryInfo
        {
            public string SetName;
            public string Id;
            public List<AvatarClassCategoryInfo> AvatarCategories;
        }
    }
}
