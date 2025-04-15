using System;
using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    public static JukeBoxSoulhomeHandler Instance;

    public AudioSource audioSource;
    public Image diskImage;
    public Transform diskTransform;

    public JukeboxSong[] songDisks;

    public float rotationSpeed = 100f;

    [SerializeField]
    private GameObject _jukeboxObject;
    [SerializeField]
    private Button _backButton;
    [SerializeField]
    private TextMeshProUGUI _songName;

    [Header("songlist")]
    public Transform songlistContent;
    public GameObject jukeboxButtonprefab;
    [Header("Queuelist")]
    public Transform QueueContent;
    [SerializeField]private GameObject Queuetextprefab;

    private bool isMainMenuMode = false;
    private Coroutine _diskSpinCoroutine;

    private void Awake()
    {
        Instance = this;
    }

     void Start()
    {
        JukeboxController.OnChangeJukeBoxSong += SetSongInfo;
        JukeboxController.OnChangeJukeBoxQueue += UpdateQueueText;
        foreach (var song in songDisks) 
        { 
            GameObject jukeboxObject = Instantiate(jukeboxButtonprefab, songlistContent);
            jukeboxObject.GetComponent<Button>().onClick.AddListener(()=> PlaySongByIndex(song));
            jukeboxObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = song.songName;
        }
        _backButton.onClick.AddListener(()=> ToggleJokeBoxScreen(false));
        SetSongInfo(AudioManager.Instance.JukeBoxCurrentSong);
        UpdateQueueText(AudioManager.Instance.JukeBoxQueue);
    }

    private IEnumerator SpinDisk()
    {
        diskTransform.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
        yield return null;
    }

    public void ToggleJokeBoxScreen(bool toggle)
    {
        _jukeboxObject.SetActive(toggle);
    }

    public void PlaySongByIndex(JukeboxSong song)
    {
        if (isMainMenuMode) return;

        AudioManager.Instance.Jukebox.PlaySongByIndex(song);
    }

    private void SetSongInfo(JukeboxSong song)
    {
        if (song == null) return;
        _songName.text = song.songName;
        diskImage.sprite = song.songDisks;

        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
            diskTransform.rotation = Quaternion.identity;
        }
        _diskSpinCoroutine = StartCoroutine(SpinDisk());
    }

    private void UpdateQueueText(Queue<JukeboxSong> songQueue) 
    {
        foreach(Transform queueText in QueueContent) 
        {
            Destroy(queueText.gameObject);
        }
        foreach (JukeboxSong song in songQueue)
        {
            GameObject jukeboxObject = Instantiate(Queuetextprefab, QueueContent);
            jukeboxObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = song.songName;
        }
    }

    public void StopAndGoToMainMenu()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.JukeBoxQueue.Clear();
        isMainMenuMode = true;
        diskImage.sprite = null;
    }

    public void ExitMainMenuMode()
    {
        isMainMenuMode = false;
    }
}
