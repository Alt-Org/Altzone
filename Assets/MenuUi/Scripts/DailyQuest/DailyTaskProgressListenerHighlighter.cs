using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public abstract class DailyTaskProgressListenerHighlighter : DailyTaskProgressListener
{


    [Header("Highlighter Settings")]
    [SerializeField] protected bool _shouldHighlight = false;
    [SerializeField] protected List<RectTransform> _highlightedObjects;
    [SerializeField] protected float _scaleMultiplier = 1.2f;
    [SerializeField] protected float _highlightPulseDuration = 3.0f;
    [SerializeField] protected float _highlightPulseWaitTime = 2.0f;

    protected Coroutine _highlightRoutine;

    protected Vector2[] _originalHighlightObjectScales;


    public override void SetState(PlayerTask task) {
        base.SetState(task);


        StopHighlight();

        if (_shouldHighlight)
        {
            StartHighlight();
        }
    }

    protected void StartHighlight()
    {
        Debug.LogWarning("Starting highlight...");

        // Stop highlight if there is already another one on
        StopHighlight();

        _highlightRoutine = StartCoroutine(HighlightLoop());
    }

    protected void StopHighlight()
    {

        if (_highlightRoutine != null)
        {
            Debug.LogWarning("Stopping highlight...");
            StopCoroutine(_highlightRoutine);

            if (_highlightedObjects == null || _originalHighlightObjectScales == null) return;

            // Reset the highlighted object scales back to normal
            for (int i = 0; i < _highlightedObjects.Count; i++)
            {
                _highlightedObjects[i].localScale = _originalHighlightObjectScales[i];
            }
        }
    }

    protected IEnumerator HighlightLoop()
    {
        // Looping
        while (true)
        {
            Debug.LogWarning("Highlight loop");

            // Wait for a moment before doing the highlight pulse
            yield return new WaitForSeconds(_highlightPulseWaitTime);


            // Do highlight pulse and wait for it to finish
            yield return StartCoroutine(HighlightPulse());

        }
    }
    protected IEnumerator HighlightPulse()
    {
        Debug.LogWarning("HIGHLIGHT");
        float elapsed = 0f;

        _originalHighlightObjectScales = new Vector2[_highlightedObjects.Count];

        for (int i = 0; i < _highlightedObjects.Count; i++)
        {
            _originalHighlightObjectScales[i] = _highlightedObjects[i].localScale;
        }



        while (elapsed < _highlightPulseDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / _highlightPulseDuration;
            float curve = Mathf.Sin(progress * Mathf.PI);

            for (int i = 0; i < _highlightedObjects.Count; i++)
            {
                _highlightedObjects[i].localScale =
                    Vector2.Lerp(_originalHighlightObjectScales[i], _originalHighlightObjectScales[i] * _scaleMultiplier, curve);
            }

            yield return null;
        }
    }
}
