using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFieldSetting : MonoBehaviour
{
    [SerializeField] private string _name;

    private TMP_InputField _inputField;

    // Start is called before the first frame update
    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();

        CheckValidity();

        GetSavedValue();
    }

    public void GetSavedValue()
    {
        if (!CheckValidity())
        {
            return;
        }

        _inputField.text = PlayerPrefs.GetString(_name, "");
    }


    public void ChangeValue()
    {
        if (!CheckValidity())
        {
            return;
        }

        PlayerPrefs.SetString(_name, _inputField.text);
    }

    private bool CheckValidity()
    {
        if (_inputField == null || _name.Length == 0)
        {
            Debug.LogWarning("ERROR: No Input Field found or name set");
            return false;
        }

        return true;
    }

}
