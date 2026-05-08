using UnityEngine;
using UnityEngine.UI;

public class ProfileIconTextToggle : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject textContainer;
    [SerializeField] private bool hideOnStart = true;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(ToggleText);
            button.onClick.AddListener(ToggleText);
        }

        if (hideOnStart && textContainer != null)
        {
            textContainer.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(ToggleText);
        }
    }

    private void ToggleText()
    {
        if (textContainer == null)
            return;

        textContainer.SetActive(!textContainer.activeSelf);
    }

    public void HideText()
    {
        if (textContainer != null)
        {
            textContainer.SetActive(false);
        }
    }
}
