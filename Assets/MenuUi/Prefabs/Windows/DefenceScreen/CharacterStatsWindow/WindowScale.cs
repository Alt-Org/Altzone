using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using MenuUi.Scripts.SwipeNavigation;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using static SettingsCarrier;



public class WindowScale : MonoBehaviour
{

    public float _interval = 2f;
    private SwipeUI _swipe;
    private GameObject[] _layoutElementsGameObjects;
    private RectTransform _scrollRectCanvas;
    private int lastWidth;
    private int lastHeight;
    private SettingsCarrier carrier = SettingsCarrier.Instance;


    private void Awake()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    private void OnEnable()
    {
        _swipe = GetComponentInParent<SwipeUI>();
        StartCoroutine(CheckWindowSize());
    }
    // Start is called before the first frame update
    void Start()
    {
        var windowManager = WindowManager.Get();
        _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("ScaleWindow");
        _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
        SetMainMenuLayoutDimensions();

    }

    // Update is called once per frame
    private void SetMainMenuLayoutDimensions()
    {
        Debug.Log("Setting dimensions");

        LayoutElement[] layoutElements = new LayoutElement[_layoutElementsGameObjects.Length];

        for (int i = 0; i < _layoutElementsGameObjects.Length; i++)
            layoutElements[i] = _layoutElementsGameObjects[i].GetComponent<LayoutElement>();

        float width = _scrollRectCanvas.sizeDelta.x;
        float height = _scrollRectCanvas.sizeDelta.y;

        foreach (LayoutElement element in layoutElements)
        {
            element.preferredWidth = width;
            element.preferredHeight = height;
        }

        _swipe.UpdateSwipeAreaValues();
    }

    private IEnumerator CheckWindowSize() //Tällä saa ikkunan koon.
    {
        while (true)
        {
            if (lastWidth != Screen.width || lastHeight != Screen.height)
            {
                SetMainMenuLayoutDimensions();
                _swipe.UpdateSwipe();
                lastWidth = Screen.width;
                lastHeight = Screen.height;
            }

            yield return new WaitForSeconds(_interval);
        }
    }
}
