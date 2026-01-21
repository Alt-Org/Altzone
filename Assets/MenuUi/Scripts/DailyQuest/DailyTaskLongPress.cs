using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(DailyTaskProgressListener))]
public class DailyTaskLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _longClickStartThresholdTime = 0.2f;
    [SerializeField] private float _longClickThresholdTime = 3f;

    [Header("For tracking multiple targets\n(Leave empty if only traking one target)")]
    [SerializeField] private string _uniqueName = "";

    private DailyTaskProgressListener _listener;
    private bool _isHeldDown = false;
    private Button _button;
    private bool _oneShot = false;

    private void Start()
    {
        _listener = GetComponent<DailyTaskProgressListener>();
        _button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_listener.On)
            return;

        _oneShot = true;
        _isHeldDown = true;
        StartCoroutine(HoldDownTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_listener.On)
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
                if (_uniqueName == "")
                    _listener.UpdateProgress("1");
                else
                    _listener.UpdateProgress(_uniqueName);

                yield break;
            }

            yield return null;
        }
    }
}
