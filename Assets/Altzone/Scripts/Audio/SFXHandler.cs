using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXHandler : MonoBehaviour
    {
        [SerializeField] private SFXReference _sFXReference;

        [SerializeField] private int _initialAudioChannelChunkAmount = 4;
        [SerializeField] private int _audioChannelAddAmount = 8;

        [SerializeField] private GameObject _audioSourcePrefab;
        [SerializeField] private Transform _channelsHolder;

        private float _maxVolume = 1.0f;
        private AudioSource _oneShotChannel;
        private List<AudioChannelChunk> _channelChunks = new List<AudioChannelChunk>();
        private List<ActiveChannelPath> _activeChannels = new List<ActiveChannelPath>();

        public enum SFXPlaybackOperationType
        {
            Stop,
            Continue,
            Clear
        }

        private void Start()
        {
            for (int i = 0; i < _initialAudioChannelChunkAmount; i++) CreateChunk();

            _oneShotChannel = GetComponent<AudioSource>();
        }

        private AudioChannelData GetAudioChannelData(ActiveChannelPath activeChannelPath)
        {
            return GetAudioChannelData(activeChannelPath.Chunk, activeChannelPath.Channel);
        }

        private AudioChannelData GetAudioChannelData(int chunkIndex, int poolIndex)
        {
            bool notValid = (_channelChunks.Count <= chunkIndex || _channelChunks[chunkIndex].AudioChannels.Count <= poolIndex);

            if (notValid) return null;

            return _channelChunks[chunkIndex].AudioChannels[poolIndex];
        }

        public void SetMaxVoulme(float volume) { _maxVolume = volume; }

        public void ChangeVolume(float volume, string target)
        {
            if (target == "all")
                foreach (ActiveChannelPath channel in _activeChannels)
                {
                    AudioChannelData data = GetAudioChannelData(channel);
                    float fixedVolume = Mathf.Lerp(0f, GetVolume(data.SoundEffectData), volume);
                    GetAudioChannelData(channel).audioSourceHandler.SetVolume(fixedVolume);
                }
            else
                foreach (ActiveChannelPath channel in _activeChannels)
                {
                    AudioChannelData data = GetAudioChannelData(channel);
                    float fixedVolume = Mathf.Lerp(0f, GetVolume(data.SoundEffectData), volume);

                    if (data.SoundEffectData.Name.ToLower() == target.ToLower()) data.audioSourceHandler.SetVolume(fixedVolume);
                }
        }

        private float GetVolume(SoundEffect soundEffect)
        {
            return soundEffect.Volume * _maxVolume;
        }

        public ActiveChannelPath? Play(string categoryName, string sFXName, string mainMenuMusicName)
        {
            SoundEffect soundEffect = null;

            if (categoryName != "")
            {
                if (categoryName.ToLower() == "MainMenu".ToLower()) // MainMenu
                    soundEffect = _sFXReference.Get("MainMenu_" + mainMenuMusicName, sFXName);
                else // Other
                    soundEffect = _sFXReference.Get(categoryName, sFXName);
            }
            else
                soundEffect = _sFXReference.Get(sFXName);

            if (soundEffect == null)
            {
                Debug.LogError($"Sound: {sFXName}, in category: {categoryName}, could not be found from SFX Reference sheet.");
                return null;
            }

            if (soundEffect.Type == SoundPlayType.OneShot)
            {
                _oneShotChannel.PlayOneShot(soundEffect.Audio, GetVolume(soundEffect));
                return null;
            }
            else
                GetFreeAudioSourceHandler(soundEffect).SetPlayAudioClip(soundEffect.Audio, (soundEffect.Type == SoundPlayType.Loop));

            return _activeChannels[_activeChannels.Count - 1];
        }

        public bool PlaybackOperation(SFXPlaybackOperationType type, string name)
        {
            foreach (ActiveChannelPath channel in _activeChannels)
            {
                AudioChannelData data = GetAudioChannelData(channel);

                if (data.SoundEffectData.Name.ToLower() == name.ToLower())
                    switch (type)
                    {
                        case SFXPlaybackOperationType.Stop:
                            {
                                data.audioSourceHandler.Stop();
                                return true;
                            }
                        case SFXPlaybackOperationType.Continue:
                            {
                                data.audioSourceHandler.Continue();
                                return true;
                            }
                        case SFXPlaybackOperationType.Clear:
                            {
                                data.audioSourceHandler.Clear();
                                return true;
                            }
                    }
            }

            return false;
        }

        public bool PlaybackOperation(SFXPlaybackOperationType type, ActiveChannelPath channel)
        {
            AudioChannelData data = GetAudioChannelData(channel);

            if (data == null) return false;

            switch (type)
            {
                case SFXPlaybackOperationType.Stop:
                    {
                        data.audioSourceHandler.Stop();
                        return true;
                    }
                case SFXPlaybackOperationType.Continue:
                    {
                        data.audioSourceHandler.Continue();
                        return true;
                    }
                case SFXPlaybackOperationType.Clear:
                    {
                        data.audioSourceHandler.Clear();
                        return true;
                    }
            }

            return false;
        }

        public void PlaybackOperationAll(SFXPlaybackOperationType type)
        {
            foreach (ActiveChannelPath channel in _activeChannels)
                switch (type)
                {
                    case SFXPlaybackOperationType.Stop:
                        {
                            GetAudioChannelData(channel).audioSourceHandler.Stop();
                            break;
                        }
                    case SFXPlaybackOperationType.Continue:
                        {
                            GetAudioChannelData(channel).audioSourceHandler.Continue();
                            break;
                        }
                    case SFXPlaybackOperationType.Clear:
                        {
                            AudioChannelData data = GetAudioChannelData(channel);
                            data.audioSourceHandler.Clear();
                            data.SoundEffectData = null;
                            //GetAudioChannelData(channel).Name = "";
                            break;
                        }
                }

            if (type == SFXPlaybackOperationType.Clear)
            {
                foreach (AudioChannelChunk chunk in _channelChunks) chunk.AmountInUse = 0;

                _activeChannels.Clear();
            }
        }

        private void AudioSourceSelfUnregister(int chunk, int channel)
        {
            for (int i = 0; i < _activeChannels.Count; i++)
                if (_activeChannels[i].Channel == channel)
                {
                    _activeChannels.RemoveAt(i);
                    //_channelChunks[chunk].AudioChannels[channel].Name = "";
                    _channelChunks[chunk].AudioChannels[channel].SoundEffectData = null;
                    _channelChunks[chunk].AmountInUse--;
                    return;
                }
        }

        private AudioSourceHandler GetFreeAudioSourceHandler(SoundEffect soundEffect)
        {
            for (int i = 0; i < _channelChunks.Count; i++)
                if (_channelChunks[i].AmountInUse < _audioChannelAddAmount)
                    for (int j = 0; j < _channelChunks[i].AudioChannels.Count; j++)
                        if (!_channelChunks[i].AudioChannels[j].audioSourceHandler.IsInUse())
                        {
                            _channelChunks[i].AmountInUse++;
                            _activeChannels.Add(new(i, j));
                            AudioChannelData data = GetAudioChannelData(i, j);
                            data.SoundEffectData = soundEffect;
                            data.audioSourceHandler.SetVolume(GetVolume(soundEffect));

                            return GetAudioChannelData(i, j).audioSourceHandler;
                        }

            //No free AudioSourceHandlers found. Creating new AudioSourceHandler chunk.
            CreateChunk();
            _channelChunks[_channelChunks.Count - 1].AmountInUse++;
            _activeChannels.Add(new(_channelChunks.Count - 1, 0));

            return GetAudioChannelData(_channelChunks.Count - 1, 0).audioSourceHandler;
        }

        private void CreateChunk()
        {
            AudioChannelChunk audioChannelChunk = new AudioChannelChunk();
            List<AudioChannelData> audioChannels = new List<AudioChannelData>();

            for (int j = 0; j < _audioChannelAddAmount; j++)
            {
                AudioChannelData audioChannelData = new AudioChannelData();
                GameObject audioObject = Instantiate(_audioSourcePrefab, _channelsHolder);
                audioChannelData.audioSourceHandler = audioObject.GetComponent<AudioSourceHandler>();
                audioChannelData.audioSourceHandler.SetChunkIndex(_channelChunks.Count, j);
                audioChannelData.audioSourceHandler.OnPlaybackFinished += AudioSourceSelfUnregister;
                audioChannels.Add(audioChannelData);
            }

            audioChannelChunk.AudioChannels = audioChannels;
            _channelChunks.Add(audioChannelChunk);
        }
    }

    public struct ActiveChannelPath
    {
        public int Chunk;
        public int Channel;

        public ActiveChannelPath(int chunk, int channel)
        {
            Chunk = chunk;
            Channel = channel;
        }
    }

    public class AudioChannelChunk
    {
        public int AmountInUse;
        public List<AudioChannelData> AudioChannels;
    }

    public class AudioChannelData
    {
        //public string Name;
        public SoundEffect SoundEffectData;
        public AudioSourceHandler audioSourceHandler;
        public SoundPlayType soundCategoryType;
    }
}
