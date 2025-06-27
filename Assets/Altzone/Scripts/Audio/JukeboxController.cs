using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class JukeboxController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private JukeboxSong[] _songs;
        [SerializeField] private Sprite _emptyDisk;

        private Queue<JukeboxSong> _songQueue = new();
        private JukeboxSong _currentSong;
        private bool _isMainMenuMode = false;
        private Coroutine _waitSongEndCoroutine;
        private JukeboxSong _emptySong;

        public Queue<JukeboxSong> SongQueue { get => _songQueue; }
        public JukeboxSong CurrentSong
        {
            get
            {
                if (_currentSong == null) return _emptySong;
                return _currentSong;
            }
        }
        public JukeboxSong[] Songs { get => _songs; }

        public delegate void ChangeJukeBoxSong(JukeboxSong song);
        public static event ChangeJukeBoxSong OnChangeJukeBoxSong;

        public delegate void ChangeJukeBoxQueue(Queue<JukeboxSong> songQueue);
        public static event ChangeJukeBoxQueue OnChangeJukeBoxQueue;

        private void Start()
        {
            _emptySong = new JukeboxSong
            {
                Name = "NoName",
                Song = null,
                Disk = _emptyDisk
            };
        }

        private IEnumerator WaitUntilSongEnd()
        {
            float songLength = _currentSong.Song.length;
            float songCurrentTime = 0f;

            while (songLength > songCurrentTime)
            {
                songCurrentTime += Time.deltaTime;
                yield return null;
            }

            CheckIfSongInQueue();
        }

        private void CheckIfSongInQueue()
        {
            if (_songQueue.Count == 0)
            {
                OnChangeJukeBoxSong?.Invoke(_emptySong);
                _currentSong = null;
                AudioManager.Instance.PlayMusic(MusicSection.SoulHome);
            }
            else
                PlayNextSongInQueue();
        }

        public void PlaySongByIndex(JukeboxSong song)
        {
            if (_isMainMenuMode)
                return;

            if ((!_audioSource.isPlaying && _songQueue.Count == 0) || _currentSong == null)
                StartSong(song);
            else
                _songQueue.Enqueue(song);

            OnChangeJukeBoxQueue.Invoke(_songQueue);
        }

        private void StartSong(JukeboxSong song)
        {
            _currentSong = song;
            _audioSource.clip = song.Song;
            _audioSource.Play();

            if (_waitSongEndCoroutine != null)
            {
                StopCoroutine(_waitSongEndCoroutine);
                _waitSongEndCoroutine = null;
            }

            _waitSongEndCoroutine = StartCoroutine(WaitUntilSongEnd());
            OnChangeJukeBoxSong?.Invoke(_currentSong);
        }

        public void ContinueSong()
        {
            if (_currentSong != null)
            {
                _audioSource.clip = _currentSong.Song;
                _audioSource.Play();
                _waitSongEndCoroutine = StartCoroutine(WaitUntilSongEnd());
            }

            OnChangeJukeBoxSong?.Invoke(_currentSong);
        }

        public void StopSong()
        {
            if (_waitSongEndCoroutine != null)
            {
                StopCoroutine(_waitSongEndCoroutine);
                _waitSongEndCoroutine = null;
            }

            _audioSource.Pause();
        }

        private void PlayNextSongInQueue()
        {
            if (_songQueue.Count == 0)
                return;

            StartSong(_songQueue.Dequeue());
            OnChangeJukeBoxQueue.Invoke(_songQueue);
        }

        public void ClearJokeBox()
        {
            _audioSource.Stop();
            _songQueue.Clear();
        }

        public void ExitMainMenuMode()
        {
            _isMainMenuMode = false;
        }
    }

    [Serializable]
    public class JukeboxSong
    {
        public string Name;
        public AudioClip Song;
        public Sprite Disk;
    }
}
