using System.Collections;
using System.Collections.Generic;
using System.Net;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProfiilinMuokkausView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _logOutButton;
    [SerializeField] private Button _privacyPolicyButton;
    [SerializeField] private Button _deleteUserButton;

    [Header("Help Text")]
    [SerializeField] private TextMeshProUGUI _helpText;

    [SerializeField] private const string LOGGED_OUT_SUCCESS = "Kirjauduttu ulos!";
    [SerializeField] private const string LOGGED_OUT_NULL = "Olet jo kirjautunut ulos!";
    [SerializeField] private const string DELETE_USER_SUCCESS = "K�ytt�j� poistettu onnistuneesti!";
    [SerializeField] private const string DELETE_USER_FAIL = "K�ytt�j�n poistamisessa tapahtui virhe!";
    [SerializeField] private const string DELETE_USER_NULL = "Et ole kirjautunut sis��n!";
    [SerializeField] private const string LEAVE_CLAN_FAIL = "Klaanista poistuminen ep�onnistui!";


    //[SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private Image _fillImage;

    [Header("Delete User Settings")]
    [SerializeField] private int _holdTime;

    public float _currentTime;
    private bool _pointerDown;

    private const string PRIVACYPOLICYADDRESS = "https://www.freeprivacypolicy.com/live/854c0df9-c25d-45a4-a6a1-022ab415f9f4";

    private void Update()
    {
        if (_pointerDown)
        {
            _currentTime += Time.deltaTime;
            _fillImage.fillAmount = _currentTime / _holdTime;

            if (_currentTime >= _holdTime)
            {
                DeleteUser();
                _currentTime = 0;
                _fillImage.fillAmount = 0;
                _pointerDown = false;
            }
        }
        else
        {
            if (!_pointerDown && _currentTime > 0)
            {
                _currentTime -= Time.deltaTime;
                _fillImage.fillAmount = _currentTime / _holdTime;
            }
        }
    }

    private void OnEnable()
    {
        if (ServerManager.Instance.isLoggedIn)
            SetLogInStatus(true);

        ServerManager.OnLogInStatusChanged += SetLogInStatus;
    }

    private void OnDisable()
    {
        ServerManager.OnLogInStatusChanged -= SetLogInStatus;
        Reset();
    }

    private void Reset()
    {
        _helpText.text = "";
        _deleteUserButton.interactable = false;
        _logOutButton.interactable = false;
        _currentTime = 0;
        _fillImage.fillAmount = 0;
        _pointerDown = false;
    }

    public void SetLogInStatus(bool isLoggedIn)
    {
        _deleteUserButton.interactable = isLoggedIn;
        _logOutButton.interactable = isLoggedIn;
    }

    public void LogOut()
    {
        if (ServerManager.Instance.Player != null)
        {
            ServerManager.Instance.LogOut();
            _helpText.text = LOGGED_OUT_SUCCESS;
        }
        else
        {
            _helpText.text = LOGGED_OUT_SUCCESS;
        }
    }

    public void ViewPrivacyPolicy()
    {
        Application.OpenURL(PRIVACYPOLICYADDRESS);
    }

    /// <summary>
    /// Deletes user data from server.
    /// </summary>
    public void DeleteUser()
    {
        if (ServerManager.Instance.Player == null)
        {
            _helpText.text = DELETE_USER_NULL;
            return;
        }

        // Leave current clan before deleting user
        //if(ServerManager.Instance.Clan != null)
        //{
        //    StartCoroutine(ServerManager.Instance.LeaveClan(success =>
        //    {
        //        if(!success)
        //        {
        //            _helpText.text = LEAVE_CLAN_FAIL;
        //            return;
        //        }
        //    }));
        //}

        StartCoroutine(WebRequests.Delete(ServerManager.SERVERADDRESS + "profile/" + PlayerPrefs.GetString("profileId", string.Empty), ServerManager.Instance.AccessToken, request =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                _helpText.text = DELETE_USER_FAIL + "\n" + request.error;
            }
            else
            {
                // Deletes the user data from DataStorage also
                var gameConfig = GameConfig.Get();
                var playerSettings = gameConfig.PlayerSettings;
                var playerGuid = playerSettings.PlayerGuid;
                var store = Storefront.Get();
                store.GetPlayerData(playerGuid, playerData =>
                {
                    if (playerData == null)
                        return;

                    store.DeletePlayerData(playerData, success =>
                    {
                        if (success)
                        {
                            Debug.Log("Player deleted from storage");
                            LogOut();
                            _helpText.text = DELETE_USER_SUCCESS;

                        }
                        else
                        {
                            Debug.LogError("Could not delete player from storage! Either it doesn't exists or couldn't be found!");
                        }
                    });
                });
            }
        }));
    }

    public void OnPointerDown()
    {
        if (!_deleteUserButton.interactable)
            return;

        _pointerDown = true;
    }
    public void OnPointerUp()
    {
        _pointerDown = false;
    }
}
