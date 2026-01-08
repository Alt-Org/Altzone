using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Audio;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(fileName = "MusicReference", menuName = "ScriptableObjects/MusicReferenceScriptableObject")]

    public enum MusicGenreType
    {
        None,
        Pop,
        Electronic,
        Jazz,
        Blues,
        Reggae,
        RAndB,
        HipHop,
        Rock,
        Metal,
        Punk,
        Folk,
        Country,
        Latin,
        Instrumental,
        NewAge,
        Indie,
        Classical
    }

    public enum MusicMoodType
    {
        None,
        Happy,
        Hopeful,
        Cheerful,
        Excited,
        Warm,
        Love,
        Romantic,
        Bright,
        Goofy,
        Calm,
        Sad,
        Angry,
        Aggressive,
        Dark,
        Cold,
        Gloomy,
        Tense,
        Stressed,
        Desperate,
        Defeated,
        Fearful,
        Cautious,
        Panic,
        Uneasy,
        Awe,
        Bored,
        Sorrow,
    }

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

        public MusicCategory GetCategory(AudioCategoryType categoryType)
        {
            foreach (MusicCategory category in _musicCategories)
                if (category.Type == categoryType)
                    return category;

            return null;
        }

        public List<MusicTrack> GetTracksOfCategoryName(string categoryName)
        {
            MusicCategory category = GetCategory(categoryName);

            if (category != null)
                return category.MusicTracks;
            else
                return null;
        }

        public List<MusicTrack> GetTracksOfCategoryName(AudioCategoryType categoryType)
        {
            MusicCategory category = GetCategory(categoryType);

            if (category != null)
                return category.MusicTracks;
            else
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
            MusicCategory category = GetCategory(categoryName);

            if (category == null) return null;

            foreach (MusicTrack track in category.MusicTracks)
                if (track.Name.ToLower() == trackName.ToLower())
                    return track;

            return null;
        }

        public MusicTrack GetTrack(AudioCategoryType categoryType, string trackName)
        {
            MusicCategory category = GetCategory(categoryType);

            if (category == null) return null;

            foreach (MusicTrack track in category.MusicTracks)
                if (track.Name.ToLower() == trackName.ToLower())
                    return track;

            return null;
        }

        public MusicTrack GetTrackById(string categoryName, string trackId)
        {
            MusicCategory category = GetCategory(categoryName);

            if (category == null) return null;

            foreach (MusicTrack track in category.MusicTracks)
                if (track.Id == trackId)
                    return track;

            return null;
        }

        public MusicTrack GetTrackById(AudioCategoryType categoryType, string trackId)
        {
            MusicCategory category = GetCategory(categoryType);

            if (category == null) return null;

            foreach (MusicTrack track in category.MusicTracks)
                if (track.Id == trackId)
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
        public AudioCategoryType Type;

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
        public JukeboxMusicTrackInfo JukeboxInfo;
    }

    [System.Serializable]
    public class JukeboxMusicTrackInfo
    {
        [Header("Jukebox")]
        public Sprite Disk;
        public MusicGenreType Genre;
        public MusicMoodType Mood;

        public List<ArtistInfo> Artists;

        public string GetArtistNames()
        {
            if (Artists.Count == 0) return "";

            string names = Artists[0].Name;

            for (int i = 1; i < Artists.Count; i++) names += ", " + Artists[i].Name;

            return names;
        }
    }

    [System.Serializable]
    public class ArtistInfo
    {
        public string Name;
        public ArtistReference Artist;
        public List<ArtistRoleType> Roles;

        public enum ArtistRoleType
        {
            Producer,
            Composer,
            Arrangement,
            Writer,
            Singer,
            Mixing,
        }
    }
}
