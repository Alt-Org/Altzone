using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopBarClanTileLayout : MonoBehaviour
{
    [SerializeField] private List<RectTransform> _clantileChildren;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private RectTransform _topBarLayout;
    [SerializeField] private RectTransform _clanTileParent;
    [SerializeField] private RectTransform _inputArrow;
    [SerializeField] private Toggle _viewMore;
    [SerializeField] private GameObject _clanTileLayout;
    private bool _Test;

    // Start is called before the first frame update
    void Start()
    {
        bool b = _toggle.isOn;
        ChangeParent(b);

        _viewMore.onValueChanged.AddListener(ViewSystem);

        _toggle.onValueChanged.AddListener(ChangeParent);


    }

    private void ChangeParent(bool isOn)
    {
        //If on: attaches the toggles to clantile toggle group
        //If off: detaches the toggles from clantile toggle group
        if (isOn)
        {
            foreach(RectTransform t in  _clantileChildren)
            {
                t.SetParent(_clanTileLayout.transform);
            }


            if(!_viewMore.isOn)
            {
            _clanTileLayout.SetActive(true);

            layoutResize(350f);
            }
            _viewMore.interactable = true;
        } else
        {

            //The idea is here is so that anytime the children would detach they would go under the clantile toggle is, instead of going to the bottom of the selection
            int parentposition = _clanTileParent.GetSiblingIndex();
            int i = 1;
            foreach (RectTransform t in _clantileChildren)
            {
                t.SetParent(_topBarLayout);
                t.SetSiblingIndex(parentposition + i);
                i++;
                
            }
            _clanTileLayout.SetActive(false);
            layoutResize(56.91293f);
            _viewMore.interactable = false;

        }
    }


    private void ViewSystem(bool isOn)
    {
        if (!_toggle.isOn)
            return;


        if(isOn)
        {
            _clanTileLayout.SetActive(false);
            layoutResize(56.91293f);
            _inputArrow.rotation = Quaternion.Euler(0, 0, 0);
        }
         else
        {
            _clanTileLayout.SetActive(true);
            layoutResize(350f);
            _inputArrow.rotation = Quaternion.Euler(0, 0, -180);
        }
    }

    //Used to resize the Clantile size toggle
    private void layoutResize(float value)
    {
        LayoutElement layoutElement = _clanTileParent.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = value;
        layoutElement.minHeight = value;
    }



}
