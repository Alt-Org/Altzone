using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Popup : MonoBehaviour
{
    public static Popup Instance;

    public enum PopupWindowType
    {
        Accept,
        Cancel
    }

    [Header("Popup Settings")]
    [SerializeField] private GameObject popupGameObject; // Assign the existing popup GameObject in the scene here
    [Space]
    [SerializeField] private GameObject _taskAcceptPopup;
    [SerializeField] private RectTransform _taskAcceptMovable;
    [SerializeField] private GameObject _taskCancelPopup;
    [Space]
    [SerializeField] private List<TextMeshProUGUI> _messageTexts;
    [Space]
    [SerializeField] private List<Button> _cancelButtons;
    [SerializeField] private List<Button> _acceptButtons;

    private bool? _result;

    private void Awake()
    {
        // Ensure there's only one instance of Popup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure the popup starts disabled
        popupGameObject.SetActive(false);
    }

    private void Start()
    {
        //Set buttons
        foreach (var abutton in _acceptButtons)
            abutton.onClick.AddListener(() => _result = true);

        foreach (var cbutton in _cancelButtons)
            cbutton.onClick.AddListener(() => _result = false);
    }

    public IEnumerator ShowPopup(string message)
    {
        // Activate the popup
        popupGameObject.SetActive(true);

        // Set the message text
        SetMessage(message);

        // Wait until one of the buttons is pressed
        yield return new WaitUntil(() => _result.HasValue);

        // Deactivate the popup
        popupGameObject.SetActive(false);

        Debug.Log($"Popup result: {_result}"); // Log the result for debugging
    }


    // Helper method to call from other scripts
    public static IEnumerator RequestPopup(string message, PopupWindowType type, Vector2? anchorLocation, System.Action<bool> callback)
    {
        if (Instance == null)
        {
            Debug.LogError("Popup instance is not set.");
            yield break;
        }

        Instance._result = null;
        Instance.WindowSwitch(type);
        if (anchorLocation != null)
            Instance.MoveAcceptWindow(anchorLocation.Value);

        // Show the popup and get the result
        yield return Instance.StartCoroutine(Instance.ShowPopup(message));
        callback(Instance._result.Value); // Use the updated _result
    }

    private void WindowSwitch(PopupWindowType type)
    {
        _taskAcceptPopup.SetActive(type == PopupWindowType.Accept);
        _taskCancelPopup.SetActive(type == PopupWindowType.Cancel);
    }

    private void MoveAcceptWindow(Vector3 location)
    {
        Vector3 centeredLocation = new
            (
                location.x/* - Screen.width / 2*/,
                location.y/* - Screen.height / 2*/,
                0f
            );
        
        _taskAcceptMovable.position = centeredLocation;
    }

    private void SetMessage(string message)
    {
        foreach (var textItem in _messageTexts)
        {
            if(textItem.IsActive())
                textItem.text = message;
        }
    }
}
