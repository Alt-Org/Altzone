using System.Collections;
using System.Collections.Generic;
using AltZone.Scripts.GA;
using UnityEngine;

public class TestEvents : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //GameAnalyticsManager.Initialize();

        /* testCharacterSelection();
        testCharacterOutcomes();
        TestBattleLaunch(); 
        TestBattlesStarted(); 
        StartCoroutine(TestSectionEnteredExited()); 
        TestShieldHits(); */
    }

    // Update is called once per frame
    /* void testCharacterSelection()
    {
        GameAnalyticsManager.Instance.CharacterSelection("rauta-rami");
        GameAnalyticsManager.Instance.CharacterSelection("vitsi-ville");
        GameAnalyticsManager.Instance.CharacterSelection("lasse_liukas");
        GameAnalyticsManager.Instance.CharacterSelection("sami_saarnaaja");
        GameAnalyticsManager.Instance.CharacterSelection("graffiti-gaya");
        GameAnalyticsManager.Instance.CharacterSelection("hannu-hodari");
        Debug.Log("character selected");
    }

    void testCharacterOutcomes()
    {
        GameAnalyticsManager.Instance.CharacterWin("Char02");
        GameAnalyticsManager.Instance.CharacterLoss("Char03");

    }

    void TestBattleLaunch()
    {
        GameAnalyticsManager.Instance.BattleLaunch();
    }

    void TestBattlesStarted()
    {
        GameAnalyticsManager.Instance.BattlesStarted();
    }

    private IEnumerator TestSectionEnteredExited()
    {
        Debug.Log("TestSectionEnteredExited started");

        GameAnalyticsManager.Instance.EnterSection("TestingRoom_A");
        yield return new WaitForSeconds(20f); // 20s

        GameAnalyticsManager.Instance.ExitSection("TestingRoom_A");
        GameAnalyticsManager.Instance.OpenSoulHome();
        yield return new WaitForSeconds(15f); // 15s

        GameAnalyticsManager.Instance.ExitSection("TestingRoom_B");
        GameAnalyticsManager.Instance.EnterSection("TestingRoom_A");

        Debug.Log("TestSectionEnteredExited completed");
    } 
    void TestShieldHits()
    {
        GameAnalyticsManager.Instance.OnShieldHit();
        GameAnalyticsManager.Instance.OnShieldHit();
        GameAnalyticsManager.Instance.OnShieldHit();
        GameAnalyticsManager.Instance.OnWallHit();
        GameAnalyticsManager.Instance.OnShieldHit();
        GameAnalyticsManager.Instance.OnShieldHit();
        GameAnalyticsManager.Instance.OnWallHit();
        GameAnalyticsManager.Instance.OnSessionEnd();
        Debug.Log("TestShieldHits completed");
    } */
}
