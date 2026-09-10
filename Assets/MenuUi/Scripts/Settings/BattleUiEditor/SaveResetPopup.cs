using System;
using System.Collections;
//using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    public class SaveReset : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;

        //[SerializeField] private TMP_Text _popupText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private BattleUiEditor _battleUiEditor;

        //private const string ResetChangesText = "Palauta UI-elementtien oletusasettelu?";

        private void OnDestroy()
        {
            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();
        }

        public IEnumerator ShowSaveResetPopup(Action<bool?> callback)
        {
            //_popupText.text = message;
            _contents.SetActive(true);

            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();

            bool? resetChanges = null;

            _okButton.onClick.AddListener(() => resetChanges = true);
            _noButton.onClick.AddListener(() => resetChanges = false);

            yield return new WaitUntil(() => resetChanges.HasValue || !_contents.activeSelf);

            CloseSaveResetPopup();

            callback(resetChanges);
        }

        public void CloseSaveResetPopup()
        {
            if (_contents.activeSelf) _contents.SetActive(false);
        }

        public void OnResetButtonClicked()
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowSaveResetPopup(resetChanges =>
            {
                //if (resetChanges == null) return;
                if (resetChanges == true) _battleUiEditor.ResetChanges();
            }));
        }
    }
}
