using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.GA;
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
        TestShieldHits(); 
        TestMoveCommands(); 
        TestDistanceToPlayer(); 
        TestDistanceToWall(); 
        TestOnBattleEnd(); 
        TestClanChanges(); */
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
    } 

    void TestMoveCommands()
    {
        GameAnalyticsManager.Instance.MoveCommand(new Vector3(1, 0, 1));
        GameAnalyticsManager.Instance.MoveCommand(new Vector3(2, 0, 2));
        GameAnalyticsManager.Instance.MoveCommand(new Vector3(3, 0, 3));
        GameAnalyticsManager.Instance.MoveCommand(new Vector3(4, 0, 4));
        GameAnalyticsManager.Instance.OnBattleEnd();
        Debug.Log("MoveCommands");
    } 

    void TestDistanceToPlayer()
    {
        Vector3 playerPosition = new Vector3(0, 0, 0);
        Vector3 otherPlayerPosition = new Vector3(3, 0, 4);
        GameAnalyticsManager.Instance.DistanceToPlayer(playerPosition, otherPlayerPosition);    
    } 

    void TestDistanceToWall()
    {
        Vector3 playerPosition = new Vector3(0, 0, 0);
        Vector3 wallPosition = new Vector3(5, 0, 0);
        GameAnalyticsManager.Instance.DistanceToWall(playerPosition, wallPosition);
    } 

    void TestOnBattleEnd()
    {
        GameAnalyticsManager.Instance.OnShieldHit("rauta-rami");
        GameAnalyticsManager.Instance.OnWallHit("Team1");
        GameAnalyticsManager.Instance.OnWallHit("Team1");
        GameAnalyticsManager.Instance.OnWallHit("Team1");
        GameAnalyticsManager.Instance.OnShieldHit("hannu-hodari");
        GameAnalyticsManager.Instance.OnShieldHit("rauta-rami");
        GameAnalyticsManager.Instance.OnShieldHit("hannu-hodari");
        GameAnalyticsManager.Instance.OnShieldHit("rauta-rami");
        GameAnalyticsManager.Instance.OnBattleEnd(); 
    } 

    void TestClanChanges()
    {
        GameAnalyticsManager.Instance.ClanChange("Clan A");
        GameAnalyticsManager.Instance.ClanChange("Clan B");
    } */
}
