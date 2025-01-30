using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace MenuUi.Scripts.Storage
{
    public class StorageFurniture
    {
        private string _name;
        private string _id = "-1";
        private Sprite _sprite;
        private string _visibleName;
        private FurnitureRarity _rarity;
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
        private FurnitureInfo _info;
        private ClanFurniture _furniture; //Unsure if this is needed for Storage but I'll leave it in for now.

        public StorageFurniture(ClanFurniture clanFurniture, GameFurniture gameFurniture)
        {
            _id = clanFurniture.Id;
            _name = clanFurniture.GameFurnitureName;
            _sprite = gameFurniture.FurnitureInfo.Image;
            _visibleName = gameFurniture.FurnitureInfo.VisibleName;
            _rarity = gameFurniture.Rarity;
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
            _info = gameFurniture.FurnitureInfo;

        }

        public string Name { get => _name;}
        public string Id { get => _id;}
        public Sprite Sprite { get => _info.Image;}
        public string VisibleName { get => _info.VisibleName;}
        public string SetName { get => _info.SetName; }
        public FurnitureRarity Rarity { get => _rarity; }
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
        public FurnitureInfo Info { get => _info; set => _info = value; }
    }
}
