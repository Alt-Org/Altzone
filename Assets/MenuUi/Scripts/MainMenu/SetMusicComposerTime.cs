using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMusicComposerTime : MonoBehaviour
{
    public void SetTime(float time)
    {
        MainMenuMusicComposer.Instance.SetMusicTime(time);
    }
}
