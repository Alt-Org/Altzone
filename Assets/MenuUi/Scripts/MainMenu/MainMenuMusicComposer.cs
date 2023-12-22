using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusicComposer : MonoBehaviour
{
    public static MainMenuMusicComposer Instance { get; private set; }

    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetMusicTime(float time)
    {
        StopAllCoroutines();
        StartCoroutine(StartFade(0.05f, SettingsCarrier.Instance.SentVolume(SettingsCarrier.SoundType.music), time));
    }

    public IEnumerator StartFade(float duration, float targetVolume, float time)
    {
        float currentTime = 0;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_STANDALONE_WIN

        float start = musicSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, 0, currentTime / duration);
            yield return null;
        }

#endif

        musicSource.volume = 0;
        musicSource.Stop();
        musicSource.time = time;
        musicSource.volume = targetVolume;
        musicSource.Play();

        currentTime = 0;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_STANDALONE_WIN

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, targetVolume, currentTime / duration);
            yield return null;
        }

#endif
        yield break;
    }
}
