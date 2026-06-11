using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPodium : MonoBehaviour
{
    [SerializeField] private GameObject[] _playerProfileButtons;
    [SerializeField] private GameObject[] _clanProfileButtons;

    [Header("First place")]
    [SerializeField] private TextMeshProUGUI _firstPlayerName;
    [SerializeField] private TextMeshProUGUI _firstClanName;
    [SerializeField] private TextMeshProUGUI _firstPoints;
    [SerializeField] private GameObject _firstClanHeart;
    [SerializeField] private GameObject _firstClanHeartPlayer;
    [SerializeField] private GameObject _firstAvatarHead;
    [SerializeField] private GameObject _firstClanBox;
    [SerializeField] private GameObject _firstPlayerBox;
    [SerializeField] private AvatarLoader _firstAvatarLoader;
    [field: SerializeField] public Button FirstOpenClanProfileButton { get; private set; }
    [field: SerializeField] public Button FirstOpenPlayerProfileButton { get; private set; }

    [Header("Second place")]
    [SerializeField] private TextMeshProUGUI _secondPlayerName;
    [SerializeField] private TextMeshProUGUI _secondClanName;
    [SerializeField] private TextMeshProUGUI _secondPoints;
    [SerializeField] private GameObject _secondClanHeart;
    [SerializeField] private GameObject _secondClanHeartPlayer;
    [SerializeField] private GameObject _secondAvatarHead;
    [SerializeField] private GameObject _secondClanBox;
    [SerializeField] private GameObject _secondPlayerBox;
    [SerializeField] private AvatarLoader _secondAvatarLoader;
    [field: SerializeField] public Button SecondOpenClanProfileButton { get; private set; }
    [field: SerializeField] public Button SecondOpenPlayerProfileButton { get; private set; }

    [Header("Third place")]
    [SerializeField] private TextMeshProUGUI _thirdPlayerName;
    [SerializeField] private TextMeshProUGUI _thirdClanName;
    [SerializeField] private TextMeshProUGUI _thirdPoints;
    [SerializeField] private GameObject _thirdClanHeart;
    [SerializeField] private GameObject _thirdClanHeartPlayer;
    [SerializeField] private GameObject _thirdAvatarHead;
    [SerializeField] private GameObject _thirdClanBox;
    [SerializeField] private GameObject _thirdPlayerBox;
    [SerializeField] private AvatarLoader _thirdAvatarLoader;
    [field: SerializeField] public Button ThirdOpenClanProfileButton { get; private set; }
    [field: SerializeField] public Button ThirdOpenPlayerProfileButton { get; private set; }

    private bool _isClanView = false;

    private PlayerData _firstPlayerData;
    private PlayerData _secondPlayerData;
    private PlayerData _thirdPlayerData;


    public void InitilializePodiumClan(int rank, string name, int points, ClanData clanData, ServerClan serverClan)
    {
        switch (rank)
        {
            case 1:
                InitializeFirstPlace(name, points, clanData: clanData, serverClan: serverClan);
                break;
            case 2:
                InitializeSecondPlace(name, points, clanData: clanData, serverClan: serverClan);
                break;
            case 3:
                InitializeThirdPlace(name, points, clanData: clanData, serverClan: serverClan);
                break;
        }
    }

    public void InitilializePodium(int rank, string name, int points, PlayerData playerData, AvatarVisualData avatarData)
    {
        switch (rank)
        {
            case 1:
                InitializeFirstPlace(name, points, playerData: playerData, avatarData: avatarData);
                break;
            case 2:
                InitializeSecondPlace(name, points, playerData: playerData, avatarData: avatarData);
                break;
            case 3:
                InitializeThirdPlace(name, points, playerData: playerData, avatarData: avatarData);
                break;
        }
    }


    public void InitilializePodium(int rank, PlayerLeaderboard ranking)
    {
        switch (rank)
        {
            case 1:
                InitializeFirstPlace(ranking.Player.Name, ranking.Points, playerData: ranking.Player, logo: ranking.Clanlogo);
                break;
            case 2:
                InitializeSecondPlace(ranking.Player.Name, ranking.Points, playerData: ranking.Player, logo: ranking.Clanlogo);
                break;
            case 3:
                InitializeThirdPlace(ranking.Player.Name, ranking.Points, playerData: ranking.Player, logo: ranking.Clanlogo);
                break;
        }
    }

    private void InitializeFirstPlace(string firstName, int firstPoints, ClanData clanData = null, ServerClan serverClan = null, PlayerData playerData = null, ClanLogo logo= null, AvatarVisualData avatarData = null)
    {
        _firstPoints.text = firstPoints.ToString() + " pts";

        _firstClanBox.gameObject.SetActive(false);
        _firstPlayerBox.gameObject.SetActive(false);

        if (_isClanView)
        {
            // Clan heart colors
            ClanHeartColorSetter clanheart = _firstClanHeart.GetComponentInChildren<ClanHeartColorSetter>();
            clanheart.SetOtherClanColors(clanData);
            _firstClanBox.gameObject.SetActive(true);
            _firstClanName.text = firstName;

            // Open Clan Profile
            FirstOpenClanProfileButton.onClick.AddListener(() =>
            {
                DataCarrier.AddData(DataCarrier.ClanListing, serverClan);
            });
        }
        else
        {
            _firstPlayerBox.gameObject.SetActive(true);
            _firstPlayerName.text = firstName;

            if (playerData != null)
            {

                FirstOpenPlayerProfileButton.onClick.RemoveListener(FirstAddDataCarrierData); // Remove in case the button already has another player's info
                _firstPlayerData = playerData;
                FirstOpenPlayerProfileButton.onClick.AddListener(FirstAddDataCarrierData);

                AvatarVisualData avatarVisualData = null;
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData);
                
                if (avatarVisualData != null && isActiveAndEnabled)
                {
                    _firstAvatarHead.GetComponent<AvatarLoader>().UpdateVisuals(avatarVisualData);
                }
            }
            else if (avatarData != null)
            {
                FirstOpenPlayerProfileButton.onClick.RemoveListener(FirstAddDataCarrierData); // Remove in case the button already has another player's info
                //_firstPlayerData = avatarData;
                FirstOpenPlayerProfileButton.onClick.AddListener(FirstAddDataCarrierData);

                if (firstName == ServerManager.Instance.Player.name.ToString())
                {
                    _firstAvatarLoader.SetUseOwnAvatarVisuals(true);
                }
                else if (avatarData != null && isActiveAndEnabled)
                {
                    _firstAvatarHead.GetComponent<AvatarLoader>().UpdateVisuals(avatarData);
                }
            }

        }
    }


    private void InitializeSecondPlace(string secondName, int secondPoints, ClanData clanData = null, ServerClan serverClan = null, PlayerData playerData = null, ClanLogo logo = null, AvatarVisualData avatarData = null)
    {
        _secondPoints.text = secondPoints.ToString() + " pts";

        _secondClanBox.gameObject.SetActive(false);
        _secondPlayerBox.gameObject.SetActive(false);

        if (secondName == ServerManager.Instance.Player.name)
        {
            _secondAvatarLoader.SetUseOwnAvatarVisuals(true);
        }

        if (_isClanView)
        {
            //Clan heart colors
            ClanHeartColorSetter clanheart = _secondClanHeart.GetComponentInChildren<ClanHeartColorSetter>();
            clanheart.SetOtherClanColors(clanData);

            _secondClanBox.gameObject.SetActive(true);
            _secondClanName.text = secondName;

            // Open Clan Profile
            SecondOpenClanProfileButton.onClick.AddListener(() =>
            {
                DataCarrier.AddData(DataCarrier.ClanListing, serverClan);
            });
        }
        else
        {
            _secondPlayerBox.gameObject.SetActive(true);
            _secondPlayerName.text = secondName;

            if (playerData != null)
            {

                SecondOpenPlayerProfileButton.onClick.RemoveListener(SecondAddDataCarrierData); // Remove in case the button already has another player's info
                _secondPlayerData = playerData;
                SecondOpenPlayerProfileButton.onClick.AddListener(SecondAddDataCarrierData);

                AvatarVisualData avatarVisualData = null;                      
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData);

                if (avatarVisualData != null && isActiveAndEnabled)
                {
                    _secondAvatarHead.GetComponent<AvatarLoader>().UpdateVisuals(avatarVisualData);
                }
            }
            else if (avatarData != null)
            {

                SecondOpenPlayerProfileButton.onClick.RemoveListener(FirstAddDataCarrierData); // Remove in case the button already has another player's info
                SecondOpenPlayerProfileButton.onClick.AddListener(FirstAddDataCarrierData);

                if (secondName == ServerManager.Instance.Player.name)
                {
                    _secondAvatarLoader.SetUseOwnAvatarVisuals(true);
                }
                else if (avatarData != null && isActiveAndEnabled)
                {
                    _secondAvatarHead.GetComponent<AvatarLoader>().UpdateVisuals(avatarData);
                }
            }
        }
    }

    private void InitializeThirdPlace(string thirdName, int thirdPoints, ClanData clanData = null, ServerClan serverClan = null, PlayerData playerData = null, ClanLogo logo = null, AvatarVisualData avatarData = null)
    {
        _thirdPoints.text = thirdPoints.ToString() + " pts";

        _thirdClanBox.gameObject.SetActive(false);
        _thirdPlayerBox.gameObject.SetActive(false);

        if (_isClanView)
        {
            //Clan heart colors
            ClanHeartColorSetter clanheart = _thirdClanHeart.GetComponentInChildren<ClanHeartColorSetter>();
            clanheart.SetOtherClanColors(clanData);
            _thirdClanBox.gameObject.SetActive(true);
            _thirdClanName.text = thirdName;

            // Open Clan Profile
            ThirdOpenClanProfileButton.onClick.AddListener(() =>
            {
                DataCarrier.AddData(DataCarrier.ClanListing, serverClan);
            });
        }
        else
        {
            _thirdPlayerBox.gameObject.SetActive(true);
            _thirdPlayerName.text = thirdName;

            if (playerData != null)
            {

                ThirdOpenPlayerProfileButton.onClick.RemoveListener(ThirdAddDataCarrierData); // Remove incase the button already has another player's info
                _thirdPlayerData = playerData;
                ThirdOpenPlayerProfileButton.onClick.AddListener(ThirdAddDataCarrierData);
                ThirdOpenPlayerProfileButton.onClick.AddListener(() =>
                {
                    
                });

                AvatarVisualData avatarVisualData = null;                      
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData);
                
                if (avatarVisualData != null && isActiveAndEnabled)
                {
                    _thirdAvatarHead.GetComponent<AvatarLoader>().UpdateVisuals(avatarVisualData);
                }
            }
            else if (avatarData != null)
            {

                ThirdOpenPlayerProfileButton.onClick.RemoveListener(FirstAddDataCarrierData); // Remove in case the button already has another player's info
                //_firstPlayerData = friendPlayer.clan_id;
                ThirdOpenPlayerProfileButton.onClick.AddListener(FirstAddDataCarrierData);

                if (thirdName == ServerManager.Instance.Player.name)
                {
                    _thirdAvatarLoader.SetUseOwnAvatarVisuals(true);
                }
                else if (avatarData != null && isActiveAndEnabled)
                {
                    _thirdAvatarHead.GetComponent<AvatarLoader>().UpdateVisuals(avatarData);
                }
            }
        }
    }

    private void FirstAddDataCarrierData()
    {
        DataCarrier.AddData(DataCarrier.PlayerProfile, _firstPlayerData);
    }
    private void SecondAddDataCarrierData()
    {
        DataCarrier.AddData(DataCarrier.PlayerProfile, _secondPlayerData);
    }
    private void ThirdAddDataCarrierData()
    {
        DataCarrier.AddData(DataCarrier.PlayerProfile, _thirdPlayerData);
    }

    public void SetPlayerView()
    {
        _isClanView = false;

        _firstClanHeart.SetActive(false);
        _firstAvatarHead.SetActive(true);
        _firstClanHeartPlayer.SetActive(true);

        _secondClanHeart.SetActive(false);
        _secondAvatarHead.SetActive(true);
        _secondClanHeartPlayer.SetActive(true);

        _thirdClanHeart.SetActive(false);
        _thirdAvatarHead.SetActive(true);
        _thirdClanHeartPlayer.SetActive(true);

        foreach (GameObject button in _clanProfileButtons)
        {
            button.SetActive(false);
        }

        foreach (GameObject button in _playerProfileButtons)
        {
            button.SetActive(true);
        }
    }

    public void SetClanView()
    {
        _isClanView = true;

        _firstAvatarHead.SetActive(false);
        _firstClanHeartPlayer.SetActive(false);
        _firstClanHeart.SetActive(true);

        _secondAvatarHead.SetActive(false);
        _secondClanHeartPlayer.SetActive(false);
        _secondClanHeart.SetActive(true);

        _thirdAvatarHead.SetActive(false);
        _thirdClanHeartPlayer.SetActive(false);
        _thirdClanHeart.SetActive(true);

        foreach (GameObject button in _clanProfileButtons)
        {
            button.SetActive(true);
        }

        foreach (GameObject button in _playerProfileButtons)
        {
            button.SetActive(false);
        }
    }
}
