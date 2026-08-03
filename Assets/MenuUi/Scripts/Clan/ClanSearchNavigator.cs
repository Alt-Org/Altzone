using System.Collections;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;

public class ClanSearchNavigator : WindowNavigation
{
    public void NavigateToClanSearch()
    {
        StartCoroutine(NavigateToTarget());
    }

    private IEnumerator NavigateToTarget()
    {
        yield return Navigate();
    }
}
