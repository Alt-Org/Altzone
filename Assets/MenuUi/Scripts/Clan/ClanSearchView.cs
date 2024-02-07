using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanSearchView : MonoBehaviour
{
    [SerializeField] private GameObject _clanPrefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private GameObject _loadMoreButton;

    private int currentPage;    // Current page found in pagination data
    private int totalPages;     // Total pages in pagination data

    private void Awake()
    {
        _loadMoreButton.GetComponent<Button>().onClick.AddListener(() => { LoadMoreClans(totalPages); });
    }

    private void OnEnable()
    {
        Reset();

        StartCoroutine(ServerManager.Instance.GetAllClans(++currentPage, new Action<List<ServerClan>, PaginationData>((clans, paginationData) =>
        {
            if (clans == null || paginationData == null)
                return;

            ListClans(clans, paginationData);
        }
        )));
    }

    private void Reset()
    {
        for (int i = 0; i < _parent.childCount - 1; i++)
            Destroy(_parent.GetChild(i).gameObject);

        totalPages = 0;
        currentPage = 0;
        _loadMoreButton.SetActive(false);
    }

    private void LoadMoreClans(int pageCount)
    {
        StartCoroutine(ServerManager.Instance.GetAllClans(++currentPage, new Action<List<ServerClan>, PaginationData>((clans, paginationData) =>
        {
            if (clans == null || paginationData == null)
                return;

            ListClans(clans, paginationData);
        }
        )));
    }

    private void ListClans(List<ServerClan> clans, PaginationData paginationData)
    {
        if (clans == null || clans.Count == 0)
            return;

        foreach (ServerClan clan in clans)
        {
            GameObject clanInstance = Instantiate(_clanPrefab, _parent);
            ClanListing clanListing = clanInstance.GetComponent<ClanListing>();
            clanListing.Clan = clan;


            if (ServerManager.Instance.Clan != null)
                if (clanListing.Clan._id == ServerManager.Instance.Clan._id)
                    clanListing.ToggleJoinButton(false);
        }

        // Only the first page in pagination data has totalPages field
        if (paginationData.pageCount != 0)
            totalPages = paginationData.pageCount;

        if (paginationData.currentPage != 0)
            currentPage = paginationData.currentPage;

        // Check if we have reached the last page of pagination data
        if (paginationData != null && paginationData.currentPage < totalPages)
            _loadMoreButton.SetActive(true);
        else
            _loadMoreButton.SetActive(false);

        _loadMoreButton.transform.SetAsLastSibling();
    }
}
