using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartPieceColorHandler : MonoBehaviour
{
    private int _pieceNumber = -1;
    private Color _pieceColor;
    private Color _initialColor;  
    private bool _isChanged = false; 

    public int PieceNumber { get => _pieceNumber; }
    public Color PieceColor { get => _pieceColor; }
    public bool IsChanged { get => _isChanged; } 

  
    public void Initialize(int ID, Color initialColor)
    {
        _pieceNumber = ID;
        _pieceColor = initialColor;
        _initialColor = initialColor;  
    }

   
    public void SetColor(Color color)
    {
        _pieceColor = color;
        GetComponent<Image>().color = color;

      
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
