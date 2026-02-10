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
using Assets.Altzone.Scripts.Model.Poco.Player;
using System.Reflection.Emit;

namespace MenuUI.Scripts.SoulHome {

    public class SoulHomeLoad : MonoBehaviour
    {
        private SoulHome _soulHomeRooms;
        [Tooltip("Instead of getting information from the server, generate random information."), SerializeField] private bool _ignoreServer;
        [Tooltip("Amount of rooms for local generation."), SerializeField] private int _roomAmount = 30;
        [Tooltip("Instead of getting furniture from the server, generate random furniture."), SerializeField] private bool _ignoreFurniture;
        [Tooltip("Is the representation isometric or not?"), SerializeField] private bool _isometric;
        [Tooltip("Use random colours when using random info"), SerializeField] private bool _randomColour;

        [SerializeField] private GameObject _roomPositions;
        [SerializeField] private GameObject _roomPrefab;
        [SerializeField] private FurnitureTrayHandler _trayHandler;
        [SerializeField] private SoulHomeController _soulHomeController;
        [SerializeField] private TowerController _towerController;
        [SerializeField] private GameObject _avatarPlaceholder;
        [SerializeField] private Camera _towerCamera;
        [SerializeField] private ClanPlayerFetcher _clanPlayerFetcher;
        [SerializeField] private SpriteLibraryAsset _avatarSpriteLibrary;
        [SerializeField] private AvatarPartsReference _avatarPartsReference;

        private List<Furniture> _furnitureList = null;

        private readonly Dictionary<string, AvatarRig> _spawnedAvatars = new(); // string is playerId

        private const string SERVER_ADDRESS = "https://altzone.fi/api/soulhome";

        private bool _furnitureFetchFinished = false;
        private bool _roomsReady = false;
        private bool _furnituresSet = false;
        private bool _loadFinished = false;

