using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Altzone.Scripts.Language;
using TMPro;

public class ChatShowUsersPopUpData : MonoBehaviour
{

    [SerializeField] private GameObject _containers;
    [SerializeField] private TextMeshProUGUI ReactionAmounText;
    //[SerializeField] private TextLanguageSelectorCaller _reactionAmount;
    private void Start()
    {
        reactiontext();
    }
    void OnEnable()
    {
        reactiontext();
    }
    // Start is called before the first frame update
    void reactiontext()
    {
        int activeChildren = 0;
        foreach (Transform container in _containers.transform)
        {
            if (container.gameObject.activeInHierarchy)
            {
                activeChildren++;
            }

        }

        ReactionAmounText.text = $"{activeChildren} reaktiota";

        //_reactionAmount.SetText(SettingsCarrier.Instance.Language, new string[1] { activeChildren.ToString() });
    }
}
