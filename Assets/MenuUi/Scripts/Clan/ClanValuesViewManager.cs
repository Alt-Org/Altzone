using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanValuesViewManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _objectsToActivate;

    [SerializeField] private GameObject[] _objectsToHide;

    [SerializeField] private ScrollRect _scrollRectComponent;

    public void ShowValuesView()
    {
        foreach (GameObject toActivate in _objectsToActivate)
        {
            toActivate.SetActive(true);
        }

        foreach(GameObject toHide in _objectsToHide)
        {
            toHide.SetActive(false);
        }

        StartCoroutine(ResetScrollPosition()); 
    }

    public void HideValuesView()
    {
        foreach (GameObject toActivate in _objectsToActivate)
        {
            toActivate.SetActive(false);
        }

        foreach (GameObject toHide in _objectsToHide)
        {
            toHide.SetActive(true);
        }
    }

    private IEnumerator ResetScrollPosition()
    {
        yield return null; 
        _scrollRectComponent.verticalNormalizedPosition = 1f;
    }
}
