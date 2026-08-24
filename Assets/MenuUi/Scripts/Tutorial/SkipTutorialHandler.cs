using UnityEngine;

public class SkipTutorialHandler : MonoBehaviour
{
    [SerializeField] private GameObject _skipTutorialPanel1;
    [SerializeField] private GameObject _skipTutorialPanel2;

    //activate the tutorial skip button with the correct position
    public void SetSkipTutorialPanel(int index)
    {
        _skipTutorialPanel1.SetActive(false);
        _skipTutorialPanel2.SetActive(false);

        if (index < 8)
        {
            _skipTutorialPanel1.SetActive(true);
        }
        else if (index >= 8)
        {
            _skipTutorialPanel2.SetActive(true);
        }
    }
}
