using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTabOpener : MonoBehaviour
{
    [SerializeField] private GameObject _TabToOpen;
    private void Start()
    {
        StartCoroutine(OpenTab());
    }
    IEnumerator OpenTab()
    {
        yield return new WaitForSeconds(0.5f);
        _TabToOpen.SetActive(true);
    }
}