        private readonly Dictionary<AvatarPart, AvatarPartSetter.AvatarResolverStruct> _resolverDictionary = new()
        {
            {
                AvatarPart.Body,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Clothes,
                    _category = "Body",
                    _suffix = "",
                }
            },
            {
                AvatarPart.R_Hand,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Hands,
                    _category = "Hands",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.L_Hand,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Hands,
                    _category = "Hands",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.L_Eyebrow,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyebrows",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.L_Eye,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category ="Eyes",
                    _suffix = "L"
                }
            },
            {
                AvatarPart.R_Eyebrow,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyebrows",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.R_Eye,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Eyes,
                    _category = "Eyes",
                    _suffix = "R"
                }
            },
            {
                AvatarPart.Nose,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Nose,
                    _category = "Nose",
                    _suffix = ""
                }
            },
            {
                AvatarPart.Mouth,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Mouth,
                    _category = "Mouth",
                    _suffix = ""
                }
            },
            {
                AvatarPart.R_Leg,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Feet,
                    _category = "Legs",
                    _suffix= "R"
                }
            },
            {
                AvatarPart.L_Leg,
                new AvatarPartSetter.AvatarResolverStruct
                {
                    _dataGetter = data => data.Feet,
                    _category = "Legs",
                    _suffix = "L"
                }
            },
        };

        public bool LoadFinished { get => _loadFinished;}

        // Start is called before the first frame update
        void Start()
        {
            _roomAmount = ServerManager.Instance.Clan != null ? ServerManager.Instance.Clan.playerCount: 1;
            if (_roomAmount > 30)
            {
                Debug.LogError($"Clan has more players ({_roomAmount}) than allowed, setting room count to 30.");
                _roomAmount = 30;
            }
            StartCoroutine(HomeLoad());
            //TestCode();
            StartCoroutine(LoadRooms());
            StartCoroutine(LoadFurniture());
            StartCoroutine(SpawnAvatar());
        }

        private void OnEnable()
        {
            _clanPlayerFetcher.OnLocalAvatarUpdated += OnLocalAvatarUpdated;
        }

        private void OnDisable()
        {
            _clanPlayerFetcher.OnLocalAvatarUpdated -= OnLocalAvatarUpdated;
        }

        private void TestCode() //This is test code block that may or may not be unrelated to SoulHome. I just use the opening of the SoulHome to trigger things I need to test and don't have dedicated way of doing so.
        {
            List<IMultipartFormSection> body = new();

            body.Add(new MultipartFormFileSection("logFile","test", null,"test.log"));

            ServerManager.Instance.BattleSendDebugLogFile(body, "my_secret", "UploadTestNiko", callback =>
             {
                 Debug.LogWarning(callback.error+ "  :"+callback.downloadHandler.text);
             });
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
            List<GameObject> roompositions = new List<GameObject>();
            int i=0;
            foreach (Transform child in _roomPositions.transform)
            {
                roompositions.Add(child.gameObject);
            }
            foreach (Room room in _soulHomeRooms.Room)
            {
                GameObject roomObject = Instantiate (_roomPrefab, roompositions[i].transform);
                Room roomInfo = /*roomObject.GetComponent<RoomData>().RoomInfo =*/ room;
                //roomObject.GetComponent<RoomData>().RoomInfo.Id = room.Id;
                //roomObject.GetComponent<RoomData>().Controller = _soulHomeController;
                roompositions[i].transform.localPosition = new(0,i* roomObject.GetComponent<BoxCollider2D>().size.y, 0);
                if (_isometric) {
                    roomObject.transform.Find("Floor").gameObject.GetComponent<Image>().color = roomInfo.Floor;
                    roomObject.transform.Find("Wall").GetChild(0).gameObject.GetComponent<Image>().color = roomInfo.Walls;
                    roomObject.transform.Find("Wall2").GetChild(0).gameObject.GetComponent<Image>().color = roomInfo.Walls;
                }
                else
                {
                    /*GameObject floor = roomObject.transform.Find("Room").Find("Floor").gameObject;
                    floor.GetComponent<SpriteRenderer>().color = room.Floor;
                    foreach (SpriteRenderer floorPiece in floor.transform.GetComponentsInChildren<SpriteRenderer>())
                    {
                        floorPiece.color = roomInfo.Floor;
                    }
                    roompositions[i].transform.GetChild(0).Find("BackWall").gameObject.GetComponent<SpriteRenderer>().color = roomInfo.Walls;

                    Color newColour = roomInfo.Walls;
                    newColour.r *= 0.8f;
                    newColour.g *= 0.8f;
                    newColour.b *= 0.8f;

                    roompositions[i].transform.GetChild(0).Find("RightWall").gameObject.GetComponent<SpriteRenderer>().color = newColour;
                    roompositions[i].transform.GetChild(0).Find("LeftWall").gameObject.GetComponent<SpriteRenderer>().color = newColour;*/
                }
                roomObject.GetComponent<RoomData>().InitializeSoulHomeRoom(room,_soulHomeController, _towerCamera, (_soulHomeRooms.Room.Count <= i+1));
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
                    }
                    i++;
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

            int spawnAmount = Mathf.Min(_roomAmount, _clanPlayerFetcher.Players.Count);

            for (int i = 0; i < spawnAmount; i++)
            {
                GameObject avatarParent = Instantiate(_avatarPlaceholder, _roomPositions.transform.GetChild(i).GetChild(0));
                GameObject avatar = avatarParent.transform.GetChild(0).gameObject;
                AvatarRig rig = avatar.GetComponent<AvatarRig>();

                PlayerData playerData = _clanPlayerFetcher.Players[i];

                ApplyAvatarToRig(rig, playerData);

                _spawnedAvatars[playerData.Id] = rig;
            }
            _loadFinished = true;
        }

        private void ApplyAvatarToRig(AvatarRig rig, PlayerData playerData)
        {
            foreach ((AvatarPart part, SpriteResolver resolver) in rig.Resolvers)
            {
                if (!_resolverDictionary.TryGetValue(part, out AvatarPartSetter.AvatarResolverStruct resolverStruct))
                {
                    continue;
                }

                AvatarPartSetter.AssignAvatarPart(resolver, resolverStruct, playerData, _avatarPartsReference, part);

                SpriteRenderer headSpriteRenderer = rig.GetRenderer(AvatarPart.Head).GetComponent<SpriteRenderer>();
                AvatarPartSetter.SetHeadColor(headSpriteRenderer, playerData);
            }
        }

        private void OnLocalAvatarUpdated(PlayerData playerData)
        {
            if (!_spawnedAvatars.TryGetValue(playerData.Id, out AvatarRig rig))
            {
                return;
            }

            ApplyAvatarToRig(rig, playerData);
        }
    }
}
