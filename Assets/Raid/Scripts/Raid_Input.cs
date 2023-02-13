using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class Raid_Input : MonoBehaviour
{
    [SerializeField, Header("Grid")] private Raid_Grid raid_Grid;

    [SerializeField, Header("Settings"), Range(0, 100)] private float _trackingSensitivityPercent = 1.0f;
    [SerializeField] private InputActionReference _tapActionRef;
    [SerializeField] private InputActionReference _positionActionRef;
    
    private InputAction _tapAction;
    private InputAction _positionAction;
    private Vector2 _startPosition;
    private Vector2 _inputPosition;
    private bool _isCheckSensitivity;
    private float _sensitivityScreenX;
    private float _sensitivityScreenY;
    
    private void Awake()
    {
        Debug.Log($"");
        Assert.IsNotNull(raid_Grid);
        
        Debug.Log($"{_tapActionRef} | {_positionActionRef}");
        Assert.IsTrue(_tapActionRef != null && _positionActionRef != null);
        _tapAction = _tapActionRef.action;
        _positionAction = _positionActionRef.action;
        Assert.IsFalse(string.IsNullOrWhiteSpace(_tapAction.interactions));
        Debug.Log($"interactions {_tapAction.interactions}");
        if (!(_trackingSensitivityPercent > 0))
        {
            return;
        }
        var resolution = Screen.currentResolution;
        _isCheckSensitivity = true;
        _sensitivityScreenX = resolution.width / 100f * _trackingSensitivityPercent;
        _sensitivityScreenY = resolution.height / 100f * _trackingSensitivityPercent;
        Debug.Log($"tracking {_trackingSensitivityPercent}% : x,y {_sensitivityScreenX:0},{_sensitivityScreenY:0} from {resolution}");
    }

    private void OnEnable()
    {
        Debug.Log($"{_tapAction} | {_positionAction}");
        _tapAction.started += TapStarted;
        _tapAction.performed += TapPerformed;
        _tapAction.Enable();
        _positionAction.Enable();
    }
    
    private void OnDisable()
    {
        Debug.Log($"{_tapAction} | {_positionAction}");
        _tapAction.started -= TapStarted;
        _tapAction.performed -= TapPerformed;
        _tapAction.Disable();
        _positionAction.Disable();
    }

    private void TapStarted(InputAction.CallbackContext ctx)
    {
        _startPosition = _positionAction.ReadValue<Vector2>();
        Debug.Log($"duration {ctx.duration:0.000} pos {_startPosition} {ctx.interaction?.GetType().Name}");
    }

    private void TapPerformed(InputAction.CallbackContext ctx)
    {
        _inputPosition = _positionAction.ReadValue<Vector2>();
        Debug.Log($"duration {ctx.duration:0.000} pos {_inputPosition} delta {_startPosition - _inputPosition} {ctx.interaction?.GetType().Name}");
        if (_isCheckSensitivity)
        {
            var delta = _startPosition - _inputPosition;
            if (Mathf.Abs(delta.x) > _sensitivityScreenX || Mathf.Abs(delta.y) > _sensitivityScreenY)
            {
                Debug.Log($"IGNORED delta {delta}");
                return;
            }
        }
        var interaction = ctx.interaction;
        switch (interaction)
        {
            case TapInteraction:
                raid_Grid.QuickTapPerformed(_inputPosition);
                break;
            case SlowTapInteraction:
                raid_Grid.SlowTapPerformed(_inputPosition);
                break;
        }
    }
}
