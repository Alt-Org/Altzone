using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine.UI;
using MenuUI.Scripts.SoulHome;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
//using static ServerManager;
using Random = UnityEngine.Random;
using Altzone.Scripts.Config;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Model.Poco.Game;
using System.Collections.ObjectModel;
using UnityEngine.U2D.Animation;

namespace MenuUI.Scripts.SoulHome {

    public class SoulHomeLoad : MonoBehaviour
    {
        private SoulHome _soulHomeRooms;
        [Tooltip("Instead of getting information from the server, generate random information."), SerializeField] private bool _ignoreServer;
        [Tooltip("Amount of rooms for local generation."), SerializeField] private int _roomAmount = 30;
        [Tooltip("Instead of getting furniture from the server, generate random furniture."), SerializeField] private bool _ignoreFurniture;
        [Tooltip("Is the representation isometric or not?"), SerializeField] private bool _isometric;
        [Tooltip("Use random colours when using random info"), SerializeField] private bool _randomColour;
        [Tooltip("Draws the state of every character in the Scene view"), SerializeField] private bool _drawAvatarDebug = true;
        [Tooltip("Rooms created while the tower loads, counted from the bottom where the camera starts. The rest are created when the camera gets near them, see TowerController."), SerializeField] private int _initialRoomCount = 3;

        [SerializeField] private GameObject _roomPositions;
        [SerializeField] private GameObject _roomPrefab;
        [SerializeField] private FurnitureTrayHandler _trayHandler;
        [SerializeField] private SoulHomeController _soulHomeController;
        [SerializeField] private TowerController _towerController;
        [SerializeField] private GameObject _avatarPlaceholder;
        [SerializeField] private Camera _towerCamera;
        [SerializeField] private ClanPlayerFetcher _clanPlayerFetcher;
        [SerializeField] private SpriteLibraryAsset _avatarSpriteLibrary;

        private List<Furniture> _furnitureList = null;

        private const string SERVER_ADDRESS = "https://altzone.fi/api/soulhome";

        private bool _furnitureFetchFinished = false;
        private bool _roomsReady = false;
        private bool _furnituresSet = false;
        private bool _loadFinished = false;
        private bool _loginStatusChanged = false;
        private bool _refreshInProgress = false;

        private int _runningCoroutines = 0;

        private string _localPlayerId;

        private AvatarRig _localPlayerRig;
        private readonly List<SoulHomeAvatarController> _avatars = new();

        private const int AvatarsPerRoom = 3;
        // Indexed by room position. Null until the room is created.
        private RoomData[] _loadedRooms;
        private int _loadedRoomCount = 0;
        private Bounds _firstRoomBounds;

        private long _frameTimeStart;

        public bool LoadFinished { get => _loadFinished;}
        public int LoadedRoomCount { get => _loadedRoomCount;}

        // Start is called before the first frame update
        void Start()
        {
            RefreshSoulHome();
            _localPlayerId = ServerManager.Instance.Player._id;
            ServerManager.OnLogInStatusChanged += UpdateOnLoginStatusChanged;
        }

        private void OnDestroy()
        {
            ServerManager.OnLogInStatusChanged -= UpdateOnLoginStatusChanged;
        }

        private void OnEnable()
        {
            _clanPlayerFetcher.OnLocalAvatarUpdated += OnLocalAvatarUpdated;
            if (_loginStatusChanged)
            {
                _loginStatusChanged = false;
                _localPlayerId = ServerManager.Instance.Player._id;
                RefreshSoulHome();
            }
            else if (_refreshInProgress)
            {
                StopCoroutine(RefreshCoroutine());
                _refreshInProgress = false;
                StopCoroutine(HomeLoad());
                StopCoroutine(LoadRooms());
                StopCoroutine(LoadFurniture());
                StopCoroutine(SpawnAvatar());
                _runningCoroutines = 0;
                RefreshSoulHome();
            }
        }

        private void OnDisable()
        {
            _clanPlayerFetcher.OnLocalAvatarUpdated -= OnLocalAvatarUpdated;
        }

        private void Update()
        {
            if (!_loadFinished) return;

            float time = Time.time;
            foreach (SoulHomeAvatarController avatar in _avatars)
            {
                if (time >= avatar.NextDecisionTime) avatar.Decide();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawAvatarDebug) return;

            foreach (SoulHomeAvatarController avatar in _avatars)
            {
                if (avatar != null) avatar.DrawDebugGizmos();
            }
        }
#endif

