using System;
using System.Collections;
using Altzone.Scripts.Window;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;


namespace MenuUi.Scripts.SwipeNavigation
{
    public enum SwipeBlockType2
    {
        All,
        Vertical,
        Horizontal,
        None
    }

    /// <summary>
    /// SwipeUI handles swiping that snaps to windows between different main menu windows.
    /// </summary>
    /// <remarks>
    /// Modified version of: https://www.youtube.com/watch?v=zeHdty9RUaA
    /// </remarks>
    public class SwipeUIStatPanel : MonoBehaviour, IBeginDragHandler
    {
        [Header("Swipe Area")]
        [SerializeField, Tooltip("The area on the sides of the screen from where swiping is disabled (between 0/1)")] private float horizontalDeadzone;
        [SerializeField, Tooltip("The area from the bottom of the screen from where swiping is disabled (between 0/1))")] private float verticalDeadzone;

        private BaseScrollRect scrollRect;
        [SerializeField] protected GameObject[] slides;
        [SerializeField] private Scrollbar scrollBar;
        [SerializeField] private Button[] buttons;
        //[SerializeField] private Button battleButton;
        private Image[] buttonImages;
        [SerializeField] private float swipeTime = 0.2f;
        private float swipeDistance = 50.0f;
        private float[] scrollPageValues;
        private float valueDistance = 0;
        public int currentPage = 1;
        protected int maxPage = 0;
        public Vector2 _startTouch;
        public Vector2 _endTouch;
        protected bool isSwipeMode = false;
        private float _startScrollvalue;
        private bool _swipeAllowed = false;
        [SerializeField] private RectTransform _scrollTransform;
        private bool _firstFrame = true;

        private bool settingScroll = false;
        public bool isEnabled;
        private Rect swipeRect;

        [SerializeField] private bool _isInMainMenu;
        [SerializeField] protected bool _willRotate;

        public Action OnCurrentPageChanged;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                if (scrollRect) ToggleScrollRect(value);

