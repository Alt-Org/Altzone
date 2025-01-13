using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.ReferenceSheets;
using UnityEngine;

namespace MenuUi.Scripts.Storage
{
    public class StorageFurniture
    {
        private string _name;
        private string _id = "-1";
        private Sprite _sprite;
        private string _visibleName;
        private Vector2Int _position = new(-1, -1);
        private int _room = -1;
        private bool _isRotated;
        private FurnitureSize _size;
        private FurnitureSize _rotatedSize;
        private FurniturePlacement _placement;
        private double _weight;
        private float _value;
        private string _material;
        private string _recycling;
        private ClanFurniture _furniture; //Unsure if this is needed for Storage but I'll leave it in for now.

        public StorageFurniture(ClanFurniture clanFurniture, GameFurniture gameFurniture, FurnitureInfo info)
        {
            _id = clanFurniture.Id;
            _name = clanFurniture.GameFurnitureName;
            _sprite = info.Image;
            _visibleName = info.VisibleName;
            _position = clanFurniture.Position;
            _room = clanFurniture.Room;
            _size = gameFurniture.Size;
            _rotatedSize = gameFurniture.RotatedSize;
            _placement = gameFurniture.Placement;
            _value = gameFurniture.Value;
            _weight = gameFurniture.Weight;
            _isRotated = clanFurniture.IsRotated;
            _material = gameFurniture.Material;
            _recycling = gameFurniture.Recycling;
            _furniture = clanFurniture;

        }

        public string Name { get => _name;}
        public string Id { get => _id;}
        public Sprite Sprite { get => _sprite;}
        public string VisibleName { get => _visibleName;}
        public Vector2Int Position { get => _position; set => _position = value; }
        public int Room { get => _room; set => _room = value; }
        public bool IsRotated { get => _isRotated; set => _isRotated = value; }
        public FurnitureSize Size { get => _size;}
        public FurnitureSize RotatedSize { get => _rotatedSize;}
        public FurniturePlacement Placement { get => _placement;}
        public double Weight { get => _weight;}
        public float Value { get => _value;}
        public string Material { get => _material;}
        public string Recycling { get => _recycling;}
    }
}
