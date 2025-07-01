using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    //public static JukeBoxSoulhomeHandler Instance;

    [Header("Disk")]
    [SerializeField] private Image _diskImage;
    [SerializeField] private Transform _diskTransform;
    [SerializeField] private float _diskRotationSpeed = 100f;

    [Header("Other")]
    [SerializeField] private GameObject _jukeboxObject;
    [SerializeField] private Button _backButton;
    [SerializeField] private TextMeshProUGUI _songName;

    [Header("Songlist")]
    [SerializeField] private Transform _songListContent;
    [SerializeField] private GameObject _jukeboxButtonPrefab;

    [Header("Queuelist")]
    [SerializeField] private Transform _queueContent;
    [SerializeField] private GameObject _queueTextPrefab;

    private bool _isMainMenuMode = false;
    private Coroutine _diskSpinCoroutine;

    public bool JukeBoxOpen { get => _jukeboxObject.activeSelf; }

    //private void Awake()
    //{
    //    if (Instance == null)
    //        Instance = this;
    //}

    void Start()
    {
        foreach (var song in AudioManager.Instance.JukeBoxSongs) 
        { 
            GameObject jukeboxObject = Instantiate(_jukeboxButtonPrefab, _songListContent);
            jukeboxObject.GetComponent<Button>().onClick.AddListener(() => PlaySong(song));
            jukeboxObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = song.Name;
        }

        _backButton.onClick.AddListener(() => ToggleJukeboxScreen(false));
    }

    private void OnEnable()
    {
        JukeboxController.OnChangeJukeBoxSong += SetSongInfo;
        JukeboxController.OnChangeJukeBoxQueue += UpdateQueueText;
        SetSongInfo(AudioManager.Instance.JukeBoxCurrentSong);
        UpdateQueueText(AudioManager.Instance.JukeBoxQueue);
    }

    private void OnDisable()
    {
        JukeboxController.OnChangeJukeBoxSong -= SetSongInfo;
        JukeboxController.OnChangeJukeBoxQueue -= UpdateQueueText;
    }

    private IEnumerator SpinDisk()
    {
        while (true)
        {
            _diskTransform.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void ToggleJukeboxScreen(bool toggle)
    {
        _jukeboxObject.SetActive(toggle);
    }

    public void PlaySong(JukeboxSong song)
    {
        if (_isMainMenuMode) return;

        AudioManager.Instance.Jukebox.PlaySong(song);
    }

    private void SetSongInfo(JukeboxSong song)
    {
        if (song == null) return;

        _songName.text = song.Name;
        _diskImage.sprite = song.Disk;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            _diskTransform.rotation = Quaternion.identity;
        }

        if (song.Song != null) _diskSpinCoroutine = StartCoroutine(SpinDisk());
    }

    private void UpdateQueueText(Queue<JukeboxSong> songQueue) 
    {
        foreach(Transform queueText in _queueContent) Destroy(queueText.gameObject);

        foreach (JukeboxSong song in songQueue)
        {
            GameObject jukeboxObject = Instantiate(_queueTextPrefab, _queueContent);
            jukeboxObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = song.Name;
        }
    }

    public void StopJukebox()
    {
        AudioManager.Instance.StopMusic();
        _diskImage.sprite = null;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            _diskTransform.rotation = Quaternion.identity;
        }
    }

    public void ExitMainMenuMode()
    {
        _isMainMenuMode = false;
    }
}
