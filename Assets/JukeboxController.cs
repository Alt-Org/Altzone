using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JukeboxController : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    public JukeboxSong[] songDisks;

    private Queue<JukeboxSong> songQueue = new();
    private JukeboxSong _currentSong;
    private bool isMainMenuMode = false;
    private Coroutine _waitSongEndCoroutine;
    private JukeboxSong _emptySong;

    public delegate void ChangeJukeBoxSong(JukeboxSong song);
    public static event ChangeJukeBoxSong OnChangeJukeBoxSong;

    public delegate void ChangeJukeBoxQueue(Queue<JukeboxSong> songQueue);
    public static event ChangeJukeBoxQueue OnChangeJukeBoxQueue;

    private void Start()
    {
        _emptySong = new JukeboxSong
        {
            songName = "NoName",
            songs = null,
            songDisks = null

        };
    }

    private IEnumerator WaitUntilSongEnd()
    {
        yield return new WaitWhile(() => (audioSource.time > 0));

        CheckIfSongInQueue();
    }

    private void CheckIfSongInQueue()
    {
        if (songQueue.Count == 0)
        {
            OnChangeJukeBoxSong?.Invoke(_emptySong);
        }
        else
        {
            PlayNextSongInQueue();
        }
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
        OnChangeJukeBoxQueue.Invoke(songQueue);
    }

    private void StartSong(JukeboxSong song)
    {
        _currentSong = song;
        audioSource.clip = song.songs;
        audioSource.Play();

        if (_waitSongEndCoroutine != null)
        {
            StopCoroutine(_waitSongEndCoroutine);
            _waitSongEndCoroutine = null;
        }
        _waitSongEndCoroutine = StartCoroutine(WaitUntilSongEnd());
        OnChangeJukeBoxSong?.Invoke(_currentSong);
    }

    private void PlayNextSongInQueue()
    {
        if (songQueue.Count == 0) return;
        StartSong(songQueue.Dequeue());
        OnChangeJukeBoxQueue.Invoke(songQueue);
    }

    public void ClearJokeBox()
    {
        audioSource.Stop();
        songQueue.Clear();
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
