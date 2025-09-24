using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles the visual queue item.
/// </summary>
public class JukeboxTrackQueueHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _trackNameText;

    private string _id; // Id that is used with JukeboxManager.
    public string Id { get { return _id; } }

    //Index of it self in the JukeboxManager's TrackQueue list.
    private int _linearIndex = -1;
    public int LinearIndex { get { return _linearIndex; } }

    private int _chunkIndex = 0;
    public int ChunkIndex {  get { return _chunkIndex; } }

    private int _poolIndex = 0;
    public int PoolIndex { get { return _poolIndex; } }

    private MusicTrack _currentTrack = null;
    public MusicTrack MusicTrack { get { return _currentTrack; } }

    /// <summary>
    /// Use when creating the gameobject that has this class. (Execute only once!)
    /// </summary>
    public void Setup(int chunkIndex, int poolIndex)
    {
        _chunkIndex = chunkIndex;
        _poolIndex = poolIndex;
        SetVisibility(false);
    }

    public bool InUse() { return _currentTrack != null; }

    public void SetTrack(string id, MusicTrack musicTrack, int linearIndex)
    {
        _id = id;
        _currentTrack = musicTrack;
        _linearIndex = linearIndex;

        if (musicTrack != null) _trackNameText.text = musicTrack.Name;

        SetVisibility(musicTrack != null);
    }

    public void SetLinearIndex(int index) { _linearIndex = index; }

    public void Clear() { _id = ""; _currentTrack = null; SetVisibility(false); }

    public void SetVisibility(bool visible) { gameObject.SetActive(visible); }
}
