using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace MenuUi.Scripts.Loader
{
    public class IntroVideoHandler : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayer _player;
        [SerializeField]
        private SceneDef _loaderscene;
        [SerializeField]
        private SceneDef _menuscene;

        private bool _videoPlaying = false;

        public event SignalBus.VideoEndHandler OnVideoEnd;

        // Start is called before the first frame update
        void Start()
        {
            EnhancedTouchSupport.Enable();
        }

        // Update is called once per frame
        void Update()
        {
            if (_videoPlaying)
            {
                if (ClickStateHandler.GetClickState() is ClickState.End)
                {
                    Debug.Log($"Skip video");
                    EndIntroVideo();
                }

            }
        }

        private void OnEnable()
        {
            _player.loopPointReached += CheckOver;
            PlayIntroVideo();
        }

        private void OnDisable()
        {
            if (_videoPlaying) EndIntroVideo();
            _player.loopPointReached -= CheckOver;
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
                Debug.Log($"End video");
                gameObject.SetActive(false);
                SignalBus.OnVideoEndSignal();
            }
            if (SceneManager.GetActiveScene().name == _menuscene.SceneName) WindowManager.Get().GoBack();
        }

        
        void CheckOver(VideoPlayer vp)
        {
            EndIntroVideo();
        }

    }
}
