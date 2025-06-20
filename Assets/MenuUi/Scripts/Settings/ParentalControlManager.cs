using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParentalControlManager : MonoBehaviour
{
    public GameObject parentalControlPanel;
    public GameObject passwordPanel;
    //public InputField passwordInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    //public Text messageText;
    public TMP_Text messageText;
    public Toggle controlToggle;
    public TMP_InputField timeLimitInput;

    private string sessionPassword = "";

    void Start()
    {
        parentalControlPanel.SetActive(true);
        passwordPanel.SetActive(true);

        controlToggle.isOn = true; // PlayerPrefs.GetInt("ParentalControl", 0) == 1;
        timeLimitInput.text = 10f.ToString(); //PlayerPrefs.GetFloat("MaxPlayTime", 2f).ToString();

    }

    public void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        messageText.text = "";
    }

    public void CheckPassword()
    {
        if (sessionPassword == "")
        {
            if (passwordInput.text.Length < 4-8)            // < -4 ??
            {
                messageText.text = "Password must be at least 4-8 characters";
                return;
            }

            if (passwordInput.text != confirmPasswordInput.text)
            {
                messageText.text = "Passwords do not match";
                return;
            }

            sessionPassword = passwordInput.text;
            messageText.text = "Password set!";
            Invoke("ShowSettings", 1.0f);
        }
        else if (passwordInput.text == sessionPassword)
        {
            messageText.text = "Access granted!";
            Invoke("ShowSettings", 0.5f);
        }
        else
        {
            messageText.text = "Incorrect password!";
        }
    }

    private void ShowSettings()
    {
        passwordInput.text = "";
        confirmPasswordInput.text = "";
        messageText.text = "";
        passwordPanel.SetActive(false);
        parentalControlPanel.SetActive(true);
    }

    public void CloseParentalControl()
    {
        parentalControlPanel.SetActive(false);
        sessionPassword = "";
    }


    public void ToggleParentalControl(bool isEnabled)
    {
        PlayerPrefs.SetInt("ParentalControl", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetTimeLimit()
    {
        if (float.TryParse(timeLimitInput.text, out float time))
        {
            PlayerPrefs.SetFloat("MaxPlayTime", time);
            PlayerPrefs.Save();
        }
    }
}
