using UnityEngine;
using UnityEngine.EventSystems;

namespace Prg.Scripts.Common.Unity.Input
{
    public class MouseHandler : BaseHandler
    {
        [SerializeField] private int clickCount;
        [SerializeField] private Vector3 curPanPosition;
        [SerializeField] private Vector3 prevPanPosition;
        [SerializeField] private float prevScrollAmount;

        private static bool Approximately(float a, float b) => Mathf.Abs(b - a) < 0.00001f; // 5 digits is more than enough for mouse precision!

        private void Update()
        {
            IsPointerOverGameObject = EventSystem.current.IsPointerOverGameObject(-1);
            if (!IsPointerOverGameObject)
            {
                if (UnityEngine.Input.GetMouseButtonDown(0))
                {
                    // Start mouse down
                    clickCount = 1;
                    curPanPosition = UnityEngine.Input.mousePosition;
                    SendMouseDown(curPanPosition, clickCount);
                    prevPanPosition = curPanPosition;
                }
                else if (UnityEngine.Input.GetMouseButton(0))
                {
                    // Continue mouse down
                    clickCount += 1;
                    curPanPosition = UnityEngine.Input.mousePosition;
                    SendMouseDown(curPanPosition, clickCount);
                    if (!Approximately(curPanPosition.x, prevPanPosition.x) || !Approximately(curPanPosition.y, prevPanPosition.y))
                    {
                        PanCamera((curPanPosition - prevPanPosition) * panSpeed);
                    }
                    prevPanPosition = curPanPosition;
                }
                else if (UnityEngine.Input.GetMouseButtonUp(0))
                {
                    // End mouse down, report last mouse position
                    clickCount = 0;
                    curPanPosition = Vector3.zero;
                    prevPanPosition = Vector3.zero;
                    SendMouseUp(UnityEngine.Input.mousePosition);
                }
            }
            // Zoom can be done even mouse is over UI element!
            var scrollAmount = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (scrollAmount == 0f && prevScrollAmount == 0f)
            {
                return; // Do not report more than one zero scroll at a time
            }
            prevScrollAmount = scrollAmount;
            ZoomCamera(scrollAmount * zoomSpeed);
        }
    }
}