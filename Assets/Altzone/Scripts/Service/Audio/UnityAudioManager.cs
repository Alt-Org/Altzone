using UnityEngine;

namespace Altzone.Scripts.Service.Audio
{
    public class UnityAudioManager : MonoBehaviour, IAudioManager
    {
        float IAudioManager.MasterVolume
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        float IAudioManager.MenuEffectsVolume
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        float IAudioManager.GameEffectsVolume
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        float IAudioManager.GameMusicVolume
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
}