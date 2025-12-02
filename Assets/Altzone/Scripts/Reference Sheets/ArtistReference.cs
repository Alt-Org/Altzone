using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(fileName = "ArtistReference", menuName = "ScriptableObjects/ArtistReferenceScriptableObject")]
    public class ArtistReference : ScriptableObject
    {
        [SerializeField] string _name;
        public string Name { get { return _name; } }

        [SerializeField] private Sprite _logo;
        public Sprite Logo { get { return _logo; } }

        [SerializeField] private string _websiteName;
        public string WebsiteName { get { return _websiteName; } }

        [SerializeField] private string _websiteAddress;
        public string WebsiteAddress { get { return _websiteAddress; } }
    }
}
