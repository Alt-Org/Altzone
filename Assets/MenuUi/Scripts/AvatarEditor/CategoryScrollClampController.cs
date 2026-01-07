using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class CategoryScrollClampController : MonoBehaviour, IEndDragHandler
    {
        [SerializeField] private ScrollRect _categoryGridScrollRect;
        [SerializeField] private ScrollBarCategoryLoader _categoryLoader;
        public void OnEndDrag(PointerEventData eventData)
        {
            ClampToCenter();
        }

        private void ClampToCenter()
        {
            float totalCellSize = _categoryLoader.cellHeight + _categoryLoader.spacing;
            float contentYPosition = _categoryGridScrollRect.content.anchoredPosition.y;
            int nearestIndex = Mathf.RoundToInt(contentYPosition / totalCellSize);
            float targetYPosition = nearestIndex * totalCellSize;

            StartCoroutine(Clamp(targetYPosition));
        }

        IEnumerator Clamp(float targetYPos)
        {
            Vector2 pos = _categoryGridScrollRect.content.anchoredPosition;
            Vector2 target = new(pos.x, targetYPos);

            float t = 0f;

            while (t < 1)
            {
                t += Time.deltaTime * 5f;
                _categoryGridScrollRect.content.anchoredPosition = Vector2.Lerp(pos, target, t);
                yield return null;
            }

            _categoryGridScrollRect.content.anchoredPosition = target;

            OnClamp(targetYPos);
        }

        private void OnClamp(float targetYPos)
        {
            float totalCellSize = _categoryLoader.cellHeight + _categoryLoader.spacing;
            int topIndex = Mathf.RoundToInt(targetYPos / totalCellSize);
            int centerIndex = topIndex + 1;

            Transform centerCell = _categoryGridScrollRect.content.GetChild(centerIndex);

            centerCell.GetComponent<Button>().onClick.Invoke();
        }
    }
}
