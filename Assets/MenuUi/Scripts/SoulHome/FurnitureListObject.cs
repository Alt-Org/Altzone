using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureListObject
    {
        private List<Furniture> _list = new();

        private string _name;
        private int _count = 0;
        private int _inRoomCount = 0;

        public string Name { get => _name;}
        public int Count { get => _count;}
        public List<Furniture> List { get => _list;}

        public void Add(Furniture furniture)
        {
            if(_count == 0) _name = furniture.Name;
            _list.Add(furniture);
            _count++;
            if(!furniture.Position.Equals(new(-1, -1))) _inRoomCount++;
        }

        public void Remove(Furniture furniture)
        {
            if (_count <= 0)  return;
            bool check = _list.Remove(furniture);
            if (check)
            {
                _count--;
                if (!furniture.Position.Equals(new(-1, -1))) _inRoomCount--;
            }
        }

        public int GetInRoomCount()
        {
            int count = 0;
            foreach (Furniture furniture in _list)
            {
                if(!furniture.Position.Equals(new(-1,-1))) count++;
            }
            return count;
        }
    }

    public enum SortType
    {
        Name,
        Count
    }

    public class FurnitureList
    {
        private List<FurnitureListObject> _list = new();

        private int _count = 0;

        public int Count { get => _count; }
        public List<FurnitureListObject> List { get => _list; }

        public void Add(Furniture furniture)
        {
            foreach(FurnitureListObject listObject in _list)
            {
                if(listObject.Name == furniture.Name)
                {
                    listObject.Add(furniture);
                    return;
                }
            }
            FurnitureListObject newListObject = new();
            newListObject.Add(furniture);
            _list.Add(newListObject);
            _count++;
        }

        public void Remove(Furniture furniture)
        {
            if (_count <= 0) return;

            foreach (FurnitureListObject listObject in _list)
            {
                if (listObject.Name == furniture.Name)
                {
                    listObject.Remove(furniture);
                    return;
                }
            }
        }

        public List<FurnitureListObject> Get()
        {
            return _list;
        }

        public void ClearEmptyLists()
        {
            if (_count <= 0) return;
            int amount = 0;
            foreach (FurnitureListObject listObject in _list)
            {
                if (listObject.Count < 1)
                {
                    _list.Remove(listObject);
                    amount++;
                }
            }
            Debug.Log("Removed "+amount+" listObjects.");
        }

        public void Sort(SortType type)
        {
            if(type == SortType.Name)
            {
                _list.Sort(SortByName);
            }
            else if(type == SortType.Count)
            {
                _list.Sort(SortByCount);
            }
        }

        private static int SortByName(FurnitureListObject value1, FurnitureListObject value2)
        {
            if (value1 == null)
            {
                if (value2 == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (value2 == null)
                {
                    return 1;
                }
                else
                {
                    return value1.Name.CompareTo(value2.Name);

                }
            }
        }

        private static int SortByCount(FurnitureListObject value1, FurnitureListObject value2)
        {
            if (value1 == null)
            {
                if (value2 == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (value2 == null)
                {
                    return 1;
                }
                else
                {
                    return value1.Count.CompareTo(value2.Count);

                }
            }
        }
    }
}
