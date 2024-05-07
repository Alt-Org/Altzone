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
        OneXFour,
        TwoXOne,
        TwoXTwo
    }

    [Serializable]
    public class Furniture
    {
        public int Id;
        public string Name;
        public Vector2Int Position;
        public bool IsRotated;
        public FurnitureSize Size;
        public FurnitureSize RotatedSize;
        public float Weight;
        public float value;

        public Furniture(int Id, string Name, Vector2Int Position, FurnitureSize Size, FurnitureSize RotatedSize, float Weight, bool IsRotated)
        {
            this.Id = Id;
            this.Name = Name;
            this.Position = Position;
            this.Size = Size;
            this.RotatedSize = RotatedSize;
            this.Weight = Weight;
            this.IsRotated = IsRotated;

        }

        public Vector2Int GetFurnitureSize()
        {
            FurnitureSize furnitureSize;
            if (IsRotated) furnitureSize = RotatedSize;
            else furnitureSize = Size;

            Debug.Log(furnitureSize);

            if (furnitureSize == FurnitureSize.OneXOne)
            {
                return new Vector2Int(1, 1);
            }
            else if (furnitureSize == FurnitureSize.OneXTwo)
            {
                return new Vector2Int(2, 1);
            }
            else if (furnitureSize == FurnitureSize.OneXFour)
            {
                return new Vector2Int(4, 1);
            }
            else if (furnitureSize == FurnitureSize.TwoXOne)
            {
                return new Vector2Int(1, 2);
            }
            else if (furnitureSize == FurnitureSize.TwoXTwo)
            {
                return new Vector2Int(2, 2);
            }
            else
            {
                Debug.LogError("Error: Invalid furniture size");
                return new Vector2Int(0, 0);
            }
        }
    }
}
