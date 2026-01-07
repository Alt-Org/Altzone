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
        private float _clampDuration = 0.2f; // in seconds
        private Coroutine _clampCoroutine;
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

        private void ClampToCenter()
        {
            float totalCellSize = _categoryLoader.cellHeight + _categoryLoader.spacing;
            float contentYPosition = _categoryGridScrollRect.content.anchoredPosition.y;
            int nearestIndex = Mathf.RoundToInt(contentYPosition / totalCellSize);
            int targetIndex = nearestIndex + 1;
            float targetYPosition = nearestIndex * totalCellSize;

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

            OnClamp(targetYPos, index);

            _clampCoroutine = null;
        }

        private void OnClamp(float targetYPos, int index)
        {
            Transform centerCell = _categoryGridScrollRect.content.GetChild(index);

            centerCell.GetComponent<Button>().onClick.Invoke();
        }
    }
}
