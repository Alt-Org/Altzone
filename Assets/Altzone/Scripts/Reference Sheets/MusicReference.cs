using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "MusicReference", menuName = "ScriptableObjects/MusicReferenceScriptableObject")]
public class MusicReference : ScriptableObject
{
    [SerializeField] private List<MusicCategory> _musicCategories = new List<MusicCategory>();

    public List<MusicCategory> MusicCategories { get =>  _musicCategories; }

    public List<MusicTrack> Get(MusicType type)
    {
        List<MusicTrack> tracks = new List<MusicTrack>();

        foreach (MusicCategory category in _musicCategories)
            if (category.Type == type)
                tracks.AddRange(category.MusicTracks);

        return tracks;
    }

    public MusicTrack Get(string name)
    {
        foreach (MusicCategory category in _musicCategories)
            foreach (MusicTrack track in category.MusicTracks)
                if (track.Name == name)
                    return track;

        return null;
    }

    public MusicTrack Get(MusicType type, string name)
    {
        foreach (MusicCategory category in _musicCategories)
            if (category.Type == type)
                foreach (MusicTrack track in category.MusicTracks)
                    if (track.Name == name)
                        return track;

        return null;
    }
}

[System.Serializable]
public class MusicCategory
{
    public string Name;
    public MusicType Type;
    public List<MusicTrack> MusicTracks;
}

public enum MusicType
{
    MainMenu,
    Soulhome,
    Jukebox,
    Battle
}

[System.Serializable]
public class MusicTrack
{
    public string Name;
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

    [Header("Shop Data")]
    public int Price;
}
