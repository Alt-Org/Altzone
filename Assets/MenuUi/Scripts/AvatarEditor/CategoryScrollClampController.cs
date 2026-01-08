using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class CategoryScrollClampController : MonoBehaviour, IEndDragHandler, IBeginDragHandler
    {
        [SerializeField] private ScrollRect _categoryGridScrollRect;
        [SerializeField] private ScrollBarCategoryLoader _categoryLoader;
        [SerializeField] private Button _topButton;
        [SerializeField] private Button _bottomButton;
        [SerializeField] private InfiniteScroll _infiniteScroll;
        private float _clampDuration = 0.2f; // in seconds
        private Coroutine _clampCoroutine;
        private float _totalCellSize;
        private float _contentYPosition;
        private int _topIndex;

        private void Start()
        {
            _topButton.onClick.AddListener(() =>
            {
                ClampToTopIndex();
            });

            _bottomButton.onClick.AddListener(() =>
            {
                ClampToBottomIndex();
            });
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            ClampToCenter();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_clampCoroutine != null)
            {
                StopCoroutine(_clampCoroutine);
            }
        }

        private void Calculate()
        {
            _totalCellSize = _categoryLoader.cellHeight + _categoryLoader.spacing;
            _contentYPosition = _categoryGridScrollRect.content.anchoredPosition.y;
            _topIndex = Mathf.RoundToInt(_contentYPosition / _totalCellSize);
        }

        private void ClampToBottomIndex()
        {
            Calculate();
            int targetIndex = _topIndex + 2;
            float targetYPosition = _topIndex * _totalCellSize + _totalCellSize;

            _clampCoroutine = StartCoroutine(Clamp(targetYPosition, targetIndex));
        }
        public void ClampToTopIndex()
        {
            Calculate();
            int targetIndex = _topIndex;
            float targetYPosition = _topIndex * _totalCellSize - _totalCellSize;

            _clampCoroutine = StartCoroutine(Clamp(targetYPosition, targetIndex));
        }

        private void ClampToCenter()
        {
            Calculate();
            int targetIndex = _topIndex + 1;
            float targetYPosition = _topIndex * _totalCellSize;

            _clampCoroutine = StartCoroutine(Clamp(targetYPosition, targetIndex));
        }

        IEnumerator Clamp(float targetYPos, int index)
        {
            Vector2 pos = _categoryGridScrollRect.content.anchoredPosition;
            Vector2 target = new(pos.x, targetYPos);

            float elapsedTime = 0f;

            // clamp "animation" plays for _clampDuration seconds
            while (elapsedTime < _clampDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / _clampDuration);
                _categoryGridScrollRect.content.anchoredPosition = Vector2.Lerp(pos, target, t);
                yield return null;
            }

            _categoryGridScrollRect.content.anchoredPosition = target;

            OnClamp(index);

            _clampCoroutine = null;
        }

        private void OnClamp(int index)
        {
            Transform centerCell = _categoryGridScrollRect.content.GetChild(index);

            centerCell.GetComponent<Button>().onClick.Invoke();
        }
    }
}
