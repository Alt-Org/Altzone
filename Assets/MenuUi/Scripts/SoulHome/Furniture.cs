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
        public string Id = "-1";
        public Vector2Int Position = new(-1,-1);
        public FurnitureGrid FurnitureGrid = FurnitureGrid.None;
        public int Room = -1;
        public bool IsRotated;
        public FurnitureSize Size;
        public FurnitureSize RotatedSize;
        public FurniturePlacement Place;
        public double Weight;
        public float Value;

        public Furniture(int Id, string Name, Vector2Int Position, FurnitureSize Size, FurnitureSize RotatedSize, FurniturePlacement Place, float Value, float Weight, bool IsRotated, int Room = -1)
        {
            this.Id = Id.ToString();
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
            Id = clanFurniture.Id;
            Name = clanFurniture.GameFurnitureName;
            Position = clanFurniture.Position;
            FurnitureGrid = clanFurniture.FurnitureGrid;
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

        public static Vector2Int GetFurnitureSize(FurnitureSize furnitureSize)
        {
            //Debug.Log(furnitureSize);

            switch (furnitureSize)
            {
                case FurnitureSize.OneXOne:
                    return new Vector2Int(1, 1);
                case FurnitureSize.OneXTwo:
                    return new Vector2Int(2, 1);
                case FurnitureSize.OneXThree:
                    return new Vector2Int(3, 1);
                case FurnitureSize.OneXFour:
                    return new Vector2Int(4, 1);
                case FurnitureSize.OneXSix:
                    return new Vector2Int(6, 1);
                case FurnitureSize.TwoXOne:
                    return new Vector2Int(1, 2);
                case FurnitureSize.TwoXTwo:
                    return new Vector2Int(2, 2);
                case FurnitureSize.TwoXThree:
                    return new Vector2Int(3, 2);
                case FurnitureSize.TwoXFour:
                    return new Vector2Int(4, 2);
                case FurnitureSize.TwoXFive:
                    return new Vector2Int(5, 2);
                case FurnitureSize.TwoXSix:
                    return new Vector2Int(6, 2);
                case FurnitureSize.TwoXSeven:
                    return new Vector2Int(7, 2);
                case FurnitureSize.TwoXEight:
                    return new Vector2Int(8, 2);
                case FurnitureSize.ThreeXTwo:
                    return new Vector2Int(2, 3);
                case FurnitureSize.ThreeXThree:
                    return new Vector2Int(3, 3);
                case FurnitureSize.ThreeXFour:
                    return new Vector2Int(4, 3);
                case FurnitureSize.ThreeXSix:
                    return new Vector2Int(6, 3);
                case FurnitureSize.ThreeXSeven:
                    return new Vector2Int(7, 3);
                case FurnitureSize.ThreeXEight:
                    return new Vector2Int(8, 3);
                case FurnitureSize.FourXTwo:
                    return new Vector2Int(2, 4);
                case FurnitureSize.FourXThree:
                    return new Vector2Int(3, 4);
                case FurnitureSize.FourXFour:
                    return new Vector2Int(4, 4);
                case FurnitureSize.FiveXTwo:
                    return new Vector2Int(2, 5);
                case FurnitureSize.FiveXThree:
                    return new Vector2Int(3, 5);
                case FurnitureSize.FiveXFive:
                    return new Vector2Int(5, 5);
                case FurnitureSize.FiveXEight:
                    return new Vector2Int(8, 5);
                case FurnitureSize.SevenXThree:
                    return new Vector2Int(3, 7);
                default:
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
