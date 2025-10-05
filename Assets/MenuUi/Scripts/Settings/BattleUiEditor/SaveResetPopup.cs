using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    public class SaveReset : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;
        [SerializeField] private TMP_Text _popupText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private BattleUiEditor _battleUiEditor;

        private const string ResetChangesText = "Palauta UI-elementtien oletusasettelu?";

        private void OnDestroy()
        {
            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();
        }

        public IEnumerator ShowSaveResetPopup(string message, Action<bool?> callback)
        {
            _popupText.text = message;
            _contents.SetActive(true);

            _okButton.onClick.RemoveAllListeners();
            _noButton.onClick.RemoveAllListeners();

            bool? saveChanges = null;

            _okButton.onClick.AddListener(() => saveChanges = true);
            _noButton.onClick.AddListener(() => saveChanges = false);

            yield return new WaitUntil(() => saveChanges.HasValue || !_contents.activeSelf);

            CloseSaveResetPopup();

            callback(saveChanges);
        }

        public void CloseSaveResetPopup()
        {
            if (_contents.activeSelf) _contents.SetActive(false);
        }

        public void OnResetButtonClicked()
        {
            StartCoroutine(ShowSaveResetPopup(ResetChangesText, resetChanges =>
            {
                if (resetChanges == null) return;
                if (resetChanges.Value == true) _battleUiEditor.ResetChanges();
            }));
        }
    }
}
