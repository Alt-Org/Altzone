using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class Popup : MonoBehaviour
{
    public static Popup Instance;

    [Header("Popup Settings")]
    public GameObject popupGameObject; // Assign the existing popup GameObject in the scene here

    private bool _result;

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

    public IEnumerator ShowPopup(string message)
    {
        // Activate the popup
        popupGameObject.SetActive(true);

        // Find the components in the popup
        var messageText = popupGameObject.transform.Find("Message").GetComponent<TMP_Text>();
        var confirmButton = popupGameObject.transform.Find("ConfirmButton").GetComponent<Button>();
        var cancelButton = popupGameObject.transform.Find("CancelButton").GetComponent<Button>();

        // Set the message text
        messageText.text = message;

        bool? result = null;

        // Remove existing listeners to avoid duplication
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        // Add listeners to the buttons
        confirmButton.onClick.AddListener(() =>
        {
            result = true;
            _result = true; // Set _result here
        });
        cancelButton.onClick.AddListener(() =>
        {
            result = false;
            _result = false; // Set _result here
        });

        // Wait until one of the buttons is pressed
        yield return new WaitUntil(() => result.HasValue);

        // Deactivate the popup
        popupGameObject.SetActive(false);

        Debug.Log($"Popup result: {result}"); // Log the result for debugging
    }


    // Helper method to call from other scripts
    public static IEnumerator RequestPopup(string message, System.Action<bool> callback)
    {
        if (Instance == null)
        {
            Debug.LogError("Popup instance is not set.");
            yield break;
        }

        // Show the popup and get the result
        yield return Instance.StartCoroutine(Instance.ShowPopup(message));
        callback(Instance._result); // Use the updated _result
    }

}
