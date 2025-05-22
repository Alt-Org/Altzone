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

        private IBeginDragHandler parentBeginDragHandler;
        private IDragHandler parentDragHandler;
        private IEndDragHandler parentEndDragHandler;

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
            parentBeginDragHandler = GetComponentOnlyInParents<IBeginDragHandler>();
            parentDragHandler = GetComponentOnlyInParents<IDragHandler>();
            parentEndDragHandler = GetComponentOnlyInParents<IEndDragHandler>();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (_swipe != null)
                _swipe.OnBeginDrag(eventData, _blockType);
            parentBeginDragHandler?.OnBeginDrag(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            parentDragHandler?.OnDrag(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            parentEndDragHandler?.OnEndDrag(eventData);
        }
    }
}
