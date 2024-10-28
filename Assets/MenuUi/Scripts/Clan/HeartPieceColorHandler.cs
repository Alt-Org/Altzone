using UnityEngine;
using UnityEngine.UI;

public class HeartPieceColorHandler : MonoBehaviour
{
    private Image _image;
    private int _pieceNumber = -1;
    private Color _pieceColor;
    private Color _initialColor;
    private bool _isChanged = false;

    public int PieceNumber { get => _pieceNumber; }
    public Color PieceColor { get => _pieceColor; }
    public bool IsChanged { get => _isChanged; }

    public void Initialize(int ID, Color initialColor)
    {
        ResetChangeFlag();
        _pieceNumber = ID;
        _pieceColor = initialColor;
        _initialColor = initialColor;

        if (_image == null) _image = GetComponent<Image>();
        _image.color = initialColor;
    }

    public void SetColor(Color color)
    {
        _pieceColor = color;
        _image.color = color;

        if (color != _initialColor)
        {
            _isChanged = true;
        }
    }

    public void ResetChangeFlag()
    {
        _isChanged = false;
    }
}
