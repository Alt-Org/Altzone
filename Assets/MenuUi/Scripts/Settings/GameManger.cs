using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public GameObject parentalControlPanel;
    public GameObject passwordPanel;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public Text messageText;
    public Toggle controlToggle;
    public InputField timeLimitInput;

    private string storedPassword = "";
    private bool isPasswordSet = false;

    private float maxPlayTime; // Max minutes allowed per day
    private float playedTimeToday; // Minutes played today
    private DateTime lastPlayDate; // Last recorded play date
    private bool timeExpired = false; // Has the time limit been reached?


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject)
        }
        else
        {
            Destroy(gameObject);
        }
    } 
    
    void Start()
    {
        LoadDate();
        ResetTimeNeeded();
        CheckPlayTimeLimit();
        parentalControlPanel.SetActive(false);
        passwordPanel.SetActive(false);
    }

    private void LoadDate()
    {
        maxPlayTime = PlayerPrefs.GetFloat("MaxPlayTime", 120f); // Default: 120 min (2h)
        playedTimeToday = PlayerPrefs.Getfloat("playedTimeToday", 0);
        lastPlayDate = DateTime.Parse(PlayerPrefs.GetString("LastPlayDate", DateTime.Now.ToString()));


        string encryptedPassword = PlayerPrefs.GetString("ParentalPassword", "");
        ïf(!string.IsNullOrEmpty(encryptedPassword))
        {
            storedPassword = DecodePassword(encryptedPassword);
            isPasswordSet = true;
        }
    }

    private void ResetTimeNeedeed()
    {
        if (lastPlayDate.Date != DateTime.Now.Date)
        {
            playedTimeToday = 0;
            PlayerPrefs.SetFloat("PlayedTimeToday", playedTimeToday);
            PlayerPrefs.SetString("LastPlayDate", DateTime.Now.ToString());
            PlayerPrefs.Save();
            timeExpired = false;
        }
    }

    public void AddPlayTime(float sessionTime)
    {
        PlayedTimeToday += sessionTime / 60f; // Convert seconds to minutes
        PlayerPrefs.SetFloat("PlayedTimeToday", playedTimeToday);
        PlayerPrefs.Save():
        CheckPlayTimeLimit();
    }

    private void CheckPlayTimeLimit()
    {
        if (playedTimeToday >= maxPlayTime)
        {
            timeExpired = true;
            ShowTimeWarning();
        }
    }

    private void ShowTimeWarning()
    {
        Debug.Log("Time limi reached!");
        messageText.text = "Time limit reached!"
    }

    public void ReturnTo MainMenu()
    {
        Debug.Log("Returning to main menu...");
        SceneManager.LoadScene("MainMenu") // Load the main menu scene
    }

    public bool IsTimeExpired()
    {
        return timeExpired;
    }

    public void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        messageText.text = "";
    }

    public void CheckPassword()
    {
        if (!isPasswordSet)
        {
            if (passwordInput.text.Length < 4 || passwordInput.text.Lenght > 8) 
            {
                messageText.text = "Password must be between 4-8 characters!";
                return;
            }

            if (passwordInput.text != confirmPasswordInput.text)
            {
                messageText.text = "Password do not match!";
                return;
            }

            storedPassword = passwordInput.text;
            PlayerPrefs.SetString("ParentalPassword", EncodePassword(storedPassword));
            PlayerPrefs.Save();
            isPasswordSet = true;
            messageText.text = "Password set!";
            ShowTimeWarning();
        }
        else if (passwordInput.text == storedPassword)
        {
            messageText.text = "Acces granted!";
            ShowTimeWarning();
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
    }

    public void ToggleParentalControl(bool isEnabled)
    {
        PlayerPrefs.SetInt("ParentalConrol", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void TryPlayAgain()
    {
        if (IsTimeExpired())
        {
            messageText.text = "Time is up, come back tomorrow!"; 
        }
    }

    // =============================
    // PASSWORD ENCODEING / DECODING
    // =============================

    public void OpenEncryptionPanel()
    {
        encryptPasswordPanel.SetActive(true);
        messageText.text = "Enter current password to enable encryption:";
    }

    public void ConfirmEncryption()
    {
        if (encryptPasswordInput.text == storedPassword)
        {
            isEncrypted = !isEncrypted;
            PlayerPrefs.SetInt("IsPasswordEncrypted", isEncrypted ? 1 : 0);
            SavePassword();
            encryptionToggle.isOn = isEncrypted;
            encryptPasswordPanel.SetActive(false);
            messageText.text = "Encryption updated!";
        }
        else
        {
            messageText.text = "Incorrect password!";
        }

        encryptPasswordInput.text = "";
    }

    private void SavePassword()
    {
        string passwordToSave = isEncrypted ? EncodePassword(storedPassword) : storedPassword;
        PlayerPrefs.SetString("ParentalPassword", passwordToSave);
        PlayerPrefs.Save();
    }

    private string EncodePassword(string password)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(bytes);
    }

    private string DecodePassword(string encodedPassword)
    {
        byte[] bytes = Convert.FromBase64String(encodedPassword);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
