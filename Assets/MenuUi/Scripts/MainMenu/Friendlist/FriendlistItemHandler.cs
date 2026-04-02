using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendlistItem : MonoBehaviour

{
    [SerializeField] private RectTransform _topPanel;
    [SerializeField] private RectTransform _bottomPanel;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;
    [SerializeField] private Image _onlineStatusIndicator;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private Button _removefriendButton;
    [SerializeField] private Button _acceptFriendButton;
    [SerializeField] private Button _declineFriendButton;

    private bool _isOnline = true;

    public delegate void FriendPanelPressed(FriendlistItem handler);
    public static event FriendPanelPressed OnPanelPressed;

    public delegate void ContentRefreshRequested();
    public static event ContentRefreshRequested OnContentRefreshRequested;

    private void OnEnable()
    {
        OnPanelPressed += ButtonPressHandle;
    }
    private void OnDisable()
    {
        OnPanelPressed -= ButtonPressHandle;
        UpdateSize(false);
    }

    public IEnumerator Initialize(string name, AvatarVisualData avatarVisualData = null, ClanLogo clanLogo = null, bool isOnline = true, Action onRemoveClick = null, Action onAcceptClick = null, Action onDeclineClick = null)
   {
        _nameText.text = name;
        _isOnline = isOnline;


        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }
    
        if (clanLogo != null)
        {
            _clanHeart.SetHeartColors(clanLogo);
        }
        
        UpdateOnlineStatusIndicator();
        
        if (_removefriendButton != null) // Show button only for accepted friends
        {
            _removefriendButton.gameObject.SetActive(onRemoveClick != null);

            _removefriendButton.onClick.RemoveAllListeners();
            _removefriendButton.onClick.AddListener(() =>
            {
                onRemoveClick?.Invoke();
            });
        }
        if (_acceptFriendButton != null) // Show button only for pending requests
        {
            _acceptFriendButton.gameObject.SetActive(onAcceptClick != null);

            _acceptFriendButton.onClick.RemoveAllListeners();
            _acceptFriendButton.onClick.AddListener(() =>
            {
                onAcceptClick?.Invoke();
            });
        }
        if (_declineFriendButton != null) // Show button only for pending requests
        {
            _declineFriendButton.gameObject.SetActive(onDeclineClick != null);

            _declineFriendButton.onClick.RemoveAllListeners();
            _declineFriendButton.onClick.AddListener(() =>
            {
                onDeclineClick?.Invoke();
            });
        }

        GetComponent<Button>().onClick.AddListener(() => UpdateSize());

        yield return new WaitForEndOfFrame();
        UpdateSize(false);
    }

    private void ButtonPressHandle(FriendlistItem handler)
    {
        UpdateSize(handler == this);
    }

    private void UpdateSize()
    {
        //StartCoroutine(UpdateSizeCoroutine(value));
        OnPanelPressed?.Invoke(this);
    }

    public void UpdateSize(bool value)
    {
        if (!value)
        { 
            GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x, Math.Min(GetComponent<RectTransform>().sizeDelta.x/5,100f));
            _topPanel.anchorMin= new(0, 0f);
            _bottomPanel.gameObject.SetActive(false);
        }
        else
        {
            GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x, Math.Min(GetComponent<RectTransform>().sizeDelta.x / 5, 100f) * 2);
            _topPanel.anchorMin = new(0, 0.5f);
            _bottomPanel.gameObject.SetActive(true);
        }
        OnContentRefreshRequested?.Invoke();
    }

    private void UpdateOnlineStatusIndicator()
    {
        _onlineStatusIndicator.color = _isOnline ? Color.green : Color.red;
    }

    public void SetOnlineStatus (bool isOnline)
    {
        _isOnline = isOnline;
        UpdateOnlineStatusIndicator();
    }
}

