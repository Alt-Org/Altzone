using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class SwipeBlocker : MonoBehaviour, IBeginDragHandler
    {
        [SerializeField]
        private SwipeBlockType _blockType = SwipeBlockType.All;
        [SerializeField]
        private SwipeUI _swipe;

        // Start is called before the first frame update
        void Start()
        {
            if (_swipe == null)
                _swipe = FindObjectOfType<SwipeUI>(true);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_swipe != null)
                _swipe.DragWithBlock(eventData, _blockType);
        }
    }
}
