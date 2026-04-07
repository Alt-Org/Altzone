using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Window;
using Assets.Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum FriendState
{
    Friend,
    Receiving,
    Sending,
    None
}

public enum OnlineState
{
    Online,
    Offline,
    Global
}

public class OnlinePlayersPanelItem : MonoBehaviour
{
    [SerializeField] private RectTransform _topPanel;
    [SerializeField] private RectTransform _bottomPanel;
    [SerializeField] private AvatarFaceLoader _avatarFaceLoader;
    [SerializeField] private Image _onlineStatusIndicator;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private ClanHeartColorSetter _clanHeart;
    [SerializeField] private Button _addfriendButton;
    [SerializeField] private Button _removefriendButton;
    [SerializeField] private Button _acceptFriendButton;
    [SerializeField] private Button _declineFriendButton;
    [SerializeField] private TextMeshProUGUI _addFriendButtonText;
    [SerializeField] private Button _profileButton;

    private bool _isOnline = true;

    public delegate void OnlinePlayerPanelPressed(OnlinePlayersPanelItem handler);
    public static event OnlinePlayerPanelPressed OnPanelPressed;

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

    public IEnumerator Initialize(ServerPlayer player, OnlineState onlineState = OnlineState.Online, FriendState friendstate = FriendState.None, Action onRemoveClick = null, Action onAcceptClick = null, Action onDeclineClick = null, Action onAddFriendClick = null)
    {
        ClanLogo clanLogo = null;
        AvatarVisualData avatarVisualData = null;

        if (player != null)
        {
            clanLogo = player.clanLogo;
            avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(new AvatarData(player.name, player.avatar));
            SetProfileListener(player);
        }

        _nameText.text = player.name;
        _isOnline = onlineState == OnlineState.Online;


        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }

        if (onlineState == OnlineState.Global)
        {
            if(clanLogo != null) _clanHeart.SetHeartColors(clanLogo);
            _clanHeart.gameObject.SetActive(true);
        }
        else
        {
            _clanHeart.gameObject.SetActive(false);
        }

        if(onlineState is OnlineState.Global || friendstate is FriendState.Receiving)
        {
            _onlineStatusIndicator.gameObject.SetActive(false);
        }
        else
        {
            _onlineStatusIndicator.gameObject.SetActive(true);
            UpdateOnlineStatusIndicator();
        }

        if (_addfriendButton != null)
        {
            //_addfriendButton.gameObject.SetActive(true);
            _addfriendButton.onClick.RemoveAllListeners();

            if (friendstate is FriendState.Friend)
            {
                _addfriendButton.interactable = false;
                _addFriendButtonText.text = "Kaveri";
            }
            else if (friendstate is FriendState.Sending)
            {
                _addfriendButton.interactable = false;
                _addFriendButtonText.text = "Kaveripyyntö lähetetty";
            }
            else
            {
                _addfriendButton.interactable = true;
                _addFriendButtonText.text = "Lisää kaveriksi";

                _addfriendButton.onClick.AddListener(() =>
                {
                    onAddFriendClick?.Invoke();
                });
            }
        }

        if (_removefriendButton != null) // Show button only for accepted friends
        {
            _removefriendButton.gameObject.SetActive(onRemoveClick != null && friendstate is FriendState.Friend);

            _removefriendButton.onClick.RemoveAllListeners();
            _removefriendButton.onClick.AddListener(() =>
            {
                onRemoveClick?.Invoke();
            });
        }
        if (_acceptFriendButton != null) // Show button only for pending requests
        {
            _acceptFriendButton.gameObject.SetActive(onAcceptClick != null && friendstate is FriendState.Receiving);

            _acceptFriendButton.onClick.RemoveAllListeners();
            _acceptFriendButton.onClick.AddListener(() =>
            {
                onAcceptClick?.Invoke();
            });
        }
        if (_declineFriendButton != null) // Show button only for pending requests
        {
            _declineFriendButton.gameObject.SetActive(onDeclineClick != null && friendstate is FriendState.Receiving);

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

    private void ButtonPressHandle(OnlinePlayersPanelItem handler)
    {
        UpdateSize(handler == this);
    }

    private void UpdateSize()
    {
        OnPanelPressed?.Invoke(this);
    }

    public void UpdateSize(bool value)
    {
        if (!value)
        {
            GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x, Math.Min(GetComponent<RectTransform>().sizeDelta.x / 5, 100f));
            _topPanel.anchorMin = new(0, 0f);
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

    public void SetOnlineStatus(bool isOnline)
    {
        _isOnline = isOnline;
        UpdateOnlineStatusIndicator();
    }

    private void SetProfileListener(ServerPlayer player)
    {
        if(player != null)
        _profileButton.onClick.AddListener(() =>
        {
            DataCarrier.AddData(DataCarrier.PlayerProfile, new PlayerData(player));
        });
    }
}

