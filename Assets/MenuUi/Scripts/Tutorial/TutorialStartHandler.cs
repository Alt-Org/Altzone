using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStartHandler : MonoBehaviour
{
    [SerializeField] private Button _advanceButton;
    [SerializeField] private Button _skipButton;

    public delegate void TutorialStarted();
    public event TutorialStarted OnTutorialStarted;

    public delegate void TutorialSkip();
    public event TutorialSkip OnTutorialSkip;

    // Start is called before the first frame update
    void Start()
    {
        _advanceButton.onClick.AddListener(() => OnTutorialStarted?.Invoke());
        _skipButton.onClick.AddListener(() => OnTutorialSkip?.Invoke());
    }

}
