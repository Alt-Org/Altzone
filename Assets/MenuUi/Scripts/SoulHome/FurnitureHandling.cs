using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = Prg.Debug;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureHandling : MonoBehaviour
    {
        public enum Direction
        {
            Front,
            Left,
            Right,
            Back
        }

        [SerializeField]
        private string _name;

        [SerializeField]
        private Sprite _furnitureSpriteFront;
        [SerializeField]
        private Sprite _furnitureSpriteRight;
        [SerializeField]
        private Sprite _furnitureSpriteLeft;
        [SerializeField]
        private Sprite _furnitureSpriteBack;

        [SerializeField]
        private Direction _defaultSpriteDirection = Direction.Front;
        private Direction _spriteDirection;
        private Direction _tempSpriteDirection;
        [SerializeField]
        private bool _spriteCanBeFlipped = true;
        private bool _isRotated = false;
        [SerializeField]
        private SortingGroup _sortingGroup;

        private Furniture _furniture;
        [SerializeField]
        private GameObject _trayFurnitureObject;
        private BoxCollider2D _collider;
        private Bounds _bounds;
        [SerializeField]
        private float cullCheckAmount = 0.1f;
        private Vector2 _position = Vector2.zero;
        private FurnitureSlot _slot;
        private FurnitureSlot _tempSlot;

        public Furniture Furniture { get => _furniture; set => _furniture = value; }
        public Vector2 Position { get => _position; set => _position = value; }
        public FurnitureSlot Slot { get => _slot;
            set
            {
                _slot = value;
                TempSlot = value;
            }
        }
        public FurnitureSlot TempSlot { get => _tempSlot; set
            {
                _tempSlot = value;
                if (_tempSlot != null)
                {
                    Furniture.Position = new(value.column, value.row);
                    Furniture.Room = value.roomId;
                }
                else
                {
                    Furniture.Position = new(-1, -1);
                    Furniture.Room = -1;
                }
            }
        }
        public GameObject TrayFurnitureObject { get => _trayFurnitureObject; set { if(!Application.isPlaying) _trayFurnitureObject = value; }}
        public string Name { get => _name; set { if (!Application.isPlaying) _name = value; } }
        public bool IsRotated { get => _isRotated;}
        public Direction TempSpriteDirection { get => _tempSpriteDirection;}
        public Sprite FurnitureSpriteFront { get => _furnitureSpriteFront; set { if (!Application.isPlaying) _furnitureSpriteFront = value; } }
        public Sprite FurnitureSpriteRight { get => _furnitureSpriteRight; set { if (!Application.isPlaying) _furnitureSpriteRight = value; } }
        public Sprite FurnitureSpriteLeft { get => _furnitureSpriteLeft; set { if (!Application.isPlaying) _furnitureSpriteLeft = value; } }
        public Sprite FurnitureSpriteBack { get => _furnitureSpriteBack; set { if (!Application.isPlaying) _furnitureSpriteBack = value; } }
        public bool SpriteCanBeFlipped { get => _spriteCanBeFlipped;}

        // Start is called before the first frame update
        void Start()
        {
            if (_furnitureSpriteFront ==  null)
            {
                if (_furnitureSpriteLeft != null) _furnitureSpriteFront = _furnitureSpriteLeft;
                else if (_furnitureSpriteRight != null) _furnitureSpriteFront = _furnitureSpriteLeft;
                else if (_furnitureSpriteBack != null) _furnitureSpriteFront = _furnitureSpriteBack;
                else Debug.LogError("Furniture Sprite not found by script. Add sprite to the script.");
            }
            _spriteDirection = _defaultSpriteDirection;

            if (_spriteDirection == Direction.Front && _furnitureSpriteFront != null)
            {
                GetComponent<SpriteRenderer>().sprite = _furnitureSpriteFront;
            }
            else if (_spriteDirection == Direction.Right && _furnitureSpriteRight != null)
            {
                GetComponent<SpriteRenderer>().sprite = _furnitureSpriteRight;
            }
            else if (_spriteDirection == Direction.Left && _furnitureSpriteLeft != null)
            {
                GetComponent<SpriteRenderer>().sprite = _furnitureSpriteLeft;
            }
            else if (_spriteDirection == Direction.Back && _furnitureSpriteBack != null)
            {
                GetComponent<SpriteRenderer>().sprite = _furnitureSpriteBack;
            }
            else
            {
                _spriteDirection = Direction.Front;
                if( _furnitureSpriteFront != null )
                GetComponent<SpriteRenderer>().sprite = _furnitureSpriteFront;
            }
            _tempSpriteDirection = _spriteDirection;

            _collider = GetComponent<BoxCollider2D>();
            _bounds = _collider.bounds;
        }

        public Vector2Int GetFurnitureSize()
        {
            return Furniture.GetFurnitureSize();
        }
        public Vector2Int GetFurnitureSizeRotated()
        {
            return Furniture.GetFurnitureSizeRotated();
        }

        public bool checkTopCollider(float hitY)
        {
            Vector2 tr = _bounds.max;
            Vector2 bl = _bounds.min;
            float limit = tr.y - Mathf.Abs(tr.y - bl.y) * cullCheckAmount;
            if (hitY >= limit) return true;
            else return false;
        }

        public void ResetFurniturePosition(bool reverse = false)
        {
            Vector2 position = Vector2.zero;
            //transform.localPosition = Vector2.zero;

            FurnitureSize furnitureSize;
            if (Furniture.IsRotated) furnitureSize = Furniture.RotatedSize;
            else furnitureSize = Furniture.Size;

            float width;
            if (furnitureSize is FurnitureSize.OneXOne or FurnitureSize.TwoXOne or FurnitureSize.ThreeXOne or FurnitureSize.FourXOne)
            {
                //if(_tempSlot != null)width = _tempSlot.width;
                /*else*/ width = transform.parent.GetComponent<FurnitureSlot>().width;
            }
            else if (furnitureSize is FurnitureSize.OneXTwo or FurnitureSize.TwoXTwo or FurnitureSize.ThreeXTwo or FurnitureSize.FourXTwo or FurnitureSize.FiveXTwo)
            {
                //if (_tempSlot != null) width = _tempSlot.width * 2;
                /*else*/ width = transform.parent.GetComponent<FurnitureSlot>().width * 2;
            }
            else if (furnitureSize is FurnitureSize.OneXThree or FurnitureSize.TwoXThree or FurnitureSize.ThreeXThree or FurnitureSize.FourXThree or FurnitureSize.FiveXThree or FurnitureSize.SevenXThree)
            {
                //if (_tempSlot != null) width = _tempSlot.width * 3;
                /*else*/
                width = transform.parent.GetComponent<FurnitureSlot>().width * 3;
            }
            else if (furnitureSize is FurnitureSize.OneXFour or FurnitureSize.TwoXFour or FurnitureSize.ThreeXFour or FurnitureSize.FourXFour)
            {
                //if (_tempSlot != null) width = _tempSlot.width * 4;
                /*else*/ width = transform.parent.GetComponent<FurnitureSlot>().width * 4;
            }
            else if (furnitureSize is FurnitureSize.TwoXFive or FurnitureSize.ThreeXFive or FurnitureSize.FiveXFive or FurnitureSize.SixXFive)
            {
                //if (_tempSlot != null) width = _tempSlot.width * 4;
                /*else*/
                width = transform.parent.GetComponent<FurnitureSlot>().width * 5;
            }
            else if (furnitureSize is FurnitureSize.OneXSix or FurnitureSize.TwoXSix or FurnitureSize.ThreeXSix or FurnitureSize.FiveXSix)
            {
                //if (_tempSlot != null) width = _tempSlot.width * 4;
                /*else*/
                width = transform.parent.GetComponent<FurnitureSlot>().width * 6;
            }
            else if (furnitureSize is FurnitureSize.TwoXSeven or FurnitureSize.ThreeXSeven)
            {
                //if (_tempSlot != null) width = _tempSlot.width * 4;
                /*else*/
                width = transform.parent.GetComponent<FurnitureSlot>().width * 7;
            }
            else if (furnitureSize is FurnitureSize.TwoXEight or FurnitureSize.ThreeXEight or FurnitureSize.FiveXEight)
            {
                //if (_tempSlot != null) width = _tempSlot.width * 4;
                /*else*/
                width = transform.parent.GetComponent<FurnitureSlot>().width * 8;
            }
            else
            {
                Debug.LogError("Invalid furniture size.");
                return;
            }
            /*if (_tempSlot != null)
            {
                position.x = (width / 2) - _tempSlot.width / 2;
                position.y = -1 * (_tempSlot.height / 2);
            }
            else
            {*/
            if(!reverse)
                position.x = (width / 2) - transform.parent.GetComponent<FurnitureSlot>().width / 2;
            else
                position.x = ((width / 2) - transform.parent.GetComponent<FurnitureSlot>().width / 2)*-1;
            position.y = -1 * (transform.parent.GetComponent<FurnitureSlot>().height / 2);
            //}

            transform.localPosition = position;
        }

        public void SaveSlot()
        {
            _slot = _tempSlot;
        }

        public void ResetSlot()
        {
            TempSlot = _slot;
        }

        public void SetScale()
        {
            SetScale(_tempSlot.row, _tempSlot.furnitureGrid, _tempSlot);
        }

        public void SetScale(int row, FurnitureGrid grid, FurnitureSlot slot)
        {
            if (Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall)
            {
                transform.localScale /= 1.0f + (slot.maxDepthScale / 100f) * ((_sortingGroup.sortingOrder < 101 ? 1 : (_sortingGroup.sortingOrder - 3) / 100 - 1) / (slot.maxRow - 1f));
                _sortingGroup.sortingOrder = 3 + (row + 1) * 100;
                transform.localScale *= (1.0f + (slot.maxDepthScale / 100f) * (((float)row) / (slot.maxRow - 1f)));
            }
            else if (Furniture.Place is FurniturePlacement.FloorNonblock)
            {
                transform.localScale /= 1.0f + (slot.maxDepthScale / 100f) * ((_sortingGroup.sortingOrder < 12 ? 1 : (_sortingGroup.sortingOrder - 1) / 10 - 2) / (slot.maxRow - 1f));
                _sortingGroup.sortingOrder = 2+ (row + 2) * 10;
                transform.localScale *= (1.0f + (slot.maxDepthScale / 100f) * (((float)row) / (slot.maxRow - 1f)));
            }
            else if (Furniture.Place is FurniturePlacement.Wall)
            {
                transform.localScale = 2.5f*Vector3.one;
                if(grid is FurnitureGrid.BackWall)
                    _sortingGroup.sortingOrder = 10 - (row+1);
                if (grid is FurnitureGrid.RightWall or FurnitureGrid.LeftWall)
                    _sortingGroup.sortingOrder = 1 + (row + 2) * 10;
                transform.localScale *= (1.0f + (slot.maxDepthScale / 100f) * (((float)row) / (slot.maxRow - 1f)));
            }
            else if (Furniture.Place is FurniturePlacement.Ceiling)
            {
                transform.localScale /= 1.0f + (slot.maxDepthScale / 100f) * ((_sortingGroup.sortingOrder < 101 ? 1 : (_sortingGroup.sortingOrder - 4) / 100 - 1) / (slot.maxRow - 1f));
                _sortingGroup.sortingOrder = 4 + (row + 1) * 100;
                transform.localScale *= (1.0f + (slot.maxDepthScale / 100f) * (((float)row) / (slot.maxRow - 1f)));
            }
            //Debug.Log("Scale 2: " +(slot.maxDepthScale / 100f) * (((float)row) / (slot.maxRow - 1f)));
        }

        /// <summary>
        /// Sets the furnitures alpha value to the desired value to produce transparency effect.
        /// </summary>
        /// <param name="alpha"></param>
        public void SetTransparency(float alpha)
        {
            alpha = Mathf.Clamp(alpha, 0.0f, 1.0f);
            Color color = GetComponent<SpriteRenderer>().color;
            color.a = alpha;
            GetComponent<SpriteRenderer>().color = color;
        }

        /// <summary>
        /// Rotates the furniture 90 degrees in clockwise direction.
        /// </summary>
        public void RotateFurniture()
        {
            if(_tempSpriteDirection == Direction.Front)
            {
                if (_furnitureSpriteRight != null || (_spriteCanBeFlipped && _furnitureSpriteLeft != null)) _tempSpriteDirection = Direction.Right;
                else if( _furnitureSpriteBack != null) _tempSpriteDirection = Direction.Back;
                else if( _furnitureSpriteLeft != null || (_spriteCanBeFlipped && _furnitureSpriteRight != null)) _tempSpriteDirection = Direction.Left;
            }
            else if(_tempSpriteDirection == Direction.Right)
            {
                if (_furnitureSpriteBack != null) _tempSpriteDirection = Direction.Back;
                else if (_furnitureSpriteLeft != null || _spriteCanBeFlipped) _tempSpriteDirection = Direction.Left;
                else if (_furnitureSpriteFront != null) _tempSpriteDirection = Direction.Front;
            }
            else if (_tempSpriteDirection == Direction.Back)
            {
                if (_furnitureSpriteLeft != null || (_spriteCanBeFlipped && _furnitureSpriteRight != null)) _tempSpriteDirection = Direction.Left;
                else if (_furnitureSpriteFront != null) _tempSpriteDirection = Direction.Front;
                else if (_furnitureSpriteRight != null || (_spriteCanBeFlipped && _furnitureSpriteLeft != null)) _tempSpriteDirection = Direction.Right;
            }
            else if (_tempSpriteDirection == Direction.Left)
            {
                if (_furnitureSpriteFront != null) _tempSpriteDirection = Direction.Front;
                else if (_furnitureSpriteRight != null || _spriteCanBeFlipped) _tempSpriteDirection = Direction.Right;
                else if (_furnitureSpriteBack != null) _tempSpriteDirection = Direction.Back;
            }

            if (_tempSpriteDirection is Direction.Front or Direction.Back) _isRotated = false;
            else _isRotated = true;

            Furniture.IsRotated = _isRotated;

            SetFurnitureSprite(_tempSpriteDirection);
        }

        /// <summary>
        /// Rotates the furniture to the desired direction provided that the direction is valid.
        /// </summary>
        public void RotateFurniture(Direction direction)
        {
            if (direction == Direction.Front)
            {
                if (_furnitureSpriteFront != null) _tempSpriteDirection = direction;
            }
            else if (direction == Direction.Right)
            {
                if (_furnitureSpriteRight != null || (_spriteCanBeFlipped && _furnitureSpriteLeft != null)) _tempSpriteDirection = direction;
            }
            else if (direction == Direction.Back)
            {
                if (_furnitureSpriteBack != null) _tempSpriteDirection = direction;
            }
            else if (direction == Direction.Left)
            {
                if (_furnitureSpriteLeft != null || (_spriteCanBeFlipped && _furnitureSpriteRight != null)) _tempSpriteDirection = direction;
            }

            if (_tempSpriteDirection is Direction.Front or Direction.Back) _isRotated = false;
            else _isRotated = true;

            Furniture.IsRotated = _isRotated;

            SetFurnitureSprite(_tempSpriteDirection);
        }

            private void SetFurnitureSprite(Direction direction)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            if (direction == Direction.Front) GetComponent<SpriteRenderer>().sprite = _furnitureSpriteFront;
            else if (direction == Direction.Left)
            {
                if (_furnitureSpriteLeft != null)
                {
                    GetComponent<SpriteRenderer>().sprite = _furnitureSpriteLeft;
                }
                else if (_spriteCanBeFlipped)
                {
                    GetComponent<SpriteRenderer>().sprite = _furnitureSpriteRight;
                    GetComponent<SpriteRenderer>().flipX = true;
                }
            }
            else if (direction == Direction.Back) GetComponent<SpriteRenderer>().sprite = _furnitureSpriteBack;
            else if (direction == Direction.Right)
            {
                if (_furnitureSpriteRight != null)
                {
                    GetComponent<SpriteRenderer>().sprite = _furnitureSpriteRight;
                }
                else if (_spriteCanBeFlipped)
                {
                    GetComponent<SpriteRenderer>().sprite = _furnitureSpriteLeft;
                    GetComponent<SpriteRenderer>().flipX = true;
                }
            }
        }

        public void ResetDirection()
        {
            if (_furnitureSpriteFront == null)
            {
                Debug.LogWarning("Front Furniture Sprite not found. Unable to perform Rotation Reset. Double-check if this is intentional.");
                return;
            }
            _tempSpriteDirection = _spriteDirection;
            SetFurnitureSprite(_spriteDirection);
            if (_spriteDirection is Direction.Front or Direction.Back) _isRotated = false;
            else _isRotated = true;
            Furniture.IsRotated = _isRotated;
        }

        public void SaveDirection()
        {
            _spriteDirection = _tempSpriteDirection;
        }

        public void SetOutline(bool outline)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_Outline", outline ? 10f : 0);
            mpb.SetColor("_OutlineColor", Color.red);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }
}
