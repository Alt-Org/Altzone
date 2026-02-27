using UnityEngine;

public class SetVolume : MonoBehaviour
{
    [SerializeField] private bool _useAudioSourceBaseVolume = true;
    public SettingsCarrier.SoundType _soundType;

    private float _audioSourceBaseVolume;
    private void Start()
    {

    }

    private void OnEnable()
    {
        if (_useAudioSourceBaseVolume) _audioSourceBaseVolume = gameObject.GetComponent<AudioSource>().volume;
        if(SettingsCarrier.Instance != null) VolumeSet();
    }

    public void VolumeSet()
    {
        // Gets the wanted volume from SettingsCarrier
        if (_useAudioSourceBaseVolume) {
            gameObject.GetComponent<AudioSource>().volume = _audioSourceBaseVolume * SettingsCarrier.Instance.SentVolume(_soundType); }
        else gameObject.GetComponent<AudioSource>().volume = SettingsCarrier.Instance.SentVolume(_soundType);
    }
}