        private void UpdateOnLoginStatusChanged(bool isLoggedIn)
        {
            _loginStatusChanged = isLoggedIn;
        }

        private void RefreshSoulHome()
        {
            if (!_refreshInProgress)
            {
                StartCoroutine(RefreshCoroutine());
            }
        }

        private IEnumerator RefreshCoroutine()
        {
            _refreshInProgress = true;

            ClearSoulHome();

            _clanPlayerFetcher.RefreshClanMembersPlayerData();

            _roomAmount = ServerManager.Instance.Clan != null ? ServerManager.Instance.Clan.playerCount : 1;
            if (_roomAmount > 30)
            {
                Debug.LogError($"Clan has more players ({_roomAmount}) than allowed, setting room count to 30.");
                _roomAmount = 30;
            }

            RunTracked(HomeLoad());
            //TestCode();
            RunTracked(LoadRooms());
            RunTracked(LoadFurniture());
            RunTracked(SpawnAvatar());

            // wait until the above Coroutines have finished
            // to make sure this doesn't run again until it is finished
            yield return WaitForLoadFinish();

            _refreshInProgress = false;
        }

        private void ClearSoulHome()
        {
            foreach (Transform child in _roomPositions.transform)
            {
                foreach (Transform grandChild in child.transform)
                {
                    Destroy(grandChild.gameObject);
                }
            }
            _soulHomeRooms = null;
            _furnitureFetchFinished = false;
            _roomsReady = false;
            _furnituresSet = false;
            _loadFinished = false;
            _furnitureList = null;
            _localPlayerId = null;
            _localPlayerRig = null;
            _avatars.Clear();
            _loadedRooms = null;
            _loadedRoomCount = 0;
        }

        private IEnumerator TrackedCoroutine(IEnumerator coroutine)
        {
            _frameTimeStart = DateTime.Now.Ticks;
            _runningCoroutines++;
            try
            {
                yield return coroutine;
            }
            finally
            {
                _runningCoroutines--;
            }
        }

        private void RunTracked(IEnumerator coroutine)
        {
            StartCoroutine(TrackedCoroutine(coroutine));
        }

        private IEnumerator WaitForLoadFinish()
        {
            yield return new WaitUntil(() => _runningCoroutines == 0);
        }

