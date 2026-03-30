using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using MenuUI.Scripts.SoulHome;
using UnityEngine;

public class ClanStallSuggestVoting : MonoBehaviour
{

    [Header("PopUp")]
    [SerializeField] private GameObject _popUp;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /*

    TO DO: kirpputorin ‰‰nestys

    public void CreatePollPopup() => StartCoroutine(CreatePollPopupCoroutine());

    public IEnumerator CreatePollPopupCoroutine()
    {
        bool? result = null;
        if (furniture != null) PollManager.CreateShopFurniturePoll(FurniturePollType.Buying, furniture, c => result = c);
        yield return new WaitUntil(() => result.HasValue);
        VotingActions.ReloadPollList?.Invoke();
        ClosePopup();
    }
    */

    //kirpputori.cs > openPopup malliksi? clan_popup?
    //confirmationPopupHandler.cs > lis‰‰ kirpputorille omat
    public void VotingPopUp()
    {
        Debug.Log("VotingPopUp painettu");
        _popUp.SetActive(true);

    }
}
