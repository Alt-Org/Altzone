using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using UnityEngine;

public class AddPlayerHeads : MonoBehaviour
{
    [SerializeField] private GameObject YesVotersContent;
    [SerializeField] private GameObject NoVotersContent;
    [SerializeField] private RectTransform YesVotersMask;
    [SerializeField] private RectTransform NoVotersMask;
    [SerializeField] private GameObject HeadPrefab;
    private List<GameObject> YesHeads = new List<GameObject>();
    private List<GameObject> NoHeads = new List<GameObject>();

    private float MaskWidth;
    private float YesHeadsCombinedWidth;
    private float NoHeadsCombinedWidth;

    private void Awake()
    {
        MaskWidth = YesVotersMask.rect.width;
    }

    public void InstantiateHeads(string pollId)
    {
        PollData pollData = PollManager.GetPollData(pollId);

        // Clear existing heads
        for (int i = 0; i < YesHeads.Count; i++)
        {
            GameObject obj = YesHeads[i];
            Destroy(obj);
        }

        for (int i = 0; i < NoHeads.Count; i++)
        {
            GameObject obj = NoHeads[i];
            Destroy(obj);
        }

        YesHeads.Clear();
        NoHeads.Clear();

        // Instantiate new heads
        foreach (var vote in pollData.YesVotes)
        {
            GameObject obj = Instantiate(HeadPrefab, YesVotersContent.transform);
            obj.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            YesHeads.Add(obj);
        }

        foreach (var vote in pollData.NoVotes)
        {
            GameObject obj = Instantiate(HeadPrefab, NoVotersContent.transform);
            obj.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            NoHeads.Add(obj);
        }

        if (YesHeads.Count != 0) YesHeadsCombinedWidth = YesHeads[0].GetComponent<RectTransform>().rect.width * YesHeads[0].transform.localScale.x * YesHeads.Count;
        if (NoHeads.Count != 0) NoHeadsCombinedWidth = NoHeads[0].GetComponent<RectTransform>().rect.width * NoHeads[0].transform.localScale.x * NoHeads.Count;

        //Debug.Log("YesHeadsCombinedWidth: " + YesHeadsCombinedWidth);
        //Debug.Log("NoHeadsCombinedWidth: " + NoHeadsCombinedWidth);

        if (YesHeadsCombinedWidth > MaskWidth) StartCoroutine(ScrollYesHeads());
        if (NoHeadsCombinedWidth > MaskWidth) StartCoroutine(ScrollNoHeads());
    }

    private IEnumerator ScrollYesHeads()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            float currentPos = YesVotersMask.rect.width;

            //Debug.Log("currentPos: " + currentPos);
            //Debug.Log("YesHeadsCombinedWidth: " + YesHeadsCombinedWidth);

            while (currentPos < YesHeadsCombinedWidth)
            {
                YesVotersContent.GetComponent<RectTransform>().anchoredPosition +=  Vector2.left * Time.deltaTime * 25;
                currentPos -= Vector2.left.x * Time.deltaTime * 25;
                yield return null;
            }

            yield return new WaitForSeconds(2);

            YesVotersContent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    private IEnumerator ScrollNoHeads()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            float currentPos = YesVotersMask.rect.width;

            while (currentPos < NoHeadsCombinedWidth)
            {
                NoVotersContent.GetComponent<RectTransform>().anchoredPosition += Vector2.left * Time.deltaTime * 25;
                currentPos -= Vector2.left.x * Time.deltaTime * 25;
                yield return null;
            }

            yield return new WaitForSeconds(2);

            NoVotersContent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
