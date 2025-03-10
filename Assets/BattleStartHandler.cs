using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Audio;
using TMPro;
using UnityEngine;

public class BattleStartHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _timerText;

    // Start is called before the first frame update
    void Start()
    {
        LobbyManager.OnStartTimeSet += StartTimer;
    }

    private void OnEnable()
    {
        AudioManager.Instance.StopMusic(); //This should have a short sfx clip playing while the battle starts.
    }
    private void OnDisable()
    {
        AudioManager.Instance.StopMusic();
    }
    private void StartTimer(long startTime)
    {
        StartCoroutine(TimerStart(startTime));
    }
    private IEnumerator TimerStart(long startTime)
    {
        float timeleft = startTime/1000f;
        do
        {
            _timerText.text = Mathf.CeilToInt(timeleft).ToString();
            timeleft -= Time.deltaTime;
            yield return null;
        } while (timeleft > 0f);
    }
}
