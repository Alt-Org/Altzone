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
        public Vector3Int FurnitureSize;
        public FurniturePlacement Place;
        public double Weight;
        public float Value;

        public Furniture(int Id, string Name, Vector2Int Position, FurniturePlacement Place, float Value, float Weight, bool IsRotated, int Room = -1)
        {
            this.Id = Id.ToString();
            this.Name = Name;
            this.Position = Position;
            this.Room = Room;
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
            FurnitureSize = gameFurniture.FurnitureSize;
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

        public Vector3Int GetFurnitureSize()
        {
            if (IsRotated) return GetFurnitureSizeRotated();
            else return GetFurnitureNormalSize();
        }

        public Vector3Int GetFurnitureNormalSize()
        {
            return Place switch
            {
                FurniturePlacement.Floor or
                FurniturePlacement.FloorByWall or
                FurniturePlacement.FloorNonblock or
                FurniturePlacement.Ceiling => new(FurnitureSize.x, FurnitureSize.z, FurnitureSize.y),
                FurniturePlacement.Wall => new(FurnitureSize.x, FurnitureSize.y, FurnitureSize.z),
                _ => new(FurnitureSize.x, FurnitureSize.z, FurnitureSize.y),
            };
        }

        public Vector3Int GetFurnitureSizeRotated()
        {
            return Place switch
            {
                FurniturePlacement.Floor or
                FurniturePlacement.FloorByWall or
                FurniturePlacement.FloorNonblock or
                FurniturePlacement.Ceiling => new(FurnitureSize.z, FurnitureSize.x, FurnitureSize.y),
                FurniturePlacement.Wall => new(FurnitureSize.y, FurnitureSize.x, FurnitureSize.z),
                _ => new(FurnitureSize.z, FurnitureSize.x, FurnitureSize.y),
            };
        }

        public void ResetPosition()
        {
            Position = new Vector2Int(-1, -1);
            Room = -1;
        }
    }
}
