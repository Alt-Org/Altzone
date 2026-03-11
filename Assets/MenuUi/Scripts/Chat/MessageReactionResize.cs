using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageReactionResize : MonoBehaviour
{


    [SerializeField] private RectTransform _rectTranformChild;
    [SerializeField] private RectTransform _rectTranformParent;
    [SerializeField] private Image _parentImage;
    [SerializeField] private GridLayoutGroup _gridLayout;

    /// <summary>
    /// This script updates the "AllReactionsScrollView" selections size
    /// </summary>
    void Update()
    {
        
        int i = 0;
        //basicly first it count how many childs the object has and then after that counts which one are active or not
        for (int j = 0; j < _rectTranformChild.childCount; j++)
        {
            if(_rectTranformChild.GetChild(j).gameObject.activeSelf)
            {
                i++;
            }
        }


        ///Updates the width of the object depending how many reactions there still is and the gridlayout's contraintCount
        if (i <= _gridLayout.constraintCount)
        _rectTranformParent.sizeDelta = new Vector2(Mathf.Clamp(i * 85, 85, _gridLayout.constraintCount * 85), 85);

        else _rectTranformParent.sizeDelta = new Vector2(Mathf.Clamp(i * 85, 85, _gridLayout.constraintCount * 85), 170);
        /*//Changes the visual colors if user somehow has put all the reactions in (not in use yet)
         if (i < 0)
             _parentImage.color = Color.gray;
         else
         _parentImage.color = Color.white;*/
    }
}
