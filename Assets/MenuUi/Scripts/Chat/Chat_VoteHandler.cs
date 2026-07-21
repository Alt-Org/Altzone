using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Window;
using MenuUi.Scripts.AvatarEditor;
using MenuUi.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static OnlinePlayersPanelItem;

public class ChatVoteHandler : AltMonoBehaviour
{
    [Header("Base Avatar")]
    [SerializeField] private AvatarFaceLoader _avatar;
    [SerializeField] private string _id;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _date;

    [Header("Select removal")]
    [SerializeField] private GameObject _voteRegime;
    [SerializeField] private GameObject _voteUserPrefab;
    [SerializeField] private Transform _candidateBoard;
    [SerializeField] private Button _voteButton;
    [SerializeField] private VotersData _selectedCandidate; 
    //Voting List
    [SerializeField] private List<VotersData> _candidatesList;
    [SerializeField] private List<SavedVotersData> _candidatesTest; //Mainly just a place holder for data testing
    [SerializeField] private List<string> _votedUsers; //Used to check who has voted already


    [Header("Removing Player")]
    [SerializeField] private GameObject _voteRemove; 
    [SerializeField] private AvatarFaceLoader _removeAvatar;
    [SerializeField] private string _removeid;
    [SerializeField] private TextMeshProUGUI _removeName;
    [SerializeField] private TextMeshProUGUI _removeDate;

    [SerializeField] private Button _checkProfile;
    [SerializeField] private Button _buttonRemoveVote;
    [SerializeField] private GameObject _votedDone;
    public static event PlayerPanelCloseRequested OnPlayerPanelCloseRequested;


    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI _countDownText;
    [SerializeField] private float  _countDownTime;
    [SerializeField] private bool _timesout;
    [SerializeField] private GameObject _voteEndCover;


    [SerializeField] private Button _revertButton;
    [SerializeField] private string _playerID;


    void Start()
    {

        StartCoroutine(GetPlayerData(player =>
        {
            _playerID = player.Id;
        }));

        SetMessageVoteInfo();

        foreach (var i in _candidatesTest)
        {
        CandidateData(i.Name, i.Id);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_candidateBoard.GetComponent<RectTransform>());




        _voteButton.onClick.AddListener(() => ChangeToRemoveSection());

        _revertButton.onClick.AddListener(() => Revert());
    }


    ///Countdown system
    private void Update()
    {
        _countDownTime -= Time.deltaTime;

        if (_countDownTime <= 0)
        {
            _countDownText.text = "Time is out!";
            enabled = false;
            _timesout = true;

            _voteRegime.SetActive(false);
            _voteRemove.SetActive(false);
            _votedDone.SetActive(false);
            _voteEndCover.SetActive(true);
            return;
        }


        int hour = Mathf.FloorToInt(_countDownTime / 3600);
        int minute = Mathf.FloorToInt((_countDownTime % 3600) / 60);
        int sec = Mathf.FloorToInt(_countDownTime % 60);

        _countDownText.text = $"{hour : 00} : {minute : 00} : {sec: 00}";
    }


    //Checks first if player has or hasent voted yet if no then proceed
    void ChangeToRemoveSection()
    {
        if (_candidatesList.All(x => x.IsSelected == false))
        {
            Debug.LogWarning("FIND ME: candidate not found");
            return;
        }

        _voteRegime.SetActive(false);
        _voteRemove.SetActive(true);

        //Imports candidate data
        RemovePlayerData(_selectedCandidate);

        //Voting system
        StartCoroutine(GetPlayerData(player =>
        {
            _buttonRemoveVote.onClick.AddListener(() => VoteRemoveButton(player.Id, player.Name));
        }));

    }


    ///Goes back to voting selection
    void Revert()
    {
        _voteRemove.SetActive(false);
        _voteRegime.SetActive(true);
    }


