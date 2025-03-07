using UnityEngine;
using UnityEngine.UI;

public class TabButtonsVisualController : MonoBehaviour
{
    [SerializeField] private Vector2 _anchoredPosition = Vector2.zero;
    [Tooltip("Distance that the button goes down when pressed.")]
    [SerializeField] float _pressedDownDistance = 20f;
    [Tooltip("Set clicked buttons interactable state to false.")]
    [SerializeField] bool _disableSelected = false;

    private Button _previousButton = null;

    private void Start()
    {
        var buttons = GetComponentsInChildren<Button>();

        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => UpdateButton(button));
        }
    }

    public void UpdateButton(Button button)
    {
        if (_previousButton)
        {
            _previousButton.interactable = true;
            _previousButton.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;
        }

        if (_disableSelected)
            button.interactable = false;

        button.GetComponent<RectTransform>().anchoredPosition = new Vector2 (0f, -_pressedDownDistance);

        _previousButton = button;
    }
}
