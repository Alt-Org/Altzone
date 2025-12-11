using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGridLoader : MonoBehaviour
{
    [SerializeField] private Transform _ColorGridContent;
    [SerializeField] private GameObject _gridCellPrefab;
    //Took the colors directly from the screenshon on github
    [SerializeField]
    private List<Color> _colors = new List<Color>
    {
        new Color(1, 0, 0),
        new Color(0.996078431372549f, 0.5529411764705883f, 0),
        new Color(1, 1, 0),
        new Color(0, 0.5568627450980392f, 0),
        new Color(0, 0.7529411764705882f, 0.7529411764705882f),
        new Color(0.25098039215686274f, 0, 0.596078431372549f),
        new Color(0.5568627450980392f, 0, 0.5568627450980392f)
    };
}
