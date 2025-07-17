using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        for (int i = 0; i < _initialAudioChannelChunkAmount; i++)
            CreateChunk();

        _oneShotChannel = GetComponent<AudioSource>();
    }

    public void SetMaxVoulme(float volume) { _maxVolume = volume; }

    public void SetVolume(float volume, string target)
    {
        float fixedVolume = Mathf.Lerp(0f, _maxVolume, volume);

        if (target == "all")
        {
            foreach (ActiveChannelPath channel in _activeChannels)
                _channelChunks[channel.Chunk].AudioChannels[channel.Channel].audioSourceHandler.SetVolume(fixedVolume);

            if (_oneShotChannel == null) _oneShotChannel = GetComponent<AudioSource>();

            if (_oneShotChannel == null) return;

            _oneShotChannel.volume = fixedVolume;
        }
        else
            foreach (ActiveChannelPath channel in _activeChannels)
                if (_channelChunks[channel.Chunk].AudioChannels[channel.Channel].Name.ToLower() == target.ToLower())
                    _channelChunks[channel.Chunk].AudioChannels[channel.Channel].audioSourceHandler.SetVolume(fixedVolume);
    }

    public void Play(string categoryName, string sFXName, string mainMenuMusicName)
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
            return;
        }

        if (soundEffect.Type == SoundPlayType.OneShot)
            _oneShotChannel.PlayOneShot(soundEffect.Audio);
        else
            GetFreeAudioSourceHandler(sFXName).SetPlayAudioClip(soundEffect.Audio, (soundEffect.Type == SoundPlayType.Loop));
    }

    public void StopAll()
    {
        foreach (ActiveChannelPath channel in _activeChannels)
            _channelChunks[channel.Chunk].AudioChannels[channel.Channel].audioSourceHandler.Stop();
    }

    public void ContinueAll()
    {
        foreach (ActiveChannelPath channel in _activeChannels)
            _channelChunks[channel.Chunk].AudioChannels[channel.Channel].audioSourceHandler.Continue();
    }

    public void ClearAll()
    {
        foreach (ActiveChannelPath channel in _activeChannels)
        {
            _channelChunks[channel.Chunk].AudioChannels[channel.Channel].audioSourceHandler.Clear();
            _channelChunks[channel.Chunk].AudioChannels[channel.Channel].Name = "";
        }

        foreach (AudioChannelChunk chunk in _channelChunks)
            chunk.AmountInUse = 0;

        _activeChannels.Clear();
    }

    private void AudioSourceSelfUnregister(int chunk, int channel)
    {
        for (int i = 0; i < _activeChannels.Count; i++)
            if (_activeChannels[i].Channel == channel)
            {
                _activeChannels.RemoveAt(i);
                _channelChunks[chunk].AmountInUse--;
                return;
            }
    }

    private AudioSourceHandler GetFreeAudioSourceHandler(string name)
    {
        for (int i = 0; i < _channelChunks.Count; i++)
            if (_channelChunks[i].AmountInUse < _audioChannelAddAmount)
                for (int j = 0; j < _channelChunks[i].AudioChannels.Count; j++)
                    if (!_channelChunks[i].AudioChannels[j].audioSourceHandler.IsInUse())
                    {
                        _channelChunks[i].AmountInUse++;
                        _activeChannels.Add(new(i, j));
                        _channelChunks[i].AudioChannels[j].Name = name;

                        return _channelChunks[i].AudioChannels[j].audioSourceHandler;
                    }

        //No free AudioSourceHandlers found. Creating new AudioSourceHandler chunk.
        CreateChunk();
        _channelChunks[_channelChunks.Count - 1].AmountInUse++;
        _activeChannels.Add(new(_channelChunks.Count - 1, 0));

        return _channelChunks[_channelChunks.Count - 1].AudioChannels[0].audioSourceHandler;
    }

    private void CreateChunk()
    {
        _channelChunks = new List<AudioChannelChunk>();

        for (int i = 0; i < _channelChunks.Count; i++)
        {
            _channelChunks[i].AudioChannels = new List<AudioChannelData>(_audioChannelAddAmount);

            for (int j = 0; j < _channelChunks[i].AudioChannels.Count; j++)
            {
                GameObject audioObject = Instantiate(_audioSourcePrefab, _channelsHolder);
                _channelChunks[i].AudioChannels[j].audioSourceHandler = audioObject.GetComponent<AudioSourceHandler>();
                _channelChunks[i].AudioChannels[j].audioSourceHandler.SetChunkIndex(i, j);
                _channelChunks[i].AudioChannels[j].audioSourceHandler.OnPlaybackFinished += AudioSourceSelfUnregister;
            }
        }
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
    public string Name;
    public AudioSourceHandler audioSourceHandler;
    public SoundPlayType soundCategoryType;
}
