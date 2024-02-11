using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
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
}
