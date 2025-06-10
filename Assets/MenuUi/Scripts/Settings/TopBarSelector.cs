using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopBarSelector : MonoBehaviour
{
    [SerializeField] private List<GameObject> _topBarList;

    void Start()
    {
        SettingsCarrier.OnTopBarChanged += SetPanel;

        SetPanel((int)SettingsCarrier.Instance.TopBarStyleSetting);
    }

    private int CheckIndexRange(int index)
    {
        if (index + 1 > _topBarList.Count)
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
        index = CheckIndexRange(index);

        foreach (GameObject go in _topBarList)
        {
            go.SetActive(false);
        }
        _topBarList[index].SetActive(true);
    }

    
}
