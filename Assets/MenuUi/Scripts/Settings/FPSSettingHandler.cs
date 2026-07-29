using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSSettingHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentFPS;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _prevButton;

    private const int _lowFPS = 30;
    private const int _highFPS = 60;

    private FpsLock30 _fpsLock;


    // Start is called before the first frame update
    void Start()
    {
        SetFPSText();

        _fpsLock = GetComponentInParent<FpsLock30>();

        if(!_fpsLock)
        {
            Debug.LogWarning("No FpsLock30 found");
        }

        _nextButton.onClick.AddListener(() => ChangeFPS(true));
        _prevButton.onClick.AddListener(() => ChangeFPS(false));
    }

    private void ChangeFPS(bool up)
    {
        int currentFPS = PlayerPrefs.GetInt("TargetFrameRate", _highFPS);
        int nextFPS = currentFPS;

        if(!_fpsLock)
        {
            Debug.LogWarning("No FpsLock30 found");
            return;
        }

        if(currentFPS == (int)Screen.currentResolution.refreshRateRatio.value)          // In case of FPS being set to native, we need to change it to -1 for switch to work
        {
            currentFPS = -1;
        }

        switch (currentFPS)
        {
            case -1:
                if(up)
                {
                    nextFPS = _lowFPS;
                }
                else
                {
                    nextFPS = _highFPS;
                }
                break;
            case _lowFPS:
                if (up)
                {
                    nextFPS = _highFPS;
                }
                else
                {
                    nextFPS = -1;
                }
                break;
            case _highFPS:
                if (up)
                {
                    nextFPS = -1;
                }
                else
                {
                    nextFPS = _lowFPS;
                }
                break;
        }

        if(nextFPS < 0)
        {
            _fpsLock.ChangeFrameRateToNative();             // Saving FPS to PlayerPrefs is handled in FPSLock -script. No need to worry about it here
        }
        else
        {
            _fpsLock.ChangeFrameRate(nextFPS);
        }

        SetFPSText();
    }

    private void SetFPSText()
    {
        int FPS = PlayerPrefs.GetInt("TargetFrameRate", _highFPS);

        if(FPS >= 0)
        {
            _currentFPS.text = FPS.ToString();
        }
        else
        {
            _currentFPS.text = "NAT";
        }
    }
}
