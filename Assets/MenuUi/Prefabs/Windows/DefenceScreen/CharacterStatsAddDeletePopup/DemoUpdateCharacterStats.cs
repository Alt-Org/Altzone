using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DemoUpdateCharacterStats : MonoBehaviour
{
    public TMP_Text CharacterName;
    public TMP_Text SpeedNumber;
    public TMP_Text ResistanceNumber;
    public TMP_Text AttackNumber;
    public TMP_Text DefenceNumber;

    public string DemoCharacterName = "yeyeye";

    // Start is called before the first frame update
    void Start()
    {    
        SpeedNumber = GameObject.Find("Speed/StatNumber").GetComponent<TMP_Text>();
        ResistanceNumber = GameObject.Find("Resistance/StatNumber").GetComponent<TMP_Text>();
        AttackNumber = GameObject.Find("Attack/StatNumber").GetComponent<TMP_Text>();
        DefenceNumber = GameObject.Find("Defence/StatNumber").GetComponent<TMP_Text>();
        CharacterName = GameObject.Find("CharacterImagePanel/CharacterName").GetComponent<TMP_Text>();

        SetDefaultCharacterStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    
    public void UpdateCharacterSpeed()
    {
        Debug.Log("eeyeeyeeee");
        //SpeedNumber.text = DemoCharacterStatsStatic.defaultCharacterStats[4].ToString() + 1;
    }

    public void SetDefaultCharacterStats()
    {
        SpeedNumber.text = DemoCharacterStatsStatic.defaultCharacterStats[4].ToString();
        ResistanceNumber.text = DemoCharacterStatsStatic.defaultCharacterStats[3].ToString();
        AttackNumber.text = DemoCharacterStatsStatic.defaultCharacterStats[2].ToString();
        DefenceNumber.text = DemoCharacterStatsStatic.defaultCharacterStats[1].ToString();
        CharacterName.text = DemoCharacterName;
    }

}
