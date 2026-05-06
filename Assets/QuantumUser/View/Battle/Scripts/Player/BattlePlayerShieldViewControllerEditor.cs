#if UNITY_EDITOR
/// @file BattlePlayerShieldViewControllerEditor.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerShieldViewControllerEditor} class which adds a default sprite
/// to the shields SpriteRenderer in the editor.
/// </summary>

// Unity usings
using UnityEditor;
using UnityEngine;

// Quantum usings
using Battle.QSimulation;

using SpriteSheetMap = Battle.View.Player.BattlePlayerCharacterViewController.SpriteSheetMap;

namespace Battle.View.Player
{
    /// <summary>
    /// Sets a default sprite to the shields SpriteRenderer in the Unity Editor.
    /// </summary>
    [CustomEditor(typeof(BattlePlayerShieldViewController))]
    public class BattlePlayerShieldViewControllerEditor : Editor
    {
        /// <summary>
        /// Override method that handles setting a default sprite to the shield SpriteRenderer in the Unity Editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            const SpriteSheetMap.Enum ErrorSpriteValue = SpriteSheetMap.Enum.Base;
            DrawDefaultInspector();

            if (_battleSpriteSheetProperty == null || _shieldSpriteRendererProperty == null || _spriteDisableProperty == null) return;

            BattleSpriteSheet spriteSheet = (BattleSpriteSheet)_battleSpriteSheetProperty.boxedValue;

            SpriteRenderer spriteRenderer = (SpriteRenderer)_shieldSpriteRendererProperty.boxedValue;

            bool spriteDisable = (bool)_spriteDisableProperty.boxedValue;

            int shieldNumber = (int)_shieldNumberProperty.boxedValue;

            if (!SpriteSheetMap.Validate(spriteSheet)) goto Error;

            SpriteSheetMap sprite = shieldNumber switch
            {
                0 => SpriteSheetMap.Enum.ShieldUp1,
                1 => SpriteSheetMap.Enum.ShieldUp2,
                2 => SpriteSheetMap.Enum.ShieldUp3,
                3 => SpriteSheetMap.Enum.ShieldUp4,

                _ => ErrorSpriteValue,
            };

            if (sprite == ErrorSpriteValue)
            {
                BattleDebugLogger.ErrorFormat(nameof(BattlePlayerShieldViewControllerEditor), "No valid shield sprite for shield number {0}.", BattleDebugLogger.LogTarget.UnityConsole, shieldNumber);
                goto Error;
            }

            if (spriteDisable) return;

            spriteRenderer.sprite = spriteSheet.GetSprite(sprite);
            return;

        Error:
            spriteRenderer.sprite = null;
        }

        /// <summary>
        /// Serialized property holding the spritesheet to get a default sprite from.
        /// </summary>
        private SerializedProperty _battleSpriteSheetProperty;

        /// <summary>
        /// Serialized property holding the gameobject to add the default sprite to.
        /// </summary>
        private SerializedProperty _shieldSpriteRendererProperty;

        /// <summary>
        /// Serialized property holding the shield number of the shield to add a default sprite to.
        /// </summary>
        private SerializedProperty _shieldNumberProperty;

        /// <summary>
        /// Serialized property holding the bool for whether to update sprites or not.
        /// </summary>
        private SerializedProperty _spriteDisableProperty;

        /// <summary>
        /// Handles initializing serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _battleSpriteSheetProperty = serializedObject.FindProperty("_spriteSheet");
            _shieldSpriteRendererProperty = serializedObject.FindProperty("_shieldSpriteRenderer");
            _shieldNumberProperty = serializedObject.FindProperty("_shieldNumber");
            _spriteDisableProperty = serializedObject.FindProperty("_autoSpriteDisable");
        }
    }
}
#endif
