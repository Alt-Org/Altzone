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

    private MusicTrack _musicTrack = null;
    public MusicTrack MusicTrack { get { return _musicTrack; } }

    /// <summary>
    /// Use when creating the gameobject that has this class. (Execute only once!)
    /// </summary>
    public void Setup(int chunkIndex, int poolIndex)
    {
        _chunkIndex = chunkIndex;
        _poolIndex = poolIndex;
        SetVisibility(false);
    }

    public bool InUse() { return !string.IsNullOrEmpty(_id); }

    public void SetTrack(string id, MusicTrack musicTrack, int linearIndex)
    {
        _id = id;
        _musicTrack = musicTrack;
        _linearIndex = linearIndex;

        if (musicTrack != null)
            _trackNameText.text = musicTrack.Name;
        else
            _trackNameText.text = "";

        Debug.LogError($"SetTrack: Id: {_id}, LinearIndex: {_linearIndex}, ChunkIndex: {_chunkIndex}, PoolIndex: {_poolIndex}");
    }

    public void SetLinearIndex(int index) { _linearIndex = index; }

    public void Clear() { _id = ""; _musicTrack = null; _trackNameText.text = ""; SetVisibility(false); }

    public void SetVisibility(bool visible) { gameObject.SetActive(visible); }

    public bool GetVisibility() { return gameObject.activeSelf; }
}
