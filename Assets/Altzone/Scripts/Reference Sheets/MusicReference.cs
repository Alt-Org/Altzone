using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "MusicReference", menuName = "ScriptableObjects/MusicReferenceScriptableObject")]
public class MusicReference : ScriptableObject
{
    [SerializeField] private List<MusicCategory> _musicCategories = new List<MusicCategory>();

    public List<MusicCategory> MusicCategories { get =>  _musicCategories; }

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

    public MusicTrack GetTrack(string categoryName, string trackName)
    {
        foreach (MusicCategory category in _musicCategories)
            if (category.Name.ToLower() == categoryName.ToLower())
                foreach (MusicTrack track in category.MusicTracks)
                    if (track.Name.ToLower() == trackName.ToLower())
                        return track;

        return null;
    }
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

    public MusicTrack Get(int id)
    {
        if (id < 0 || id >= MusicTracks.Count) return null;

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
    public int Id;
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
