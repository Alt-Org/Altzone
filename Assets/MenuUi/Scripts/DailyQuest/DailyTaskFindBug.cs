using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class DailyTaskFindBug : DailyTaskProgressListener, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _textToChange;
    private string _originalText;

    [SerializeField] private float _longClickStartThresholdTime = 0.2f;
    [SerializeField] private float _longClickThresholdTime = 3f;
    private bool _isHeldDown = false;
    private bool _oneShot = false;

    protected override void Start()
    {
        base.Start();

        if (_textToChange != null)
            _originalText = _textToChange.text;

        if (On)
            InstantiateBug();
    }

    public override void SetState(PlayerTask task)
    {
        base.SetState(task);

        if (On)
            InstantiateBug();
        else
            RemoveBug();
    }

    private void InstantiateBug()
    {
        if (_textToChange != null)
            _textToChange.text = "";
    }

    private void RemoveBug()
    {
        if (_textToChange != null)
            _textToChange.text = _originalText;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!On)
            return;
        
        _oneShot = true;
        _isHeldDown = true;
        StartCoroutine(HoldDownTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!On)
        {
            if (_oneShot)
            {
                if (_button)
                    _button.enabled = true;

                _oneShot = false;
            }

            return;
        }

        _oneShot = false;
        _isHeldDown = false;

        if (_button)
            _button.enabled = true;
    }

    private IEnumerator HoldDownTimer()
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            if (_isHeldDown == false)
                yield break;

            if (_button && timer >= _longClickStartThresholdTime)
                _button.enabled = false;

            if (timer >= _longClickThresholdTime)
            {
                UpdateProgress("1");
                RemoveBug();
                yield break;
            }

            yield return null;
        }
    }
}
