#if USE_FMOD
using System;
using Altzone.Scripts.Config;
using FMOD;
using FMODUnity;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// AudioManager service FMOD implementation.
    /// </summary>
    public class FmodAudioManager : MonoBehaviour, IAudioManager
    {
        private const string MasterChannelGroupName = "Master";
        private const string MenuEffectsChannelGroupName = "MenuEffects";
        private const string GameEffectsChannelGroupName = "GameEffects";
        private const string GameMusicChannelGroupName = "GameMusic";

        private static int _maxChannelGroupNameLen;

        private ChannelGroup _master;
        private ChannelGroup _menuEffects;
        private ChannelGroup _gameEffects;
        private ChannelGroup _gameMusic;

        public float MasterVolume
        {
            get => GetChannelGroupVolume(_master);
            set => SetChannelGroupVolume(_master, value);
        }

        public float MenuEffectsVolume
        {
            get => GetChannelGroupVolume(_menuEffects);
            set => SetChannelGroupVolume(_menuEffects, value);
        }

        public float GameEffectsVolume
        {
            get => GetChannelGroupVolume(_gameEffects);
            set => SetChannelGroupVolume(_gameEffects, value);
        }

        public float GameMusicVolume
        {
            get => GetChannelGroupVolume(_gameMusic);
            set => SetChannelGroupVolume(_gameMusic, value);
        }

        private void Awake()
        {
            var result = RuntimeManager.CoreSystem.getVersion(out var version);
            Assert.AreEqual(RESULT.OK, result);
            Assert.IsTrue(RuntimeManager.IsInitialized, "FMODUnity.RuntimeManager.IsInitialized");

            var features = RuntimeGameConfig.Get().Features;
            var isMuted = features._isMuteAllSounds;
            RuntimeManager.MuteAllEvents(isMuted);

            var verMajor = version >> 16;
            var verMinor = (version >> 8) & 0xF;
            var verDev = version & 0xF;
            Debug.Log($"{name} FMOD ver {verMajor}.{verMinor:00}.{verDev:00} mute {RuntimeManager.IsMuted}");

            _master = CreateChannelGroup(MasterChannelGroupName, ref _maxChannelGroupNameLen);
            _menuEffects = CreateChannelGroup(MenuEffectsChannelGroupName, ref _maxChannelGroupNameLen);
            _gameEffects = CreateChannelGroup(GameEffectsChannelGroupName, ref _maxChannelGroupNameLen);
            _gameMusic = CreateChannelGroup(GameMusicChannelGroupName, ref _maxChannelGroupNameLen);
        }

        private static ChannelGroup CreateChannelGroup(string name, ref int maxNameLen)
        {
            var result = RuntimeManager.CoreSystem.createChannelGroup(name, out var channelGroup);
            if (result != RESULT.OK)
            {
                throw new UnityException($"Unable to create ChannelGroup {name}: {result}");
            }
            if (maxNameLen < name.Length)
            {
                maxNameLen = name.Length;
            }
            return channelGroup;
        }

        private static float GetChannelGroupVolume(ChannelGroup channelGroup)
        {
            var result = channelGroup.getVolume(out var volume);
            if (result != RESULT.OK)
            {
                Debug.LogWarning(channelGroup.getName(out var channelGroupName, _maxChannelGroupNameLen) == RESULT.OK
                    ? $"Failed to get volume for channel {channelGroupName}: {result}"
                    : $"Failed to get volume for channel ???: {result}");
                return 0;
            }
            return volume;
        }

        private static void SetChannelGroupVolume(ChannelGroup channelGroup, float value)
        {
            var result = channelGroup.setVolume(value);
            if (result != RESULT.OK)
            {
                Debug.LogWarning(channelGroup.getName(out var channelGroupName, _maxChannelGroupNameLen) == RESULT.OK
                    ? $"Failed to set volume {value} for channel {channelGroupName}: {result}"
                    : $"Failed to set volume {value} for channel ???: {result}");
            }
        }
    }
}
#endif