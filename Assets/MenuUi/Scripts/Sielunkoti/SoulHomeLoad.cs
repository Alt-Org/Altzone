using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome {

    public enum FurnitureSize
    {
        OneXOne,
        OneXTwo
    }

    [System.Serializable]
    public class Furniture
    {
        public int Id;
        public string Name;
        public Vector2 Position;
        public FurnitureSize Size;
        public float Weight;

        public Furniture(int Id, string Name, Vector2 Position, FurnitureSize Size, float Weight)
        {
            this.Id = Id;
            this.Name = Name;
            this.Position = Position;
            this.Size = Size;
            this.Weight = Weight;
            
        }
    }

    [System.Serializable]
    public class Room
    {
        public int Id;
        public bool Active = true;
        public Color floor;
        public Color walls;
        public List<Furniture> Furnitures;
    }

    [System.Serializable]
    public class SoulHome
    {
        public int Id;
        public ClanMemberRole editPermission;
        public ClanMemberRole addRemovePermission;
        public List<Room> Room;
    }

    public class SoulHomeLoad : MonoBehaviour
    {
        private SoulHome _soulHomeRooms;
        [Tooltip("Instead of getting information from the server, generate random information."), SerializeField] private bool _ignoreServer;
        [Tooltip("Use random colours when using random info"), SerializeField] private bool _randomColour;

        [SerializeField] private GameObject _roomPositions;
        [SerializeField] private GameObject _roomPrefab;

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(HomeLoad());
            //TestCode();
            LoadRooms();
        }

        public IEnumerator HomeLoad()
        {
            string json;
            // Get info from database. Eli pyydetään serveriltä RESTin yli klaanin huoneiden tiedot.
            if (!_ignoreServer) {
            CoroutineWithData cd = new CoroutineWithData(this, GetSoulHomeData());
            yield return cd.coroutine;
            Debug.Log("result is " + cd.result);
            json = cd.result.ToString();
            }
            else //Generate random room info.
            {
                SoulHome soulHome = new SoulHome();
                soulHome.Id = 1;
                soulHome.editPermission = ClanMemberRole.Officer;
                soulHome.addRemovePermission = ClanMemberRole.Admin;
                soulHome.Room = new List<Room>();
                for(int i = 0; i<30; i++)
                {
                    Room room = new Room();
                    room.Id = i;
                    if (_randomColour) //If using random colours.
                    {
                        room.floor = Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);
                        room.walls = Random.ColorHSV(0f, 1f, 1f, 1f, 0f, 1f);
                    }
                    else //And if not.
                    {
                        room.floor = Color.white;
                        room.walls = Color.yellow;
                    }
                    soulHome.Room.Add(room);
                }
                json = JsonUtility.ToJson(soulHome);
                Debug.Log(json);
            }

            _soulHomeRooms = JsonUtility.FromJson<SoulHome>(json);
            
        }

        public void TestCode()
        {
            Room test = new Room();
            test.Id = 1;
            test.Furnitures = new List<Furniture>();
            var test2 = new Furniture(1,"Name",new Vector2(0,0),FurnitureSize.OneXOne,15f);
            test.Furnitures.Add(test2);
            var test3 = new Furniture(2, "Name", new Vector2(1, 1), FurnitureSize.OneXTwo, 15f);
            test.Furnitures.Add(test3);
            string testTojson = JsonUtility.ToJson(test);
            Debug.Log(testTojson);
            Debug.Log(JsonUtility.FromJson<Room>(testTojson).Furnitures[1].Id);
        }
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
                Instantiate (_roomPrefab, roompositions[i].transform);
                //roompositions[i].transform.GetChild(0).Find("Floor").gameObject.GetComponent<Image>().color = room.floor;
                //roompositions[i].transform.GetChild(0).Find("Wall").GetChild(0).gameObject.GetComponent<Image>().color = room.walls;
                //roompositions[i].transform.GetChild(0).Find("Wall2").GetChild(0).gameObject.GetComponent<Image>().color = room.walls;
                i++;
            }
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

private IEnumerator GetSoulHomeData()
        {
            UnityWebRequest request = new UnityWebRequest("http://someurl");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success){
                yield return request.downloadHandler.text;
            }
            else
            {
                yield return "fail";
            }
        }
    }
}
