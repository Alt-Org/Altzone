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

    private int _chunkIndex = 0;
    public int ChunkIndex {  get { return _chunkIndex; } }

    private int _poolIndex = 0;
    public int PoolIndex { get { return _poolIndex; } }

    private MusicTrack _currentTrack = null;
    public MusicTrack CurrentTrack { get { return _currentTrack; } }

    public void Setup(int chunkIndex, int poolIndex)
    {
        
        _chunkIndex = chunkIndex;
        _poolIndex = poolIndex;
        gameObject.SetActive(false);
    }

    public bool InUse() { return _currentTrack != null; }

    public void SetTrack(string id, MusicTrack musicTrack)
    {
        _id = id;
        _currentTrack = musicTrack;
        _trackNameText.text = musicTrack.Name;
        gameObject.SetActive(true);
    }

    public void Clear() { _id = ""; _currentTrack = null; gameObject.SetActive(false); }
}
