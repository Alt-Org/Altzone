using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DailyTaskSelectButtons : DailyTaskProgressListener
{
    private float _requiredHoldTime = 3f;
    private float _coolDownTime = 0.2f;

    private Dictionary<Button, List<EventTrigger.Entry>> _addedEntries = new();
    private HashSet<EventTrigger> _addedTriggers = new();

    public static event Action<Button> OnButtonSelected;

    // Adds listeners if the task is active at startup
    protected override void Start()
    {
        base.Start();

        if (On)
        {
            AddListeners();
        }
    }

    private void OnEnable()
    {
        SelectButtonsPopup.OnConfirm += ButtonsConfirmed;
    }

    private void OnDisable()
    {
        SelectButtonsPopup.OnConfirm -= ButtonsConfirmed;
    }

    // Adds or removes listeners depending on the task active state
    public override void SetState(PlayerTask task)
    {
        base.SetState(task);

        if (On)
        {
            AddListeners();
        }
        else
        {
            RemoveListeners();
        }
    }

    // Adds EventTrigger listeners to every button in the scene
    private void AddListeners()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button button in allButtons)
        {
            if (_addedEntries.ContainsKey(button)) continue;

            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
                _addedTriggers.Add(trigger);
            }

            var entries = new List<EventTrigger.Entry>();

            // Starts the hold coroutine when the button is pressed down
            var downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            downEntry.callback.AddListener((eventData) => StartCoroutine(ButtonHold(button)));
            trigger.triggers.Add(downEntry);
            entries.Add(downEntry);

            // Stops the hold coroutine and re-enables the button after a cooldown when the button is released
            var upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            upEntry.callback.AddListener((eventData) =>
            {
                StopAllCoroutines();
                if (button != null)
                {
                    StartCoroutine(ReEnableAfterCooldown(button));
                }
            });
            trigger.triggers.Add(upEntry);
            entries.Add(upEntry);

            _addedEntries[button] = entries;
        }
    }

    // Removes all the added EventTrigger listeners from buttons
    private void RemoveListeners()
    {
        foreach (var addedEntry in _addedEntries)
        {
            Button button = addedEntry.Key;
            List<EventTrigger.Entry> entries = addedEntry.Value;

            if (button != null)
            {
                EventTrigger trigger = button.GetComponent<EventTrigger>();
                if (trigger != null)
                {
                    foreach (var entry in entries)
                    {
                        trigger.triggers.Remove(entry);
                    }

                    if (_addedTriggers.Contains(trigger) && trigger.triggers.Count == 0)
                    {
                        Destroy(trigger);
                        _addedTriggers.Remove(trigger);
                    }
                }
            }
        }

        _addedEntries.Clear();
    }

    private IEnumerator ButtonHold(Button button)
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= _requiredHoldTime)
            {
                button.interactable = false;
                OnButtonSelected?.Invoke(button);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator ReEnableAfterCooldown(Button button)
    {
        yield return new WaitForSeconds(_coolDownTime);
        if (button != null) button.interactable = true;
    }

    private void ButtonsConfirmed()
    {
        UpdateProgress("1");
    }
}
