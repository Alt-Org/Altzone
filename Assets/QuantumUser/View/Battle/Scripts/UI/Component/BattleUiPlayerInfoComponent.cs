/// @file BattleUiPlayerInfoComponent.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiPlayerInfoComponent} class which holds references related to a BattleUiPlayerInfo prefab.
/// </summary>
///
/// This script:<br/>
/// Holds references related to a BattleUiPlayerInfo prefab.

// Unity usings
using UnityEngine;
using TMPro;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">PlayerInfo @uicomponentlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Holds references related to a BattleUiPlayerInfo prefab. Attached to BattleUiPlayerInfo prefab's <a href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject@u-exlink</a>.
    /// </summary>
    public class BattleUiPlayerInfoComponent : MonoBehaviour
    {
        /// <value>Reference to the <a href="https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html">TMP_Text@u-exlink</a> component which displays the player's name.</value>
        public TMP_Text PlayerName;

        /// <value>Array of BattleUiCharacterButtonComponent references which are attached to the character buttons of the BattleUiPlayerInfo prefab.</value>
        public BattleUiCharacterButtonComponent[] CharacterButtons;

        /// <value>Array of BattleUiCharacterFrameHandler references which are attached to the character buttons of the BattleUiPlayerInfo prefab.</value>
        public BattleUiCharacterFrameComponent[] FrameComponents;
    }
}
