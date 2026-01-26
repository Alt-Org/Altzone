using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Signals;
using UnityEngine;

public class LoadoutEditorOpener : MonoBehaviour
{
    public void OpenEdit()
    {
        SignalBus.OnDefenceGalleryEditPanelRequestedSignal();
    }
}
