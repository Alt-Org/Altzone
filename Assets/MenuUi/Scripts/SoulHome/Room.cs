using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    [Serializable]
    public class Room
    {
        public int id;
        public bool active = true;
        public Color floor;
        public Color walls;
        public List<Furniture> furnitures = new();

        public int Id { get => id; set => id = value; }
        public bool Active { get => active; set => active = value; }
        public Color Floor { get => floor; set => floor = value; }
        public Color Walls { get => walls; set => walls = value; }
        public List<Furniture> Furnitures { get => furnitures; set => furnitures = value; }

        /*public Room(int Id)
        {
            id = Id;

        }*/
    }
}
