using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Audio;
using UnityEngine;

public class BattleStartHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        AudioManager.Instance.StopMusic(); //This should have a short sfx clip playing while the battle starts.
    }
    private void OnDisable()
    {
        AudioManager.Instance.StopMusic();
    }
}
