using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using Altzone.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPodium : MonoBehaviour
{
    [SerializeField] private GameObject[] _playerProfileButtons;
    [SerializeField] private GameObject[] _clanProfileButtons;

    [Header("First place")]
    [SerializeField] private TextMeshProUGUI _firstName;
    [SerializeField] private TextMeshProUGUI _firstPoints;
    [SerializeField] private GameObject _firstClanHeart;
    [SerializeField] private GameObject _firstClanHeartPlayer;
    [SerializeField] private GameObject _firstAvatarHead;
    [field: SerializeField] public Button FirstOpenClanProfileButton { get; private set; }
    [field: SerializeField] public Button FirstOpenPlayerProfileButton { get; private set; }

    [Header("Second place")]
    [SerializeField] private TextMeshProUGUI _secondName;
    [SerializeField] private TextMeshProUGUI _secondPoints;
    [SerializeField] private GameObject _secondClanHeart;
    [SerializeField] private GameObject _secondClanHeartPlayer;
    [SerializeField] private GameObject _secondAvatarHead;
    [field: SerializeField] public Button SecondOpenClanProfileButton { get; private set; }
    [field: SerializeField] public Button SecondOpenPlayerProfileButton { get; private set; }

    [Header("Third place")]
    [SerializeField] private TextMeshProUGUI _thirdName;
    [SerializeField] private TextMeshProUGUI _thirdPoints;
    [SerializeField] private GameObject _thirdClanHeart;
    [SerializeField] private GameObject _thirdClanHeartPlayer;
    [SerializeField] private GameObject _thirdAvatarHead;
    [field: SerializeField] public Button ThirdOpenClanProfileButton { get; private set; }
    [field: SerializeField] public Button ThirdOpenPlayerProfileButton { get; private set; }

    private bool _isClanView = false;

    private PlayerData _firstPlayerData;
    private PlayerData _secondPlayerData;
    private PlayerData _thirdPlayerData;

    private string TruncateName(string name, int maxLength = 24)
    {
        if (string.IsNullOrEmpty(name)) return "";
        if (name.Length <= maxLength) return name;
        return name.Substring(0, maxLength - 3) + "...";
    }

    public void InitilializePodium(int rank, string name, int points, ClanData clanData, ServerClan serverClan)
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

    public void InitilializePodium(int rank, string name, int points, PlayerData playerData)
    {
        switch (rank)
        {
            case 1:
                InitializeFirstPlace(name, points, playerData: playerData);
                break;
            case 2:
                InitializeSecondPlace(name, points, playerData: playerData);
                break;
            case 3:
                InitializeThirdPlace(name, points, playerData: playerData);
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

    private void InitializeFirstPlace(string firstName, int firstPoints, ClanData clanData = null, ServerClan serverClan = null, PlayerData playerData = null, ClanLogo logo= null)
    {
        _firstName.text = TruncateName(firstName);
        _firstPoints.text = firstPoints.ToString();

        if (_isClanView)
        {
            // Clan heart colors
            ClanHeartColorSetter clanheart = _firstClanHeart.GetComponentInChildren<ClanHeartColorSetter>();
            clanheart.SetOtherClanColors(clanData);

            // Open Clan Profile
            FirstOpenClanProfileButton.onClick.AddListener(() =>
            {
                DataCarrier.AddData(DataCarrier.ClanListing, serverClan);
            });
        }
        else
        {
            if (playerData != null)
            {
                FirstOpenPlayerProfileButton.onClick.RemoveListener(FirstAddDataCarrierData); // Remove in case the button already has another player's info
                _firstPlayerData = playerData;
                FirstOpenPlayerProfileButton.onClick.AddListener(FirstAddDataCarrierData);

                AvatarVisualData avatarVisualData = null;

               
                
                    avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData);
                
                if (avatarVisualData != null)
                {
                    _firstAvatarHead.GetComponent<AvatarFaceLoader>().UpdateVisuals(avatarVisualData);
                }
            }

        }
    }

    private void InitializeSecondPlace(string secondName, int secondPoints, ClanData clanData = null, ServerClan serverClan = null, PlayerData playerData = null, ClanLogo logo = null)
    {
        _secondName.text = TruncateName(secondName);
        _secondPoints.text = secondPoints.ToString();

        if (_isClanView)
        {
            //Clan heart colors
            ClanHeartColorSetter clanheart = _secondClanHeart.GetComponentInChildren<ClanHeartColorSetter>();
            clanheart.SetOtherClanColors(clanData);

            // Open Clan Profile
            SecondOpenClanProfileButton.onClick.AddListener(() =>
            {
                DataCarrier.AddData(DataCarrier.ClanListing, serverClan);
            });
        }
        else
        {
            if (playerData != null)
            {
                SecondOpenPlayerProfileButton.onClick.RemoveListener(SecondAddDataCarrierData); // Remove in case the button already has another player's info
                _secondPlayerData = playerData;
                SecondOpenPlayerProfileButton.onClick.AddListener(SecondAddDataCarrierData);

                AvatarVisualData avatarVisualData = null;

            
                
                    avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData);
                
                if (avatarVisualData != null)
                {
                    _secondAvatarHead.GetComponent<AvatarFaceLoader>().UpdateVisuals(avatarVisualData);
                }
            }
        }
    }

    private void InitializeThirdPlace(string thirdName, int thirdPoints, ClanData clanData = null, ServerClan serverClan = null, PlayerData playerData = null, ClanLogo logo = null)
    {
        _thirdName.text =TruncateName(thirdName);
        _thirdPoints.text = thirdPoints.ToString();

        if (_isClanView)
        {
            //Clan heart colors
            ClanHeartColorSetter clanheart = _thirdClanHeart.GetComponentInChildren<ClanHeartColorSetter>();
            clanheart.SetOtherClanColors(clanData);

            // Open Clan Profile
            ThirdOpenClanProfileButton.onClick.AddListener(() =>
            {
                DataCarrier.AddData(DataCarrier.ClanListing, serverClan);
            });
        }
        else
        {
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
                
                if (avatarVisualData != null)
                {
                    _thirdAvatarHead.GetComponent<AvatarFaceLoader>().UpdateVisuals(avatarVisualData);
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
