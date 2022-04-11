using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prg.Scripts.FMODManager.FMODManager
{
    public interface IFMODManager
    {

        /// <summary>
        /// Get instance of FMOD studiolistener. Create one if one doesn't exist.
        /// </summary>
        void GetStudioListener();

        /// <summary>
        /// Get instance of FMOD event emitter. Create one if one doesn't exist.
        /// </summary>
        void GetEventEmitter();

        /// <summary>
        /// Play sound once. Takes clip name as string "name".
        /// </summary>
        /// <param name="name"></param>
        void PlaySound(string name);

        /// <summary>
        /// Start playing background music set to MusicEvent.
        /// </summary>
        void StartBGM();

        /// <summary>
        /// Stop playing background music set to MusicEvent and release instance from memory.
        /// </summary>
        void StopBGM();

        /// <summary>
        /// Change background music by intensity level. Takes FMOD PARAMETER_ID for intensity, float value for amount of intensity and
        /// bool to ignore seek speed, bool is set to false by deafault.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="intensity"></param>
        /// <param name="ignoreseekspeed"></param>
        void SetBGMIntensity(FMOD.Studio.PARAMETER_ID id, float intensity, bool ignoreseekspeed);

        void SetShieldLevel(FMOD.Studio.PARAMETER_ID id, float intensity, bool ignoreseekspeed);
    }
}

