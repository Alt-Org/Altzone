using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSetting : MonoBehaviour
{
    [SerializeField] private string _name;

    private Toggle _toggle;

    // Start is called before the first frame update
    void Start()
    {
        _toggle = GetComponent<Toggle>();

        CheckValidity();

        GetSavedValue();
    }

    public void GetSavedValue()
    {
        if (!CheckValidity())
        {
            return;
        }

        _toggle.isOn = (PlayerPrefs.GetInt(_name, 0) != 0);
    }


    public void ChangeValue()
    {
        if(!CheckValidity())
        {
            return;
        }

        if (_toggle.isOn) PlayerPrefs.SetInt(_name, 1);
        else PlayerPrefs.SetInt(_name, 0);
    }

    private bool CheckValidity()
    {
        if(_toggle == null || _name.Length == 0)
        {
            Debug.LogWarning("ERROR: No Toggle found or name set");
            return false;
        }

        return true;
    }
}
