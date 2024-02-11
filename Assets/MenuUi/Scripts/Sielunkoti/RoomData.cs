using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;

namespace MenuUI.Scripts.SoulHome
{
    public class RoomData : MonoBehaviour
    {
        private int id;
        private bool active = true;
        private Color floor;
        private Color walls;
        private List<Furniture> furnitures;

        public int Id { get => id; set => id = value; }

        public RoomData(int Id)
        {
            id = Id;

        }
    }
}
