using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class SwipeBlocker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private SwipeBlockType _blockType = SwipeBlockType.All;
        [SerializeField]
        private SwipeUI _swipe;

        public BaseScrollRect parentScrollRect;

        // Start is called before the first frame update
        void Start()
        {
            if (_swipe == null)
                _swipe = FindObjectOfType<SwipeUI>(true);
            CacheParentContainerComponents();
        }

        private void OnEnable()
        {
            CacheParentContainerComponents();
        }

        private T GetComponentOnlyInParents<T>()
        {
            if (transform.parent != null)
                return transform.parent.GetComponentInParent<T>();

            return default(T);
        }

        private void CacheParentContainerComponents()
        {
            if(parentScrollRect == null) parentScrollRect = GetComponentOnlyInParents<BaseScrollRect>();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (_swipe != null)
                _swipe.DragWithBlock(eventData, _blockType);
            parentScrollRect.OnBeginDrag(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            parentScrollRect.OnDrag(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            parentScrollRect.OnEndDrag(eventData);
        }
    }
}
