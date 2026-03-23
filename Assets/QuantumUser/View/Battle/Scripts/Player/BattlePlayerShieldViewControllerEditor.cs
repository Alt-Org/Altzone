#if UNITY_EDITOR
/// @file BattlePlayerShieldViewControllerEditor
/// <summary>
/// Contains @cref {Battle.View.Player,BattlePlayerShieldViewControllerEditor} class which adds a default sprite
/// to the shields SpriteRenderer in the editor.
/// </summary>
using Battle.QSimulation;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Battle.View.Player
{
    /// <summary>
    /// Adds a default sprite to the shields SpriteRenderer in the editor.
    /// </summary>
    [CustomEditor(typeof(BattlePlayerShieldViewController))]
    public class BattlePlayerShieldViewControllerEditor : Editor
    {
        /// <summary>
        /// Override method that handles adding a default sprite to the shield SpriteRenderer.
        /// </summary>
        public override void OnInspectorGUI()
        {
            const BattlePlayerCharacterViewController.SpriteSheetMap.Enum errorSpriteValue = BattlePlayerCharacterViewController.SpriteSheetMap.Enum.Base;
            DrawDefaultInspector();

            if (_battleSpriteSheetProp == null || _gameObjectProp == null) return;

            BattleSpriteSheet spriteSheet = (BattleSpriteSheet)_battleSpriteSheetProp.boxedValue;

            GameObject gameObject = (GameObject)_gameObjectProp.boxedValue;

            int shieldNumber = (int)_shieldNumber.boxedValue;

            if (spriteSheet.Array.Length != BattlePlayerCharacterViewController.SpriteSheetMap.Count)
            {
                BattleDebugLogger.ErrorFormat(nameof(BattlePlayerShieldViewControllerEditor), "Invalid number of sprites in spriteSheet\nCount: {0}\nExpected count: {1}", BattleDebugLogger.LogTarget.UnityConsole,
                    spriteSheet.Array.Length, BattlePlayerCharacterViewController.SpriteSheetMap.Count
                );
                goto Error;
            }

            BattlePlayerCharacterViewController.SpriteSheetMap sprite = shieldNumber switch
            {
                0 => BattlePlayerCharacterViewController.SpriteSheetMap.Enum.ShieldUp1,
                1 => BattlePlayerCharacterViewController.SpriteSheetMap.Enum.ShieldUp2,
                2 => BattlePlayerCharacterViewController.SpriteSheetMap.Enum.ShieldUp3,
                3 => BattlePlayerCharacterViewController.SpriteSheetMap.Enum.ShieldUp4,

                _ => errorSpriteValue,
            };

            if (sprite == errorSpriteValue)
            {
                BattleDebugLogger.ErrorFormat(nameof(BattlePlayerShieldViewControllerEditor), "No valid shield sprite for shield number {0}.", BattleDebugLogger.LogTarget.UnityConsole, shieldNumber);
                goto Error;
            }

            gameObject.GetComponent<SpriteRenderer>().sprite = spriteSheet.GetSprite(sprite);
            return;

        Error:
            gameObject.GetComponent<SpriteRenderer>().sprite = null;
        }

        /// <summary>
        /// Serialized property holding the spritesheet to get a default sprite from.
        /// </summary>
        private SerializedProperty _battleSpriteSheetProp;

        /// <summary>
        /// Serialized property holding the gameobject to add the default sprite to.
        /// </summary>
        private SerializedProperty _gameObjectProp;

        private SerializedProperty _shieldNumber;

        /// <summary>
        /// Handles getting the spritesheet and shield gameobject to add a default sprite to.
        /// </summary>
        private void OnEnable()
        {
            _battleSpriteSheetProp = serializedObject.FindProperty("_spriteSheet");
            _gameObjectProp = serializedObject.FindProperty("_shieldGameObject");
            _shieldNumber = serializedObject.FindProperty("_shieldNumber");
        }
    }
}
#endif
