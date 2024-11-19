using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileMenuTabs : MonoBehaviour
{

    [Header("Toggles")]
    public Toggle basicToggle;
    public Toggle statsToggle;
    public Toggle roleplayToggle;


    [Header("Objects")]
    public GameObject basicObject;
    public GameObject statsObject;
    public GameObject roleplayObject;


    // Start is called before the first frame update
    void Start()
    {
        basicToggle.onValueChanged.AddListener((isOn) => UpdateVisibility(basicObject, isOn));
        statsToggle.onValueChanged.AddListener((isOn) => UpdateVisibility(statsObject, isOn));
        roleplayToggle.onValueChanged.AddListener((isOn) => UpdateVisibility(roleplayObject, isOn));

        UpdateVisibility(basicObject, basicToggle.isOn);
        UpdateVisibility(statsObject, statsToggle.isOn);
        UpdateVisibility(roleplayObject, roleplayToggle.isOn);
    }

    void UpdateVisibility(GameObject obj, bool isVisible)
    {
        obj.SetActive(isVisible);
    }
}
