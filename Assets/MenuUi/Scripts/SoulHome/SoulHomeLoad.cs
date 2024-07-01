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
using static ServerManager;
using Random = UnityEngine.Random;
using Altzone.Scripts.Config;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Player;

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


        [SerializeField] private List<Furniture> _furnitureList;


        private const string SERVER_ADDRESS = "https://altzone.fi/api/soulhome";

        // Start is called before the first frame update
        void Start()
        {
            _roomAmount = ServerManager.Instance.Clan.playerCount;
            StartCoroutine(HomeLoad());
            //TestCode();
            LoadRooms();
            LoadFurniture();
            SpawnAvatar();
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
                    int slotRows = ((int)_roomPrefab.GetComponent<RoomData>().SlotRows);
                    int slotColumn = ((int)_roomPrefab.GetComponent<RoomData>().SlotColumns);

                    int furniture1X = Random.Range(1, slotColumn -1);
                    int furniture1Y = Random.Range(1, slotRows);
                    int furniture2X;
                    int furniture2Y;
                    while (true)
                    {
                        furniture2X = Random.Range(0, slotColumn -3);
                        furniture2Y = Random.Range(1, slotRows);
                        if ((furniture2X == furniture1X && furniture2Y == furniture1Y)
                            || (furniture2X == furniture1X - 1 && furniture2Y == furniture1Y)
                            || (furniture2X == furniture1X - 2 && furniture2Y == furniture1Y)
                            || (furniture2X == furniture1X - 3 && furniture2Y == furniture1Y)
                            || (furniture2X == furniture1X + 1 && furniture2Y == furniture1Y)) continue;
                        else break;
                    }

                    var test2 = new Furniture(i * 10 + 1, "Floorlamp_Taakka", new Vector2Int(furniture1X, furniture1Y), FurnitureSize.TwoXTwo, FurnitureSize.TwoXTwo, FurniturePlace.Floor, 10f, 15f, false);
                    room.Furnitures.Add(test2);
                    _soulHomeController.AddFurniture(test2);
                    var test3 = new Furniture(i * 10 + 2, "Sofa_Taakka", new Vector2Int(furniture2X, furniture2Y), FurnitureSize.TwoXFour, FurnitureSize.ThreeXThree, FurniturePlace.Floor, 10f, 15f, false);
                    room.Furnitures.Add(test3);
                    _soulHomeController.AddFurniture(test3);

                    soulHome.Room.Add(room);
                }
                json = JsonUtility.ToJson(soulHome);
                Debug.Log(json);
                _soulHomeRooms = JsonUtility.FromJson<SoulHome>(json);
            }

            //_soulHomeRooms = JsonUtility.FromJson<SoulHome>(json);
            
        }

        /*public void TestCode()
        {
            Room test = new Room();
            test.Id = 1;
            test.Furnitures = new List<Furniture>();
            var test2 = new Furniture(1,"Name",new Vector2Int(0,0),FurnitureSize.OneXOne,15f, false);
            test.Furnitures.Add(test2);
            var test3 = new Furniture(2, "Name", new Vector2Int(1, 1), FurnitureSize.OneXTwo, 15f, false);
            test.Furnitures.Add(test3);
            string testTojson = JsonUtility.ToJson(test);
            Debug.Log(testTojson);
            Debug.Log(JsonUtility.FromJson<Room>(testTojson).Furnitures[1].Id);
        }*/
        public void LoadRooms()
        {
            List<GameObject> roompositions = new List<GameObject>();
            int i=0;
            foreach (Transform child in _roomPositions.transform)
            {
                roompositions.Add(child.gameObject);
            }
            foreach (Room room in _soulHomeRooms.Room)
            {
                GameObject roomObject = Instantiate (_roomPrefab, roompositions[i].transform);
                Room roomInfo = roomObject.GetComponent<RoomData>().RoomInfo = room;
                roomObject.GetComponent<RoomData>().RoomInfo.Id = room.Id;
                roomObject.GetComponent<RoomData>().Controller = _soulHomeController;
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
                roompositions[i].transform.GetChild(0).GetComponent<RoomData>().InitializeRoom();
                i++;
            }
            SetSoulhomeHeight();
        }

        public void SetSoulhomeHeight()
        {
            int roomNumber = _soulHomeRooms.Room.Count;
            Transform soulhomeBase = transform.Find("Square").transform;
            float roomHeight = _roomPrefab.GetComponent<BoxCollider2D>().size.y;
            soulhomeBase.localScale = new Vector2(150, roomHeight * roomNumber + 20);
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

        public void LoadFurniture()
        {
            if (_ignoreFurniture)
            {
                int i = 0;
                while (i < 2)
                {
                    //var test2 = new Furniture(i*5+1, "Standard", new Vector2Int(-1, -1), FurnitureSize.OneXTwo, FurnitureSize.OneXOne, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test2);
                    //var test3 = new Furniture(i * 5 + 2, "ShortWide", new Vector2Int(-1, -1), FurnitureSize.OneXFour, FurnitureSize.OneXOne, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test3);
                    var test4 = new Furniture(i * 1000 + 3, "Sofa_Taakka", new Vector2Int(-1, -1), FurnitureSize.TwoXFour, FurnitureSize.ThreeXThree, FurniturePlace.Floor, 10f, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test4);
                    _soulHomeController.AddFurniture(test4);
                    var test5 = new Furniture(i * 1000 + 4, "Mirror_Taakka", new Vector2Int(-1, -1), FurnitureSize.TwoXTwo, FurnitureSize.TwoXTwo, FurniturePlace.Floor, 10f, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test5);
                    _soulHomeController.AddFurniture(test5);
                    var test6 = new Furniture(i * 1000 + 5, "Floorlamp_Taakka", new Vector2Int(-1, -1), FurnitureSize.TwoXTwo, FurnitureSize.TwoXTwo, FurniturePlace.Floor, 10f, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test6);
                    _soulHomeController.AddFurniture(test6);
                    var test7 = new Furniture(i * 1000 + 6, "Toilet_Schrodinger", new Vector2Int(-1, -1), FurnitureSize.OneXTwo, FurnitureSize.TwoXOne, FurniturePlace.Floor, 10f, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test7);
                    _soulHomeController.AddFurniture(test7);
                    var test8 = new Furniture(i * 1000 + 7, "Sink_Schrodinger", new Vector2Int(-1, -1), FurnitureSize.OneXTwo, FurnitureSize.TwoXOne, FurniturePlace.FloorByWall, 10f, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test8);
                    _soulHomeController.AddFurniture(test8);
                    var test9 = new Furniture(i * 1000 + 8, "Closet_Taakka", new Vector2Int(-1, -1), FurnitureSize.TwoXFour, FurnitureSize.TwoXThree, FurniturePlace.Floor, 10f, 15f, false);
                    //_trayHandler.AddFurnitureInitial(test9);
                    _soulHomeController.AddFurniture(test9);
                    i++;
                }
            }
            _trayHandler.InitializeTray();
        }

        public void SpawnAvatar()
        {
            for (int i = 0; i < _roomAmount; i++)
            {
                Instantiate(_avatarPlaceholder, _roomPositions.transform.GetChild(i).GetChild(0));
            }
        }

        
    }
}
