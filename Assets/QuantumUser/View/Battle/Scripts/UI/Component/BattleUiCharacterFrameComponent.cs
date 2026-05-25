/// @file BattleUiCharacterFrameComponent.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiCharacterFrameComponent} class which controls character icon frames.
/// </summary>

// System usings
using System;

// Unity usings
using UnityEngine;
using UnityEngine.UI;

// Quantum usings
using Quantum;

namespace Battle.View.UI
{
    /// <summary>
    /// Controls the class specific frames shown around character icons in Battle
    /// </summary>
    public class BattleUiCharacterFrameComponent : MonoBehaviour
    {
        /// @anchor BattleUiCharacterFrameComponent-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the Image component of the Frame.</summary>
        /// @ref BattleUiCharacterFrameComponent-SerializeFields
        [SerializeField] private Image _frameImage;

        /// <summary>[SerializeField] Reference to the Image component of the background.</summary>
        /// @ref BattleUiCharacterFrameComponent-SerializeFields
        [SerializeField] private Image _backgroundImage;

        /// <summary>[SerializeField] An array containing all the sprites and colors for each character classes frame.</summary>
        /// @ref BattleUiCharacterFrameComponent-SerializeFields
        [SerializeField] private FrameGraphics[] _frameGraphics;

        /// @}

        /// <summary>
        /// Sets the character frame on the icon this is attached to to the specified classes frame.
        /// </summary>
        ///
        /// <param name="characterClass">The class of the character that the frame should match.</param>
        public void SetCharacterFrame(BattlePlayerCharacterClass characterClass)
        {
            int index = characterClass switch
            {
                BattlePlayerCharacterClass.Class100     => 0,
                BattlePlayerCharacterClass.Class200     => 1,
                BattlePlayerCharacterClass.Class300     => 2,
                BattlePlayerCharacterClass.Class400     => 3,
                BattlePlayerCharacterClass.Class500     => 4,
                BattlePlayerCharacterClass.Class600     => 5,
                BattlePlayerCharacterClass.Class700     => 6,

                _ => -1,
            };

            if (index == -1) return;

            _frameImage.gameObject.SetActive(true);
            _frameImage.sprite = _frameGraphics[index].FrameSprite;
            _backgroundImage.color = _frameGraphics[index].FrameBackgroundColor;
        }

        /// <summary>
        /// Struct for more easily setting both the frame sprite and background color in the Unity inspector.
        /// </summary>
        [Serializable]
        private struct FrameGraphics
        {
            public Sprite FrameSprite;
            public Color FrameBackgroundColor;
        }
    }
}
