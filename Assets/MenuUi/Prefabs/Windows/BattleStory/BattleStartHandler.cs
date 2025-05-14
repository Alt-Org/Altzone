using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleStartHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _timerText;
    [SerializeField]
    private Image _loadImage;
    [SerializeField]
    private Sprite _sittingDownSprite;
    [SerializeField]
    private Sprite _tableAboveSprite;

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
    void OnDestroy()
    {
        LobbyManager.OnStartTimeSet -= StartTimer;
    }

    private IEnumerator TimerStart(long startTime)
    {
        _loadImage.sprite = _sittingDownSprite;
        _loadImage.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        _loadImage.transform.localScale = new Vector2(1f, 1f);
        float timeleft = startTime/1000f;
        do
        {
            timeleft -= Time.deltaTime;
            _timerText.text = Mathf.CeilToInt(timeleft).ToString();

            if(timeleft <= 3)
            {
                if (_loadImage.sprite.Equals(_sittingDownSprite))
                {
                    _loadImage.sprite = _tableAboveSprite;
                    _loadImage.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                }
                float scale = 1f + 0.4f * (1 - (Mathf.Clamp(timeleft,0,3f) / 3f));
                _loadImage.transform.localScale = new Vector2(scale, scale);
            }

            yield return null;
        } while (timeleft > 0f);
    }
}
