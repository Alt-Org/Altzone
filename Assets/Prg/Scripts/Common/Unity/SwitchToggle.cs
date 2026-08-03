using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : Toggle
{
    public enum SpriteMode
    {
        Disable,
        SpriteSwap,
    }

    [SerializeField] private SpriteMode _spriteMode = SpriteMode.SpriteSwap;
    [SerializeField] private Image _togglesprite;
    [SerializeField] private Sprite _activeSprite;
    [SerializeField] private Sprite _inactiveSprite;

    public SpriteMode Mode { get => _spriteMode; }

    public delegate void ToggleStateChanged(bool isOn);
    public event ToggleStateChanged OnToggleStateChanged;

    protected override void Start()
    {
        base.Start();

        if (_togglesprite != null)
        {
            switch (_spriteMode)
            {
                case SpriteMode.Disable:
                    if (isOn) _togglesprite.enabled = true;
                    else _togglesprite.enabled = false;
                    break;
                case SpriteMode.SpriteSwap:
                    if (isOn) _togglesprite.sprite = _activeSprite;
                    else _togglesprite.sprite = _inactiveSprite;
                    break;
        }
        }
        onValueChanged.AddListener(ChangeState);
    }
    protected override void OnDestroy()
    {
        onValueChanged.RemoveAllListeners();
    }

    private void ChangeState(bool value)
    {
        if (_togglesprite != null)
        {
            if (value) _togglesprite.sprite = _activeSprite;
            else _togglesprite.sprite = _inactiveSprite;
        }

        OnToggleStateChanged?.Invoke(value);
    }

    public void SetState(bool value)
    {
        if (value == isOn) return;
        isOn = value;

    }

#if UNITY_EDITOR
    [MenuItem("GameObject/UI/SwitchToggle", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/ToggleSwitch"));
        go.name = "ToggleSwitch";
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}
