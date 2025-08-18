using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JukeboxTrackButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _trackNameText;

    private MusicTrack _currentTrack = null;

    public delegate void TrackPressed(MusicTrack track);
    public event TrackPressed OnTrackPressed;

    private void Awake() { GetComponent<Button>().onClick.AddListener(() => ButtonClicked()); }

    public bool InUse() { return _currentTrack != null; }

    public void ButtonClicked() { if (_currentTrack != null) OnTrackPressed.Invoke(_currentTrack); }

    public void SetTrack(MusicTrack musicTrack)
    {
        _currentTrack = musicTrack;
        _trackNameText.text = musicTrack.Name;
        gameObject.SetActive(true);
    }

    public void Clear() { _currentTrack = null; gameObject.SetActive(false); }
}
