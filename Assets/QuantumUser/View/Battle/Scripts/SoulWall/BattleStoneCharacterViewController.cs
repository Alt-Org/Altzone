/// @file BattleStoneCharacterViewController.cs
/// <summary>
/// Handles StoneCharacter graphics.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace Battle.View
{
    /// <summary>
    /// %StoneCharacters' entityview script.<br/>
    /// Handles %StoneCharacters' sprites and emotion colors.
    /// </summary>
    public class BattleStoneCharacterViewController : MonoBehaviour
    {
        /// <value>[SerializeField] An array of the individual destroyable parts of the top StoneCharacter.</value>
        [SerializeField] private GameObject[] _topCharacterParts;

        /// <value>[SerializeField] An array of the individual destroyable parts of the bottom StoneCharacter.</value>
        [SerializeField] private GameObject[] _bottomCharacterParts;

        /// <value>[SerializeField] An array of the emotion color indicator SpriteRenderers for the top StoneCharacter.</value>
        [SerializeField] private SpriteRenderer[] _topCharacterEmotionIndicators;

        /// <value>[SerializeField] An array of the emotion color indicator SpriteRenderers for the bottom StoneCharacter.</value>
        [SerializeField] private SpriteRenderer[] _bottomCharacterEmotionIndicators;

        /// <value>[SerializeField] An array of the possible emotion colors.</value>
        [SerializeField] private Color[] _emotionColors;

        /// <summary>
        /// Public method that is called when the projectile collides with a hitbox linked to a StoneCharacter. <br/>
        /// Hides the specified part.
        /// </summary>
        /// <param name="wallNumber">The number of the character part to be destroyed.</param>
        /// <param name="team">The team of the part to be destroyed.</param>
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
        /// Public method that is called when the StoneCharacters are initialized. <br/>
        /// Sets the color indicator for the specified part to the specified emotion.
        /// </summary>
        /// <param name="wallNumber">The number of the part to be affected.</param>
        /// <param name="team">The team of the part to be affected.</param>
        /// <param name="emotionIndex">The emotion that the part's color indicator should match.</param>
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
