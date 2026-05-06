using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Window;
using MenuUi.Scripts.Lobby.InLobby;
using MenuUi.Scripts.AvatarEditor;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Signals;
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

public class OnlinePlayersPanelItem : AltMonoBehaviour
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
    [SerializeField] private Button _inviteButton;

    private ServerPlayer _player = null;
    private FriendPlayer _friend = null;
    private OnlineState _onlineState = OnlineState.Offline;
    private FriendState _friendstate = FriendState.None;

    public ServerPlayer Player => _player;
    public FriendPlayer Friend => _friend;
    public bool IsOnline => _onlineState is OnlineState.Online;
    public FriendState Friendstate => _friendstate;

    public delegate void OnlinePlayerPanelPressed(OnlinePlayersPanelItem handler);
    public static event OnlinePlayerPanelPressed OnPanelPressed;

    public delegate void ContentRefreshRequested();
    public static event ContentRefreshRequested OnContentRefreshRequested;

    public delegate void PlayerPanelCloseRequested();
    public static event PlayerPanelCloseRequested OnPlayerPanelCloseRequested;

    private void OnEnable()
    {
        OnPanelPressed += ButtonPressHandle;
    }
    private void OnDisable()
    {
        OnPanelPressed -= ButtonPressHandle;
        UpdateSize(false);
    }

    public IEnumerator Initialize(FriendPlayer player, OnlineState onlineState = OnlineState.Online, FriendState friendstate = FriendState.None, Action onRemoveClick = null, Action onAcceptClick = null, Action onDeclineClick = null, Action onAddFriendClick = null)
    {
        ClanLogo clanLogo = null;
        AvatarVisualData avatarVisualData = null;

        if (player != null)
        {
            _friend = player;
            avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(player.avatar);
            SetProfileListener(player._id);
        }

        _nameText.text = player.name;
        _onlineState = onlineState;
        _friendstate = friendstate;

        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }

        yield return SetStates(player._id, clanLogo, onlineState, friendstate, onRemoveClick, onAcceptClick, onDeclineClick, onAddFriendClick);
    }

    public IEnumerator Initialize(ServerPlayer player, OnlineState onlineState = OnlineState.Online, FriendState friendstate = FriendState.None, Action onRemoveClick = null, Action onAcceptClick = null, Action onDeclineClick = null, Action onAddFriendClick = null)
    {
        ClanLogo clanLogo = null;
        AvatarVisualData avatarVisualData = null;

        if (player != null)
        {
            _player = player;
            clanLogo = player.clanLogo;
            avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(new PlayerData(player));
            SetProfileListener(player);
        }

        _nameText.text = player.name;
        _onlineState = onlineState;
        _friendstate = friendstate;

        if (avatarVisualData != null)
        {
            _avatarFaceLoader.UpdateVisuals(avatarVisualData);
        }
        
        yield return SetStates(player._id, clanLogo, onlineState, friendstate, onRemoveClick, onAcceptClick, onDeclineClick, onAddFriendClick);
    }

    private IEnumerator SetStates(string id, ClanLogo clanLogo, OnlineState onlineState, FriendState friendstate, Action onRemoveClick, Action onAcceptClick, Action onDeclineClick, Action onAddFriendClick)
    {
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

            if (id == ServerManager.Instance.Player._id)
            {
                _addfriendButton.gameObject.SetActive(false);
            }
            else
            {
                if (friendstate is FriendState.Friend)
                {
                    _addfriendButton.gameObject.SetActive(false);
                    _addfriendButton.interactable = false;
                    _addFriendButtonText.text = "Kaveri";
                }
                else if (friendstate is FriendState.Sending)
                {
                    _addfriendButton.gameObject.SetActive(true);
                    _addfriendButton.interactable = false;
                    _addFriendButtonText.text = "Kaveripyyntö lähetetty";
                }
                else if (friendstate is FriendState.Receiving)
                {
                    _addfriendButton.gameObject.SetActive(false);
                    _addfriendButton.interactable = false;
                    _addFriendButtonText.text = "Kaveripyyntö tulossa.";
                }
                else
                {
                    _addfriendButton.gameObject.SetActive(true);
                    _addfriendButton.interactable = true;
                    _addFriendButtonText.text = "Lisää kaveriksi";

                    _addfriendButton.onClick.AddListener(() =>
                    {
                        onAddFriendClick?.Invoke();
                    });
                }
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

        if (_inviteButton != null)
        {
            _inviteButton.onClick.RemoveAllListeners();

            if (id == ServerManager.Instance.Player._id || string.IsNullOrEmpty(id))
            {
                _inviteButton.gameObject.SetActive(false);
            }
            else
            {
                _inviteButton.gameObject.SetActive(true);
                // Disable the invite button if the player is offline
                _inviteButton.interactable = IsOnline;

                // Only attempt to send invite if the player is online when the button is pressed
                _inviteButton.onClick.AddListener(() =>
                {
                    if (IsOnline) StartCoroutine(InviteSelectedPlayerRoutine());
                });
            }
        }

        GetComponent<Button>().onClick.AddListener(() => UpdateSize());

        yield return new WaitForEndOfFrame();

        UpdateSize(false);
    }

    private void ButtonPressHandle(OnlinePlayersPanelItem handler)
    {
        bool value = handler == this;
        if (value && _bottomPanel.gameObject.activeSelf) value = false;
        UpdateSize(value);
    }

    private void UpdateSize()
    {
        OnPanelPressed?.Invoke(this);
    }

    public void UpdateSize(bool value)
    {
        if (!value)
        {
            GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x, 100f);
            _topPanel.anchorMin = new(0, 0f);
            _bottomPanel.gameObject.SetActive(false);
        }
        else
        {
            GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x, 200f);
            _topPanel.anchorMin = new(0, 0.5f);
            _bottomPanel.gameObject.SetActive(true);
        }
        OnContentRefreshRequested?.Invoke();
    }

    private void UpdateOnlineStatusIndicator()
    {
        _onlineStatusIndicator.color = IsOnline ? Color.green : Color.red;
    }

    private void UpdateFriendIndicator()
    {
        if (_friendstate is FriendState.Friend)
        {
            _addfriendButton.gameObject.SetActive(false);
            _addfriendButton.interactable = false;
            _addFriendButtonText.text = "Kaveri";
            if (_onlineState is not OnlineState.Global)
            {
                _onlineStatusIndicator.gameObject.SetActive(true);
                UpdateOnlineStatusIndicator();
            }
            _acceptFriendButton.gameObject.SetActive(false);
            _declineFriendButton.gameObject.SetActive(false);
            _removefriendButton.gameObject.SetActive(true);
        }
        else if (_friendstate is FriendState.Sending)
        {
            _addfriendButton.gameObject.SetActive(true);
            _addfriendButton.interactable = false;
            _addFriendButtonText.text = "Kaveripyyntö lähetetty";
            if (_onlineState is not OnlineState.Global)
            {
                _onlineStatusIndicator.gameObject.SetActive(true);
                UpdateOnlineStatusIndicator();
            }
            _acceptFriendButton.gameObject.SetActive(false);
            _declineFriendButton.gameObject.SetActive(false);
            _removefriendButton.gameObject.SetActive(false);
        }
        else if (_friendstate is FriendState.Receiving && _friend != null)
        {
            _addfriendButton.gameObject.SetActive(false);
            _addfriendButton.interactable = false;
            _addFriendButtonText.text = "Kaveripyyntö tulossa.";
            _onlineStatusIndicator.gameObject.SetActive(false);
            _acceptFriendButton.gameObject.SetActive(true);
            _declineFriendButton.gameObject.SetActive(true);
            _removefriendButton.gameObject.SetActive(false);
        }
        else
        {
            _addfriendButton.gameObject.SetActive(true);
            _addfriendButton.interactable = true;
            _addFriendButtonText.text = "Lisää kaveriksi";
            if (_onlineState is not OnlineState.Global)
            {
                _onlineStatusIndicator.gameObject.SetActive(true);
                UpdateOnlineStatusIndicator();
            }
            _acceptFriendButton.gameObject.SetActive(false);
            _declineFriendButton.gameObject.SetActive(false);
            _removefriendButton.gameObject.SetActive(false);
        }
    }

    public void SetOnlineStatus(OnlineState onlineState)
    {
        _onlineState = onlineState;
        UpdateOnlineStatusIndicator();
    }
    public void SetFriendStatus(FriendState friendstate)
    {
        _friendstate = friendstate;
        UpdateFriendIndicator();
    }

    private void SetProfileListener(ServerPlayer player)
    {
        if(player != null)
        _profileButton.onClick.AddListener(() =>
        {
            StartCoroutine(GetProfile(player));
        });
    }
    private void SetProfileListener(string id)
    {
        if (!string.IsNullOrEmpty(id))
            _profileButton.onClick.AddListener(() =>
            {
                StartCoroutine(GetFriendProfile(id));
            });
    }

    private IEnumerator GetProfile(ServerPlayer player)
    {
        DataCarrier.AddData(DataCarrier.PlayerProfile, new PlayerData(player));
        yield return _profileButton.GetComponent<WindowNavigation>().Navigate();
        OnPlayerPanelCloseRequested?.Invoke();

    }

    private IEnumerator GetFriendProfile(string id)
    {
        ServerPlayer serverPlayer = null;
        bool timeout = false;

        StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(id, c => serverPlayer = c)); // Get friend data
        StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
        yield return new WaitUntil(() => serverPlayer != null || timeout);

        DataCarrier.AddData(DataCarrier.PlayerProfile, new PlayerData(serverPlayer));
        yield return _profileButton.GetComponent<WindowNavigation>().Navigate();
        OnPlayerPanelCloseRequested?.Invoke();
    }

    private IEnumerator InviteSelectedPlayerRoutine()
    {
        string invitedUserId = string.Empty;
        if (_player != null) invitedUserId = _player._id;
        else if (_friend != null) invitedUserId = _friend._id;
        if (string.IsNullOrEmpty(invitedUserId)) yield break;

        bool result = false;
        try
        {
            GameType targetGameType = InLobbyController.SelectedGameType == GameType.Clan2v2
                ? GameType.Clan2v2
                : GameType.Random2v2;
            Debug.Log($"InviteSelectedPlayerRoutine: sending invite to '{invitedUserId}' for targetGameType={targetGameType}");
            result = PhotonRealtimeClient.SendPremadeInvite(invitedUserId, targetGameType);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"InviteSelectedPlayerRoutine: exception while sending invite: {ex.Message}");
        }

        if (!result)
        {
            Debug.LogWarning($"InviteSelectedPlayerRoutine: invite failed to send to {invitedUserId}");
        }
        else
        {
            // Open the battle popup for the active invite mode so the in-room waiting panel appears
            try
            {
                SignalBus.OnBattlePopupRequestedSignal(
                    InLobbyController.SelectedGameType == GameType.Clan2v2 ? GameType.Clan2v2 : GameType.FriendLobby);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"InviteSelectedPlayerRoutine: failed to open battle popup: {ex.Message}");
            }

            OnPlayerPanelCloseRequested?.Invoke();
        }

        yield break;
    }
}

