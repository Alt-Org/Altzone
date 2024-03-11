using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;

namespace MenuUI.Scripts.SoulHome
{
    public class RoomData : MonoBehaviour
    {
        [SerializeField]
        private float _roomWidth = 50;
        [SerializeField]
        private float _slotRows = 3;
        [SerializeField]
        private float _slotColumns = 8;

        private Room _roomInfo;

        public Room RoomInfo { get => _roomInfo; set => _roomInfo = value; }

        void Start()
        {
            //roomInfo = new Room();
        }
        public void InitializeRoom()
        {
            Transform points = transform.Find("FurniturePoints");
            int row = 0;
            int col = 0;
            /*for(int i = 0; i < _slotRows; i++)
            {
                Instantiate(new GameObject(),points.transform);
                for (int j = 0; j < _slotColumns; j++)
                {

                }
            }*/

            foreach (Transform pointRow in points)
            {
                col = 0;
                foreach (Transform point in pointRow)
                {
                    point.GetComponent<FurnitureSlot>().InitializeSlot(row, col);
                    col++;
                }
                row++;
            }
            if (_roomInfo.Furnitures.Count > 0) InitialSetFurniture();
        }

        private void InitialSetFurniture()
        {
            foreach (Furniture furniture in _roomInfo.Furnitures)
            {
                int furnitureRow = furniture.Position.y;
                int furnitureColumn = furniture.Position.x;
                bool check = CheckFurniturePosition(furnitureRow, furnitureColumn, furniture.Size, furniture);
                if (check) SetFurniture(furnitureRow, furnitureColumn, furniture.Size, furniture);
            }
        }
        private bool CheckFurniturePosition(int row, int column, FurnitureSize size, Furniture furniture)
        {
            int furnitureRowSize;
            int furnitureColumnSize;
            Transform points = transform.Find("FurniturePoints");
            if (size == FurnitureSize.OneXOne)
            {
                furnitureRowSize = 1;
                furnitureColumnSize = 1;
            }
            else if (size == FurnitureSize.OneXTwo)
            {
                furnitureRowSize = 2;
                furnitureColumnSize = 1;
            }
            else
            {
                Debug.Log("Error: Invalid furniture size");
                return false;
            }
            int startRow = row - (furnitureColumnSize - 1);
            int endColumn = column + (furnitureRowSize - 1);

            for (int i = startRow; i <= row; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    if (points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture != null
                        && points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture != furniture) return false;
                }
            }
            return true;
        }
        private void SetFurniture(int row, int column, FurnitureSize size, Furniture furniture)
        {
            int furnitureRowSize;
            int furnitureColumnSize;
            Transform points = transform.Find("FurniturePoints");
            if (size == FurnitureSize.OneXOne)
            {
                furnitureRowSize = 1;
                furnitureColumnSize = 1;
            }
            else if (size == FurnitureSize.OneXTwo)
            {
                furnitureRowSize = 2;
                furnitureColumnSize = 1;
            }
            else
            {
                Debug.Log("Error: Invalid furniture size");
                return;
            }

            int startRow = row - (furnitureColumnSize - 1);
            int endColumn = column + (furnitureRowSize - 1);

            for (int i = startRow; i <= row; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                }
            }

