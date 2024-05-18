using UnityEngine;
using UnityEngine.UI;

public class ChatTask : MonoBehaviour
{
    public delegate void ButtonPressedEventHandler(Button button);
    public static event ButtonPressedEventHandler OnButtonPressed;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        OnButtonPressed?.Invoke(button);
    }
}
