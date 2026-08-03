using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldToLeaveClanButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 2f;

    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Fill Colors")]
    [SerializeField] private Color colorStart = new Color32(255, 0, 4, 255);      // #FF0004
    [SerializeField] private Color colorSecond = new Color32(255, 110, 1, 255);   // #FF6E01
    [SerializeField] private Color colorThird = new Color32(255, 161, 0, 255);    // #FFA100
    [SerializeField] private Color colorEnd = new Color32(47, 163, 107, 255);     // #2FA36B

    private bool _isHolding;
    private bool _hasCompleted;
    private float _holdTimer;

    public event Action OnHoldCompleted;

    private void Awake()
    {
        ResetHold();
    }

    private void Update()
    {
        if (!_isHolding || _hasCompleted)
            return;

        _holdTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(_holdTimer / holdDuration);

        if (fillImage != null)
        {
            fillImage.fillAmount = progress;
            fillImage.color = EvaluateFillColor(progress);
        }

        if (progress >= 1f)
        {
            CompleteHold();
        }
    }

    private Color EvaluateFillColor(float progress)
    {
        if (progress <= 0.33f)
        {
            float t = progress / 0.33f;
            return Color.Lerp(colorStart, colorSecond, t);
        }

        if (progress <= 0.66f)
        {
            float t = (progress - 0.33f) / 0.33f;
            return Color.Lerp(colorSecond, colorThird, t);
        }

        float finalT = (progress - 0.66f) / 0.34f;
        return Color.Lerp(colorThird, colorEnd, finalT);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_hasCompleted)
            return;

        _isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_hasCompleted)
            return;

        ResetHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hasCompleted)
            return;

        ResetHold();
    }

    private void CompleteHold()
    {
        _isHolding = false;
        _hasCompleted = true;

        if (fillImage != null)
            fillImage.fillAmount = 1f;

        OnHoldCompleted?.Invoke();
    }

    public void ResetHold()
    {
        _isHolding = false;
        _hasCompleted = false;
        _holdTimer = 0f;

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = colorStart;
        }
    }
}