        public IEnumerator HomeLoad()
        {
            string json;
            // Get info from database. Eli pyydetään serveriltä RESTin yli klaanin huoneiden tiedot.
            if (!_ignoreServer) {
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
                for(int i = 0; i< _roomAmount; i++)
                {
                    Room room = new Room();
                    room.Id = i;
                    if (_randomColour) //If using random colours.
                    {
                        room.Floor = Random.ColorHSV(0f, 1f, 1f, 1f, .3f, 1f);
                        room.Walls = Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);
                    }
                    else //And if not.
                    {
                        room.Floor = new Color(0.4f,0,0);
                        room.Walls = new Color(0.7f, 0, 0);
                    }
                    /*int slotRows = ((int)_roomPrefab.GetComponent<RoomData>().SlotRows);
                    int slotColumn = ((int)_roomPrefab.GetComponent<RoomData>().SlotColumns);

                    int furniture1X = Random.Range(1, slotColumn -1);
                    int furniture1Y = Random.Range(1, slotRows);
                    int furniture2X;
                    int furniture2Y;
                    while (true)
                    {
                        furniture2X = Random.Range(0, slotColumn -7);
                        furniture2Y = Random.Range(1, slotRows);
                        if ((furniture2X >= furniture1X-7 && furniture2X <= furniture1X + 1 && furniture2Y >= furniture1Y - 1 && furniture2Y <= furniture1Y + 2)) continue;
                        else break;
                    }

                    var test2 = new Furniture(i * 10 + 1, "Floorlamp_Taakka", new Vector2Int(furniture1X, furniture1Y), FurnitureSize.TwoXTwo, FurnitureSize.TwoXTwo, FurniturePlace.Floor, 10f, 15f, false);
                    room.Furnitures.Add(test2);
                    _soulHomeController.AddFurniture(test2);
                    var test3 = new Furniture(i * 10 + 2, "Sofa_Taakka", new Vector2Int(furniture2X, furniture2Y), FurnitureSize.ThreeXEight, FurnitureSize.SevenXThree, FurniturePlace.Floor, 10f, 15f, false);
                    room.Furnitures.Add(test3);
                    _soulHomeController.AddFurniture(test3);*/

                    soulHome.Room.Add(room);
                }
                if (FrameCheck())
                {
                    yield return null;
                    _frameTimeStart = DateTime.Now.Ticks;
                }
                StartCoroutine(GetFurniture());
                yield return new WaitUntil(()=> _furnitureFetchFinished == true);
                //Debug.LogWarning("Test");
                if(_furnitureList != null)
                foreach (Furniture furniture in _furnitureList)
                {
                    if(furniture.Room >= 0 && furniture.Room < _roomAmount && furniture.Position.x >= 0 && furniture.Position.y >= 0)
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

            int roomCount = Mathf.Min(_soulHomeRooms.Room.Count, _roomPositions.transform.childCount);
            if (roomCount < _soulHomeRooms.Room.Count) Debug.LogError($"Soulhome has {_soulHomeRooms.Room.Count} rooms but only {roomCount} room positions.");
            _loadedRooms = new RoomData[roomCount];
            _loadedRoomCount = 0;

            // All positions are set right away so the tower has its full height, the rooms themselves are created later.
            float roomHeight = _roomPrefab.GetComponent<BoxCollider2D>().size.y;
            for (int i = 0; i < roomCount; i++)
            {
                _roomPositions.transform.GetChild(i).localPosition = new(0, i * roomHeight, 0);
            }

            // The camera starts at the bottom of the tower, so only the bottom rooms are created
            int initialRooms = Mathf.Min(Mathf.Max(_initialRoomCount, 1), roomCount);
            for (int i = 0; i < initialRooms; i++)
            {
                LoadRoom(i);
                if (FrameCheck())
                {
                    yield return null;
                    _frameTimeStart = DateTime.Now.Ticks;
                }
            }
            SetSoulhomeHeight();
            _roomsReady = true;
        }

        private void LoadRoom(int index)
        {
            Room room = _soulHomeRooms.Room[index];
            GameObject roomObject = Instantiate(_roomPrefab, _roomPositions.transform.GetChild(index));
            if (_isometric)
            {
                roomObject.transform.Find("Floor").gameObject.GetComponent<Image>().color = room.Floor;
                roomObject.transform.Find("Wall").GetChild(0).gameObject.GetComponent<Image>().color = room.Walls;
                roomObject.transform.Find("Wall2").GetChild(0).gameObject.GetComponent<Image>().color = room.Walls;
            }

            RoomData roomData = roomObject.GetComponent<RoomData>();
            roomData.InitializeSoulHomeRoom(room, _soulHomeController, _towerCamera, index >= _soulHomeRooms.Room.Count - 1);
            _loadedRooms[index] = roomData;
            _loadedRoomCount++;

            if (index == 0)
            {
                _firstRoomBounds = roomObject.GetComponent<BoxCollider2D>().bounds;
                _towerController.RoomBounds = _firstRoomBounds;
            }
        }

        private bool IsRoomLoaded(int index) => _loadedRooms != null && index >= 0 && index < _loadedRooms.Length && _loadedRooms[index] != null;

        /// <summary>
        /// Called by the <see cref="TowerController"/> every frame with the world Y range around the camera view.
        /// Creates the missing room in that range that's closest to the middle of it, one room per call so the frame time stays even while scrolling.
        /// </summary>
        public void LoadRoomsInRange(float minY, float maxY)
        {
            if (!_loadFinished || _loadedRooms == null || _loadedRoomCount >= _loadedRooms.Length) return;

            // Every room is the same size, so the bounds of the others are the first room's bounds moved to their position.
            float firstRoomY = _roomPositions.transform.GetChild(0).position.y;
            float centerY = (minY + maxY) / 2;
            int closestRoom = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < _loadedRooms.Length; i++)
            {
                if (_loadedRooms[i] != null) continue;

                float offset = _roomPositions.transform.GetChild(i).position.y - firstRoomY;
                float bottom = _firstRoomBounds.min.y + offset;
                float top = _firstRoomBounds.max.y + offset;
                if (top < minY || bottom > maxY) continue;

                float distance = Mathf.Abs((bottom + top) / 2 - centerY);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRoom = i;
                }
            }
            if (closestRoom < 0) return;

            LoadRoom(closestRoom);
            if (closestRoom < _roomAmount)
            {
                for (int j = 0; j < AvatarsPerRoom; j++) SpawnAvatarInRoom(closestRoom);
            }
        }

