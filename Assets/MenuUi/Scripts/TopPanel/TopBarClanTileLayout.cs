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
    [SerializeField] private List<TopBarToggleDrag> _toggleDrag;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private RectTransform _topBarLayout;
    [SerializeField] private RectTransform _clanTileParent;
    [SerializeField] private RectTransform _inputArrow;
    [SerializeField] private Toggle _viewMore;
    [SerializeField] private GameObject _clanTileLayout;

    [SerializeField] private List<TopBarToggleHandler> _objectToggles;

    [SerializeField] private string SavedViewMore;


    // Start is called before the first frame update
    void Start()
    {
        _viewMore.isOn = PlayerPrefs.GetInt(SavedViewMore, 0) == 1;

        _viewMore.interactable = _toggle.isOn;

        if(_viewMore.isOn)
        {
            _inputArrow.rotation = Quaternion.Euler(0, 0, -90);
        } else
        {
            _inputArrow.rotation = Quaternion.Euler(0, 0, -270);
        }

        _viewMore.onValueChanged.AddListener(ViewSystem);


        _toggle.onValueChanged.AddListener(ChangeParent);
        


    }

    private void ChangeParent(bool isOn)
    {
        //If on: attaches the toggles to clantile toggle group
        //If off: detaches the toggles from clantile toggle group
        if (isOn)
        {
            foreach(var e in _toggleDrag)
            {
                e.enabled = false;
            }
            foreach(RectTransform t in  _clantileChildren)
            {
                t.SetParent(_clanTileLayout.transform);
            }


            _clanTileLayout.SetActive(_viewMore.isOn);
            ViewSystem(_viewMore.isOn);
            _viewMore.interactable = true;


        } else
        {

            //The idea is here is so that anytime the children would detach they would go under the clantile toggle is, instead of going to the bottom of the selection
            int parentposition = _clanTileParent.GetSiblingIndex();
            int i = 1;
            foreach (var e in _toggleDrag)
            {
                e.enabled = true;
            }
            foreach (RectTransform t in _clantileChildren)
            {
                t.SetParent(_topBarLayout);
                t.SetSiblingIndex(parentposition + i);
                i++;
                
            }

            _clanTileLayout.SetActive(false);
            ViewSystem(_viewMore.isOn);
            layoutResize(56.91293f);
            _viewMore.interactable = false;

        }
    }


    private void ViewSystem(bool isOn)
    {
        if (isOn)
        {
            PlayerPrefs.SetInt(SavedViewMore, 1);
            PlayerPrefs.Save();
            _clanTileLayout.SetActive(false);
            layoutResize(56.91293f);
            _inputArrow.rotation = Quaternion.Euler(0, 0, -90);
        }
         else
        {
            PlayerPrefs.SetInt(SavedViewMore, 0);
            PlayerPrefs.Save();
            _clanTileLayout.SetActive(true);
            layoutResize(80 * _clanTileLayout.transform.childCount);
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
        foreach(var i in _toggleDrag)
        {
            i.enabled = true;
        }


        _clantileChildren.Clear();
        _toggleDrag.Clear();
        layoutResize(56.91293f);

        foreach (var i in TopBarOrderBridge.TileObjects)
        {
            foreach(var o in _objectToggles)
                if(o.item == i.Tag)
                {
                    _clantileChildren.Add(o.gameObject.transform);
                    _toggleDrag.Add(o.TogglesDrag);
                }
        }

        //This is only here because the arangement otherwise be ruined
        if(_toggle.isOn)
            ChangeParent(_toggle.isOn);
        else
        {
            foreach (RectTransform t in _clantileChildren)
            {
                t.SetParent(_topBarLayout);
            }
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
