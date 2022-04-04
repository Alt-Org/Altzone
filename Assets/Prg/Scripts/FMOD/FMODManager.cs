using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;



namespace Prg.Scripts.Service.FMOD
{
    /// <summary>
    /// AudioManager using FMOD
    /// </summary>
    public class FMODManager : MonoBehaviour, IFMODManager

    {
        public StudioListener studioListener = new StudioListener();
        public StudioEventEmitter eventEmitter;
        [FMODUnity.EventRef]
        public string PlayerStateEvent = "event:/the-cradle-of-your-soul-15700";
        [FMODUnity.EventRef]
        public string HealEvent = "";





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
            gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
            //eventEmitter = gameObject.GetComponent<FMODUnity.StudioEventEmitter>();
        }

        public void PlayBGM()
        {
            //   FMODUnity.RuntimeManager.AddListener();
        }

        public void GetStudioListener()
        {
            studioListener = FindObjectOfType<FMODUnity.StudioListener>();
            if (studioListener == null)
            {

                UnityExtensions.CreateGameObjectAndComponent<FMODUnity.StudioListener>(nameof(FMODUnity.StudioListener), true);

            }


        }

        public void Test()
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/the-cradle-of-your-soul-15700");

        }

        private void EventEmitter()
        {
            eventEmitter = GetComponent<FMODUnity.StudioEventEmitter>();
            if (eventEmitter == null)
            {

                UnityExtensions.CreateGameObjectAndComponent<FMODUnity.StudioEventEmitter>(nameof(FMODUnity.StudioEventEmitter), true);

            }

        }

        public void PlaySound(string name) { }

        public void StartBGM()
        {
            throw new NotImplementedException();
        }

        public void StopBGM()
        {
            throw new NotImplementedException();
        }

        public void SetBGMIntensity(int intensity)
        {
            throw new NotImplementedException();
        }


        /*public static IFMODManager Get()
         {
             var fMODManager = FindObjectOfType<FMODManager>();
             if (fMODManager == null)
             {

                 UnityExtensions.CreateGameObjectAndComponent<FMODManager>(nameof(FMODManager), true);

             }
             */
    }

}
