using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartSaveManager : MonoBehaviour
{
    public Transform heartParent;
    private List<HeartPieceData> savedHeartPieces = new List<HeartPieceData>();

    [System.Serializable]
    public class HeartPieceData
    {
        public int pieceNumber;
        public Color pieceColor;

        public HeartPieceData(int number, Color color)
        {
            pieceNumber = number;
            pieceColor = color;
        }
    }

   
    public void SaveHeartPieces()
    {
        savedHeartPieces.Clear();

        foreach (Transform heartPiece in heartParent)
        {
            HeartPieceColorHandler handler = heartPiece.GetComponent<HeartPieceColorHandler>();
            if (handler != null)
            {
                savedHeartPieces.Add(new HeartPieceData(handler.PieceNumber, handler.PieceColor));
                handler.ResetChangeFlag();  
            }
        }

        Debug.Log("HeartPieces are saved: " + savedHeartPieces.Count);
    }

  
    public bool IsAnyPieceChanged()
    {
        foreach (Transform heartPiece in heartParent)
        {
            HeartPieceColorHandler handler = heartPiece.GetComponent<HeartPieceColorHandler>();
            if (handler != null && handler.IsChanged)
            {
                return true; 
            }
        }
        return false;
    }
}
