using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    [Serializable]
    public class Furniture
    {
        /*private int _id;
        [SerializeField]
        private string _name;
        private Vector2Int _position;
        [SerializeField]
        private bool _isRotated;
        [SerializeField]
        private FurnitureSize _size;
        [SerializeField]
        private FurnitureSize _rotatedSize;
        [SerializeField]
        private FurniturePlace _place;
        [SerializeField]
        private float _weight;
        [SerializeField]
        private float _value;*/

        public string Name;
        public int Id = -1;
        public Vector2Int Position = new(-1,-1);
        public int Room = -1;
        public bool IsRotated;
        public FurnitureSize Size;
        public FurnitureSize RotatedSize;
        public FurniturePlacement Place;
        public double Weight;
        public float Value;

        public Furniture(int Id, string Name, Vector2Int Position, FurnitureSize Size, FurnitureSize RotatedSize, FurniturePlacement Place, float Value, float Weight, bool IsRotated, int Room = -1)
        {
            this.Id = Id;
            this.Name = Name;
            this.Position = Position;
            this.Room = Room;
            this.Size = Size;
            this.RotatedSize = RotatedSize;
            this.Place = Place;
            this.Value = Value;
            this.Weight = Weight;
            this.IsRotated = IsRotated;

        }

        public Furniture(ClanFurniture clanFurniture, GameFurniture gameFurniture/*, FurnitureInfo info*/)
        {
            Id = int.Parse(clanFurniture.Id);
            Name = clanFurniture.GameFurnitureName;
            Position = clanFurniture.Position;
            Room = clanFurniture.Room;
            Size = gameFurniture.Size;
            RotatedSize = gameFurniture.RotatedSize;
            Place = gameFurniture.Placement;
            Value = gameFurniture.Value;
            Weight = gameFurniture.Weight;
            IsRotated = clanFurniture.IsRotated;

        }

        /*public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public Vector2Int Position { get => _position; set => _position = value; }
        public bool IsRotated { get => _isRotated; set => _isRotated = value; }
        public FurnitureSize Size { get => _size; set => _size = value; }
        public FurnitureSize RotatedSize { get => _rotatedSize; set => _rotatedSize = value; }
        public FurniturePlace Place { get => _place; set => _place = value; }
        public float Weight { get => _weight; set => _weight = value; }
        public float Value { get => _value; set => _value = value; }*/

        public Vector2Int GetFurnitureSize()
        {
            FurnitureSize furnitureSize;
            if (IsRotated) furnitureSize = RotatedSize;
            else furnitureSize = Size;

            return GetFurnitureSize(furnitureSize);
        }

        public Vector2Int GetFurnitureSizeRotated()
        {
            FurnitureSize furnitureSize;
            if (IsRotated) furnitureSize = Size;
            else furnitureSize = RotatedSize;

            return GetFurnitureSize(furnitureSize);
        }

        private Vector2Int GetFurnitureSize(FurnitureSize furnitureSize)
        {
            Debug.Log(furnitureSize);

            if (furnitureSize == FurnitureSize.OneXOne)
            {
                return new Vector2Int(1, 1);
            }
            else if (furnitureSize == FurnitureSize.OneXTwo)
            {
                return new Vector2Int(2, 1);
            }
            else if (furnitureSize == FurnitureSize.OneXThree)
            {
                return new Vector2Int(3, 1);
            }
            else if (furnitureSize == FurnitureSize.OneXFour)
            {
                return new Vector2Int(4, 1);
            }
            else if (furnitureSize == FurnitureSize.OneXSix)
            {
                return new Vector2Int(6, 1);
            }
            else if (furnitureSize == FurnitureSize.TwoXOne)
            {
                return new Vector2Int(1, 2);
            }
            else if (furnitureSize == FurnitureSize.TwoXTwo)
            {
                return new Vector2Int(2, 2);
            }
            else if (furnitureSize == FurnitureSize.TwoXThree)
            {
                return new Vector2Int(3, 2);
            }
            else if (furnitureSize == FurnitureSize.TwoXFour)
            {
                return new Vector2Int(4, 2);
            }
            else if (furnitureSize == FurnitureSize.TwoXSix)
            {
                return new Vector2Int(6, 2);
            }
            else if (furnitureSize == FurnitureSize.TwoXSeven)
            {
                return new Vector2Int(7, 2);
            }
            else if (furnitureSize == FurnitureSize.TwoXEight)
            {
                return new Vector2Int(8, 2);
            }
            else if (furnitureSize == FurnitureSize.ThreeXTwo)
            {
                return new Vector2Int(2, 3);
            }
            else if (furnitureSize == FurnitureSize.ThreeXThree)
            {
                return new Vector2Int(3, 3);
            }
            else if (furnitureSize == FurnitureSize.ThreeXSeven)
            {
                return new Vector2Int(7, 3);
            }
            else if (furnitureSize == FurnitureSize.ThreeXEight)
            {
                return new Vector2Int(8, 3);
            }
            else if (furnitureSize == FurnitureSize.FourXTwo)
            {
                return new Vector2Int(2, 4);
            }
            else if (furnitureSize == FurnitureSize.SevenXThree)
            {
                return new Vector2Int(3, 7);
            }
            else
            {
                Debug.LogError("Error: Invalid furniture size");
                return new Vector2Int(0, 0);
            }
        }

        public void ResetPosition()
        {
            Position = new Vector2Int(-1, -1);
            Room = -1;
        }
    }
}