    //Sets the users who put up the vote up data (it only uses the current users data right now as a place holder)
    public void SetMessageVoteInfo()
    {
        StartCoroutine(GetPlayerData(player =>
        {
         if (player.AvatarData != null) _avatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(player.AvatarData));
        _id = player.Id;
        _name.text = player.Name;


            DateTime now = DateTime.Now;

            _time.text = $"{now.Hour:D2}:{now.Minute:D2}";
            _date.text = $"{now.Day}/{now.Month}/{now.Year}";
        }));

    }


    ///Adds the Candidates to the selection
    private void CandidateData(string Usersname, string UserID)
    {
        if (!_voteRegime.gameObject.activeSelf)
            return;

        GameObject newCandidate = Instantiate(_voteUserPrefab, _candidateBoard);
        Chat_VoteNameData userData = newCandidate.GetComponent<Chat_VoteNameData>();




        _candidatesList.Add(new VotersData { VoteObject = userData.gameObject, Name = Usersname, Id = UserID, Button = userData.VoteButton, IsSelected = false, ButtonColor = userData.ButtonColor});
        userData.SetUserInfo(Usersname, UserID);
        userData.VoteButton.onClick.AddListener(() => buttonClicked(userData));

    }

    ///Selecting the Candidate:
    /*[Sets the every button back to original color before checking who got selected/deselected]*/
    public void buttonClicked(Chat_VoteNameData userData)
    {
        if (_timesout)
            return;


        _voteButton.interactable = false;

        _selectedCandidate = null;

        foreach (var i in _candidatesList)
        {
            i.ButtonColor.color = new Color32(29, 25, 25, 255);

            if (i.Id == userData.UserID)
            {
                i.IsSelected = !i.IsSelected;

                if(i.IsSelected)
                {
                    _voteButton.interactable = true;
                    _selectedCandidate = i;
                    userData.ButtonColor.color = Color.blue;
                }

            } else
            {
                i.IsSelected = false;
            }

        }
    }

    //Voting System
    public void VoteRemoveButton(string votersID, string votersName)
    {

        if(_timesout)
            return;


        Debug.LogWarning("FIND ME THIS USER " + votersName + " VOTED TO == " + _selectedCandidate.Name);
        _selectedCandidate.Votes++;
        _selectedCandidate.VotersID.Add(votersID);
        _selectedCandidate.IsSelected = false;
        _votedUsers.Add(votersID);


        _voteRemove.SetActive(false);

        //_selectedCandidate.ButtonColor.color = new Color32(29, 25, 25, 255);
        _selectedCandidate = null;


        _votedDone.SetActive(true);

    }


    //imports candidates data
    private void RemovePlayerData(VotersData userData)
    {

        if (_timesout)
            return;

        ///This player is simply a place holder as i dont have any other candidates to put in here rn
        ///        |    |
        ///      \\      //
        ///       \\    //
        ///        \\  //
        ///         \\//
        StartCoroutine(GetPlayerData(player =>
        {
            if (player.AvatarData != null) _removeAvatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(player.AvatarData));

            if (player != null)
            _checkProfile.onClick.AddListener(() =>
            {
                StartCoroutine(GetProfile(player.Id));
            });
        }));

        _removeid = userData.Id;
        _removeName.text = userData.Name;

        DateTime now = DateTime.Now;

        //_date.text = $"{message.Timestamp.Day}/{message.Timestamp.Month}/{message.Timestamp.Year}";

    }

    //Check Profile of the candidate system
    private IEnumerator GetProfile(string playerID)
    {
        ServerPlayer serverPlayer = null;
        bool timeout = false;

        StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(playerID, c => serverPlayer = c));

        StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
        yield return new WaitUntil(() => serverPlayer != null || timeout);

        DataCarrier.AddData(DataCarrier.PlayerProfile, new PlayerData(serverPlayer));
        yield return _checkProfile.GetComponent<WindowNavigation>().Navigate();
        OnPlayerPanelCloseRequested?.Invoke();
    }

}


[Serializable]
public class SavedVotersData
{
    public string Name;
    public string Id;
}


[Serializable]
public class VotersData
{
    public GameObject VoteObject;
    public string Name;
    public string Id;
    public Button Button;
    public bool IsSelected;
    public Image ButtonColor;
    public int Votes;
    public List<string> VotersID = new List<string>(); //Checks who has voted this candidate
    public AvatarFaceLoader Avatar;

}
