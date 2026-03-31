using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Prg.Scripts.Common;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using Altzone.Scripts.Model.Poco.Clan;
using System;

public class ClanHeartColorChanger : MonoBehaviour
{
    private Color _selectedColor = Color.white;   // default Color
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;

    [SerializeField] private Transform _heartContainer;
    [SerializeField] private ColorButton[] _colorButtons;

    private List<HeartPieceColorHandler> _heartPieceHandlers = new();
    private List<HeartPieceData> _originalHeartPieces;
    private bool _fillWholeHeart = false;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
        if (_raycaster == null) _raycaster = FindObjectOfType<GraphicRaycaster>();
        if (_eventSystem == null) _eventSystem = FindObjectOfType<EventSystem>();
    }

    public void SetFillWholeHeart(bool enable)
    {
        _fillWholeHeart = enable;
    }

    public void InitializeClanHeart(List<HeartPieceData> heartPieces)
    {
        _heartPieceHandlers.Clear();

        if (heartPieces == null)
        {
            heartPieces = new List<HeartPieceData>();
        }

        if (heartPieces.Count == 0)
        {
            for (int j = 0; j < 50; j++)
            {
                heartPieces.Add(new HeartPieceData(j, Color.white));
            }
        }

        int i = 0;
        foreach (Transform heartPiece in _heartContainer)
        {
            if (heartPiece.TryGetComponent<HeartPieceColorHandler>(out HeartPieceColorHandler heartPieceColorHandler))
            {
                int index = i < heartPieces.Count ? i : heartPieces.Count - 1;
                var data = heartPieces[index];

                heartPieceColorHandler.Initialize(data.pieceNumber, data.pieceColor);
                _heartPieceHandlers.Add(heartPieceColorHandler);
                i++;
            }
        }

        if (_colorButtons != null && _colorButtons.Length > 0)
        {
            _selectedColor = ColorConstants.GetColorConstant(_colorButtons[0].color);
        }
        else
        {
            _selectedColor = Color.white;
        }

        foreach (ColorButton colorButton in _colorButtons)
        {
            Color color = ColorConstants.GetColorConstant(colorButton.color);
            colorButton.button.onClick.RemoveAllListeners();
            colorButton.button.onClick.AddListener(() => SetSelectedColor(color));
        }
    }

    private void FillWholeHeart(Color color)
    {
        foreach (var handler in _heartPieceHandlers)
        {
            handler.SetColor(color);
        }
    }

    private void SetSelectedColor(Color color)
    {
        _selectedColor = color;

        if (_fillWholeHeart)
        {
            FillWholeHeart(_selectedColor);
        }
    }

    public List<HeartPieceData> GetHeartPieceDatas()
    {
        List<HeartPieceData> heartPieces = new();
        foreach (HeartPieceColorHandler handler in _heartPieceHandlers)
        {
            heartPieces.Add(new HeartPieceData(handler.PieceNumber, handler.PieceColor));
        }
        return heartPieces;
    }

    public bool IsAnyPieceChanged()
    {
        foreach (HeartPieceColorHandler colorhandler in _heartPieceHandlers)
        {
            if (colorhandler.IsChanged) return true;
        }
        return false;
    }

    private void Update()
    {
        if (ClickStateHandler.GetClickState() == ClickState.Start)
        {
            Vector2 currentPosition = new();
            if (Touch.activeTouches.Count == 1)
                currentPosition = Touch.activeFingers[0].screenPosition;
            else if (Mouse.current != null)
                currentPosition = Mouse.current.position.ReadValue();

            PointerEventData pointerData = new PointerEventData(_eventSystem)
            {
                position = currentPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            _raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.TryGetComponent<HeartPieceColorHandler>(out HeartPieceColorHandler heartPieceColorHandler))
                {
                    heartPieceColorHandler.SetColor(_selectedColor);
                }
            }
        }
    }

    /*public void InitializeClanHeart(List<HeartPieceData> heartPieces)
    {

        if (heartPieces.Count == 0)
        {
            for (int j = 0; j < 50; j++) heartPieces.Add(new HeartPieceData(j, Color.white));
        }

        int i = 0;
        foreach (Transform heartPiece in _heartContainer)
        {
            if (heartPiece.TryGetComponent<HeartPieceColorHandler>(out HeartPieceColorHandler heartPieceColorHandler))
            {
                heartPieceColorHandler.Initialize(heartPieces[i].pieceNumber, heartPieces[i].pieceColor);
                _heartPieceHandlers.Add(heartPieceColorHandler);
                i++;
            }
        }

        _selectedColor = ColorConstants.GetColorConstant(_colorButtons[0].color);
        foreach (ColorButton colorButton in _colorButtons)
        {
            Color color = ColorConstants.GetColorConstant(colorButton.color);
            colorButton.button.onClick.RemoveAllListeners();
            colorButton.button.onClick.AddListener(() => SetSelectedColor(color));
        }
    }

    private void SetSelectedColor(Color color) => _selectedColor = color;

    public List<HeartPieceData> GetHeartPieceDatas()
    {
        List<HeartPieceData> heartPieces = new();
        foreach (HeartPieceColorHandler handler in _heartPieceHandlers)
        {
            heartPieces.Add(new HeartPieceData(handler.PieceNumber, handler.PieceColor));
        }
        return heartPieces;
    }

    public bool IsAnyPieceChanged()
    {
        foreach (HeartPieceColorHandler colorhandler in _heartPieceHandlers)
        {
            if (colorhandler.IsChanged) return true;
        }
        return false;
    }

    private void Update()
    {
        if (ClickStateHandler.GetClickState() == ClickState.Start)
        {
            Vector2 currentPosition = new();
            if (Touch.activeTouches.Count == 1) currentPosition = Touch.activeFingers[0].screenPosition;
            else if (Mouse.current != null) currentPosition = Mouse.current.position.ReadValue();

            PointerEventData pointerData = new PointerEventData(_eventSystem)
            {
                position = currentPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            _raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.TryGetComponent<HeartPieceColorHandler>(out HeartPieceColorHandler heartPieceColorHandler))
                {
                    heartPieceColorHandler.SetColor(_selectedColor);
                }
            }
        }
    }*/
}

