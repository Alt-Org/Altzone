using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class SoulHomeNaviButton : NaviButton
    {
        [SerializeField] private GameObject _popup;
        protected override void OnNaviButtonClick()
        {
            if (ServerManager.Instance.Clan == null)
            {
                _popup = transform.root.Find("MainMenuViewSwipe/Scroll View/Viewport/UIPanel/Interactables/InfoPopup").gameObject; // This needs better solution. Probably this should be held by a carrier script in MainManuViewSwipe.
                if (_popup != null) _popup.GetComponent<PopupController>().ActivatePopUp("Sinun pitää liittyä klaaniin päästäksesi sielunkotiin.");
                return;
            }

            base.OnNaviButtonClick();
        }
    }
}
