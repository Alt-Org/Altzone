using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Voting;
using UnityEngine;
using UnityEngine.UI;

public class VoteManager : MonoBehaviour // Manages the display and interaction for polls
{
    [SerializeField] private GameObject VotedListContent;
    [SerializeField] private GameObject NotVotedListContent;

    [Header ("Shop Content")]
    [SerializeField] private GameObject ShopVotedListContent;
    [SerializeField] private GameObject ShopNotVotedListContent;
    [SerializeField] private GameObject ShopSection;
    [SerializeField] private GameObject NoPollsShop;

    [Header ("Admin Content")]
    [SerializeField] private GameObject AdminNotVotedListContent;
    [SerializeField] private GameObject AdminVotedListContent;
    [SerializeField] private GameObject AdminSection;
    [SerializeField] private GameObject NoPollsAdmin;

    [Header ("Stuff")]
    [SerializeField] private GameObject PollObjectPrefab;
    [SerializeField] private GameObject PollPopup;
    [SerializeField] private GameObject Blocker;

    [SerializeField] private Color AdminThemeColor = new Color(0f, 1f, 1f);  // cyan
    [SerializeField] private Color ShopThemeColor = new Color(1f, 0.75f, 0f);  // yellow

    private List<GameObject> Polls = new List<GameObject>();
    private PlayerData player;

    [SerializeField] private PollInfoPopup pollInfoPopupWindow;

    private void Start()
    {
        Debug.Log("VoteManager START");
        pollInfoPopupWindow?.InitializeIfNeeded();
    }

