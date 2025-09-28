using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JukeboxTrackButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _trackNameText;

    private int _trackLinearIndex = 0;

    private MusicTrack _currentTrack = null;
    public MusicTrack CurrentTrack {  get { return _currentTrack; } }

    [SerializeField] Button _addButton;

    //public delegate void TrackPressed(int startIndex);
    public delegate void TrackPressed(MusicTrack musicTrack);
    public event TrackPressed OnTrackPressed;

    private void Awake() { _addButton.onClick.AddListener(() => ButtonClicked()); }

    public bool InUse() { return _currentTrack != null; }

    //public void ButtonClicked() { if (_currentTrack != null) OnTrackPressed.Invoke(_trackLinearIndex); }
    public void ButtonClicked() { if (_currentTrack != null) OnTrackPressed.Invoke(_currentTrack); }

    public void SetTrack(MusicTrack musicTrack, int trackLinearIndex)
    {
        _trackLinearIndex = trackLinearIndex;
        _currentTrack = musicTrack;
        _trackNameText.text = musicTrack.Name;
        gameObject.SetActive(true);
    }

    public void Clear() { _currentTrack = null; gameObject.SetActive(false); }

    public void SetVisibility(bool value) { gameObject.SetActive(value); } 
}
