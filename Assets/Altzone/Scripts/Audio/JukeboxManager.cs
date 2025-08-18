using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class JukeboxManager : MonoBehaviour
    {
        public static JukeboxManager Instance { get; private set; }

        private Queue<TrackQueueData> _trackQueue = new Queue<TrackQueueData>();

        private MusicTrack _currentMusicTrack;
        public MusicTrack CurrentMusicTrack { get { return _currentMusicTrack; } }

        private Coroutine _trackEndingControlCoroutine;

        [SerializeField] private bool _loopLastTrack = true;

        #region Events & Delegates
        public delegate JukeboxTrackQueueHandler GetFreeJukeboxTrackQueueHandler();
        public event GetFreeJukeboxTrackQueueHandler OnGetFreeJukeboxTrackQueueHandler;

        public delegate void OptimizeVisualQueueChunks();
        public event OptimizeVisualQueueChunks OnOptimizeVisualQueueChunks;

        public delegate void ReduceQueueHandlerChunkActiveCount(int index);
        public event ReduceQueueHandlerChunkActiveCount OnReduceQueueHandlerChunkActiveCount;

        public delegate void SetSongInfo(MusicTrack track);
        public event SetSongInfo OnSetSongInfo;

        public delegate void StopJukeboxVisual();
        public event StopJukeboxVisual OnStopJukeboxVisual;
        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public string PlayTrack() { return PlayTrack(_currentMusicTrack); }

        public string PlayTrack(MusicTrack musicTrack)
        {
            if (_trackEndingControlCoroutine != null) return null;

            string name = AudioManager.Instance.PlayMusic("Jukebox", musicTrack);

            if (string.IsNullOrEmpty(name)) return null;

            if (OnSetSongInfo != null) OnSetSongInfo.Invoke(musicTrack);

            _currentMusicTrack = musicTrack;
            _trackEndingControlCoroutine = StartCoroutine(TrackEndingControl());

            return name;
        }

        public void StopJukebox()
        {
            if (_trackEndingControlCoroutine != null)
            {
                StopCoroutine(_trackEndingControlCoroutine);
                _trackEndingControlCoroutine = null;
            }
        }

        private IEnumerator TrackEndingControl()
        {
            float timer = 0f;

            while (timer < _currentMusicTrack.Music.length)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            PlayNextJukeboxTrack();
        }

        private void PlayNextJukeboxTrack()
        {
            if (_trackEndingControlCoroutine != null)
            {
                StopCoroutine(_trackEndingControlCoroutine);
                _trackEndingControlCoroutine = null;
            }

            if (_trackQueue.Count > 0) //Play the next track in queue.
            {
                TrackQueueData trackQueueHandler = _trackQueue.Peek();
                string name = PlayTrack(trackQueueHandler.MusicTrack);

                if (name == null) return;

                _trackQueue.Dequeue();

                if (OnReduceQueueHandlerChunkActiveCount != null)
                    OnReduceQueueHandlerChunkActiveCount.Invoke(trackQueueHandler.QueueHandler.ChunkIndex);

                trackQueueHandler.QueueHandler.Clear();
            }
            else if (_loopLastTrack) //Keep playing the current track.
            {
                PlayTrack(_currentMusicTrack);
            }
            else //Go back to latest requested music in AudioManager.
            {
                AudioManager manager = AudioManager.Instance;
                _currentMusicTrack = null;
                StopJukebox();
                if (!string.IsNullOrEmpty(manager.FallbackMusicCategory))
                    manager.PlayMusic(manager.FallbackMusicCategory, manager.FallbackMusicTrack);
                else
                    manager.PlayMusic("Soulhome", "");

                if (OnStopJukeboxVisual != null) OnStopJukeboxVisual.Invoke();
            }

            if (OnOptimizeVisualQueueChunks != null) OnOptimizeVisualQueueChunks.Invoke();
        }

        #region Queue
        public void QueueTrack(MusicTrack musicTrack)
        {
            if (_currentMusicTrack != null)
                AddToQueue(musicTrack);
            else
                PlayTrack(musicTrack);
        }

        private void AddToQueue(MusicTrack musicTrack)
        {
            TrackQueueData data = new();

            if (OnGetFreeJukeboxTrackQueueHandler != null)
            {
                data.QueueHandler = OnGetFreeJukeboxTrackQueueHandler.Invoke();
                data.QueueHandler.SetTrack(musicTrack);
            }

            data.MusicTrack = musicTrack;
            _trackQueue.Enqueue(data);
        }
        #endregion

        /// <summary>
        /// Reassembles the <c>_trackQueue</c> from visual jukebox queue. If not done after optimizing <c>_queueHandlerChunks</c>,
        /// the <c>_trackQueue</c> will point to empty <c>_JukeboxTrackQueueHandler</c>s.
        /// </summary>
        public void ReassembleDataQueue(List<Chunk<JukeboxTrackQueueHandler>> queueHandlerChunks)
        {
            _trackQueue.Clear();
            Debug.LogError("optimize");
            foreach (Chunk<JukeboxTrackQueueHandler> chunk in queueHandlerChunks)
                foreach (JukeboxTrackQueueHandler handler in chunk.Pool)
                    if (handler.InUse())
                    {
                        TrackQueueData data = new();
                        data.QueueHandler = handler;
                        data.MusicTrack = handler.CurrentTrack;
                        _trackQueue.Enqueue(data);
                    }
        }
    }

    public class TrackQueueData
    {
        public JukeboxTrackQueueHandler QueueHandler;
        public MusicTrack MusicTrack;
    }
}
