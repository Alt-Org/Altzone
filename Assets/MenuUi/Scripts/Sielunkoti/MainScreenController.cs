using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MenuUI.Scripts.SoulHome
{
    public enum ClickState
    {
        Start,
        Hold,
        Move,
        End
    }


    public class MainScreenController : MonoBehaviour
    {
        [SerializeField]
        private TowerController _soulHomeTower;
        [SerializeField]
        private Camera _camera;

        private bool _rotated = false;
        Vector3 startPosition;
        float startTime;

        Vector3 _currentPosition;
        private bool _moving;

        float _prevTapTime = 0;
        float backDelay = 0;
        float inDelay = 0;

        private bool _trayOpen = false;
        private GameObject _selectedFurnitureTray = null;

        public bool TrayOpen { get => _trayOpen; set => _trayOpen = value; }
        public GameObject SelectedFurnitureTray { get => _selectedFurnitureTray;}

        // Start is called before the first frame update
        void Start()
        {
            if (AppPlatform.IsMobile || AppPlatform.IsSimulator)
            {
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            EnhancedTouchSupport.Enable();
            EnableTray(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (GetComponent<RectTransform>().rect.width != GetComponent<BoxCollider2D>().size.x || GetComponent<RectTransform>().rect.height != GetComponent<BoxCollider2D>().size.y)
            //if ((Screen.orientation == ScreenOrientation.LandscapeLeft && !rotated) || (Screen.orientation == ScreenOrientation.Portrait && rotated))
            {
                _rotated = !_rotated;
                StartCoroutine(SetColliderSize());
            }

            if (AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButton(0))
            {
                if (!Mouse.current.position.ReadValue().Equals(_currentPosition))
                {
                    _moving = true;
                }
                else _moving = false;
            }

            Touch touch =new();
            if (Touch.activeFingers.Count > 0) touch = Touch.activeTouches[0];


            if ((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButtonDown(0)) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Began))
            {
                if(Touch.activeFingers.Count >= 1) startPosition = touch.finger.screenPosition;
                else startPosition = Mouse.current.position.ReadValue();
                startTime = Time.time;
                //Debug.Log(startPosition);
                //Debug.Log(startTime);
                RayPoint(ClickState.Start);
            }
            else if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButtonUp(0)) || (Touch.activeFingers.Count > 0 && (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended|| touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled))))
            {
                Vector2 endPosition;
                if (Touch.activeFingers.Count >= 1) endPosition = touch.finger.screenPosition;
                else endPosition = Mouse.current.position.ReadValue();
                float endTime = Time.time;

                //Debug.Log(endPosition);
                //Debug.Log(endTime);

                //if (endTime - startTime > 0.2f || Mathf.Abs(startPosition.x-endPosition.x)+Mathf.Abs(startPosition.y - endPosition.y) > 1) return;

                RayPoint(ClickState.End);
                startPosition = Vector3.zero;
            }
            else if ((((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && _moving && Input.GetMouseButton(0))) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)) )
            {
                if (Touch.activeFingers.Count >= 1) _currentPosition = touch.finger.screenPosition;
                else _currentPosition = Mouse.current.position.ReadValue();

                RayPoint(ClickState.Move);
            }
            else if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && !_moving && Input.GetMouseButton(0))) || (Touch.activeFingers.Count > 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary) )
            {
                if (Touch.activeFingers.Count >= 1) _currentPosition = touch.finger.screenPosition;
                else _currentPosition = Mouse.current.position.ReadValue();

                RayPoint(ClickState.Hold);
            }
            bool doubleTap = false;
            if((Touch.activeFingers.Count > 0 && touch.phase is UnityEngine.InputSystem.TouchPhase.Ended))
            {
                if(Time.time < _prevTapTime+0.4f) doubleTap = true;
                _prevTapTime = Time.time;
            }
            if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButtonUp(1)) || doubleTap /*(Touch.activeFingers.Count > 0 && touch.tapCount > 1)*/) && backDelay + 0.4f < Time.time)
            {
                _soulHomeTower.ZoomOut();
                //inDelay = Time.time;
            }

        }
        private void OnEnable()
        {
            if (AppPlatform.IsMobile || AppPlatform.IsSimulator)
            {
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            EnableTray(false);
        }

        private void OnDisable()
        {
            if (AppPlatform.IsMobile || AppPlatform.IsSimulator) Screen.orientation = ScreenOrientation.Portrait;
            _soulHomeTower.ResetChanges();
        }

        private void RayPoint(ClickState click)
        {
            Debug.Log(click);
            Debug.Log(Screen.orientation);
            Touch touch = new();
            Ray ray;
            if (Touch.activeFingers.Count >= 1)
            {
                touch = Touch.activeTouches[0];
                ray = _camera.ScreenPointToRay(touch.screenPosition);
            }
            else ray = _camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit2D[] hit;
            hit = Physics2D.GetRayIntersectionAll(ray, 1000);
            foreach (RaycastHit2D hit2 in hit)
            {
                if (hit2.collider.gameObject.CompareTag("SoulHomeScreen"))
                {
                    if (_selectedFurnitureTray != null)
                    {
                        if(_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = false;
                        if(_soulHomeTower.SelectedFurniture == null)
                        {
                            _soulHomeTower.SetFurniture(_selectedFurnitureTray);
                        }
                    }
                    if (_soulHomeTower.SelectedFurniture != null)
                    {
                        if (!_soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                        {
                            _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                            _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                        }
                    }

                    //Debug.Log(hit.collider.gameObject.name);
                    Vector3 hitPoint = hit2.transform.InverseTransformPoint(hit2.point);
                    //Debug.Log(hitPoint);
                    float x = hit2.transform.GetComponent<RectTransform>().rect.width;
                    float y = hit2.transform.GetComponent<RectTransform>().rect.height;
                    Vector2 relPos = new((x / 2 + hitPoint.x) / x, (y / 2 + hitPoint.y) / y);
                    //Debug.Log(relPos);
                    bool check = _soulHomeTower.FindRayPoint(relPos, click);
                    //if(check)backDelay = Time.time;
                }
                if (hit2.collider.gameObject.CompareTag("FurnitureTray"))
                {
                    if (_selectedFurnitureTray == null)
                    {
                        if (_soulHomeTower.SelectedFurniture != null)
                        {
                            SetFurniture(_soulHomeTower.SelectedFurniture);
                            _selectedFurnitureTray.transform.position = hit2.point;
                        }
                    }
                    else
                    {
                        if (!_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = true;
                        _selectedFurnitureTray.transform.position = hit2.point;

                    }
                    if (_soulHomeTower.SelectedFurniture != null)
                    {
                        if (_soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                        {
                            _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = false;
                            _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = false;
                        }

                        if (click is ClickState.End)
                        {
                            _soulHomeTower.RemoveFurniture();
                        }
                    }
                }
                if (hit2.collider.gameObject.CompareTag("FurnitureTrayItem"))
                {
                    if (_selectedFurnitureTray == null && click is ClickState.Start)
                    {
                        _selectedFurnitureTray = hit2.collider.transform.GetChild(1).gameObject;
                        transform.GetChild(0).Find("Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                        transform.GetChild(0).Find("Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
                    }
                }
            }
            if (click is ClickState.End)
            {
                if (_selectedFurnitureTray != null)
                {
                    if (!_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))
                    {
                        //Destroy(_selectedFurnitureTray); //This is temporaty setup until a create the handling to up the furniture into the tray.
                        AddTrayItem(_selectedFurnitureTray.GetComponent<TrayFurniture>().Furniture);
                    }
                    else
                    {
                        if (!_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = true;
                        _selectedFurnitureTray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        _selectedFurnitureTray = null;
                    }
                    transform.GetChild(0).Find("Scroll View").gameObject.GetComponent<ScrollRect>().enabled = true;
                }

                if (_soulHomeTower.SelectedFurniture != null)
                {
                    if (!_soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                    {
                        _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                        _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                    }
                    _soulHomeTower.DeselectFurniture();
                }
            }
        }

        public void ResetChanges()
        {
            _soulHomeTower.ResetChanges();
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().ResetChanges();
        }

        public void SaveChanges()
        {
            _soulHomeTower.SaveChanges();
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().SaveChanges();
        }

        public void ToggleTray()
        {
            float width = GetComponent<BoxCollider2D>().size.x;
            GameObject tray = transform.Find("Itemtray").gameObject;
            if (!_trayOpen)
            {
                tray.transform.localPosition = new Vector2(tray.transform.localPosition.x -width+100, tray.transform.localPosition.y);
                _trayOpen = true;
                transform.Find("DiscardChangesButton").gameObject.SetActive(true);
                transform.Find("SaveChangesButton").gameObject.SetActive(true);
                if (!_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
            }
            else
            {
                tray.transform.localPosition = new Vector2(tray.transform.localPosition.x + width - 100, tray.transform.localPosition.y);
                _trayOpen = false;
                transform.Find("DiscardChangesButton").gameObject.SetActive(false);
                transform.Find("SaveChangesButton").gameObject.SetActive(false);
                if (_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
            }
        }
        public void EnableTray(bool enable)
        {
            if (enable)
            {
                GameObject tray = transform.Find("Itemtray").gameObject;
                tray.SetActive(true);
            }
            else
            {
                GameObject tray = transform.Find("Itemtray").gameObject;
                tray.SetActive(false);
            }
        }

        public void SetFurniture(GameObject trayFurniture)
        {
            var furnitureObject = Instantiate(trayFurniture.GetComponent<FurnitureHandling>().TrayFurnitureObject, transform.Find("Itemtray"));
            furnitureObject.GetComponent<TrayFurniture>().Furniture = trayFurniture.GetComponent<FurnitureHandling>().Furniture;
            _selectedFurnitureTray = furnitureObject;
        }

        public void AddTrayItem(Furniture furniture)
        {
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().AddFurniture(furniture);
            if (_selectedFurnitureTray != null)
            {
                Destroy(_selectedFurnitureTray);
                _selectedFurnitureTray = null;
            }
        }

        public void RemoveTrayItem(GameObject trayFurniture)
        {
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().RemoveFurniture(trayFurniture);
            _selectedFurnitureTray = null;
        }

        private IEnumerator SetColliderSize()
        {
            yield return new WaitForEndOfFrame();
            RectTransform rect = GetComponent<RectTransform>();
            float x = rect.rect.width;
            float y = rect.rect.height;
            GetComponent<BoxCollider2D>().size = new(x, y);
        }

    }
}
