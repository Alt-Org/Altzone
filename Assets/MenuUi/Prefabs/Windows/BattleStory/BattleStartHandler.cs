using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Lobby.SelectedCharacters;
using Altzone.Scripts.Lobby.Wrappers;

public class BattleStartHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _battleStartText;
    [SerializeField]
    private TextMeshProUGUI _timerText;
    [SerializeField]
    private Image _loadImage;
    [SerializeField]
    private List<Sprite> _startAnimationSprites;
    [SerializeField]
    private float _animationFrameTime = 0.5f;
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
        _loadImage.sprite = _startAnimationSprites[0];
        _loadImage.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.8f);
        _loadImage.transform.localScale = new Vector2(1f, 1f);
        _battleStartText.gameObject.SetActive(false);
        //_timerText.gameObject.SetActive(false);
        //float timeleft = startTime/1000f;
        float frametimeleft = 0;

        foreach (Sprite sprite in _startAnimationSprites)
        {
            if (sprite != _startAnimationSprites[_startAnimationSprites.Count - 1])
            {
                frametimeleft += _animationFrameTime;
                _loadImage.sprite = sprite;
                do
                {
                    yield return null;
                    //timeleft -= Time.deltaTime;
                    frametimeleft -= Time.deltaTime;
                } while (frametimeleft > 0);
            }
            else
            {
                _loadImage.sprite = sprite;
                yield return null;
                break;
            }
        }
        yield return new WaitForSeconds(3);
        LobbyManager.Instance.IsStartFinished = true;

        //_timerText.gameObject.SetActive(true);
        /*do
        {
            //_timerText.text = Mathf.CeilToInt(timeleft).ToString();

            if (!_loadImage.sprite.Equals(_tableAboveSprite))
            {
                _loadImage.sprite = _tableAboveSprite;
                _loadImage.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            }
            //float scale = 1f + 0.4f * (1 - (Mathf.Clamp(timeleft,0,3f) / 3f));
            //_loadImage.transform.localScale = new Vector2(scale, scale);

            yield return null;
            timeleft -= Time.deltaTime;
        } while (timeleft > 0f);*/
    }
}
