using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class InputFieldSizeChange : MonoBehaviour
{

    [SerializeField]
    TMP_InputField _inputField;
    [SerializeField]
    TextMeshProUGUI _textMeshProUGUI;
    [SerializeField]
    RectTransform _rect;

    //[SerializeField]
    float PreferredHeight;

    private void OnEnable()
    {
        SetHeight();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetHeight();
    }

    protected virtual void SetHeight()
    {
        if (_textMeshProUGUI == null)
            return;

        if (_inputField.text.Length != 0)
        {
            PreferredHeight = _textMeshProUGUI.preferredHeight;
            if (PreferredHeight < 100)
            {
                _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, 100);
            }
            else
            {
                _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, PreferredHeight);
            }
        }
        else
        {
            _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, 100);
        }
        
    }

}
