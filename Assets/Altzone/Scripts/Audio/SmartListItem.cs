using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class SmartListItem : MonoBehaviour
    {
        protected RectTransform _selfRectTransform;
        public RectTransform SelfRectTransform  { get { return _selfRectTransform; } }

        public bool Visible  { get { return gameObject.activeSelf; } }

        public SmartListItem() { }

        public SmartListItem(RectTransform selfRectTransform) { _selfRectTransform = selfRectTransform; }

        /// <summary>
        /// Checks that the given class T1 is the same type as T2
        /// </summary>
        /// <param name="data1"></param>
        /// <typeparam name="T1">Target that we want to validate.</typeparam>
        /// <typeparam name="T2">The type we want the T1 to be.</typeparam>
        /// <returns>True if same type.</returns>
        protected static bool CheckClassType<T1, T2>(T1 data1, out T2 returnData)
        {
            if (data1 is T2 tempData)
            {
                returnData = tempData;
                return true;
            }

            returnData = default(T2);
            Debug.LogError("Error: data is of type: " + data1.GetType() + ", and wanted data type is: " + typeof(T2));
            return false;
        }

        public bool GetVisibility() { return gameObject.activeSelf; }

        public virtual void SetVisibility(bool visible) { gameObject.SetActive(visible); }

        public virtual void SetData<T1>(T1 data1) { }

        public virtual void ClearData() { }

        public void SetSelfRectTransform() { _selfRectTransform = GetComponent<RectTransform>(); }
    }
}
