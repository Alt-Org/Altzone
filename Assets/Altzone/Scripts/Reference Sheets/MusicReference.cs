using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Interface;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(fileName = "MusicReference", menuName = "ScriptableObjects/MusicReferenceScriptableObject")]
    public class MusicReference : ScriptableObject
    {
        [SerializeField] private List<MusicCategory> _musicCategories = new List<MusicCategory>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
            _hasInstance = false;
        }

        private static MusicReference _instance;
        private static bool _hasInstance;

        public static MusicReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<MusicReference>($"Audio/MusicDataReference");
                    _hasInstance = _instance != null;
                }
                return _instance;
            }
        }

        public List<MusicCategory> MusicCategories { get => _musicCategories; }

        public MusicCategory GetCategory(string categoryName)
        {
            foreach (MusicCategory category in _musicCategories)
                if (category.Name.ToLower() == categoryName.ToLower())
                    return category;

            return null;
        }

        public List<MusicTrack> GetTracksOfCategoryName(string categoryName)
        {
            foreach (MusicCategory category in _musicCategories)
                if (category.Name.ToLower() == categoryName.ToLower())
                    return category.MusicTracks;

            return null;
        }

        public List<string> ConvertToStringList(List<MusicTrack> tracks)
        {
            List<string> list = new List<string>();

            foreach (MusicTrack track in tracks)
                list.Add(track.Name);

            return list;
        }

        public MusicTrack GetTrack(string categoryName, string trackName)
        {
            foreach (MusicCategory category in _musicCategories)
                if (category.Name.ToLower() == categoryName.ToLower())
                    foreach (MusicTrack track in category.MusicTracks)
                        if (track.Name.ToLower() == trackName.ToLower())
                            return track;

            return null;
        }

        #region ISelectionBoxFetchable

        public List<string> GetStringList(string listName) { return ConvertToStringList(GetTracksOfCategoryName(listName)); }

        #endregion
    }

    [System.Serializable]
    public class MusicCategory
    {
        public string Name;
        public List<MusicTrack> MusicTracks;

        public MusicTrack Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return MusicTracks[0];

            foreach (MusicTrack track in MusicTracks)
                if (track.Name.ToLower() == name.ToLower())
                    return track;

            return null;
        }

        public MusicTrack GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (MusicTrack track in MusicTracks)
                if (track.Id == id)
                    return track;

            return null;
        }
    }

    [System.Serializable]
    public class MusicTrack
    {
        public string Name;
        public string Id;
        public AudioClip Music;
        public MusicTrackInfo Info;
    }

    [System.Serializable]
    public class MusicTrackInfo
    {
        [Header("Jukebox")]
        public Sprite Disk;
        public string Genre;
        public string SubGenre;

        [Header("Artist Info")]
        public string ArtistName;
        public Sprite ArtistLogo;
    }
}
