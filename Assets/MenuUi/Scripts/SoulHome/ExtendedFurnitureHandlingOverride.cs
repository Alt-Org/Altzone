using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using UnityEngine;

public class ExtendedFurnitureHandlingOverride : FurnitureHandling
{
    [SerializeField] private GameObject _screenFront;
    [SerializeField] private GameObject _screenSide;

    protected override void Start()
    {
        base.Start();
        Debug.Log("OVERRIDE VOID START");
        Debug.Log(_tempSpriteDirection);
        Debug.Log(_screenSide);
        Debug.Log(_screenFront);

    }
    public override void RotateFurniture()
    {
        base.RotateFurniture();

        if (_tempSpriteDirection is Direction.Front)
        {
            Debug.Log("DIRECTION FRONT SCREEN");
            _screenSide.SetActive(false);
            _screenFront.SetActive(true);
        }
        else if (_tempSpriteDirection is Direction.Right || _tempSpriteDirection is Direction.Left )
        {
            Debug.Log("DIRECTION SIDE SCREEN");
            Debug.Log("TEMPSPRITEDIRECTION: " + _tempSpriteDirection);
            _screenSide.SetActive(true);
            _screenFront.SetActive(false);
        }
    }

    public override void RotateFurniture(Direction direction)
    {
        base.RotateFurniture(direction);

        if (_tempSpriteDirection is Direction.Front)
        {
            _screenSide.SetActive(false);
            _screenFront.SetActive(true);
        }
        else if (_tempSpriteDirection is Direction.Right || _tempSpriteDirection is Direction.Left )
        {
            Debug.Log("TEMPSPRITEDIRECTION 2: " + _tempSpriteDirection);
            _screenSide.SetActive(true);
            _screenFront.SetActive(false);
        }
    }
}
