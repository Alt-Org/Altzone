using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(fileName = "ArtistReference", menuName = "ScriptableObjects/ArtistReferenceScriptableObject")]
    public class ArtistReference : ScriptableObject
    {
        public string Name;
        public Sprite Logo;
        public string WebsiteName;
        public string WebsiteAddress;
    }
}
