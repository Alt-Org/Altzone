using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using UnityEngine;

public class ExtendedFurnitureHandlingOverride : ExtendedFurnitureHandling
{
    [SerializeField] private GameObject _screenFront;
    [SerializeField] private GameObject _screenSide;

    public override void Start()
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
        else if (_tempSpriteDirection is Direction.Right)
        {
            Debug.Log("DIRECTION SIDE SCREEN");
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
        else if (_tempSpriteDirection is Direction.Right)
        {
            _screenSide.SetActive(true);
            _screenFront.SetActive(false);
        }
    }
}