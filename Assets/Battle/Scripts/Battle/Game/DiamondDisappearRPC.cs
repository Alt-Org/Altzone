using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DiamondDisappearRPC : MonoBehaviour
{
    [SerializeField] float min;
    [SerializeField] float max;
    [SerializeField] DiamondDisappear DiamondDisappear;

    void Start()
    {
        DiamondDisappear = GetComponentInParent<DiamondDisappear>();
        DiamondDisappear.DiamondsArray.Add(gameObject);
        if (DiamondDisappear.Master == true)
        {
            StartCoroutine(Disappear());
        }
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(Random.Range(min, max));
        DiamondDisappear.DiamondDestroy();
    }
}
