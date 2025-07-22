using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    public class OptionsPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _optionsContents;
        [SerializeField] private Button _resetButton;

        private const string ResetChangesText = "Palauta UI-elementtien oletusasettelu?";
        private BattleUiEditor _battleUiEditor;

        private void Awake()
        {
            _resetButton.onClick.AddListener(OnResetButtonClicked);

            CloseOptionsDropdown();
        }

        private void OnDestroy()
        {
            _resetButton.onClick.RemoveAllListeners();
        }

        public void Initialize(BattleUiEditor battleUiEditor)
        {
            _battleUiEditor = battleUiEditor;
        }

        public void ToggleOptionsDropdown()
        {
            if (_optionsContents.activeSelf)
            {
                CloseOptionsDropdown();
            }
            else
            {
                OpenOptionsDropdown();
            }
        }

        public void OpenOptionsDropdown()
        {
            _battleUiEditor.OnUiElementSelected(null);
            _optionsContents.SetActive(true);
        }

        public void CloseOptionsDropdown()
        {
            _optionsContents.SetActive(false);
        }

        private void OnResetButtonClicked()
        {
            _battleUiEditor.StartCoroutine(_battleUiEditor.ShowSaveResetPopup(ResetChangesText, resetChanges =>
            {
                if (resetChanges == null) return;
                if (resetChanges.Value == true) _battleUiEditor.ResetChanges();
            }));
        }
    }
}
