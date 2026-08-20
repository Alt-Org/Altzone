using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Settings;
using MenuUI.Scripts.TopPanel;
using UnityEngine;
using UnityEngine.UI;
using static MenuUI.Scripts.TopPanel.TopBarTargets;

public class TopBarClanTileLayout : MonoBehaviour
{

    [SerializeField] private TopBarDefs.TopBarItem _togglesClanTile;

    [Header("Layout System")]
    [SerializeField] private Toggle _toggle;
    [SerializeField] private RectTransform _topBarLayout;
    [SerializeField] private RectTransform _clanTileParent;
    [SerializeField] private GameObject _clanTileLayout;

    [Header("View More")]
    [SerializeField] private string SavedViewMore;
    [SerializeField] private RectTransform _inputArrow;
    [SerializeField] private Toggle _viewMore;


    [Header("List")]
    [SerializeField] private List<TopBarToggleHandler> _objectToggles;
    [SerializeField] private List<Transform> _clantileChildren;
    [SerializeField] private List<TopBarToggleDrag> _toggleDrag;

    void Start()
    {
        _viewMore.isOn = PlayerPrefs.GetInt(SavedViewMore, 0) == 1;

        if(_viewMore.isOn)
        {
            _inputArrow.rotation = Quaternion.Euler(0, 0, -90);
        } else
        {
            _inputArrow.rotation = Quaternion.Euler(0, 0, -270);
        }

         _viewMore.onValueChanged.AddListener(ViewSystem);


        _toggle.onValueChanged.AddListener(ChangeParent);

        if (_clanTileLayout.transform.childCount > 0)
        {
            _viewMore.gameObject.SetActive(true);
        }
        else
        {
            layoutResize(56.91293f);
            _viewMore.gameObject.SetActive(false);
        }

    }

    private void ChangeParent(bool isOn)
    {
        //If on: attaches the toggles to clantile toggle group
        //If off: detaches the toggles from clantile toggle group
        if (isOn)
        {
            foreach(var t in _toggleDrag)
            {
                t.enabled = false;
            }
            foreach(RectTransform r in  _clantileChildren)
            {
                r.SetParent(_clanTileLayout.transform);
            }

        } else
        {

            //The idea is here is so that anytime the children would detach they would go under the clantile toggle is, instead of going to the bottom of the selection
            int parentposition = _clanTileParent.GetSiblingIndex();
            int i = 1;
            foreach (var t in _toggleDrag)
            {
                t.enabled = true;
            }
            foreach (RectTransform r in _clantileChildren)
            {
                r.SetParent(_topBarLayout);
                r.SetSiblingIndex(parentposition + i);
                i++;
                
            }            
        }

        ///Used when clantile has no children so viewmore arrow will be removed
        ///Incase no one likes the visually it, just CTRL + DELETE <the cref="_viewMore.gameObject.SetActive(false);"/>
        if (_clanTileLayout.transform.childCount > 0)
        {
            _viewMore.gameObject.SetActive(true);
            ViewSystem(_viewMore.isOn);
            _viewMore.interactable = true;
        }
        else
        {
            layoutResize(56.91293f);
            _viewMore.gameObject.SetActive(false);
        }
    }

    //Used for clantile's children
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

    //Used when theme is changed to check if this theme has a clantile or not
    public IEnumerator IsThereATile(TopBarTargets TopBarOrderBridge)
    {
        yield return new WaitForEndOfFrame();

        foreach (var e in TopBarOrderBridge.PTileManagement)
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

    //Rearranges clantile's toggles as some themes objects are/arent on clantile
    public void Rearrange(TileManagement TopBarOrderBridge)
    {
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

    //Clears Toggles from clantile for toggle changes
    public void SetTogglesFree()
    {
        for (int i = 0; i < _clantileChildren.Count; i++)
        {
            _clantileChildren[i].transform.SetParent(_topBarLayout.transform);
        }
        foreach (var i in _toggleDrag)
        {
            i.enabled = true;
        }

        _clantileChildren.Clear();
        _toggleDrag.Clear();
        layoutResize(56.91293f);
    }
    //Used to resize the Clantile size toggle
    private void layoutResize(float value)
    {
        LayoutElement layoutElement = _clanTileParent.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = value;
        layoutElement.minHeight = value;
    }



}
