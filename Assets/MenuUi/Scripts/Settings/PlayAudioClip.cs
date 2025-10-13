using Altzone.Scripts.Audio;
//using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum PlayType
{
    OnClick = 0,
    OnPointerDown = 1,
    Both = 2,
}

public class PlayAudioClip : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] string _audioCategory = "";
    [SerializeField] string _audioName = "";
    //[SerializeField] SoundPlayType _audioPlayType = SoundPlayType.Default;
    [SerializeField, Tooltip("Determines which event to use in order to play the audio clip. OnClick: sound will play when the button is released, OnPointerDown: sound will play when button press is started, Both: sound will play at both of these events")]
    private PlayType _playType = PlayType.OnClick;

    private void Start()
    {
        switch (_playType)
        {
            case PlayType.OnClick:
            case PlayType.Both:
                GetComponent<Button>()?.onClick.AddListener(PlayAudio);
                break;
        }

        GetComponent<Toggle>()?.onValueChanged.AddListener(PlayAudio);
    }

    //Since we use editor calls we omit this function on build time
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void Reset()
    {
        Button source = GetComponent<Button>();
        Toggle light = GetComponent<Toggle>();

#if UNITY_EDITOR
        if (source == null && light == null)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Choose a Component", "You are missing one of the required componets. Please choose one to add", "Button", "Toggle"))
                gameObject.AddComponent<Button>();
            else
                gameObject.AddComponent<Toggle>();
        }
#endif
    }

    public void PlayAudio(bool value) => PlayAudio();
    public void PlayAudio()
    {
        AudioManager manager = AudioManager.Instance;

        if (manager == null)
        {
            Debug.LogError("Cannot find audio manager. Check if AudioManager is added to the scene.");
            return;
        }

        manager.PlaySfxAudio(_audioCategory, _audioName);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (_playType)
        {
            case PlayType.OnPointerDown:
            case PlayType.Both:
                PlayAudio();
                break;
        }
    }
}
