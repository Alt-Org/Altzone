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

    [Header("Regime Voting")]
    [SerializeField] private GameObject _voteRegime;
    [SerializeField] private GameObject _voteUserPrefab;
    [SerializeField] private Transform _candidateBoard;
    [SerializeField] private Button  _voteButton;

    //Voting List
    [SerializeField] private List<VotersData> _candidatesList;
    [SerializeField] private List<SavedVotersData> _savedData; //Mainly just a place holder for data testing
    [SerializeField] private List<string> _votedUsers;


    [Header("Removing Player")]
    [SerializeField] private GameObject _voteRemove; 
    [SerializeField] private AvatarFaceLoader _removeAvatar;
    [SerializeField] private string _removeid;
    [SerializeField] private TextMeshProUGUI _removeName;
    [SerializeField] private TextMeshProUGUI _removeDate;

    [SerializeField] private Button _checkProfile;
    [SerializeField] private Button _buttonRemoveVote;

    [SerializeField] private Button _changeOptionVote; //Place holder for moving the other voting system

    public static event PlayerPanelCloseRequested OnPlayerPanelCloseRequested;


    void Start()
    {

        _voteRegime.SetActive(true);
        _voteRemove.SetActive(true);


        SetMessageInfo();

        //_Sav
        foreach (var i in _savedData)
        {
        CandidateData(i.Name, i.Id);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_candidateBoard.GetComponent<RectTransform>());

        StartCoroutine(GetPlayerData(player =>
        {
             RemovePlayer(player);
            _voteButton.onClick.AddListener(() => VotedButton(player.Id, player.Name));
        }));

        _changeOptionVote.onClick.AddListener(() => change());

        _voteRemove.SetActive(false);
    }

    void change()
    {
        _voteRegime.SetActive(!_voteRegime.activeSelf);
        _voteRemove.SetActive(!_voteRemove.activeSelf);
    }


    //Sets the users who put up the vote up data (it only uses the current users data right now as a place  holder)
    public void SetMessageInfo()
    {
        StartCoroutine(GetPlayerData(player =>
        {
         if (player.AvatarData != null) _avatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(player.AvatarData));
        _id = player.Id;
        _name.text = player.Name;


            

            //_time.text = $"{message.Timestamp.Hour}:{message.Timestamp.Minute:D2}";
            //_date.text = $"{message.Timestamp.Day}/{message.Timestamp.Month}/{message.Timestamp.Year}";
        }));

    }

      ///               ///
     /// Voting Regime ///
    ///               ///

    ///Adds the Candidates to the selection
    private void CandidateData(string Usersname, string UserID)
    {
        if (!_voteRegime.gameObject.activeSelf)
            return;

        GameObject newCandidate = Instantiate(_voteUserPrefab, _candidateBoard);
        Chat_VoteNameData userData = newCandidate.GetComponent<Chat_VoteNameData>();

        StartCoroutine(GetPlayerData(player =>
        {
            userData.VoteButton.onClick.AddListener(() => buttonClicked(userData, player.Id));
        }));

        _candidatesList.Add(new VotersData { Name = Usersname, Id = UserID, Button = userData.VoteButton, IsSelected = false, ButtonColor = userData.ButtonColor});
        userData.SetUserInfo(Usersname, UserID);



    }

    ///Selecting the Candidate:
    /*[Sets the every button back to original color before checking who got selected/deselected]*/
    public void buttonClicked(Chat_VoteNameData userData, string votersID)
    {

        foreach(var i in _votedUsers)
        {
            if(i == votersID)
            {
                Debug.LogWarning("FIND ME: Sorry but u have already voted");
                return;
            }
        }



        foreach(var i in _candidatesList)
        {
            i.ButtonColor.color = new Color32(29, 25, 25, 255);

            if (i.Id == userData.UserID)
            {
                i.IsSelected = !i.IsSelected;

                if(i.IsSelected)
                {
                    userData.ButtonColor.color = Color.blue;
                }

            } else
            {
                i.IsSelected = false;
            }

        }
    }

    //Voting System
    public void VotedButton(string votersID, string votersName)
    {
        if(_candidatesList.All(x => x.IsSelected == false))
        {
            Debug.LogWarning("FIND ME: candidate not found");
            return;
        }


        foreach(var i in _candidatesList)
        {

            if(i.IsSelected & i.VotersID != votersID)
            {
                i.ButtonColor.color = new Color32(29, 25, 25, 255);

                Debug.LogWarning("FIND ME THIS USER " + votersName + " VOTED TO == " + i.Name);
                i.Votes++;
                i.VotersID = votersID;
                i.IsSelected = false;
                _votedUsers.Add(votersID);
            }

        }
        


    }
      ///               ///
     /// Remove Player ///
    ///               ///
    private void RemovePlayer(PlayerData player)
    {

            if (player.AvatarData != null) _removeAvatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(player.AvatarData));
            _removeid = player.Id;
            _removeName.text = player.Name;

            //_date.text = $"{message.Timestamp.Day}/{message.Timestamp.Month}/{message.Timestamp.Year}";


        if (player != null)
            _checkProfile.onClick.AddListener(() =>
            {
                StartCoroutine(GetProfile(player.Id));
            });

        _buttonRemoveVote.onClick.AddListener(() => Debug.LogWarning("Find me: You have voted to remove this player == " + _removeName.text));


    }

    //Check Profile
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
    public string Name;
    public string Id;
    public Button Button;
    public bool IsSelected;
    public Image ButtonColor;
    public int Votes;
    public string VotersID;


}
