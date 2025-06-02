/// @file BattleUiGameOverHandler.cs
/// <summary>
/// Has a class BattleUiDiamondsHandler which handles setting collected diamond amount text.
/// </summary>
///
/// This script:<br/>
/// Handles setting collected diamond amount text.

using UnityEngine;
using TMPro;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">Diamonds @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles setting collected diamond amount text.
    /// </summary>
    public class BattleUiDiamondsHandler : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to the BattleUiMovableElement script which is attached to a BattleUiDiamonds prefab.</value>
        [SerializeField] private BattleUiMovableElement _movableUiElement;

        /// <value>[SerializeField] Reference to the TMP_Text component which the diamond amount text is set to.</value>
        [SerializeField] private TMP_Text _diamondText;

        /// @}
        
        /// <value>Is the %UI element visible or not.</value>
        public bool IsVisible => MovableUiElement.gameObject.activeSelf;

        /// <value>Public getter for #_movableUiElement.</value>
        public BattleUiMovableElement MovableUiElement => _movableUiElement;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            MovableUiElement.gameObject.SetActive(show);
        }

        /// <summary>
        /// Sets the diamonds amount to the #_diamondText.
        /// </summary>
        /// <param name="diamondAmount">The current diamond amount as integer.</param>
        public void SetDiamondsText(int diamondAmount)
        {
            _diamondText.text = diamondAmount.ToString();
        }
    }
}
