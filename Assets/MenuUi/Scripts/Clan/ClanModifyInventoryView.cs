using Altzone.Scripts.Model.Poco.Game;
using System.Collections.ObjectModel;
using UnityEngine;
using Altzone.Scripts;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using Altzone.Scripts.Model.Poco.Clan;
using System.Globalization;
using System;
using UnityEngine.UI;

public class ClanModifyInventoryView : MonoBehaviour
{
    [SerializeField] private GameObject clanFurnitureListingPrefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private Button returnToClanMainViewButton;

    private List<ClanFurnitureListing> _scrollViewListings;

    private void Awake()
    {
        _scrollViewListings = new List<ClanFurnitureListing>();
    }

    private void OnEnable()
    {
        Reset();
        PopulateFurnitureList();
        GetStockData();
    }

    private void OnDisable()
    {
        Reset();
    }

    private void Reset()
    {
        foreach (Transform child in _parent)
            Destroy(child.gameObject);

        _scrollViewListings.Clear();
    }

    /// <summary>
    /// Fetches the most current clan information from server, gets clan stock or creates a new one if not available
    /// and fetches the stocks items.
    /// </summary>
    private void GetStockData()
    {
        StartCoroutine(ServerManager.Instance.GetClanFromServer(clan =>
        {
            if(clan == null)
                return;

            if (clan.stockCount == 0)
            {
                StartCoroutine(ServerManager.Instance.PostStockToServer(null));
            }
            else
            {
                if(ServerManager.Instance.Stock != null)
                {
                    StartCoroutine(ServerManager.Instance.GetStockFromServer(ServerManager.Instance.Clan, stock =>
                    {
                        if(stock != null)
                        {
                            StartCoroutine(ServerManager.Instance.GetStockItemsFromServer(stock, new List<ServerItem>(), null, 0, items =>
                            {
                                if (items != null)
                                {
                                    AddItemsToUI(items);
                                }
                            }));
                        }
                    }));
                }
            }

        }));
    }

    /// <summary>
    /// Populates the list of items that we can add to clan stock using GameFurniture items located in DemoModels.json file.
    /// </summary>
    private void PopulateFurnitureList()
    {
        ReadOnlyCollection<GameFurniture> allItems = null;
        Storefront.Get().GetAllGameFurnitureYield(result => allItems = result);

        foreach (var clanFurniture in allItems)
        {
            // We skip the "pommi" items as they are not furniture
            if (clanFurniture.Id.Contains("pommi"))
                continue;

            GameObject clanFurnitureInstance = Instantiate(clanFurnitureListingPrefab, _parent);
            ClanFurnitureListing clanFurnitureListing = clanFurnitureInstance.GetComponent<ClanFurnitureListing>();
            clanFurnitureListing.Furniture = clanFurniture;
            clanFurnitureListing.InventoryView = this;
            _scrollViewListings.Add(clanFurnitureListing);
        }
    }

    /// <summary>
    /// Adds the correct amount of items to the list.
    /// </summary>
    /// <param name="items"></param>
    private void AddItemsToUI(List<ServerItem> items)
    {
        foreach (var item in items)
        {
            ClanFurniture clanFurniture = new ClanFurniture(item._id, item.name.Trim().ToLower(CultureInfo.GetCultureInfo("en-US")).Replace(" ", "."));

            // Checks that ServerItem GameFurnitureId is in GameFurniture list
            var listing = _scrollViewListings.Find(i => i.Furniture.Id == clanFurniture.GameFurnitureName);

            if (listing)
            {
                listing.ServerItems.Add(item);
                listing.UpdateUI();
            }
        }
    }

    public void Save()
    {
        StartCoroutine(SaveCoroutine());
    }

    public IEnumerator SaveCoroutine()
    {
        yield return StartCoroutine(ServerManager.Instance.SaveClanFromServerToDataStorage(ServerManager.Instance.Clan));
        ServerManager.Instance.RaiseClanInventoryChangedEvent();
        returnToClanMainViewButton.onClick.Invoke();
    }

    /// <summary>
    /// Posts item to clan stock.
    /// </summary>
    /// <param name="furniture">Furniture to add to stock</param>
    /// <param name="callback">Callback</param>
    public void AddItemToServer(GameFurniture furniture, Action<ServerItem> callback)
    {
        ServerStock stock = ServerManager.Instance.Stock;
        string body = string.Empty;

        if (furniture == null)
            return;

        if (furniture.UnityKey == string.Empty)
            furniture.UnityKey = "NoKey";

        if (furniture.Filename == string.Empty)
            furniture.Filename = "NoFileName";

        // Column and row number not implemented
        body = @$"{{""name"":""{furniture.Name}"",""shape"":""{furniture.Shape}"",""weight"":{(int)furniture.Weight},
                                ""material"":""{furniture.Material}"",""recycling"":""{furniture.Recycling}"", ""unityKey"":""{furniture.UnityKey}"",
                                ""filename"":""{furniture.Filename}"",""rowNumber"":0,""columnNumber"":0,""isInStock"":true,""isFurniture"":true,""stock_id"":""{stock._id}""}}";

        StartCoroutine(WebRequests.Post(ServerManager.SERVERADDRESS + "item", body, ServerManager.Instance.AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                ServerItem item = result["data"]["Item"].ToObject<ServerItem>();

                if (callback != null)
                    callback(item);

            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    /// <summary>
    /// Removes item from clan stock.
    /// </summary>
    /// <param name="itemToRemove">Item to remove</param>
    /// <param name="callback">Callback</param>
    public void RemoveItemFromServer(ServerItem itemToRemove, Action<bool> callback)
    {
        StartCoroutine(WebRequests.Delete(ServerManager.SERVERADDRESS + "item/" + itemToRemove._id, ServerManager.Instance.AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (callback != null)
                    callback(true);
            }
            else
            {
                if (callback != null)
                    callback(false);
            }
        }));
    }
}
