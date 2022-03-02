using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Apu
{
    // http://www.badunity.com/scrolling-text-ui/
    public class ScrollerText : MonoBehaviour
    {
        private static readonly Vector3[] _worldCorners = new Vector3[4];

        public float initialDelay;
        public float mouseDelay;
        public float scrollSpeed;
        public bool repeat;
        public GameObject textToScroll;
        public Transform textToScrollTransform;
        public RectTransform textToScrollRectTransform;
        public Rect textRect;
        public Rect screenRect;
        public float textHeight;

        private Vector3 startPosition;

        private void Awake()
        {
            textToScrollTransform = textToScroll.GetComponent<Transform>();
            textToScrollRectTransform = textToScroll.GetComponent<RectTransform>();
            // Get screen position once
            getRect(GetComponent<RectTransform>(), ref screenRect);
            startPosition = textToScrollTransform.position;
        }

        private void OnEnable()
        {
            textToScrollTransform.position = startPosition;
            initialDelay += Time.time;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                initialDelay = Time.time + mouseDelay;
            }
            if (textHeight == 0f)
            {
                // Text measurements must be done when text has been initialized properly by UNITY!
                textHeight = LayoutUtility.GetPreferredHeight(textToScrollRectTransform);
                textToScrollRectTransform.sizeDelta = new Vector2(textToScrollRectTransform.rect.width, textHeight);
                // Create a clone so the text seems to wrap around from end
                var clone = Instantiate(textToScroll.GetComponent<Text>());
                var cloneRect = clone.GetComponent<RectTransform>();
                cloneRect.SetParent(textToScrollRectTransform);
                cloneRect.localScale = Vector3.one;
                // Move it vertically "after" original so it seems to follow the original seamlessly.
                var localPosition = textToScrollTransform.localPosition;
                localPosition.y = -textHeight;
                cloneRect.transform.localPosition = localPosition;
            }
            if (Time.time < initialDelay)
            {
                return;
            }
            // Get scroller position
            getRect(textToScrollRectTransform, ref textRect);
            if (textRect.Overlaps(screenRect))
            {
                textToScrollTransform.Translate(Vector3.up * (scrollSpeed * Time.deltaTime));
                return;
            }
            if (repeat)
            {
                textToScrollTransform.position = startPosition;
                return;
            }
            // scroll just once
            enabled = false;
        }

        private static void getRect(RectTransform rectTransform, ref Rect rect)
        {
            // Grab the corners of the rect transform.
            rectTransform.GetWorldCorners(_worldCorners);

            // Calculate rectangle position and size
            // rect = new Rect(wc[0].x, wc[0].y, wc[2].x - wc[0].x, wc[2].y - wc[0].y);
            rect.x = _worldCorners[0].x;
            rect.y = _worldCorners[0].y;
            rect.width = _worldCorners[2].x - _worldCorners[0].x;
            rect.height = _worldCorners[2].y - _worldCorners[0].y;
        }
    }
}