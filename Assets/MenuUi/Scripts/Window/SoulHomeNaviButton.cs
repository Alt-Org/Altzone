using MenuUi.Scripts.Window.ScriptableObjects;
using MenuUI.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    public class SoulHomeNaviButton : NaviButton
    {
        [SerializeField] private GameObject _popup;
        [SerializeField] private bool _ignoreClanStatus = false;
        protected override void OnNaviButtonClick()
        {
            if (!(_ignoreClanStatus && (AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild))) {
                if (ServerManager.Instance.Clan == null)
                {
                    OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Error,"Sinun pitää liittyä klaaniin päästäksesi sielunkotiin.");
                    return;
                }
            }

            base.OnNaviButtonClick();
        }
    }
}
