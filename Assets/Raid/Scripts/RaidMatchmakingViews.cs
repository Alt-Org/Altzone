using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaidMatchmakingViews : MonoBehaviour
{
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI matchmakingTitleText;
    [SerializeField] private TextMeshProUGUI matchmakingStatusText;
    [SerializeField] private TextMeshProUGUI matchmakingDetailText;
    [SerializeField] private TextMeshProUGUI lobbyCountdownText;
    [SerializeField] private Transform participantListRoot;
    [SerializeField] private RaidLobbyClanListItem clanListItemTemplate;
    [SerializeField] private Button surrenderButton;
    [SerializeField] private Button debugStartButton;

    public GameObject Root => gameObject;
    public GameObject MatchmakingPanel => matchmakingPanel;
    public GameObject LobbyPanel => lobbyPanel;
    public TextMeshProUGUI MatchmakingTitleText => matchmakingTitleText;
    public TextMeshProUGUI MatchmakingStatusText => matchmakingStatusText;
    public TextMeshProUGUI MatchmakingDetailText => matchmakingDetailText;
    public TextMeshProUGUI LobbyCountdownText => lobbyCountdownText;
    public Transform ParticipantListRoot => participantListRoot;
    public RaidLobbyClanListItem ClanListItemTemplate => clanListItemTemplate;

    private Action _surrenderAction;
    private Action _debugStartAction;

    private void OnDestroy()
    {
        if (surrenderButton != null)
        {
            surrenderButton.onClick.RemoveListener(OnSurrenderPressed);
        }

        if (debugStartButton != null)
        {
            debugStartButton.onClick.RemoveListener(OnDebugStartPressed);
        }
    }

    public void Initialize(Action surrenderAction, Action debugStartAction)
    {
        _surrenderAction = surrenderAction;
        _debugStartAction = debugStartAction;

        if (surrenderButton != null)
        {
            surrenderButton.onClick.RemoveListener(OnSurrenderPressed);
            surrenderButton.onClick.AddListener(OnSurrenderPressed);
        }

        if (debugStartButton != null)
        {
            debugStartButton.onClick.RemoveListener(OnDebugStartPressed);
            debugStartButton.onClick.AddListener(OnDebugStartPressed);
        }
    }

    private void OnSurrenderPressed()
    {
        _surrenderAction?.Invoke();
    }

    private void OnDebugStartPressed()
    {
        _debugStartAction?.Invoke();
    }
}
