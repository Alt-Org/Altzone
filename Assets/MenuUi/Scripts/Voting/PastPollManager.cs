using System;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using UnityEngine;

public class PastPollManager : MonoBehaviour
{
    [SerializeField] private GameObject PollObjectPrefab;
    [SerializeField] private GameObject NoPollsText;

    [SerializeField] private GameObject PastAdminNotVotedListContent;
    [SerializeField] private GameObject PastAdminVotedListContent;
    [SerializeField] private GameObject PastShopNotVotedListContent;
    [SerializeField] private GameObject PastShopVotedListContent;

    [SerializeField] private GameObject PastAdminSection;
    [SerializeField] private GameObject PastShopSection;

    [SerializeField] private Color AdminThemeColor = new Color(0f, 1f, 1f);
    [SerializeField] private Color ShopThemeColor = new Color(1f, 0.75f, 0f);

    public static Action OnPastPollsChanged;

    private List<GameObject> PastPolls = new List<GameObject>();

    private void OnEnable()
    {
        InstantiatePastPolls();
        OnPastPollsChanged += InstantiatePastPolls;
    }

    private void OnDisable()
    {
        OnPastPollsChanged -= InstantiatePastPolls;
    }

    public void InstantiatePastPolls()
    {
        // Destroy old objects
        foreach (GameObject obj in PastPolls)
        {
            Destroy(obj);
        }
        PastPolls.Clear();

        // Also clear container children (important to avoid duplicates)
        ClearContainer(PastAdminNotVotedListContent);
        ClearContainer(PastAdminVotedListContent);
        ClearContainer(PastShopNotVotedListContent);
        ClearContainer(PastShopVotedListContent);

        var pastPollList = PollManager.GetPastPollList();


#if UNITY_EDITOR
        if (pastPollList == null || pastPollList.Count == 0)
        {
            Debug.Log("PAST TEST MODE: Creating fake UI entries");

            for (int i = 0; i < 3; i++)   // ← 3 test pollia
            {
                GameObject fakeAdmin = Instantiate(PollObjectPrefab, PastAdminVotedListContent.transform);
                PollObject adminObj = fakeAdmin.GetComponent<PollObject>();
                adminObj.SetPollId("PAST_TEST_ADMIN");
                adminObj.SetTheme(AdminThemeColor);
                fakeAdmin.GetComponent<UnityEngine.UI.Button>().interactable = false;
                PastPolls.Add(fakeAdmin);

                GameObject fakeShop = Instantiate(PollObjectPrefab, PastShopVotedListContent.transform);
                PollObject shopObj = fakeShop.GetComponent<PollObject>();
                shopObj.SetPollId("PAST_TEST_SHOP");
                shopObj.SetTheme(ShopThemeColor);
                fakeShop.GetComponent<UnityEngine.UI.Button>().interactable = false;
                PastPolls.Add(fakeShop);
            }

        }
        else
#endif


        {
            foreach (var pollData in pastPollList)
            {
                GameObject obj = null;

                bool isShopPoll = pollData is FurniturePollData;

                if (isShopPoll)
                {
                    obj = Instantiate(PollObjectPrefab, PastShopVotedListContent.transform);
                }
                else
                {
                    obj = Instantiate(PollObjectPrefab, PastAdminVotedListContent.transform);
                }

                if (obj != null)
                {
                    PollObject pollObject = obj.GetComponent<PollObject>();
                    pollObject.SetPollId(pollData.Id);

                    if (isShopPoll)
                        pollObject.SetTheme(ShopThemeColor);
                    else
                        pollObject.SetTheme(AdminThemeColor);

                    obj.GetComponent<UnityEngine.UI.Button>().interactable = false;
                    PastPolls.Add(obj);
                }
            }
        }


        Debug.Log("Admin children: " + PastAdminVotedListContent.transform.childCount);
        Debug.Log("Shop children: " + PastShopVotedListContent.transform.childCount);



        bool hasAdminPolls =
            PastAdminNotVotedListContent.transform.childCount > 0 ||
            PastAdminVotedListContent.transform.childCount > 0;

        bool hasShopPolls =
            PastShopNotVotedListContent.transform.childCount > 0 ||
            PastShopVotedListContent.transform.childCount > 0;

        PastAdminSection.SetActive(hasAdminPolls);
        PastShopSection.SetActive(hasShopPolls);

        NoPollsText.SetActive(!hasAdminPolls && !hasShopPolls);
    }

    private void ClearContainer(GameObject container)
    {
        if (container == null) return;

        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
