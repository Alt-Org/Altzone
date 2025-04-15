using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBoxSoulhomeHandler : MonoBehaviour
{
    public static JukeBoxSoulhomeHandler Instance;

    public AudioSource audioSource;
    public Image diskImage;
    public Sprite noDisk;
    public Transform diskTransform;

    public JukeboxSong[] songDisks;

    public float rotationSpeed = 100f;

    [SerializeField]
    private GameObject _jokeboxObject;
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

    

    private Queue<JukeboxSong> songQueue = new();
    private JukeboxSong _currentSong;
    private bool isMainMenuMode = false;
    private Coroutine _diskSpinCoroutine;
    private Coroutine _waitSongEndCoroutine;

    private void Awake()
    {
        Instance = this;
    }

     void Start()
    {
        foreach (var song in songDisks) 
        { 
            GameObject jukeboxObject = Instantiate(jukeboxButtonprefab, songlistContent);
            jukeboxObject.GetComponent<Button>().onClick.AddListener(()=> PlaySongByIndex(song));
            jukeboxObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = song.songName;
        }
        _backButton.onClick.AddListener(()=> ToggleJokeBoxScreen(false));
    }

    private IEnumerator WaitUntilSongEnd()
    {
        yield return new WaitWhile(() => (audioSource.time > 0));

        CheckIfSongInQueue();
    }

    private void CheckIfSongInQueue()
    {
        if (songQueue.Count == 0 )
        {
            diskImage.sprite = noDisk;
        }
        else
        {
            if (_diskSpinCoroutine != null)
            {
                StopCoroutine(_diskSpinCoroutine);
                _diskSpinCoroutine = null;
                diskTransform.rotation = Quaternion.identity;
            }
            PlayNextSongInQueue();
            _diskSpinCoroutine = StartCoroutine(SpinDisk());
        }
    }

    private IEnumerator SpinDisk()
    {
        diskTransform.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
        yield return null;
    }

    public void ToggleJokeBoxScreen(bool toggle)
    {
        _jokeboxObject.SetActive(toggle);
    }

    public void PlaySongByIndex(JukeboxSong song)
    {
        if (isMainMenuMode) return;

        if (!audioSource.isPlaying && songQueue.Count == 0)
        {
            StartSong(song);
        }
        else
        {
            songQueue.Enqueue(song);
        }
        updateQueuetext();
    }

    private void StartSong(JukeboxSong song)
    {
        //currentSongIndex = index;
        _currentSong = song;
        audioSource.clip = song.songs;
        audioSource.Play();
        _songName.text = song.songName;

        diskImage.sprite = song.songDisks;
        if (_waitSongEndCoroutine != null)
        {
            StopCoroutine(_waitSongEndCoroutine);
            _waitSongEndCoroutine = null;
        }
        _waitSongEndCoroutine = StartCoroutine(WaitUntilSongEnd());
        if(_diskSpinCoroutine == null) _diskSpinCoroutine = StartCoroutine(SpinDisk());
    }

    private void PlayNextSongInQueue()
    {
        if (songQueue.Count == 0) return;
        StartSong(songQueue.Dequeue());
        updateQueuetext();
    }

    private void updateQueuetext() 
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
        audioSource.Stop();
        songQueue.Clear();
        isMainMenuMode = true;
        diskImage.sprite = null;
    }

    public void ExitMainMenuMode()
    {
        isMainMenuMode = false;
    }
    [Serializable]
    public class JukeboxSong 
    {
        public string songName;
        public AudioClip songs;
        public Sprite songDisks;
    }
}
