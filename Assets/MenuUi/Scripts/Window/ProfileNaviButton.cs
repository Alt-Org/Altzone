using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Window;
using MenuUI.Scripts;
using UnityEditor;
using UnityEngine;

namespace MenuUi.Scripts.Window
{
    public class ProfileNaviButton : NaviButton
    {
        protected override void OnNaviButtonClick()
        {
            DataCarrier.AddData<PlayerData>(DataCarrier.PlayerProfile, null);
            base.OnNaviButtonClick();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(ProfileNaviButton))]
        public class ProfileNaviButtonEditor : NaviButtonEditor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
