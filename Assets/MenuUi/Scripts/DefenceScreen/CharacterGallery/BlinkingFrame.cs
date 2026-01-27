using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class BlinkingFrame : MonoBehaviour
{
    [SerializeField] private float blinkSpeed = 1.7f;
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 1f;

    private Image _image;
    private Coroutine _blinkRoutine;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void StartBlinking()
    {
        StopBlinking();
        _blinkRoutine = StartCoroutine(Blink());
    }

    public void StopBlinking()
    {
        if (_blinkRoutine != null)
        {
            StopCoroutine(_blinkRoutine);
            _blinkRoutine = null;
        }
        //Hide frame when its not active
        SetAlpha(0f);
    }

    /// <summary>
    /// // Continuously changes the image alpha back and forth between minAlpha and maxAlpha using unscaled time
    /// </summary>
    private IEnumerator Blink()
    {
        float t = 0f;

        while (true)
        {
            t += Time.unscaledDeltaTime * blinkSpeed;

            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(t, 1f));
            SetAlpha(alpha);

            yield return null;
        }
    }

    private void SetAlpha(float a)
    {
        Color c = _image.color;
        c.a = a;
        _image.color = c;
    }
}

