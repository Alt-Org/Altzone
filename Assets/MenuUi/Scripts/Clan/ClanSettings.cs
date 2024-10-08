using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClanSettings : MonoBehaviour
{
    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanCoins;
    [SerializeField] private TextMeshProUGUI _clanTrophies;
    [SerializeField] private TextMeshProUGUI _clanGlobalRanking;

    [Header("Input fields")]
    [SerializeField] private TMP_InputField _clanPhrase;
    [SerializeField] private TMP_InputField _clanPassword;

    [Header("Toggles and dropdowns")]
    [SerializeField] private Toggle _clanOpenToggle;
    [SerializeField] private TMP_Dropdown _clanLanguageDropdown;
    [SerializeField] private TMP_Dropdown _clanGoalDropdown;

    [Header("Other settings fields")]
    [SerializeField] Transform _valueRowFirst;
    [SerializeField] Transform _valueRowSecond;

    [Header("Prefabs")]
    [SerializeField] GameObject _valuePrefab;


    public void SaveClanSettings()
    {
        Debug.Log("Saving settings not yet implimented.");
    }
}
