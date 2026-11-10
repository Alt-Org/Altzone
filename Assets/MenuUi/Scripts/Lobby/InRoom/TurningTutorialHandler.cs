using System.Collections;
using System.Collections.Generic;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurningTutorialHandler : MonoBehaviour
{
    [SerializeField] private Transform _shield;
    [SerializeField] private GameObject _panel;

    private bool _touching = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ClickStateHandler.GetClickState() is ClickState.Start or ClickState.Hold or ClickState.Move)
        {
            if (_panel.activeSelf)
            {
                if (CheckIfPanel())
                {
                    _touching = true;
                }
                else _touching = false;
            }
        }

        if (_touching && ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll)
        {
            float rotation = ClickStateHandler.GetRotationValue();
            _shield.rotation = Quaternion.Euler(0, 0, rotation);
        }
        if (ClickStateHandler.GetClickState() is ClickState.End) _shield.rotation = Quaternion.Euler(Vector3.zero);
    }

    private bool CheckIfPanel()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData data = new(EventSystem.current);
        data.position = ClickStateHandler.GetClickPosition();
        if (data.position == Vector2.negativeInfinity) return false;
        var modules = RaycasterManager.GetRaycasters();
        foreach (var module in modules)
        {
            module.Raycast(data, results);
        }
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == _panel) return true;
        }
        return false;
    }
}