                if (!IsEnabled)
                {
                    _startTouch = Vector2.zero;
                    _endTouch = Vector2.zero;
                }
            }
        }

        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (isSwipeMode && gameObject.activeInHierarchy) return;
                if (currentPage != value)
                {
                    currentPage = value;
                    if (_isInMainMenu) SettingsCarrier.Instance.mainMenuWindowIndex = currentPage;
                    UpdateButtonContent();
                    if (_firstFrame) StartCoroutine(SetScrollBarValue(CurrentPage, true));
                    else StartCoroutine(OnSwipeOneStep(CurrentPage));
                    OnCurrentPageChanged?.Invoke();
                }
            }
        }

        public bool IsInMainMenu { get => _isInMainMenu; }

        public float ScrollbarValue { get => scrollBar.value; }

        protected virtual void Awake()
        {
            scrollPageValues = new float[slides.Length];

            valueDistance = 1f / (scrollPageValues.Length - 1f);

            for (int i = 0; i < scrollPageValues.Length; ++i)
            {
                scrollPageValues[i] = 1-(valueDistance * i);
            }

            maxPage = slides.Length - 1;

            if (_isInMainMenu)
            {
                //CurrentPage = SettingsCarrier.Instance.mainMenuWindowIndex;
                currentPage = 2;
            }
            else
            {
                //currentPage = DataCarrier.GetData<int>(DataCarrier.RequestedWindow, false);
            }

            scrollRect = GetComponent<BaseScrollRect>();
            UpdateSwipeAreaValues();
            StartCoroutine(SetScrollBarValue(currentPage, true));
        }

        private void Start()
        {
            if (buttons != null && buttons.Length != 0)
            {
                buttonImages = new Image[buttons.Length];

                for (int i = 0; i < buttons.Length; ++i)
                {
                    Button button = buttons[i];
                    int index = i;
                    button.onClick.AddListener(() => StartCoroutine(SetScrollBarValue(index, false)));
                    buttonImages[i] = button.GetComponent<Image>();
                }
            }

            IsEnabled = true;
            //StartCoroutine(SetScrollBarValue(CurrentPage, true));
            EnhancedTouchSupport.Enable();
        }

        private void OnEnable()
        {
            _startTouch = Vector2.zero;
            _endTouch = Vector2.zero;

            if (_isInMainMenu)
            {
                CurrentPage = SettingsCarrier.Instance.mainMenuWindowIndex;
            }
            else
            {
                CurrentPage = DataCarrier.GetData<int>(DataCarrier.RequestedWindow, false, suppressWarning: true);
            }
            settingScroll = false;
            StartCoroutine(SetScrollBarValue(CurrentPage, true));
            _firstFrame = true;
            isSwipeMode = true;
        }
        private void Update()
        {
            UpdateInput();
            //UpdateButtonContent();
        }
        private void LateUpdate()
        {
            if (_firstFrame)
            {
                _scrollTransform.localPosition = new(_scrollTransform.localPosition.x , - 1 * (_scrollTransform.rect.height * scrollPageValues[CurrentPage] * (1 - 1f / scrollPageValues.Length)), 0);
                _firstFrame = false;
                isSwipeMode = false;
                DataCarrier.GetData<int>(DataCarrier.RequestedWindow, true, suppressWarning: true);
            }
            if (!isSwipeMode && !_swipeAllowed) scrollBar.value = scrollPageValues[CurrentPage];

        }

        /// <summary>
        /// Updates the area where swiping is allowed based on screen width and height.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Swiping has been disabled on bottom buttons and the very edges of the screen
        /// to better work with Android's gesture navigation
        /// </remarks>
        public Rect UpdateSwipeAreaValues()
        {
            float windowWidth = Screen.width * horizontalDeadzone;
            float windowHeight = Screen.height * verticalDeadzone;
            float x = (Screen.width - windowWidth) / 2;
            float y = (Screen.height - windowHeight);
            swipeRect = new Rect(x, y, windowWidth, windowHeight);

            return swipeRect;
        }

        /// <summary>
        /// Sets the scroll view scrollbar value and scroll to the correct point.
        /// </summary>
        /// <param name="index">Index of the window to scroll to.</param>
        /// <returns></returns>
        public IEnumerator SetScrollBarValue(int index, bool instant)
        {
            if (settingScroll) yield break;
            settingScroll = true;
            yield return new WaitForEndOfFrame();
            if (scrollBar)
            {
                if (!IsEnabled)
                    IsEnabled = true;

                if (!instant) StartCoroutine(OnSwipeOneStep(index));
                else
                {
                    scrollBar.value = scrollPageValues[index];
                    settingScroll = false;
                }
            }
            else settingScroll = false;
            CurrentPage = index;
        }

        private void UpdateInput()
        {
            // Return if currently swiping
            if (isSwipeMode == true) return;

            //Checks mouse input first and then touch input
            //Since WebGL can be run on PC or mobile we need to check both
            if (ClickStateHandler.GetClickState() is ClickState.Start)
            {
                // Return if input is outside of scroll area
                if (Touch.activeTouches.Count == 1 && !swipeRect.Contains(Touch.activeFingers[0].screenPosition)
                    || (Application.platform is RuntimePlatform.WebGLPlayer && Mouse.current != null && !swipeRect.Contains(Mouse.current.position.ReadValue())))
                {
                    IsEnabled = false;
                    return;
                }

                IsEnabled = true;
                if (Touch.activeTouches.Count == 1) _startTouch = Touch.activeFingers[0].screenPosition;
                else if (Mouse.current != null) _startTouch = Mouse.current.position.ReadValue();
                _startScrollvalue = scrollBar.value;
                _swipeAllowed = false;

            }
            else if (ClickStateHandler.GetClickState() is ClickState.Move)
            {
                if (_startTouch == Vector2.zero) return;

                Vector2 currentTouch;
                if (Touch.activeTouches.Count >= 1) currentTouch = Touch.activeFingers[0].screenPosition;
                else currentTouch = Mouse.current.position.ReadValue();

                float minSwipeAllowed = Screen.height * (2f / 100f);
               
                //Debug.Log(Time.time + ", min: " + minSwipeAllowed + ", current: " + (_startTouch.x - currentTouch.x));
                if (Mathf.Abs(_startTouch.y - currentTouch.y) > minSwipeAllowed)
                {
                    _swipeAllowed = true;
                }
                if (_swipeAllowed && IsEnabled)
                {
                    float totalSlideHeight = 0;
                    foreach (var slide in slides)
                    {
                        totalSlideHeight += slide.GetComponent<RectTransform>().rect.height;
                    }
                    float currentSwipeDistance = _startTouch.y - currentTouch.y;
                    float currentScrollvalue = Mathf.Clamp(_startScrollvalue + currentSwipeDistance / totalSlideHeight, 0, 1);
                    scrollBar.value = currentScrollvalue;
                }

                if (Mathf.Abs(_startTouch.y - currentTouch.y) > swipeDistance && !_swipeAllowed)
                {
                    scrollRect.StopMovement();
                    IsEnabled = false;
                    StartCoroutine(OnSwipeOneStep(CurrentPage));
                }
            }
            else if (ClickStateHandler.GetClickState() is ClickState.End)
            {
                // Update swipe when mouse is released
                if (_startTouch != Vector2.zero)
                {
                    if (Touch.activeTouches.Count == 1) _endTouch = Touch.activeFingers[0].screenPosition;
                    else if (Mouse.current != null) _endTouch = Mouse.current.position.ReadValue();
                    UpdateSwipe();
                }

                IsEnabled = true;
                _swipeAllowed = false;
            }
        }

        /// <summary>
        /// Checks swipe direction and length and starts swiping to next window.
        /// </summary>
        public void UpdateSwipe()
        {
            if (isSwipeMode || !IsEnabled)
                return;
           // Debug.Log("Value: " + Mathf.Abs(_startScrollvalue - scrollBar.value) + ", Marginal:  " + ((1f / scrollPageValues.Length) * (20f / 100f)));
            //Checks that the swipe was long enough
            if ((Mathf.Abs(_startTouch.y - _endTouch.y) < swipeDistance || Mathf.Abs(_startTouch.x - _endTouch.x) > swipeDistance) && Mathf.Abs(_startScrollvalue - scrollBar.value) < ((1f / scrollPageValues.Length) * (20f / 100f)))
            {
                // Swipe back to the previous window
                StartCoroutine(OnSwipeOneStep(CurrentPage));
                return;
            }

            bool isDown = _startTouch.y < _endTouch.y ? true : false;

            if (isDown == true)
            {
                NextSlide();
            }
            else
            {
                PreviousSlide();
            }

        }

        public virtual void NextSlide()
        {
            if (CurrentPage == maxPage)
            {
                if (_willRotate)
                {
                    CurrentPage = 0; // Goes to the first slide when swiping right on the last slide
                }
            }
            else
            {
                CurrentPage++;
            }
        }

        public virtual void PreviousSlide()
        {
            if (CurrentPage == 0)
            {
                if (_willRotate)
                {
                    CurrentPage = maxPage; // Goes to the last slide when swiping left on the first slide
                }
            }
            else
            {
                CurrentPage--;
            }
        }

        /// <summary>
        /// Snaps to next/pervious window.
        /// </summary>
        /// <param name="index">Index of the page we are snapping to.</param>
        /// <returns></returns>
        protected IEnumerator OnSwipeOneStep(int index)
        {
            float start = scrollBar.value;
            float current = 0;
            float percent = 0;
            isSwipeMode = true;
            if (scrollRect)
            {
                if (scrollRect.enabled)
                    while (percent < 1)
                    {
                        current += Time.deltaTime;
                        percent = current / swipeTime;

                        scrollBar.value = Mathf.Lerp(start, scrollPageValues[index], percent);

                        yield return null;
                    }
                else
                    while (percent < 1)
                    {
                        current += Time.deltaTime;
                        percent = current / swipeTime;
                        scrollRect.enabled = true;
                        scrollBar.value = Mathf.Lerp(start, scrollPageValues[index], percent);
                        scrollRect.enabled = false;

                        yield return null;
                    }
            }
            isSwipeMode = false;
            _startTouch = Vector2.zero;
            _endTouch = Vector2.zero;
            IsEnabled = true;
            settingScroll = false;
        }

        /// <summary>
        /// Changes the color of currently active main menu widow's button.
        /// </summary>
        private void UpdateButtonContent()
        {
            if (buttonImages == null || buttonImages.Length == 0 || !_isInMainMenu) return;

            for (int i = 0; i < buttonImages.Length; i++)
            {
                if (i == CurrentPage)
                {
                    buttons[i].transform.localScale = Vector3.one * 1.2f;
                }
                else
                {
                    buttons[i].transform.localScale = Vector3.one;
                }
            }
        }

        public void ToggleScrollRect(bool value)
        {
            scrollRect.enabled = value;
        }

        public void OnBeginDrag(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;

            if (Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y))
            {
                IsEnabled = false;
            }
            else
            {
                if (_startTouch.y != 0)
                    IsEnabled = true;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;

            if (Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y))
            {
                IsEnabled = false;
            }
            else
            {
                if (_startTouch.y != 0)
                    IsEnabled = true;
            }

        }

        public void OnBeginDrag(PointerEventData eventData, SwipeBlockType2 blockType = SwipeBlockType2.None)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (blockType == SwipeBlockType2.All)
            {
                IsEnabled = false;
            }
            else if (blockType is SwipeBlockType2.Horizontal && Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y))
            {
                IsEnabled = false;
            }
            else
            {
                if (_startTouch.y != 0)
                    IsEnabled = true;
            }

        }
    }
}
