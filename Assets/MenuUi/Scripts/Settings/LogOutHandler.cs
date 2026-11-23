using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;

public class LogOutHandler : WindowNavigation
{
    // Start is called before the first frame update
    void Start()
    {
        ServerManager.OnLogInStatusChanged += HandleLogOut;
    }

    private void OnDestroy()
    {
        ServerManager.OnLogInStatusChanged -= HandleLogOut;
    }

    private void HandleLogOut(bool loggedIn)
    {
        if(!loggedIn) StartCoroutine(Navigate());
    }
}
