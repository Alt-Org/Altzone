using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.UI;
public class Chat_VotingCandidatePopUp : AltMonoBehaviour
{

    [Header("User Data")]
    [SerializeField] private GameObject _votingCandidates; //Prefab

    private int _count;

    private PlayerData _playerData; //Place holder for avatar and ID

    [SerializeField] private List<CandidatesData> _candidatesData;
    [SerializeField] private List<PlaceHolderCandidates> _placeholdercandidates; //place holder
    [SerializeField] private List<string> _votedUsers; //Used to check who has voted already

    [Header("Voting Systems")]
    [SerializeField] private Button _voteButton;
    [SerializeField] private GridLayoutGroup _layoutContent;
    [SerializeField] private GameObject _votingSystem;
    [SerializeField] private GameObject _votingDone;

    [SerializeField] private Button[] _closeButtons;

    // Start is called before the first frame update
    void Start()
    {
       
        foreach (Button button in _closeButtons)
        {
            button.onClick.AddListener(ClosePopup);
        }

        StartCoroutine(GetPlayerData(player =>
        {
            _playerData = player;
        }));


        foreach(var i in _placeholdercandidates)
        {
            AddCandidates(i._userID, i._userName);
        }


        _voteButton.onClick.AddListener(() => Voting());


        //Checks if the user had already voted before
        foreach (var i in _votedUsers)
        {
            if (i == _playerData.Id)
            _votingSystem.SetActive(false);
            _votingDone.SetActive(true);
        }
    }

    //Closing popup system
    private void ClosePopup()
    {
        gameObject.SetActive(false);
    }



    //Data importer
    void AddCandidates(string UserID, string UserName)
    {

        GameObject newCandidateData = Instantiate(_votingCandidates, _layoutContent.transform);
        Chat_VoteCandidateData candidateData = newCandidateData.GetComponent<Chat_VoteCandidateData>();

        _count++;
        _candidatesData.Add(new CandidatesData { _avatar = _playerData.AvatarData, _userID = UserID, _userName = UserName, _candidateOrder = _count, _voteToggle = candidateData._candidateToggle });
        candidateData.SetCandidateData(_candidatesData[_candidatesData.Count - 1]._avatar, _candidatesData[_candidatesData.Count - 1]._userID, _candidatesData[_candidatesData.Count - 1]._userName, _candidatesData[_candidatesData.Count - 1]._candidateOrder);

        candidateData._candidateToggle.onValueChanged.AddListener(IsOn =>
        {
            if (!IsOn)
            {
                _voteButton.interactable = false;
                return;
            }
            Toggle(candidateData);

        });


    }

    //Toggle system
    void Toggle(Chat_VoteCandidateData candidate)
    {

        foreach (var i in _candidatesData)
        {
            if(i._userID != candidate.UserID)
            {
                i._voteToggle.isOn = false;
                continue;
            }
        }
        _voteButton.interactable = true;

    }

    //Voting system
    void  Voting()
    {
        foreach(var i in _candidatesData)
        {
            if(i._voteToggle.isOn == true)
            {
                i.Votes++;
                _votedUsers.Add(_playerData.Id);
                i.VotersID.Add(_playerData.Id);

                _voteButton.interactable = false;
                i._voteToggle.isOn = false;

                Debug.LogWarning("FIND ME: User: " + _playerData.Name + " Voted to: " + i._userName + " Count is now: " + i.Votes); 

                _votingSystem.SetActive(false);
                _votingDone.SetActive(true);
            }
        }
    }


}
[Serializable]
public class CandidatesData
{
    public AvatarData _avatar;
    public string _userID;
    public string _userName;
    public int _candidateOrder;
    public Toggle _voteToggle;

    public int Votes;
    public List<string> VotersID = new List<string>(); //Checks who has voted this candidate

}

[Serializable]
public class PlaceHolderCandidates
{
    public string _userID;
    public string _userName;
}
