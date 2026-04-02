using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageReactionResize : MonoBehaviour
{


    private RectTransform _rectTranformChild;
    [SerializeField] private RectTransform _rectTranformParent;
    [SerializeField] private GridLayoutGroup _gridLayout;

    private void Awake()
    {
        _rectTranformChild = _gridLayout.GetComponent<RectTransform>();
    }
    /// <summary>
    /// This script updates the "AllReactionsScrollView" selections size
    /// </summary>
    public void UpdateSize()
    { 
        int activeChildren = 0;
        //basicly first it count how many childs the object has and then after that counts which one are active or not
        for (int i = 0; i < _rectTranformChild.childCount; i++)
        {
            if(_rectTranformChild.GetChild(i).gameObject.activeSelf) activeChildren++;
        }

        ///Updates the width of the object depending how many reactions there still is and the gridlayout's contraintCount
        if (activeChildren <= _gridLayout.constraintCount)
        _rectTranformParent.sizeDelta = new Vector2(Mathf.Clamp(activeChildren * 100, 100, _gridLayout.constraintCount * 100), 100);
        else _rectTranformParent.sizeDelta = new Vector2(Mathf.Clamp(activeChildren * 100, 100, _gridLayout.constraintCount * 100), 200);

        /*//Changes the visual colors if user somehow has put all the reactions in (not in use yet)
         if (i < 0)
             _rectTranformParent.GetComponent<Image>().color = Color.gray;
         else
            _rectTranformParent.GetComponent<Image>().color = Color.white;*/
    }
}
