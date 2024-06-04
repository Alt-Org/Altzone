using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEvents : MonoBehaviour
{
    
    void Start()
    {
        GameAnalyticsManager.Initialize();

        TestBattleLaunch();
        TestOpenSoulHome();
    }

    
    void TestBattleLaunch()
    {
        GameAnalyticsManager.BattleLaunch();
        Debug.Log("test: BattleLaunch called");
    }

    void TestOpenSoulHome()
    {
        GameAnalyticsManager.OpenSoulHome();
        Debug.Log("Test: OpenSoulHome called");
    }
}
