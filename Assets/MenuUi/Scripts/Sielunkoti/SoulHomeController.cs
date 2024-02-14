using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;
using TMPro;

namespace MenuUI.Scripts.SoulHome
{
    public class SoulHomeController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _roomName;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetRoomName(GameObject room)
        {
            if (room != null)
            {
                _roomName.SetActive(true);
                string roomName = room.GetComponent<RoomData>().RoomInfo.Id.ToString();
                _roomName.GetComponent<TextMeshProUGUI>().text = "Huone " + roomName;
            }
            else _roomName.SetActive(false);
        }
    }
}
