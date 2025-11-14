using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// From: https://discussions.unity.com/t/4-6-ui-scrollbar-force-handle-size-to-be-fixed/128253 , with coroutine addition.

/// <summary>
/// Sets scroll bar handle size to 0 at the start for a fixed handle size.
/// </summary>
public class ScrollBarHandleSizeFix : MonoBehaviour
{
    //private float _time = 0.1f;

    void Start() { StartCoroutine(DelayedExecution()); }

    private IEnumerator DelayedExecution()
    {
        //yield return new WaitForSeconds(_time);
        yield return new WaitForEndOfFrame();

        transform.GetComponent<Scrollbar>().size = 0;
    }
}
