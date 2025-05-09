using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPodium : MonoBehaviour
{
    [Header("First place")]
    [SerializeField] private TextMeshProUGUI _firstName;
    [SerializeField] private TextMeshProUGUI _firstPoints;
    [SerializeField] private GameObject _firstClanHeart;
    [SerializeField] private GameObject _firstClanHeartPlayer;
    [SerializeField] private GameObject _firstAvatarHead;
    [field: SerializeField] public Button FirstOpenClanProfileButton { get; private set; }

    [Header("Second place")]
    [SerializeField] private TextMeshProUGUI _secondName;
    [SerializeField] private TextMeshProUGUI _secondPoints;
    [SerializeField] private GameObject _secondClanHeart;
    [SerializeField] private GameObject _secondClanHeartPlayer;
    [SerializeField] private GameObject _secondAvatarHead;
    [field: SerializeField] public Button SecondOpenClanProfileButton { get; private set; }

    [Header("Third place")]
    [SerializeField] private TextMeshProUGUI _thirdName;
    [SerializeField] private TextMeshProUGUI _thirdPoints;
    [SerializeField] private GameObject _thirdClanHeart;
    [SerializeField] private GameObject _thirdClanHeartPlayer;
    [SerializeField] private GameObject _thirdAvatarHead;
    [field: SerializeField] public Button ThirdOpenClanProfileButton { get; private set; }

    private bool _isClanView = false;

    public void InitilializePodium(int rank, string name, int points, ClanData clanData, ServerClan serverClan)
    {
        switch(rank)
        {
            case 1:
                InitializeFirstPlace(name, points, clanData, serverClan);
                break;
            case 2:
                InitializeSecondPlace(name, points, clanData, serverClan);
                break;
            case 3:
                InitializeThirdPlace(name, points, clanData, serverClan);
                break;
        }
    }

    private void InitializeFirstPlace(string firstName, int firstPoints, ClanData clanData, ServerClan serverClan)
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
            
    }

    private void InitializeSecondPlace(string secondName, int secondPoints, ClanData clanData, ServerClan serverClan)
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
    }

    private void InitializeThirdPlace(string thirdName, int thirdPoints, ClanData clanData, ServerClan serverClan)
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
    }

    public void SetPlayerView()
    {
        _isClanView = false;

        FirstOpenClanProfileButton.interactable = false;
        _firstClanHeart.SetActive(false);
        _firstAvatarHead.SetActive(true);
        _firstClanHeartPlayer.SetActive(true);

        SecondOpenClanProfileButton.interactable = false;
        _secondClanHeart.SetActive(false);
        _secondAvatarHead.SetActive(true);
        _secondClanHeartPlayer.SetActive(true);

        ThirdOpenClanProfileButton.interactable = false;
        _thirdClanHeart.SetActive(false);
        _thirdAvatarHead.SetActive(true);
        _thirdClanHeartPlayer.SetActive(true);
    }

    public void SetClanView()
    {
        _isClanView= true;

        _firstAvatarHead.SetActive(false);
        _firstClanHeartPlayer.SetActive(false);
        _firstClanHeart.SetActive(true);
        FirstOpenClanProfileButton.interactable = true;

        _secondAvatarHead.SetActive(false);
        _secondClanHeartPlayer.SetActive(false);
        _secondClanHeart.SetActive(true);
        SecondOpenClanProfileButton.interactable = true;

        _thirdAvatarHead.SetActive(false);
        _thirdClanHeartPlayer.SetActive(false);
        _thirdClanHeart.SetActive(true);
        ThirdOpenClanProfileButton.interactable = true;
    }
}