    private void OnEnable()
    {
        Debug.Log("VoteManager OnEnable");

        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            Debug.Log("Player data loaded");
            player = data;
            FetchPolls();
        });

        VotingActions.ReloadPollList += FetchPolls;
        VotingActions.PassPollId += SetPollPopupPollId;
    }

    private void OnDisable()
    {
        VotingActions.ReloadPollList -= FetchPolls;
        VotingActions.PassPollId -= SetPollPopupPollId;
    }

    private bool HasPlayerVoted(PollData pollData, string playerId)
    {
        return !pollData.NotVoted.Contains(playerId);
    }

    private void FetchPolls()
    {
        Debug.Log("FetchPolls CALLED");
        StartCoroutine(FetchPollsCoroutine());
    }

    private IEnumerator FetchPollsCoroutine()
    {
        Debug.Log("Fetching Polls from server");

        bool ready = false;

        yield return StartCoroutine(ServerManager.Instance.GetClanVoteListFromServer(polls =>
        {
            Debug.Log("Server returned polls");

            if (polls != null)
            {
                Debug.Log("Server polls count: " + polls.Count);

                ClanData clanData = null;

                Storefront.Get().GetClanData(ServerManager.Instance.Player.clan_id, data =>
                {
                    clanData = data;
                });

                if (clanData != null)
                {
                    clanData.Polls.Clear();



                    foreach (ServerPoll poll in polls)
                    {
                        Debug.Log("Server poll type: " + poll.type);
                        if (poll.type == "flea_market_sell_item" || poll.type == "shop_buy_item")
                        {
                            FurniturePollData pollData = new FurniturePollData(poll, clanData);
                            if (pollData.Furniture == null) continue;

                            clanData.Polls.Add(pollData);
                        }
                    }

                    Debug.Log("Server polls processed. ClanData.Polls count: " + clanData.Polls.Count);

                    Storefront.Get().SaveClanData(clanData, null);
                    ServerManager.Instance.RaiseClanPollsChangedEvent();
                }
                else
                {
                    Debug.LogWarning("ClanData was null");
                }
            }

            ready = true;
        }));

        yield return new WaitUntil(() => ready);


        Debug.Log("Calling InstantiatePolls");
        InstantiatePolls();
    }


    private void ClearContainer(GameObject container)
    {
        if (container == null) return;

        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void InstantiatePolls()
    {
        Debug.Log("InstantiatePolls CALLED");
        Debug.Log("Poll list count from manager: " + PollManager.GetPollList().Count);

        ClearContainer(AdminNotVotedListContent);
        ClearContainer(AdminVotedListContent);
        ClearContainer(ShopNotVotedListContent);
        ClearContainer(ShopVotedListContent);


        if (AdminNotVotedListContent == null ||
            AdminVotedListContent == null ||
            ShopNotVotedListContent == null ||
            ShopVotedListContent == null)
        {
            Debug.LogError("One or more ListContent references are missing!");
        }

        for (int i = 0; i < Polls.Count; i++)
        {
            Destroy(Polls[i]);
        }
        Polls.Clear();

        if (player == null)
        {
            Debug.LogWarning("Player data not loaded yet.");
            return;
        }

        // ****************** var pollList = PollManager.GetPollList();
        // ******************         Debug.Log("Poll count: " + pollList.Count);



        var pollList = PollManager.GetPollList();

        Debug.Log("AdminSection active: " + AdminSection.activeSelf);
        Debug.Log("ShopSection active: " + ShopSection.activeSelf);
/*
#if UNITY_EDITOR
        if (pollList.Count == 0)
        {
            Debug.Log("TEST MODE: Creating fake UI entries");

            for (int i = 0; i < 7; i++)   // ← 7 test pollia
            {
                // ADMIN
                GameObject admin = Instantiate(PollObjectPrefab, AdminNotVotedListContent.transform);
                PollObject adminObj = admin.GetComponent<PollObject>();
                adminObj.SetPollId("ADMIN_TEST_" + i);
                adminObj.SetTheme(AdminThemeColor);

                // SHOP
                GameObject shop = Instantiate(PollObjectPrefab, ShopNotVotedListContent.transform);
                PollObject shopObj = shop.GetComponent<PollObject>();
                shopObj.SetPollId("SHOP_TEST_" + i);
                shopObj.SetTheme(ShopThemeColor);
            }
        }
#endif
*/
        Debug.Log("Poll count: " + pollList.Count);

        foreach (var pollData in pollList)
        {
            GameObject obj = null;


            bool isShopPoll = pollData is FurniturePollData;

            // Test bit from earlier
            //#if unity_editor
            //                        // ===== test mode: pakotetaan jako näkyviin =====
            //                        bool isshoppoll = polldata.id.gethashcode() % 2 == 0;
            //#else
            //            bool isshoppoll = polldata is furniturepolldata;
            //#endif

            Debug.Log("isShopPoll = " + isShopPoll);



            bool hasVoted = !pollData.NotVoted.Contains(player.Id);


            Debug.Log("Instantiating poll " + pollData.Id +
          " | IsShop: " + isShopPoll +
          " | HasVoted: " + hasVoted);

            if (isShopPoll)
            {
                obj = hasVoted
                    ? Instantiate(PollObjectPrefab, ShopVotedListContent.transform)
                    : Instantiate(PollObjectPrefab, ShopNotVotedListContent.transform);
            }
            else
            {
                obj = hasVoted
                    ? Instantiate(PollObjectPrefab, AdminVotedListContent.transform)
                    : Instantiate(PollObjectPrefab, AdminNotVotedListContent.transform);
            }

            if (obj != null)
            {
                PollObject pollObject = obj.GetComponent<PollObject>();
                pollObject.SetPollId(pollData.Id);
                Debug.Log("About to set theme");

                if (isShopPoll)
                    pollObject.SetTheme(ShopThemeColor);
                else
                    pollObject.SetTheme(AdminThemeColor);


                // =====================

                Polls.Add(obj);

                // ----------------


                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (!HasPlayerVoted(pollData, player.Id))
                    {
                        PollPopup.SetActive(true);
                        VotingActions.PassPollId?.Invoke(pollData.Id);
                    }
                    else
                    {
                        Debug.Log("You have already voted on this poll.");
                    }
                });
            }
        }

        bool hasAdminPolls =
            AdminNotVotedListContent.transform.childCount > 0 ||
            AdminVotedListContent.transform.childCount > 0;

        bool hasShopPolls =
            ShopNotVotedListContent.transform.childCount > 0 ||
            ShopVotedListContent.transform.childCount > 0;


        Debug.Log("Admin section visible: " + hasAdminPolls);
        Debug.Log("Shop section visible: " + hasShopPolls);

        AdminSection.SetActive(hasAdminPolls);
        ShopSection.SetActive(hasShopPolls);

        NoPollsAdmin.SetActive(!hasAdminPolls);
        NoPollsShop.SetActive(!hasShopPolls);

        /*
        if (Polls.Count == 0)
            NoPollsText.SetActive(true);
        else
            NoPollsText.SetActive(false);
        */
        Canvas.ForceUpdateCanvases();
    }

    public void SetPollPopupPollId(string pollId)
    {
        PollPopup.GetComponent<PollPopup>().SetPollId(pollId);
    }
}
