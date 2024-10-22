using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using MenuUI.Scripts.SoulHome;
using UnityEngine;
using UnityEngine.Networking;

public class RaidRoomLoad : MonoBehaviour
{
    private SoulHome _soulHomeRooms;
    [Tooltip("Instead of getting information from the server, generate random information."), SerializeField] private bool _ignoreServer;
    [Tooltip("Amount of rooms for local generation."), SerializeField] private int _roomAmount = 3;

    [SerializeField] private GameObject _roomPositions;
    [SerializeField] private GameObject _roomPrefab;
    [SerializeField] private SoulHomeController _soulHomeController;
    [SerializeField] private RaidTowerController _towerController;
    [SerializeField] private GameObject _avatarPlaceholder;
    [SerializeField] private Camera _towerCamera;
    [SerializeField]
    private SpriteRenderer _backgroundSprite;

    private List<Furniture> _furnitureList = null;


    private const string SERVER_ADDRESS = "https://altzone.fi/api/soulhome";

    private bool _furnitureFetchFinished = false;
    private bool _roomsReady = false;
    private bool _furnituresSet = false;
    private bool _loadFinished = false;

    public bool LoadFinished { get => _loadFinished; }

    // Start is called before the first frame update
    void Start()
    {
        //_roomAmount = ServerManager.Instance.Clan != null ? ServerManager.Instance.Clan.playerCount : 1;
        StartCoroutine(HomeLoad());
        StartCoroutine(LoadRooms());
        StartCoroutine(LoadFurniture());
        //StartCoroutine(SpawnAvatar());
    }

    public IEnumerator HomeLoad()
    {
        string json;
        // Get info from database. Eli pyydetään serveriltä RESTin yli klaanin huoneiden tiedot.
        if (!_ignoreServer)
        {
            //CoroutineWithData cd = new CoroutineWithData(this, GetSoulHomeData());
            //yield return cd.coroutine;
            yield return StartCoroutine(GetSoulHomeData(ServerManager.Instance.Clan, data => {
                Debug.Log("result is " + ServerManager.Instance.Clan._id);
                if (data != "fail")
                {
                    Debug.Log("result is " + data);
                    json = data.ToString();
                    _soulHomeRooms = JsonUtility.FromJson<SoulHome>(json);
                }
                else Debug.Log("result is " + data);
            }));
            //Debug.Log("result is " + cd.result);
            //json = cd.result.ToString();
        }
        else //Generate random room info.
        {
            SoulHome soulHome = new SoulHome();
            soulHome.Id = 1;
            soulHome.editPermission = ClanMemberRole.Officer;
            soulHome.addRemovePermission = ClanMemberRole.Admin;
            soulHome.Room = new List<Room>();
            for (int i = 0; i < _roomAmount; i++)
            {
                Room room = new Room();
                room.Id = i;

                soulHome.Room.Add(room);
            }
            StartCoroutine(GetFurniture());
            yield return new WaitUntil(() => _furnitureFetchFinished == true);

            if (_furnitureList != null)
                foreach (Furniture furniture in _furnitureList)
                {
                    if (furniture.Room >= 0 && furniture.Position.x >= 0 && furniture.Position.y >= 0)
                    {
                        soulHome.Room[furniture.Room].Furnitures.Add(furniture);
                    }
                }

            json = JsonUtility.ToJson(soulHome);
            Debug.Log(json);
            _soulHomeRooms = JsonUtility.FromJson<SoulHome>(json);
        }

        //_soulHomeRooms = JsonUtility.FromJson<SoulHome>(json);

    }

    public IEnumerator LoadRooms()
    {
        yield return new WaitUntil(() => _soulHomeRooms != null);
        List<GameObject> roompositions = new List<GameObject>();
        int i = 0;
        foreach (Transform child in _roomPositions.transform)
        {
            roompositions.Add(child.gameObject);
        }
        foreach (Room room in _soulHomeRooms.Room)
        {
            GameObject roomObject = Instantiate(_roomPrefab, roompositions[i].transform);
            Room roomInfo = room;
            roompositions[i].transform.localPosition = new(0, i * roomObject.GetComponent<BoxCollider2D>().size.y, 0);

            roomObject.GetComponent<RoomData>().InitializeRaidRoom(room);
            if (i == 0)
            {
                _towerController.RoomBounds = roomObject.GetComponent<BoxCollider2D>();
            }
            i++;
        }
        SetSoulhomeHeight();
        _roomsReady = true;
    }

