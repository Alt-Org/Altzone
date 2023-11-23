using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMainMenuWindowIndex : MonoBehaviour
{
    public void SetMainMenuWindowIndexValue(int index)
    {
        FindObjectOfType<SwipeUI>(true).CurrentPage = index;
    }
}
