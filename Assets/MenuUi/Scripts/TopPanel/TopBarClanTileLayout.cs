using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Settings;
using UnityEngine;
using UnityEngine.UI;
using static MenuUI.Scripts.TopPanel.TopBarTargets;

public class TopBarClanTileLayout : MonoBehaviour
{
    [SerializeField] private TopBarDefs.TopBarItem _togglesClanTile;
    [SerializeField] private List<Transform> _clantileChildren;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private RectTransform _topBarLayout;
    [SerializeField] private RectTransform _clanTileParent;
    [SerializeField] private RectTransform _inputArrow;
    [SerializeField] private Toggle _viewMore;
    [SerializeField] private GameObject _clanTileLayout;

    [SerializeField] private List<TopBarToggleHandler> _objectToggles;

    // Start is called before the first frame update
    void Start()
    {
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

            layoutResize(60 * _clanTileLayout.transform.childCount);
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
            _inputArrow.rotation = Quaternion.Euler(0, 0, -90);
        }
         else
        {
            _clanTileLayout.SetActive(true);
            layoutResize(65 * _clanTileLayout.transform.childCount);
            _inputArrow.rotation = Quaternion.Euler(0, 0, -270);
        }
    }

    public IEnumerator IsThereATile(TopBarOrderBridge TopBarOrderBridge)
    {
        yield return new WaitForEndOfFrame();

        foreach (var i in TopBarOrderBridge.TargetsByStyle)
        {
            if (!i.gameObject.activeSelf)
                continue;

            foreach (var e in i.PTileManagement)
            {
                if (e.Tile == _togglesClanTile)
                {
                    _clanTileParent.gameObject.SetActive(true);
                    Rearrange(e);
                    break;
                }
                else
                {
                    _clanTileParent.gameObject.SetActive(false);
                }
            }
        }
    }

    public void Rearrange(TileManagement TopBarOrderBridge)
    {

        for (int i = 0; i < _clantileChildren.Count; i++)
        {
            _clantileChildren[i].transform.SetParent(_topBarLayout.transform);
        }

        _clantileChildren.Clear();
        layoutResize(56.91293f);

        foreach (var i in TopBarOrderBridge.TileObjects)
        {
            foreach(var o in _objectToggles)
                if(o.item == i.Tag)
                {
                    _clantileChildren.Add(o.gameObject.transform);
                }
        }

        //This is only here because the arangement otherwise be ruined
        if(_toggle.isOn)
            ChangeParent(_toggle.isOn);
    }

    //Used to resize the Clantile size toggle
    private void layoutResize(float value)
    {
        LayoutElement layoutElement = _clanTileParent.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = value;
        layoutElement.minHeight = value;
    }



}
