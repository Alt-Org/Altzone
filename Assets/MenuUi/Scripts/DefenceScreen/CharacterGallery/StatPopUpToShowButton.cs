using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
using UnityEngine.UI;
using UnityEngine;
using MenuUi.Scripts.Signals;

namespace MenuUi.Scripts.CharacterGallery
{
    [RequireComponent(typeof(Button))]
    public class StatPopUpToShowButton : MonoBehaviour
    {
        [SerializeField] SlotBase _characterSlot;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        protected void OnButtonClick()
        {
            if (_characterSlot is CharacterSlot charSlot && charSlot.IsUsed)
                return;

            SignalBus.OnDefenceGalleryStatPopupRequestedSignal(_characterSlot.Id);
        }
    }
}
