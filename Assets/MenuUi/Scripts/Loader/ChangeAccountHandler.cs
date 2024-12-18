using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAccountHandler : MonoBehaviour
{
    [SerializeField]
    private Button _changeAccountButton;

    public delegate void ChangeAccountEvent();
    public event ChangeAccountEvent OnChangeAccountEvent;

    private void Awake()
    {
        _changeAccountButton.onClick.AddListener(ChangeAccount);
    }

    private void OnDestroy()
    {
        _changeAccountButton.onClick.RemoveAllListeners();
    }

    private void ChangeAccount()
    {
        OnChangeAccountEvent?.Invoke();
    }
}
