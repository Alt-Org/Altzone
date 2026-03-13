#if UNITY_EDITOR
/// @file BattlePlayerShieldViewControllerEditor
/// <summary>
/// Contains @cref {Battle.View.Player,BattlePlayerShieldViewControllerEditor} class which adds a default sprite
/// to the shields SpriteRenderer in the editor.
/// </summary>
using UnityEditor;
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
            DrawDefaultInspector();

            if (_battleSpriteSheetProp == null || _gameObjectProp == null) return;

            BattleSpriteSheet spriteSheet = (BattleSpriteSheet)_battleSpriteSheetProp.boxedValue;

            GameObject gameObject = (GameObject)_gameObjectProp.boxedValue;

            if (spriteSheet.Array.Length == 64)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = spriteSheet.GetSprite<BattlePlayerCharacterViewController.SpriteSheetMap>(BattlePlayerCharacterViewController.SpriteSheetMap.Enum.ShieldUp1);
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = null;
            }
        }

        /// <summary>
        /// Serialized property holding the spritesheet to get a default sprite from.
        /// </summary>
        private SerializedProperty _battleSpriteSheetProp;

        /// <summary>
        /// Serialized property holding the gameobject to add the default sprite to.
        /// </summary>
        private SerializedProperty _gameObjectProp;

        /// <summary>
        /// Handles getting the spritesheet and shield gameobject to add a default sprite to.
        /// </summary>
        private void OnEnable()
        {
            _battleSpriteSheetProp = serializedObject.FindProperty("_spriteSheet");
            _gameObjectProp = serializedObject.FindProperty("_shieldGameObject");
        }
    }
}
#endif
