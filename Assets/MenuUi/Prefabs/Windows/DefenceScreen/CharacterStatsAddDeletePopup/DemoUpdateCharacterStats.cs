using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DemoUpdateCharacterStats : MonoBehaviour
{
    public TMP_Text SpeedNumber;
    public TMP_Text ResistanceNumber;
    public TMP_Text AttackNumber;
    public TMP_Text DefenceNumber;

    public string DemoCharacterName = "yeyeye";
    public float Speed = 3f;
    public float Resistance = 6f;
    public float Attack = -2f;
    public float Defence = 2f;
    public float Hp;

    // Start is called before the first frame update
    void Start()
    {
        SpeedNumber = GameObject.Find("Speed/StatNumber").GetComponent<TMP_Text>();
        ResistanceNumber = GameObject.Find("Resistance/StatNumber").GetComponent<TMP_Text>();
        AttackNumber = GameObject.Find("Attack/StatNumber").GetComponent<TMP_Text>();
        DefenceNumber = GameObject.Find("Defence/StatNumber").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        SpeedNumber.text = Speed.ToString();
        ResistanceNumber.text = Resistance.ToString();
        AttackNumber.text = Attack.ToString();
        DefenceNumber.text = Defence.ToString();
    }


}