            Instantiate(points.GetComponent<FurnitureReference>().GetFurniture(furniture.Name), points.GetChild(row).GetChild(column));
            GameObject furnitureObject = points.GetChild(row).GetChild(column).GetChild(0).gameObject;
            furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture;
            furnitureObject.GetComponent<FurnitureHandling>().Position = new(column, row);
            furnitureObject.GetComponent<FurnitureHandling>().Slot = points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            if (row == 0)
            {
                furnitureObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
                furnitureObject.transform.localScale *= 1.0f;
            }
            else if (row == 1)
            {
                furnitureObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
                furnitureObject.transform.localScale *= 1.1f;
            }
            else if (row == 2)
            {
                furnitureObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
                furnitureObject.transform.localScale *= 1.2f;
            }
        }
        public void MoveFurniture(int row, int column, FurnitureSize size, GameObject furniture, bool hover)
        {
            int furnitureRowSize;
            int furnitureColumnSize;
            Transform points = transform.Find("FurniturePoints");
            if (!hover) { 
                if (size == FurnitureSize.OneXOne)
                {
                    furnitureRowSize = 1;
                    furnitureColumnSize = 1;
                }
                else if (size == FurnitureSize.OneXTwo)
                {
                    furnitureRowSize = 2;
                    furnitureColumnSize = 1;
                }
                else
                {
                    Debug.Log("Error: Invalid furniture size");
                    return;
                }

                int startRow = row - (furnitureColumnSize - 1);
                int endColumn = column + (furnitureRowSize - 1);

                for (int i = startRow; i <= row; i++)
                {
                    for (int j = column; j <= endColumn; j++)
                    {
                        points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                    }
                }
                if (furniture.GetComponent<FurnitureHandling>().Slot != null)
                {
                    int prevRow = furniture.GetComponent<FurnitureHandling>().Slot.row;
                    int prevColumn = furniture.GetComponent<FurnitureHandling>().Slot.column;

                    startRow = prevRow - (furnitureColumnSize - 1);
                    endColumn = prevColumn + (furnitureRowSize - 1);

                    for (int i = startRow; i <= prevRow; i++)
                    {
                        for (int j = prevColumn; j <= endColumn; j++)
                        {
                            points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                        }
                    }
                }
                furniture.GetComponent<FurnitureHandling>().Slot = points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            }
            furniture.transform.SetParent(points.GetChild(row).GetChild(column));
            furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            if (furniture.GetComponent<SpriteRenderer>().sortingOrder == 4) furniture.transform.localScale /= 1.1f;
            else if (furniture.GetComponent<SpriteRenderer>().sortingOrder == 5) furniture.transform.localScale /= 1.2f;

            if (row == 0)
            {
                furniture.GetComponent<SpriteRenderer>().sortingOrder = 3;
                furniture.transform.localScale *= 1.0f;
            }
            else if (row == 1)
            {
                furniture.GetComponent<SpriteRenderer>().sortingOrder = 4;
                furniture.transform.localScale *= 1.1f;
            }
            else if (row == 2)
            {
                furniture.GetComponent<SpriteRenderer>().sortingOrder = 5;
                furniture.transform.localScale *= 1.2f;
            }
        }

        public bool HandleFurniturePosition(Vector2 point, Camera camera, GameObject furniture, bool hover)
        {
            if (furniture.GetComponent<FurnitureHandling>() == null) return false;

            //Debug.Log("Pass.");
            Ray ray = new(camera.transform.position, (Vector3)point - camera.transform.position);
            //Ray ray = camera.ScreenPointToRay(point);
            RaycastHit2D[] hit;
            hit = Physics2D.GetRayIntersectionAll(ray, 1000);
            //Debug.Log(point - ray.origin);
            //Debug.Log(hit);
            foreach (RaycastHit2D hit2 in hit)
            {
                if (hit2.collider != null)
                {
                    //Debug.Log(hit2 +": Collider found," +hit2.transform.tag);
                    if (hit2.collider.gameObject.GetComponent<FurnitureSlot>() != null)
                    {
                        //Debug.Log(hit2 + ": Slot found, Room "+ hit2.collider.transform.parent.parent.parent.GetComponent<RoomData>().RoomInfo.Id+
                        //    ", Slot " + hit2.collider.GetComponent<FurnitureSlot>().row +":"+ hit2.collider.GetComponent<FurnitureSlot>().column);
                        GameObject slot = hit2.collider.gameObject;
                        Furniture furnitureInfo = furniture.GetComponent<FurnitureHandling>().Furniture;
                        bool check = CheckFurniturePosition(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furnitureInfo.Size, furnitureInfo);
                        if (check)
                        {
                            MoveFurniture(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furnitureInfo.Size, furniture, hover);
                            return true;
                        }
                    }
                }
            }
            if (!hover)
            {
                if (furniture.GetComponent<FurnitureHandling>().Slot == null)
                {
                    Destroy(furniture);
                    return false;
                }
                ResetPosition(furniture);
            }
            return false;
        }
        public void FreeFurnitureSlots(FurnitureSize size ,FurnitureSlot slot)
        {
            int furnitureRowSize;
            int furnitureColumnSize;
            Transform points = transform.Find("FurniturePoints");
            if (size == FurnitureSize.OneXOne)
            {
                furnitureRowSize = 1;
                furnitureColumnSize = 1;
            }
            else if (size == FurnitureSize.OneXTwo)
            {
                furnitureRowSize = 2;
                furnitureColumnSize = 1;
            }
            else
            {
                Debug.Log("Error: Invalid furniture size");
                return;
            }

            if (slot != null)
            {
                int prevRow = slot.row;
                int prevColumn = slot.column;

                int startRow = prevRow - (furnitureColumnSize - 1);
                int endColumn = prevColumn + (furnitureRowSize - 1);

                for (int i = startRow; i <= prevRow; i++)
                {
                    for (int j = prevColumn; j <= endColumn; j++)
                    {
                        points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                    }
                }
            }
        }

        public void ResetPosition(GameObject furniture)
        {
            int prevRow = furniture.GetComponent<FurnitureHandling>().Slot.row;
            int prevColumn = furniture.GetComponent<FurnitureHandling>().Slot.column;
            furniture.transform.SetParent(transform.Find("FurniturePoints").GetChild(prevRow).GetChild(prevColumn));
        }
    }
}
