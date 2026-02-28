using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[RequireComponent(typeof(DailyTaskProgressListener))]
public class DailyTaskLongPress : DailyTaskProgressListener, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected float _longClickStartThresholdTime = 0.2f;
    [SerializeField] protected float _longClickThresholdTime = 3f;

    [Header("For tracking multiple targets\n(Leave empty if only traking one target)")]
    [SerializeField] private string _uniqueName = "";

    private bool _isHeldDown = false;
    private Button _button;
    protected bool _oneShot = false;

    private GameObject _wheel;
    private Canvas _canvas;

    protected override void Start()
    {
        base.Start();
        _button = GetComponent<Button>();
    }
    protected virtual void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (ProgressWheelHandler.Instance == null) InstantiateProgressWheel();
        else _wheel = ProgressWheelHandler.Instance.gameObject;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!On)
            return;

        _oneShot = true;
        _isHeldDown = true;

        StartCoroutine(HoldDownTimer(ScreenToWorldPoint(eventData.position)));
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

    protected virtual IEnumerator HoldDownTimer(Vector3 clickPosition)
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            if (_isHeldDown == false)
            {
                ProgressWheelHandler.Instance.DeactivateProgressWheel();
                yield break;
            }

            if (timer >= _longClickStartThresholdTime)
            {
                if(_button)_button.enabled = false;
                ProgressWheelHandler.Instance.StartProgressWheelAtPosition(clickPosition, _longClickStartThresholdTime, _longClickThresholdTime);
            }

            if (timer >= _longClickThresholdTime)
            {
                ProgressWheelHandler.Instance.DeactivateProgressWheel();
                if (_uniqueName == "")
                    UpdateProgress("1");
                else
                    UpdateProgress(_uniqueName);

                yield break;
            }

            yield return null;
        }
    }

    private void InstantiateProgressWheel()
    {
        _wheel = Instantiate((GameObject)Resources.Load("ProgressWheel"), _canvas.transform);
        _wheel.SetActive(false);
    }

    protected Vector3 ScreenToWorldPoint(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _canvas.transform as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out Vector3 worldPoint
        );
        return worldPoint;
    }
}
