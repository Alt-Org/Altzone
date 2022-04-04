using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prg.Scripts.Service.FMOD
{
    public interface IFMODManager
    {

        /// <summary>
        /// Get instance of FMOD studiolistener. Create one if one doesn't exist
        /// </summary>
        void GetStudioListener();

        void PlaySound(string name);

        void StartBGM();

        void StopBGM();

        void SetBGMIntensity(int intensity);
    }
}

