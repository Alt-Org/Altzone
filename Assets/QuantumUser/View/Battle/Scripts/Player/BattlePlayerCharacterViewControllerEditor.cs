#if UNITY_EDITOR
/// @file BattlePlayerCharacterViewControllerEditor.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerCharacterViewControllerEditor} class which adds a default sprite
/// to every character body part's SpriteRenderer in the editor.
/// </summary>

// Unity usings
using UnityEditor;
using UnityEngine;

using SpriteSheetMap = Battle.View.Player.BattlePlayerCharacterViewController.SpriteSheetMap;

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

                _spriteRenderers[SpriteRendererHeadIndex]   = gameObject.transform.Find("Head")   .GetComponent<SpriteRenderer>();
                _spriteRenderers[SpriteRendererBodyIndex]   = gameObject.transform.Find("Body")   .GetComponent<SpriteRenderer>();
                _spriteRenderers[SpriteRendererHandsIndex]  = gameObject.transform.Find("Hands")  .GetComponent<SpriteRenderer>();
                _spriteRenderers[SpriteRendererFeetIndex]   = gameObject.transform.Find("Feet")   .GetComponent<SpriteRenderer>();
                _spriteRenderers[SpriteRendererShadowIndex] = gameObject.transform.Find("Shadow") .GetComponent<SpriteRenderer>();
                if (SpriteSheetMap.Validate(spriteSheet))
                {
                    _spriteRenderers[SpriteRendererHeadIndex]   .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.Head1);
                    _spriteRenderers[SpriteRendererBodyIndex]   .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.Body1);
                    _spriteRenderers[SpriteRendererHandsIndex]  .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.BaseHands);
                    _spriteRenderers[SpriteRendererFeetIndex]   .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.BaseShoes);
                    _spriteRenderers[SpriteRendererShadowIndex] .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.Shadow);
                }
                else
                {
                    _spriteRenderers[SpriteRendererHeadIndex]   .sprite = null;
                    _spriteRenderers[SpriteRendererBodyIndex]   .sprite = null;
                    _spriteRenderers[SpriteRendererHandsIndex]  .sprite = null;
                    _spriteRenderers[SpriteRendererFeetIndex]   .sprite = null;
                    _spriteRenderers[SpriteRendererShadowIndex] .sprite = null;
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

        private SpriteRenderer[] _spriteRenderers = new SpriteRenderer[5];

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
