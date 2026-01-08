using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SliderToggle : Toggle
{
    [SerializeField] private Slider _sliderSwitch;
    [SerializeField] private Image _background;
    [SerializeField] private Image _fill;
    [SerializeField] private Image _handle;

    [SerializeField] private Color _oncolour;
    [SerializeField] private Color _offcolour;

    public delegate void ToggleStateChanged(bool isOn);
    public event ToggleStateChanged OnToggleStateChanged;

    protected override void Start()
    {
        base.Start();

        if (_sliderSwitch != null)
        {
            _background.color = _offcolour;
            _fill.color = _oncolour;
            if (isOn) _sliderSwitch.value = _sliderSwitch.maxValue;
            else _sliderSwitch.value = _sliderSwitch.minValue;
            SetColours(isOn);
        }
        onValueChanged.AddListener(ChangeState);
    }
    protected override void OnDestroy()
    {
        onValueChanged.RemoveAllListeners();
    }

    private void ChangeState(bool value)
    {
        if (_sliderSwitch != null)
        {
            if (value) _sliderSwitch.value = _sliderSwitch.maxValue;
            else _sliderSwitch.value = _sliderSwitch.minValue;
            SetColours(value);
        }

        OnToggleStateChanged?.Invoke(value);
    }

    public void SetState(bool value)
    {
        if (value == isOn) return;
        isOn = value;

    }
    private void SetColours(bool isOn)
    {
        if (isOn)
        {
            _handle.color = _oncolour;
        }
        else
        {
            _handle.color = _offcolour;
        }
    }


    /// <summary>
    /// Transition the Selectable to the entered state.
    /// </summary>
    /// <param name="state">State to transition to</param>
    /// <param name="instant">Should the transition occur instantly.</param>
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if(transition == Transition.ColorTint)
        {
            Color tintColor;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = colors.normalColor;
                    break;
                case SelectionState.Highlighted:
                    tintColor = colors.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    tintColor = colors.pressedColor;
                    break;
                case SelectionState.Selected:
                    tintColor = colors.selectedColor;
                    break;
                case SelectionState.Disabled:
                    tintColor = colors.disabledColor;
                    break;
                default:
                    tintColor = Color.black;
                    break;
            }

            StartColorTween(tintColor * colors.colorMultiplier, instant);
        }
        else
        {
            base.DoStateTransition(state, instant);
        }
    }

    void StartColorTween(Color targetColor, bool instant)
    {
        if (_background != null)
            _background.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        if (_fill != null)
            _fill.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        if (_handle != null)
            _handle.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);

    }
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/SliderToggle", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/ToggleSlider"));
        go.name = "ToggleSlider";
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}
