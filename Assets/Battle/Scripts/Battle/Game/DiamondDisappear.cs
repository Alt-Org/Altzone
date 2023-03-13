using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DiamondDisappear : MonoBehaviour
{
    [SerializeField] float min;
    [SerializeField] float max;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        yield return new WaitForSeconds(Random.Range(min, max));
        //if (PhotonNetwork.IsMasterClient)
        //{
            Destroy(gameObject);    //PhotonNetwork
        //}
    }
}
