using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressWheelHandler : MonoBehaviour
{
    public static ProgressWheelHandler Instance; // singleton

    [SerializeField] private GameObject _wheel;
    [SerializeField] private TextMeshProUGUI _seconds;

    private Coroutine _coroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    //Function for setting wheel on(if not aleady) and updating progress
    public void StartProgressWheelAtPosition(Vector3 position, float startTime, float finishThreshold)
    {
        if (_wheel.activeSelf)
            return;
        _wheel.SetActive(true);
        _wheel.transform.position = position;
        _wheel.GetComponent<Image>().fillAmount = 0f;
        _coroutine = StartCoroutine(RunWheel(startTime, finishThreshold));
        Debug.Log("ProgressWheel started at position: " + position);
    }

    //Deactivation of wheel when task is done
    public void DeactivateProgressWheel()
    {
        if(_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = null;
        if (_wheel.activeSelf)
            _wheel.SetActive(false);
    }

    private IEnumerator RunWheel(float startTime, float finishThreshold)
    {
        float timer = startTime;
        float progress = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            progress = Mathf.Lerp(0, 1, timer / finishThreshold);
            _wheel.GetComponent<Image>().fillAmount = progress;
            SetTime(timer);
            yield return null;
        }
    }

    public void SetTime(float timer, float finishThreshold)
    {
        _seconds.text = GetSeconds(timer, finishThreshold);
    }

    //Function for seconds calculation syntax
    private string GetSeconds(float timer, float finishThreshold)
    {
        float seconds = finishThreshold - timer;

        if (seconds < 0)
        {
            seconds = 0f;
        }

        string secondsInText = seconds.ToString("0.0");

        return secondsInText;
    }
}
