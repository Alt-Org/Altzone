using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderRubberband : MonoBehaviour
{
    [SerializeField] private float _rubberbandDuration = 0.5f;
    [SerializeField] private AnimationCurve _rubberbandCurve;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();

        if (_rubberbandCurve.length < 2)
        {
            _rubberbandCurve.AddKey(0f, 0f);
            _rubberbandCurve.AddKey(1f, 1f);
        }
    }

    public void Reset()
    {
        _slider.value = 0f;
    }

    public IEnumerator StartRubberband(float timeElapsed, float targetDuration, System.Action<bool> done = null)
    {
        float totalProgress = timeElapsed / targetDuration;
        float estimatedLocation = (timeElapsed + _rubberbandDuration) / targetDuration;
        float sliderStartLocation = _slider.value;

        float timer = 0f;

        while (timer < _rubberbandDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            Debug.LogError(timer);
            _slider.value = Mathf.Lerp(sliderStartLocation, estimatedLocation, _rubberbandCurve.Evaluate(timer / _rubberbandDuration));
        }

        if (done != null) done(true);
    }
}
