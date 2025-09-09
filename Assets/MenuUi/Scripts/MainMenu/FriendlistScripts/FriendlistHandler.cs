using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // lisätty
using Altzone.Scripts.Model.Poco.Clan; // tarvittava kirjasto serverin tietoja hakemista varten ?

public class FriendlistHandler : MonoBehaviour

{

    // UI-komponentit

    [SerializeField] private GameObject _friendlistPanel;
    [SerializeField] private Text _friendlistTitle;
    [SerializeField] private Text _friendlistOnlineTitle;
    [SerializeField] private RectTransform _friendlistForeground;
    [SerializeField] private ScrollRect _friendlistScrollView;
    [SerializeField] private Button _closeFriendlistButton;
    [SerializeField] private Button _openFriendlistButton;




    // Start is called before the first frame update
    void Start()
    {
        _openFriendlistButton.onClick.AddListener(OpenFriendlist);
        _closeFriendlistButton.onClick.AddListener(CloseFriendlist);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OpenFriendlist()
    {
        _friendlistPanel.SetActive(true); //aktivoi
    }

    void CloseFriendlist()
    { 
        _friendlistPanel.SetActive(false); // piilottaa
    }
}
