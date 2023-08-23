using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTimer : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
