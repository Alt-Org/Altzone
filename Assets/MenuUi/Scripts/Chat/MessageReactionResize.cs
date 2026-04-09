using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageReactionResize : MonoBehaviour
{


    private RectTransform _rectTranformChild;
    [SerializeField] private RectTransform _rectTranformParent;
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private int _limitX;
    [SerializeField] private int _limitY;

    private void Awake()
    {
        _rectTranformChild = _gridLayout.GetComponent<RectTransform>();
    }
    /// <summary>
    /// This script updates the "AllReactionsScrollView" selections size
    /// </summary>
    ///

    private void OnEnable()
    {
        UpdateSize();
    }


    public void UpdateSize()
    {
        ///side note u can just make the _limitX, Y to 0 if  anchors works normally


        int activeChildren = 0;
        //basicly first it count how many childs the object has and then after that counts which one are active or not
        for (int i = 0; i < _rectTranformChild.childCount; i++)
        {
            if(_rectTranformChild.GetChild(i).gameObject.activeSelf) activeChildren++;
        }

        ///Updates the width of the object depending how many reactions there still is and the gridlayout's contraintCount
        ///
        _rectTranformParent.sizeDelta = new Vector2(Mathf.Clamp(activeChildren * _limitX, _limitX, _gridLayout.constraintCount * _limitX), _limitY);


        /*//Changes the visual colors if user somehow has put all the reactions in (not in use yet)
         if (i < 0)
             _rectTranformParent.GetComponent<Image>().color = Color.gray;
         else
            _rectTranformParent.GetComponent<Image>().color = Color.white;*/
    }
}
