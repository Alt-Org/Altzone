using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chat_VoteCandidateData : MonoBehaviour
{
    [SerializeField] private AvatarFaceLoader _avatar;
    public AvatarFaceLoader UsersAvatar { get => _avatar;}

    private string _userID;
    private string _userName;
    [SerializeField] private TextMeshProUGUI _userNameText;
    [SerializeField] private TextMeshProUGUI _orderNumber;
    [SerializeField] private int _candidateOrder;
    public Toggle _candidateToggle;

    public string UserID { get => _userID; }



    public void SetCandidateData(AvatarData Avatar,  string userID, string UserName, int order)
    {
        if (Avatar != null) _avatar.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(Avatar));
        _userID = userID;
        _userName = UserName;
        _userNameText.text = UserName;
        _candidateOrder = order;
        _orderNumber.text = _candidateOrder.ToString();
    }
}
