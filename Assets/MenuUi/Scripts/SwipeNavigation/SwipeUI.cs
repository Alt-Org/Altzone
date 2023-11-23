using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeUI : MonoBehaviour, IBeginDragHandler
{
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

    private bool isEnabled;

    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            isEnabled = value;
            scrollRect.enabled = value;

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

    public IEnumerator SetScrollBarValue(int index)
    {
        yield return new WaitForEndOfFrame();

        CurrentPage = index;

        if (scrollBar)
            scrollBar.value = scrollPageValues[index];
    }

    private void UpdateInput()
    {
        if (isSwipeMode == true) return;
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            startTouchX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!IsEnabled)
            {
                IsEnabled = true;
            }
            else
            {
                endTouchX = Input.mousePosition.x;
                UpdateSwipe();
            }
        }
#endif

#if UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchX = touch.position.x;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (!IsEnabled)
                {
                    IsEnabled = true;
                }
                else
                {
                    endTouchX = touch.position.x;
                }
                UpdateSwipe();
            }
        }
#endif
    }

    private void UpdateSwipe()
    {

        if (Mathf.Abs(startTouchX - endTouchX) < swipeDistance)
        {
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

        //for (int i = 0; i < scrollPageValues.Length; ++i)
        //{
        //    //buttons[i].interactable = true;

        //    if (scrollBar.value < scrollPageValues[i] + (valueDistance / 2) && scrollBar.value > scrollPageValues[i] - (valueDistance / 2))
        //    {
        //        //buttons[i].interactable = false;
        //        buttonImages[i].color = buttons[i].colors.disabledColor;
        //    }
        //    else
        //    {
        //        buttonImages[i].color = buttons[i].colors.normalColor;
        //    }
        //}
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
            IsEnabled = true;
        }

    }
}
