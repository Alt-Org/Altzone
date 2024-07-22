using System.Collections;
using System.Collections.Generic;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Video;

namespace MenuUi.Scripts.Loader
{
    public class IntroVideoHandler : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayer _player;

        private bool _videoPlaying = false;

        public event SignalBus.VideoEndHandler OnVideoEnd;

        // Start is called before the first frame update
        void Start()
        {
            _player.loopPointReached += CheckOver;
            EnhancedTouchSupport.Enable();
        }

        // Update is called once per frame
        void Update()
        {
            if (_videoPlaying)
            {
                if (ClickStateHandler.GetClickState() is ClickState.End)
                    EndIntroVideo();

            }
        }

        private void OnEnable()
        {
            PlayIntroVideo();
        }

        private void OnDisable()
        {
            if (_videoPlaying) EndIntroVideo();
        }

        public void PlayIntroVideo()
        {
            gameObject.SetActive(true);
            Debug.Log($"Play video");
            _player.Play();
            _videoPlaying = true;

        }

        public void EndIntroVideo()
        {
            if (_videoPlaying)
            {
                _player.Stop();
                _videoPlaying = false;
                Debug.Log($"Skip video");
                gameObject.SetActive(false);
                SignalBus.OnVideoEndSignal();
            }
        }

        void CheckOver(VideoPlayer vp)
        {
            _videoPlaying = false;
            Debug.Log($"End video");
            gameObject.SetActive(false);
            SignalBus.OnVideoEndSignal();
        }

    }
}
