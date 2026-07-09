using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chat_VoteNameData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _userNameText;
    [SerializeField] private string _userID;
    [SerializeField] private string _userName;
    public Button VoteButton;
    public Image ButtonColor;

    public string UserID { get => _userID; }

    public void SetUserInfo(string UserName, string UserID)
    {
        _userID = UserID;
        _userName = UserName;

        _userNameText.text = UserName;

    }
}
