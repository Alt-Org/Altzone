#if UNITY_EDITOR
/// @file BattlePlayerCharacterViewControllerEditor
/// <summary>
/// Contains @cref {Battle.View.Player,BattlePlayerCharacterViewControllerEditor} class which adds a default sprite
/// to every character body part's SpriteRenderer in the editor.
/// </summary>
using UnityEditor;
using UnityEngine;

namespace Battle.View.Player
{
    /// <summary>
    /// Adds a default sprite to every character body part's SpriteRenderer in the editor.
    /// </summary>
    [CustomEditor(typeof(BattlePlayerCharacterViewController))]
    public class BattlePlayerCharacterViewControllerEditor : Editor
    {
        /// <summary>
        /// Override method that handles adding a default sprite to every character body part's SpriteRenderer in the editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            const int SpriteRendererHeadIndex   = 0;
            const int SpriteRendererBodyIndex   = 1;
            const int SpriteRendererHandsIndex  = 2;
            const int SpriteRendererFeetIndex   = 3;
            const int SpriteRendererShadowIndex = 4;
            DrawDefaultInspector();

            if (_battleSpriteSheetProp == null || _gameObjectProp == null) return;

            BattleSpriteSheet spriteSheet = (BattleSpriteSheet)_battleSpriteSheetProp.boxedValue;

            for (int i = 0; i < _gameObjectProp.arraySize; i++)
            {
                SerializedProperty property = _gameObjectProp.GetArrayElementAtIndex(i);
                GameObject gameObject = (GameObject)property.objectReferenceValue;
                SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
                if (spriteSheet.Array.Length == 64)
                {
                    spriteRenderers[SpriteRendererHeadIndex].sprite = spriteSheet.GetSprite<BattlePlayerCharacterViewController.SpriteSheetMap>(BattlePlayerCharacterViewController.SpriteSheetMap.Enum.Head1);
                    spriteRenderers[SpriteRendererBodyIndex].sprite = spriteSheet.GetSprite<BattlePlayerCharacterViewController.SpriteSheetMap>(BattlePlayerCharacterViewController.SpriteSheetMap.Enum.Body1);
                    spriteRenderers[SpriteRendererHandsIndex].sprite = spriteSheet.GetSprite<BattlePlayerCharacterViewController.SpriteSheetMap>(BattlePlayerCharacterViewController.SpriteSheetMap.Enum.BaseHands);
                    spriteRenderers[SpriteRendererFeetIndex].sprite = spriteSheet.GetSprite<BattlePlayerCharacterViewController.SpriteSheetMap>(BattlePlayerCharacterViewController.SpriteSheetMap.Enum.BaseShoes);
                    spriteRenderers[SpriteRendererShadowIndex].sprite = spriteSheet.GetSprite<BattlePlayerCharacterViewController.SpriteSheetMap>(BattlePlayerCharacterViewController.SpriteSheetMap.Enum.Shadow);
                }
                else
                {
                    spriteRenderers[SpriteRendererHeadIndex].sprite = null;
                    spriteRenderers[SpriteRendererBodyIndex].sprite = null;
                    spriteRenderers[SpriteRendererHandsIndex].sprite = null;
                    spriteRenderers[SpriteRendererFeetIndex].sprite = null;
                    spriteRenderers[SpriteRendererShadowIndex].sprite = null;
                }
            }
        }
        /// <summary>
        /// Serialized property holding the spritesheet to get default sprites from.
        /// </summary>
        private SerializedProperty _battleSpriteSheetProp;

        /// <summary>
        /// Serialized property holding parent objects to get the body part SpriteRenderers from.
        /// </summary>
        private SerializedProperty _gameObjectProp;

        /// <summary>
        /// Handles getting the spritesheet and character gameobjects to add default sprites to.
        /// </summary>
        private void OnEnable()
        {
            _battleSpriteSheetProp = serializedObject.FindProperty("_spriteSheet");
            _gameObjectProp = serializedObject.FindProperty("_characterGameObjects");
        }
    }
}
#endif
