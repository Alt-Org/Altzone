using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets the main menu index to the selected main menu view
/// </summary>
/// <remarks>
/// Since the swipe main menu is now 5 views inside of a scroll view and not 5 separate views
/// we need to know which button was pressed so we can scroll the scroll view to correct place.
/// </remarks>
public class SetMainMenuWindowIndex : MonoBehaviour
{
    public void SetMainMenuWindowIndexValue(int index)
    {
        FindObjectOfType<SwipeUI>(true).CurrentPage = index;
    }
}
