using System.Collections;
using UnityEngine;

public class JukeboxTextPopup : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _text;
    [SerializeField] private GameObject _content;
    [Space]
    [SerializeField] private float _visibleTime = 1.5f;

    private Coroutine _visibilityCoroutine;

    public void Set(string text)
    {
        _text.text = text;

        if (!isActiveAndEnabled) return;

        _content.SetActive(true);
        StartCoroutine(VisibilityController());
    }

    private IEnumerator VisibilityController()
    {
        float timer = 0f;

        while (timer < _visibleTime)
        {
            yield return null;

            timer += Time.deltaTime;
        }

        _content.SetActive(false);
    }
}
