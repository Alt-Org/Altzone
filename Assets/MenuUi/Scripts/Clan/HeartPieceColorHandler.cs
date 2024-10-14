using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartPieceColorHandler : MonoBehaviour
{

    private int _pieceNumber = -1;
    private Color _pieceColor;

    public int PieceNumber { get => _pieceNumber; }
    public Color PieceColor { get => _pieceColor; }

    public void Initialize(int ID)
    {
        if (ID == -1) _pieceNumber = ID;
    }

    public void SetColor(Color color)
    {
        _pieceColor = color;
        GetComponent <Image> ().color = color;

    }

 
}
