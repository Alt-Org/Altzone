using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectButtonsPopup : MonoBehaviour
{
    private Button[] _selectedButtons = new Button[3];

    [SerializeField] private GameObject _popupWindow;
    [Space]
    [SerializeField] private Button[] _popupButtons = new Button[3];
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _expandButton;
    [Space]
    [SerializeField] private float _hiddenXOffSet;
    [SerializeField] private float _expandTime = 2.0f;

    private static bool _expanded = false;
    private static bool _shown = false;

    private float _ogXPos;
    private RectTransform _rect;
    private float _offSetXPos {
        get { return _ogXPos + _hiddenXOffSet; }
    }

    private bool _initialized = false;

    public static event Action OnConfirm;

    private void Initialize()
    {
        // Get og pos
        _rect = _popupWindow.GetComponent<RectTransform>();
        _ogXPos = _rect.anchoredPosition.x;

        _initialized = true;
    }

    private void OnEnable()
    {
        if (!_initialized) Initialize();

        ShowPopup(_shown);

        // Move to offSetted position (hidden from screen)
        if (!_expanded)
        {
            _rect.anchoredPosition = new Vector2(_offSetXPos, _rect.anchoredPosition.y);
        }
        else
        {
            _rect.anchoredPosition = new Vector2(_ogXPos, _rect.anchoredPosition.y);
        }
        FlipExpandArrow(_expanded);

        DailyTaskSelectButtons.OnButtonSelected += HandlePopup;
        DailyTaskSelectButtons.OnStateChange += ShowPopup;

        if (_confirmButton != null)
        {
            _confirmButton.onClick.AddListener(ConfirmSelection);
            UpdateConfirmButton();
        }
        if (_expandButton != null)
        {
            _expandButton.onClick.AddListener(ExpandPopup);
        }
    }

    private void OnDisable()
    {
        DailyTaskSelectButtons.OnButtonSelected -= HandlePopup;
        DailyTaskSelectButtons.OnStateChange -= ShowPopup;
        if (_confirmButton != null)
        {
            _confirmButton.onClick.RemoveAllListeners();
            UpdateConfirmButton();
        }
        if (_expandButton != null)
        {
            _expandButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Shows or hides the popup (fully)
    /// </summary>
    /// <param name="showHide">Show = true/false</param>
    private void ShowPopup(bool showHide)
    {
        _popupWindow.SetActive(showHide);
        _shown = showHide;
    }

    /// <summary>
    /// Moves the popup in or out from the side of the screen
    /// </summary>
    private void ExpandPopup()
    {
        _expanded = !_expanded;

        FlipExpandArrow(_expanded);

        _expandButton.interactable = false;
        StartCoroutine(ExpandInOut(_expanded, _expandTime));
    }

    /// <summary>
    /// Flips the arrow button that call's ExpandPopup() to look good on UI
    /// </summary>
    /// <param name="facingRight">If the arrow should be facing right(true) or left(false)</param>
    private void FlipExpandArrow(bool facingRight)
    {

        float x;
        if (facingRight) x = 1f;
        else x = -1f;

        // Flip on X-axis to turn the arrow
        _expandButton.transform.localScale
            = new Vector3(
                x,
                _expandButton.transform.localScale.y,
                _expandButton.transform.localScale.z);
    }

    /// <summary>
    /// Moves the popup in or out from the side of the screen
    /// </summary>
    /// <param name="expandIn">If the popup should move in(true) or out(false)</param>
    /// <param name="duration">The duration of the "animation"</param>
    /// <returns></returns>
    IEnumerator ExpandInOut(bool expandIn, float duration)
    {
        float targetX;

        // Select target X coordinate depending if the popup is moving in or out
        if (expandIn) targetX = _ogXPos;
        else targetX = _offSetXPos;

        Vector2 startPos = _rect.anchoredPosition;
        Vector2 endPos = new Vector2(targetX, startPos.y);
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentage = elapsed / duration;

            // Move the popup
            _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, percentage);

            yield return null; // Wait for the next frame
        }

        _rect.anchoredPosition = endPos; // Ensure it snaps to the final spot
        _expandButton.interactable = true;
    }

    private void HandlePopup(DailyTaskSelectButtons.SelectButtonObject button)
    {
        // Get first empty slot from the popup
        int emptySlot = -1;
        for (int i = 0; i < _selectedButtons.Length; i++)
        {
            if (_selectedButtons[i] == null)
            {
                emptySlot = i;
                break;
            }
        }

        if (emptySlot == -1) return;

        // Add the selected button to a list to keep track of the selected buttons
        _selectedButtons[emptySlot] = button.Button;

        // Get the button on the popup
        Button popupButton = _popupButtons[emptySlot];

        // Get the image from the selected button
        Image sourceImage = button.Image;

        // Get the slot's image component so we can replace it
        Image popupImage = popupButton.gameObject.GetComponent<Image>();

        // Set the slot's image to the same as the selected button's image
        if (sourceImage != null && popupImage != null)
        {

            popupImage.sprite = sourceImage.sprite;
            popupImage.preserveAspect = true;
        }

        popupButton.gameObject.SetActive(true);
        popupImage.gameObject.SetActive(true);

        popupButton.onClick.AddListener(() =>
        {
            RemoveSelectedButton(emptySlot);
        });

        _popupWindow.SetActive(true);

        UpdateConfirmButton();
    }

    private void RemoveSelectedButton(int index)
    {
        _selectedButtons[index] = null;

        Button popupButton = _popupButtons[index];
        Image popupImage = popupButton.gameObject.GetComponent<Image>();

        if (popupImage != null)
        {
            popupImage.sprite = null;
        }

        popupButton.gameObject.SetActive(false);
        popupButton.onClick.RemoveAllListeners();
        popupImage.gameObject.SetActive(false);

        UpdateConfirmButton();
    }

    private void ConfirmSelection()
    {
        OnConfirm?.Invoke();

        _popupWindow.SetActive(false);

        for (int i = 0; i < _selectedButtons.Length; i++)
        {
            RemoveSelectedButton(i);
        }
    }

    private void UpdateConfirmButton()
    {
        if (_confirmButton != null)
        {
            bool slotsFull = true;
            for (int i = 0; i < _selectedButtons.Length; i++)
            {
                if (_selectedButtons[i] == null)
                {
                    slotsFull = false;
                    break;
                }
            }
            _confirmButton.gameObject.SetActive(slotsFull);
            _confirmButton.interactable = slotsFull;
        }
    }
}
