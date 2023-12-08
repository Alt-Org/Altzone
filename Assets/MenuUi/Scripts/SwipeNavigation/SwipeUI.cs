using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeUI : MonoBehaviour, IBeginDragHandler
{
    [Header("Swipe Area")]
    [SerializeField, Tooltip("The area on the sides of the screen from where swiping is disabled (between 0/1)")] private float horizontalDeadzone;
    [SerializeField, Tooltip("The area from the bottom of the screen from where swiping is disabled (between 0/1))")] private float verticalDeadzone;

    private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollBar;
    [SerializeField] private Button[] buttons;
    private Image[] buttonImages;
    [SerializeField] private float swipeTime = 0.2f;
    private float swipeDistance = 50.0f;
    private float[] scrollPageValues;
    private float valueDistance = 0;
    public int currentPage = 0;
    private int maxPage = 0;
    public float startTouchX;
    public float endTouchX;
    private bool isSwipeMode = false;

    public bool isEnabled;
    private Rect swipeRect;

    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            isEnabled = value;
            ToggleScrollRect(value);

            if (!IsEnabled)
            {
                startTouchX = 0;
                endTouchX = 0;
            }
        }
    }

    public int CurrentPage
    {
        get { return currentPage; }
        set
        {
            currentPage = value;
            SettingsCarrier.Instance.mainMenuWindowIndex = currentPage;
            UpdateButtonContent();
        }
    }

    private void Awake()
    {
        scrollPageValues = new float[5];

        valueDistance = 1f / (scrollPageValues.Length - 1f);

        for (int i = 0; i < scrollPageValues.Length; ++i)
        {
            scrollPageValues[i] = valueDistance * i;
        }

        maxPage = 5;
        CurrentPage = SettingsCarrier.Instance.mainMenuWindowIndex;
        scrollRect = GetComponent<ScrollRect>();
        UpdateSwipeAreaValues();
    }

    private void Start()
    {
        buttonImages = new Image[buttons.Length];

        for (int i = 0; i < buttons.Length; ++i)
        {
            Button button = buttons[i];
            int index = i;
            button.onClick.AddListener(() => StartCoroutine(SetScrollBarValue(index)));
            buttonImages[i] = button.GetComponent<Image>();
        }

        IsEnabled = true;
        StartCoroutine(SetScrollBarValue(CurrentPage));
    }

    private void OnEnable()
    {
        startTouchX = 0;
        endTouchX = 0;
        CurrentPage = SettingsCarrier.Instance.mainMenuWindowIndex;
        StartCoroutine(SetScrollBarValue(CurrentPage));
    }
    private void Update()
    {
        UpdateInput();
        //UpdateButtonContent();
    }

    public Rect UpdateSwipeAreaValues()
    {
        float windowWidth = Screen.width * horizontalDeadzone;
        float windowHeight = Screen.height * verticalDeadzone;
        float x = (Screen.width - windowWidth) / 2;
        float y = (Screen.height - windowHeight);
        swipeRect = new Rect(x, y, windowWidth, windowHeight);

        return swipeRect;
    }

    public IEnumerator SetScrollBarValue(int index)
    {
        yield return new WaitForEndOfFrame();

        CurrentPage = index;

        if (scrollBar)
        {
            if (!IsEnabled)
                IsEnabled = true;

            scrollBar.value = scrollPageValues[index];
        }
    }

    private void UpdateInput()
    {
        if (isSwipeMode == true) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!swipeRect.Contains(Input.mousePosition))
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;
            startTouchX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (startTouchX != 0)
            {
                endTouchX = Input.mousePosition.x;
                UpdateSwipe();
            }

            IsEnabled = true;
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && swipeRect.Contains(touch.position))
            {
                if (!swipeRect.Contains(touch.position))
                {
                    IsEnabled = false;
                    return;
                }

                IsEnabled = true;
                startTouchX = touch.position.x;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (startTouchX != 0)
                {
                    endTouchX = touch.position.x;
                    UpdateSwipe();
                }

                IsEnabled = true;
            }
        }
    }

    public void UpdateSwipe()
    {
        if (isSwipeMode)
            return;

        if (Mathf.Abs(startTouchX - endTouchX) < swipeDistance)
        {
            StartCoroutine(OnSwipeOneStep(CurrentPage));
            return;
        }

        bool isLeft = startTouchX < endTouchX ? true : false;

        if (isLeft == true)
        {
            if (CurrentPage == 0) return;
            CurrentPage--;
        }
        else
        {
            if (CurrentPage == maxPage - 1) return;
            CurrentPage++;
        }

        StartCoroutine(OnSwipeOneStep(CurrentPage));
    }

    private IEnumerator OnSwipeOneStep(int index)
    {
        float start = scrollBar.value;
        float current = 0;
        float percent = 0;

        isSwipeMode = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / swipeTime;

            scrollBar.value = Mathf.Lerp(start, scrollPageValues[index], percent);

            yield return null;
        }

        isSwipeMode = false;
        startTouchX = 0;
        endTouchX = 0;
    }

    private void UpdateButtonContent()
    {
        if (buttonImages == null || buttonImages.Length == 0) return;

        for (int i = 0; i < buttonImages.Length; i++)
        {
            if (i == CurrentPage)
            {
                buttonImages[i].color = buttons[i].colors.disabledColor;
            }
            else
            {
                buttonImages[i].color = buttons[i].colors.normalColor;
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

        if (Mathf.Abs(pointerData.delta.y) > Mathf.Abs(pointerData.delta.x))
        {
            IsEnabled = false;
        }
        else
        {
            if (startTouchX != 0)
                IsEnabled = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;

        if (Mathf.Abs(pointerData.delta.y) > Mathf.Abs(pointerData.delta.x))
        {
            IsEnabled = false;
        }
        else
        {
            if (startTouchX != 0)
                IsEnabled = true;
        }

    }
}
