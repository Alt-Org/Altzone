using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelHandler : MonoBehaviour
{
    [SerializeField] private Button _tutorialAdvanceButton;

    public void SetData(Action advanceAction)
    {
        if(_tutorialAdvanceButton != null)
            _tutorialAdvanceButton.onClick.AddListener(advanceAction.Invoke);
    }

}