        public void SetSoulhomeHeight()
        {
            int roomNumber = _soulHomeRooms.Room.Count;
            Transform soulhomeBase = transform.Find("Square").transform;
            float roomHeight = _roomPrefab.GetComponent<BoxCollider2D>().size.y;
            soulhomeBase.localScale = new Vector2(150, roomNumber < 2? roomHeight * roomNumber + 40 : roomHeight * roomNumber + 20);
            soulhomeBase.localPosition = new Vector2(0, soulhomeBase.localScale.y/2 -10);
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
            yield return StartCoroutine(WebRequests.Get(SERVER_ADDRESS+"?search=_id=\"" + clan._id + "\"", ServerManager.Instance.AccessToken, request =>
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
            store.GetPlayerData(ServerManager.Instance.Player?.uniqueIdentifier, playerData =>
            {
                if (playerData == null || !playerData.HasClanId)
                {
                    clanFurnitureList = new List<ClanFurniture>();
                    store.GetDefaultFurniture(p => clanFurnitureList = p);
                    //return;
                }
                else
                {
                    store.GetClanData(playerData.ClanId, clanData =>
                    {
                        clanFurnitureList = clanData?.Inventory.Furniture ?? new List<ClanFurniture>();
                    });
                }
            });
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
                if (FrameCheck())
                {
                    yield return null;
                    _frameTimeStart = DateTime.Now.Ticks;
                }
            }
            _furnitureList = items;
            _furnitureFetchFinished = true;
        }

        public IEnumerator LoadFurniture()
        {
            yield return new WaitUntil(() => _roomsReady);
            if (_ignoreFurniture)
            {
                var store = Storefront.Get();
                ReadOnlyCollection<GameFurniture> allItems = null;
                yield return store.GetAllGameFurnitureYield(result => allItems = result);

                int i = 0;
                int j;
                while (i < 2)
                {
                    j = 1;
                    foreach(GameFurniture furniture in allItems)
                    {
                        Furniture storageFurniture = new(new(i*1000+j.ToString(), furniture.Name), furniture);
                        _soulHomeController.AddFurniture(storageFurniture);
                        j++;
                        if (FrameCheck())
                        {
                            yield return null;
                            _frameTimeStart = DateTime.Now.Ticks;
                        }
                    }
                    i++;
                    if (FrameCheck())
                    {
                        yield return null;
                        _frameTimeStart = DateTime.Now.Ticks;
                    }
                }
            }
            else
            {
                if(_furnitureList != null)
                foreach (Furniture furniture in _furnitureList)
                {
                    _soulHomeController.AddFurniture(furniture);
                }
            }
                _trayHandler.InitializeTray();
            _furnituresSet = true;
        }

        public IEnumerator SpawnAvatar()
        {
            yield return new WaitUntil(() => _furnituresSet);
            yield return new WaitUntil(() => _clanPlayerFetcher.PlayersLoaded);

            for (int i = 0; i < _roomAmount; i++)
            {
                // The characters of the rooms that aren't created yet are spawned with the room, see LoadRoomsInRange.
                if (!IsRoomLoaded(i)) continue;

                for (int j = 0; j < AvatarsPerRoom; j++)
                {
                    SpawnAvatarInRoom(i);
                    if (FrameCheck())
                    {
                        yield return null;
                        _frameTimeStart = DateTime.Now.Ticks;
                    }
                }
            }
            _loadFinished = true;
        }

        private void SpawnAvatarInRoom(int roomIndex)
        {
            GameObject avatarParent = Instantiate(_avatarPlaceholder, _loadedRooms[roomIndex].transform);
            GameObject avatar = avatarParent.transform.GetChild(0).gameObject;
            AvatarRig rig = avatar.GetComponent<AvatarRig>();

            PlayerData playerData = _clanPlayerFetcher.Players[roomIndex];

            SoulHomeAvatarController avatarController = avatarParent.GetComponent<SoulHomeAvatarController>();
            avatarController.InitializeAvatar(playerData);
            _avatars.Add(avatarController);

            if (playerData?.Id == _localPlayerId)
            {
                _localPlayerRig = rig;
            }

            rig.ApplyAvatarToRig(playerData);
        }


        private void OnLocalAvatarUpdated(PlayerData playerData)
        {
            if (_localPlayerRig == null)
            {
                return;
            }

            _localPlayerRig.ApplyAvatarToRig(playerData);
        }

        private bool FrameCheck()
        {
            if ((new TimeSpan(DateTime.Now.Ticks - _frameTimeStart)).TotalSeconds > 1f/60f)
            {
                return true;
            }
            return false;
        }
    }
}
