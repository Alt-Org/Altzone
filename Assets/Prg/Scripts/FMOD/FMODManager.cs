using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Prg.Scripts.FMODManager.FMODManager
{
    /// <summary>
    /// AudioManager using FMOD
    /// </summary>
    public class FMODManager : MonoBehaviour, IFMODManager

    {
        public StudioListener studioListener = new StudioListener();
        public StudioEventEmitter eventEmitter;
        EventReference shielStateEvent;
        EventInstance shieldState;
        EventReference musicStateEvent;
        EventInstance musicState;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {

            var fMODManager = FindObjectOfType<FMODManager>();
            if (fMODManager == null)
            {

                UnityExtensions.CreateGameObjectAndComponent<FMODManager>(nameof(FMODManager), true);

            }



        }

        public void Start()
        {
            //gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
        }

        public void GetStudioListener()
        {
            studioListener = FindObjectOfType<StudioListener>();
            if (studioListener == null)
            {

                UnityExtensions.CreateGameObjectAndComponent<StudioListener>(nameof(StudioListener), true);

            }


        }


        public void GetEventEmitter()
        {
            eventEmitter = GetComponent<StudioEventEmitter>();
            if (eventEmitter == null)
            {

                UnityExtensions.CreateGameObjectAndComponent<StudioEventEmitter>(nameof(StudioEventEmitter), true);

            }

        }

        public void PlaySound(string name)
        {
            RuntimeManager.PlayOneShot(name);
        }

        public void StartBGM()
        {
            musicState = RuntimeManager.CreateInstance(musicStateEvent);
            musicState.start();
        }

        public void StopBGM()
        {
            musicState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicState.release();
        }

        public void SetBGMIntensity(PARAMETER_ID id,float intensity, bool ignoreseekspeed)
        {
            musicState.setParameterByID(id, intensity);
        }

        public void SetShieldLevel(PARAMETER_ID id, float intensity, bool ignoreseekspeed)
        {
            shieldState.setParameterByID(id, intensity);
        }
    }

}
