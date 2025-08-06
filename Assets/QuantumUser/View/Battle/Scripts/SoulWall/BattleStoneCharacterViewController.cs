/// @file BattleStoneCharacterViewController.cs
/// <summary>
/// Has a class BattleStoneCharacterViewController which handles stone characters visual functionality.
/// </summary>
///
/// This script:<br/>
/// Handles stone characters visual functionality.

using UnityEngine;
using Quantum;

namespace Battle.View
{
    /// <summary>
    /// <span class="brief-h">Stone character view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles stone characters visual functionality.
    /// </summary>
    public class BattleStoneCharacterViewController : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Array of the top stone character parts <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> references.</value>
        [SerializeField] private GameObject[] _topCharacterParts;

        /// <value>[SerializeField] Array of the bottom stone character parts <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a> references.</value>
        [SerializeField] private GameObject[] _bottomCharacterParts;

        /// <value>[SerializeField] Array of the top stone character emotion indicators <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SpriteRenderer.html">SpriteRenderer@u-exlink</a> references.</value>
        [SerializeField] private SpriteRenderer[] _topCharacterEmotionIndicators;

        /// <value>[SerializeField] Array of the bottom stone character emotion indicators <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SpriteRenderer.html">SpriteRenderer@u-exlink</a> references.</value>
        [SerializeField] private SpriteRenderer[] _bottomCharacterEmotionIndicators;

        /// <value>[SerializeField] Array of all the different emotion colors for emotion indicators.</value>
        [SerializeField] private Color[] _emotionColors;

        /// @}

        /// <summary>
        /// Destroys (deactivates) the character part which corresponds to the wall number and team.
        /// </summary>
        /// 
        /// <param name="wallNumber">The wall number which to destroy.</param>
        /// <param name="team">The BattleTeamNumber which character part to destroy.</param>
        public void DestroyCharacterPart(int wallNumber, BattleTeamNumber team)
        {
            switch (team)
            {
                case BattleTeamNumber.TeamAlpha:
                    _bottomCharacterParts[wallNumber].gameObject.SetActive(false);
                    break;
                case BattleTeamNumber.TeamBeta:
                    _topCharacterParts[wallNumber].gameObject.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// Sets new emotion color to emotion indicator which corresponds to the wall number and team.
        /// </summary>
        /// 
        /// <param name="wallNumber">The wall number which emotion indicator color to set.</param>
        /// <param name="team">The BattleTeamNumber which emotion indicator color to set.</param>
        /// <param name="emotionIndex">The new emotion color's index.</param>
        public void SetEmotionIndicator(int wallNumber, BattleTeamNumber team, int emotionIndex)
        {
            switch (team)
            {
                case BattleTeamNumber.TeamAlpha:
                    _bottomCharacterEmotionIndicators[wallNumber].color = _emotionColors[emotionIndex];
                    break;
                case BattleTeamNumber.TeamBeta:
                    _topCharacterEmotionIndicators[wallNumber].color = _emotionColors[emotionIndex];
                    break;
            }
        }
    }
}
