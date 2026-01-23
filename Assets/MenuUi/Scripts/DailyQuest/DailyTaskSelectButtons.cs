using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DailyTaskSelectButtons : DailyTaskProgressListener
{
    [SerializeField] private bool _limitToThis = false;
    [SerializeField] private List<SelectButtonObject> _limitedButtons;
    private float _requiredHoldTime = 3f;
    private float _coolDownTime = 0.2f;

    private Dictionary<SelectButtonObject, List<EventTrigger.Entry>> _addedEntries = new();
    private HashSet<EventTrigger> _addedTriggers = new();

    public static event Action<SelectButtonObject> OnButtonSelected;

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

    public void AddButton(SelectButtonObject button)
    {
        if (_limitToThis)
        {
            _limitedButtons.Add(button);
        }
    }

    public void ResetList()
    {
        if (_limitToThis)
        {
            _limitedButtons.Clear();
        }
    }

    public void RefreshListeners()
    {
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
        SelectButtonObject[] allButtonObjects;
        if (!_limitToThis)
        {
            Button[] allButtons = FindObjectsOfType<Button>(true);
            List<SelectButtonObject> allButtonObjectsList = new();
            foreach ( Button button in allButtons)
            {
                allButtonObjectsList.Add(new(button, button.GetComponent<Image>()));
            }
            allButtonObjects = allButtonObjectsList.ToArray();
        }
        else allButtonObjects = _limitedButtons.ToArray();
        foreach (SelectButtonObject buttonObject in allButtonObjects)
        {
            if (_addedEntries.ContainsKey(buttonObject)) continue;

            EventTrigger trigger = buttonObject.Button.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = buttonObject.Button.gameObject.AddComponent<EventTrigger>();
                _addedTriggers.Add(trigger);
            }

            var entries = new List<EventTrigger.Entry>();

            // Starts the hold coroutine when the button is pressed down
            var downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            downEntry.callback.AddListener((eventData) => StartCoroutine(ButtonHold(buttonObject)));
            trigger.triggers.Add(downEntry);
            entries.Add(downEntry);

            // Stops the hold coroutine and re-enables the button after a cooldown when the button is released
            var upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            upEntry.callback.AddListener((eventData) =>
            {
                StopAllCoroutines();
                if (buttonObject?.Button != null)
                {
                    StartCoroutine(ReEnableAfterCooldown(buttonObject));
                }
            });
            trigger.triggers.Add(upEntry);
            entries.Add(upEntry);

            _addedEntries[buttonObject] = entries;
        }
    }

    // Removes all the added EventTrigger listeners from buttons
    private void RemoveListeners()
    {
        foreach (var addedEntry in _addedEntries)
        {
            SelectButtonObject button = addedEntry.Key;
            List<EventTrigger.Entry> entries = addedEntry.Value;

            if (button != null)
            {
                EventTrigger trigger = button.Button.GetComponent<EventTrigger>();
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

    private IEnumerator ButtonHold(SelectButtonObject button)
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= _requiredHoldTime)
            {
                button.Button.interactable = false;
                OnButtonSelected?.Invoke(button);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator ReEnableAfterCooldown(SelectButtonObject button)
    {
        yield return new WaitForSeconds(_coolDownTime);
        if (button != null) button.Button.interactable = true;
    }

    private void ButtonsConfirmed()
    {
        UpdateProgress("1");
    }

    [Serializable]
    public class SelectButtonObject
    {
        public Button Button;
        public Image Image;

        public SelectButtonObject(Button button, Image image)
        {
            Button = button;
            Image = image;
        }
    }
}
