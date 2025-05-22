using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using MenuUi.Scripts.Window;
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

    public void InitilializePodium(int rank, string name, int points, ClanData clanData, ServerClan serverClan)
    {
        switch (rank)
        {
            case 1:
                InitializeFirstPlace(name, points, clanData, serverClan, null);
                break;
            case 2:
                InitializeSecondPlace(name, points, clanData, serverClan, null);
                break;
            case 3:
                InitializeThirdPlace(name, points, clanData, serverClan, null);
                break;
        }
    }

    public void InitilializePodium(int rank, string name, int points, PlayerData playerData)
    {
        switch (rank)
        {
            case 1:
                InitializeFirstPlace(name, points, null, null, playerData);
                break;
            case 2:
                InitializeSecondPlace(name, points, null, null, playerData);
                break;
            case 3:
                InitializeThirdPlace(name, points, null, null, playerData);
                break;
        }
    }

    private void InitializeFirstPlace(string firstName, int firstPoints, ClanData clanData, ServerClan serverClan, PlayerData playerData)
    {
        _firstName.text = firstName;
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

                if (/*ranking.Player.SelectedCharacterId != 201 &&*/ playerData.SelectedCharacterId != 0)
                {
                    avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(playerData);
                }
                if (avatarVisualData != null)
                {
                    _firstAvatarHead.GetComponent<AvatarFaceLoader>().UpdateVisuals(avatarVisualData);
                }
            }

        }
    }

    private void InitializeSecondPlace(string secondName, int secondPoints, ClanData clanData, ServerClan serverClan, PlayerData playerData)
    {
        _secondName.text = secondName;
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

                if (/*ranking.Player.SelectedCharacterId != 201 &&*/ playerData.SelectedCharacterId != 0)
                {
                    avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(playerData);
                }
                if (avatarVisualData != null)
                {
                    _secondAvatarHead.GetComponent<AvatarFaceLoader>().UpdateVisuals(avatarVisualData);
                }
            }
        }
    }

    private void InitializeThirdPlace(string thirdName, int thirdPoints, ClanData clanData, ServerClan serverClan, PlayerData playerData)
    {
        _thirdName.text = thirdName;
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

                if (/*ranking.Player.SelectedCharacterId != 201 &&*/ playerData.SelectedCharacterId != 0)
                {
                    avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(playerData);
                }
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
