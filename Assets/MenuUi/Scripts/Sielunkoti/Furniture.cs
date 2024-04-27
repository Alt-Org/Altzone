using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public enum FurnitureSize
    {
        OneXOne,
        OneXTwo,
        OneXFour
    }

    [Serializable]
    public class Furniture
    {
        public int Id;
        public string Name;
        public Vector2Int Position;
        public bool IsRotated;
        public FurnitureSize Size;
        public float Weight;
        public float value;

        public Furniture(int Id, string Name, Vector2Int Position, FurnitureSize Size, float Weight, bool IsRotated)
        {
            this.Id = Id;
            this.Name = Name;
            this.Position = Position;
            this.Size = Size;
            this.Weight = Weight;
            this.IsRotated = IsRotated;

        }
    }
}
