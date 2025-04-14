using System;
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
    private int? currentSongIndex = null;
    private bool isMainMenuMode = false;
    private bool isDiskSpinning = false;

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

    private void Update()
    {
        if (!audioSource.isPlaying && songQueue.Count > 0 && !isMainMenuMode)
        {
            PlayNextSongInQueue();
        }

        if (!audioSource.isPlaying && songQueue.Count == 0 && !isMainMenuMode)
        {
            isDiskSpinning = false;
            diskImage.sprite = noDisk;
        }

        if (isDiskSpinning)
        {
            diskTransform.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
        }

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
        audioSource.clip = song.songs;
        audioSource.Play();
        _songName.text = song.songName;

        diskImage.sprite = song.songDisks;
        isDiskSpinning = true;
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
        isDiskSpinning = false;
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
