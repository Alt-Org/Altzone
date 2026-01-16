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

    [Header("Long press wheel")]
    [SerializeField] private GameObject _wheelPrefab;

    private DailyTaskProgressListener _listener;
    private bool _isHeldDown = false;
    private Button _button;
    private bool _oneShot = false;
    private GameObject _wheelInstance;
    private Image _wheelImage;

    private void Start()
    {
        _listener = GetComponent<DailyTaskProgressListener>();
        _button = GetComponent<Button>();

        // Väliaikainen testi: wheel ilmestyy heti napin päälle
        Instantiate(_wheelPrefab, transform);
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
            HideWheel();
            return;
        }

        _oneShot = false;
        _isHeldDown = false;

        if (_button)
            _button.enabled = true;

        HideWheel();
    }

    private IEnumerator HoldDownTimer()
    {
        float timer = 0f;
        bool wheelShown = false;

        while (true)
        {
            timer += Time.deltaTime;

            if (_isHeldDown == false){
                HideWheel();
                yield break;
            }

            // Start threshold, lukitse button + näytä wheel
            if (!wheelShown && timer >= _longClickStartThresholdTime){
                wheelShown = true;

                if (_button)
                _button.enabled = false;

                ShowWheel();
            }
            //Edistymisen näyttäminen
            if (wheelShown && _wheelImage != null)
            {
                float progress = Mathf.InverseLerp(
                    _longClickStartThresholdTime,
                    _longClickThresholdTime,
                    timer);

                _wheelImage.fillAmount = progress;
            }

            // Long press valmis
            if (timer >= _longClickThresholdTime)
            {
                if (_uniqueName == "")
                    _listener.UpdateProgress("1");
                else
                    _listener.UpdateProgress(_uniqueName);

                HideWheel();
                yield break;
            }

            yield return null;
        }
    }
    private void ShowWheel()
{
    if (_wheelInstance != null)
        return;

    //instantiate listenerin alle
    _wheelInstance = Instantiate(_wheelPrefab, transform);
    _wheelImage = _wheelInstance.GetComponent<Image>();

    _wheelImage.fillAmount = 1f;
}

private void HideWheel()
{
    if (_wheelInstance == null)
        return;

    Destroy(_wheelInstance);
    _wheelInstance = null;
    _wheelImage = null;
}
}
