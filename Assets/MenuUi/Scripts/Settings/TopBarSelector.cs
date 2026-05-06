using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopBarSelector : MonoBehaviour
{
    [SerializeField] private List<GameObject> _topBarList;

    private static int s_topbar = 0;

    void Start()
    {
        SetPanel((int)SettingsCarrier.Instance.TopBarStyleSetting);
    }

    private void OnEnable()
    {
        SettingsCarrier.OnTopBarChanged += SetPanel;

        SetPanel((int)SettingsCarrier.Instance.TopBarStyleSetting);
    }

    private void OnDisable()
    {
        SettingsCarrier.OnTopBarChanged -= SetPanel;
    }

    private int CheckIndexRange(int index)
    {
        if (index >= _topBarList.Count)
        {
            return 0;
        }
        else
        {
            return index;
        }
    }

    private void SetPanel(int index)
    {
        s_topbar = CheckIndexRange(index);

        foreach (GameObject go in _topBarList)
        {
            go.SetActive(false);
        }
        _topBarList[s_topbar].SetActive(true);
    }

    
}
