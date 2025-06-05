using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class ClanSearchView : MonoBehaviour
{
    [SerializeField] ClanSearchFiltersPanel _filtersPanel;
    [SerializeField] private GameObject _clanPrefab;
    [SerializeField] private Transform _clanListParent;
    [SerializeField] private GameObject _loadMoreButton;
    [SerializeField] private GameObject _clanPopup;

    private int _currentPage;    // Current page found in pagination data
    private int _totalPages;     // Total pages in pagination data
    private List<ClanListing> _listedClans = new();
    private ClanSearchFilters _filters = new ClanSearchFilters() { clanName = "" };

    private void Awake()
    {
        _loadMoreButton.GetComponent<Button>().onClick.AddListener(() => { LoadMoreClans(); });
    }

    private void OnEnable()
    {
        Reset();
        LoadMoreClans();

        _filtersPanel.OnFiltersChanged += UpdateFilters;
    }

    private void OnDisable()
    {
        _filtersPanel.OnFiltersChanged -= UpdateFilters;
    }

    private void Reset()
    {
        for (int i = 0; i < _clanListParent.childCount - 1; i++)
        {
            Destroy(_clanListParent.GetChild(i).gameObject);
        }

        _totalPages = 0;
        _currentPage = 0;
        _loadMoreButton.SetActive(false);
        _listedClans.Clear();
    }

    private void LoadMoreClans()
    {
        StartCoroutine(ServerManager.Instance.GetAllClans(++_currentPage, new Action<List<ServerClan>, PaginationData>((clans, paginationData) =>
        {
            if (clans == null || paginationData == null) return;

            ListClans(clans, paginationData);
        }
        )));
    }

    private void ListClans(List<ServerClan> clans, PaginationData paginationData)
    {
        if (clans == null || clans.Count == 0) return;

        foreach (ServerClan clan in clans)
        {
            GameObject clanInstance = Instantiate(_clanPrefab, _clanListParent);
            ClanListing clanListing = clanInstance.GetComponent<ClanListing>();
            clanListing.Clan = clan;
            _listedClans.Add(clanListing);

            clanListing.OpenProfileButton.onClick.RemoveAllListeners();
            clanListing.OpenProfileButton.onClick.AddListener(() =>
            {
                _clanPopup.SetActive(true);
                _clanPopup.GetComponent<ClanSearchPopup>().SetClanInfo(clan, clanListing);
            });

            if (ServerManager.Instance.Clan != null && clanListing.Clan._id == ServerManager.Instance.Clan._id)
            {
                clanListing.ToggleJoinButton(false);
            }
        }

        // Only the first page in pagination data has totalPages field
        if (paginationData.pageCount != 0) _totalPages = paginationData.pageCount;
        if (paginationData.currentPage != 0) _currentPage = paginationData.currentPage;

        // Check if we have reached the last page of pagination data
        if (paginationData != null && paginationData.currentPage < _totalPages) _loadMoreButton.SetActive(true);
        else _loadMoreButton.SetActive(false);

        _loadMoreButton.transform.SetAsLastSibling();
        //FilterListings();
    }

    private void UpdateFilters(ClanSearchFilters newFilters)
    {
        _filters = newFilters;
        FilterListings();
    }

    private void FilterListings()
    {
        foreach (ClanListing clanListing in _listedClans)
        {
            bool hidelisting = (_filters.isOpen != clanListing.Clan.isOpen)
                || (_filters.clanName != "" && !clanListing.Clan.name.ToLower().Contains(_filters.clanName.ToLower()))
                || (_filters.language != Language.None && _filters.language != clanListing.Clan.language)
                || (_filters.age != ClanAge.None && _filters.age != clanListing.Clan.ageRange)
                || (_filters.goal != Goals.None && _filters.goal != clanListing.Clan.goal);
            clanListing.gameObject.SetActive(!hidelisting);
        }
    }
}
