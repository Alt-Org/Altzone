using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour, IDragHandler
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _categoryGridContent;
    [SerializeField] private ScrollBarCategoryLoader _categoryLoader;
    [SerializeField] private RectTransform _categoryGridViewport;

    private int _uniqueCellAmount;
    private float _totalCellSize;
    private float _spacing;
    private float _contentHeight;
    private float _bottomLimit;
    private float _viewPortHeight;

    public void OnDrag(PointerEventData eventData)
    {
        _uniqueCellAmount = _categoryLoader.uniqueCellAmount;
        _totalCellSize = _categoryLoader.cellHeight + _categoryLoader.spacing;
        _spacing = _categoryLoader.spacing;
        _contentHeight = _categoryGridContent.rect.height;
        _viewPortHeight = _categoryGridViewport.rect.height;
        _bottomLimit = _contentHeight - _viewPortHeight - _totalCellSize;

        if (_categoryGridContent.anchoredPosition.y < _totalCellSize)
        {
            //Canvas.ForceUpdateCanvases();
            _categoryGridContent.anchoredPosition = new Vector2(_categoryGridContent.anchoredPosition.x,
                _categoryGridContent.anchoredPosition.y + _totalCellSize * _uniqueCellAmount - _spacing);
            _scrollRect.OnBeginDrag(eventData);
        }

        if (_categoryGridContent.anchoredPosition.y > _bottomLimit)
        {
            //Canvas.ForceUpdateCanvases();
            _categoryGridContent.anchoredPosition = new Vector2(_categoryGridContent.anchoredPosition.x,
                _categoryGridContent.anchoredPosition.y - _totalCellSize * _uniqueCellAmount + _spacing);
            _scrollRect.OnBeginDrag(eventData);
        }
    }
}