    public void SetSoulhomeHeight()
    {
        int roomNumber = _soulHomeRooms.Room.Count;
        Transform soulhomeBase = _backgroundSprite.transform;
        float roomHeight = _roomPrefab.GetComponent<BoxCollider2D>().size.y;
        soulhomeBase.localScale = new Vector2(150, roomHeight * roomNumber + 20);
        soulhomeBase.localPosition = new Vector2(0, soulhomeBase.localScale.y / 2 - 10);
        _towerController.SetCameraBounds();
    }

    public class CoroutineWithData
    {
        public Coroutine coroutine { get; private set; }
        public object result;
        private IEnumerator target;

        public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
        {
            this.target = target;
            this.coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }

    private IEnumerator GetSoulHomeData(ServerClan clan, Action<string> callback)
    {
        Debug.Log(ServerManager.Instance.AccessToken);
        yield return StartCoroutine(WebRequests.Get(SERVER_ADDRESS + "?search=_id=\"" + clan._id + "\"", ServerManager.Instance.AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text);
            }
            else
            {
                callback("fail");
            }
        }));
    }

    private IEnumerator GetFurniture()
    {
        var store = Storefront.Get();
        List<ClanFurniture> clanFurnitureList = null;
        /*store.GetPlayerData(ServerManager.Instance.Player?.uniqueIdentifier, playerData =>
        {
            if (playerData == null || !playerData.HasClanId)
            {
                clanFurnitureList = new List<ClanFurniture>();
                return;
            }
            store.GetClanData(playerData.ClanId, clanData =>
            {
                clanFurnitureList = clanData?.Inventory.Furniture ?? new List<ClanFurniture>();
            });
        });*/
        clanFurnitureList = new();
        for (int i = 0; i < _roomAmount; i++)
        {
            int slotRows = 8;
            int slotColumn = 20;

            int furniture1X = UnityEngine.Random.Range(1, slotColumn - 1);
            int furniture1Y = UnityEngine.Random.Range(1, slotRows);
            int furniture2X;
            int furniture2Y;
            while (true)
            {
                furniture2X = UnityEngine.Random.Range(0, slotColumn - 7);
                furniture2Y = UnityEngine.Random.Range(2, slotRows);
                if ((furniture2X >= furniture1X - 7 && furniture2X <= furniture1X + 1 && furniture2Y >= furniture1Y - 1 && furniture2Y <= furniture1Y + 2)) continue;
                else break;
            }

            clanFurnitureList.Add(new ClanFurniture((10000 + 300 + 3 + i).ToString(), "Floorlamp_Taakka", furniture1X, furniture1Y, i, false));
            clanFurnitureList.Add(new ClanFurniture((10000 + 100 + 3 + i).ToString(), "Sofa_Taakka", furniture2X, furniture2Y, i, false));

        }

        yield return new WaitUntil(() => clanFurnitureList != null);

        // Create furniture list for UI.
        List<Furniture> items = new List<Furniture>();
        if (clanFurnitureList.Count == 0)
        {
            Debug.Log($"found clan items {items.Count}");
            _furnitureFetchFinished = true;
            yield break;
        }

        // Find actual furniture pieces for the UI.
        ReadOnlyCollection<GameFurniture> allItems = null;
        yield return store.GetAllGameFurnitureYield(result => allItems = result);
        Debug.Log($"all items {allItems.Count}");
        foreach (var clanFurniture in clanFurnitureList)
        {
            var gameFurnitureId = clanFurniture.GameFurnitureName;
            var furniture = allItems.FirstOrDefault(x => x.Name == gameFurnitureId);
            if (furniture == null)
            {
                continue;
            }
            Furniture storageFurniture = new(clanFurniture, furniture/*, _furnitureReference.GetFurnitureInfo(clanFurniture.GameFurnitureName)*/);
            items.Add(storageFurniture);
        }
        _furnitureList = items;
        _furnitureFetchFinished = true;
    }

    public IEnumerator LoadFurniture()
    {
        yield return new WaitUntil(() => _roomsReady);

        /*if (_furnitureList != null)
            foreach (Furniture furniture in _furnitureList)
            {
                _soulHomeController.AddFurniture(furniture);
            }*/
        //_soulHomeRooms.Room;
        _furnituresSet = true;
        _loadFinished = true;
    }

    /*public IEnumerator SpawnAvatar()
    {
        yield return new WaitUntil(() => _furnituresSet);
        for (int i = 0; i < _roomAmount; i++)
        {
            Instantiate(_avatarPlaceholder, _roomPositions.transform.GetChild(i).GetChild(0));
        }
        _loadFinished = true;
    }*/
}
