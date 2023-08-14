using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplotionTimer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log(""+transform.position);
        StartCoroutine(die());
    }
    private IEnumerator die()
    {
        yield return new WaitForSeconds(2.8f);
        Destroy(gameObject);
    }
}
