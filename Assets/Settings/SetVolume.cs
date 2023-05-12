using UnityEngine;

public class SetVolume : MonoBehaviour
{
    [SerializeField] SettingsCarrier.SoundType _soundType;
    private void Start()
    {
        VolumeSet();
    }

    public void VolumeSet()
    {
        // Gets the wanted volume from SettingsCarrier
        gameObject.GetComponent<AudioSource>().volume = SettingsCarrier.Instance.SentVolume(_soundType);
    }
}
