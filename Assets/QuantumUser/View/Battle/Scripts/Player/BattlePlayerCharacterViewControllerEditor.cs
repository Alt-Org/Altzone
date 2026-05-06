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
    /// Sets a default sprite to every character body part's SpriteRenderer in the Unity Editor.
    /// </summary>
    ///
    [CustomEditor(typeof(BattlePlayerCharacterViewController))]
    public class BattlePlayerCharacterViewControllerEditor : Editor
    {
        /// <summary>
        /// Override method that handles setting a default sprite to every character body part's SpriteRenderer in the Unity Editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            const int SpriteRendererHeadIndex   = 0;
            const int SpriteRendererBodyIndex   = 1;
            const int SpriteRendererHandsIndex  = 2;
            const int SpriteRendererFeetIndex   = 3;
            const int SpriteRendererShadowIndex = 4;

            DrawDefaultInspector();

            if (_battleSpriteSheetProperty == null || _characterGameObjectsProperty == null || _spriteDisableProperty == null) return;

            BattleSpriteSheet spriteSheet = (BattleSpriteSheet)_battleSpriteSheetProperty.boxedValue;

            bool spriteDisable = (bool)_spriteDisableProperty.boxedValue;

            if (spriteDisable) return;

            for (int i = 0; i < _characterGameObjectsProperty.arraySize; i++)
            {
                SerializedProperty property = _characterGameObjectsProperty.GetArrayElementAtIndex(i);
                GameObject gameObject = (GameObject)property.objectReferenceValue;

                _bodyPartSpriteRenderers[SpriteRendererHeadIndex]   = gameObject.transform.Find("Head")   .GetComponent<SpriteRenderer>();
                _bodyPartSpriteRenderers[SpriteRendererBodyIndex]   = gameObject.transform.Find("Body")   .GetComponent<SpriteRenderer>();
                _bodyPartSpriteRenderers[SpriteRendererHandsIndex]  = gameObject.transform.Find("Hands")  .GetComponent<SpriteRenderer>();
                _bodyPartSpriteRenderers[SpriteRendererFeetIndex]   = gameObject.transform.Find("Feet")   .GetComponent<SpriteRenderer>();
                _bodyPartSpriteRenderers[SpriteRendererShadowIndex] = gameObject.transform.Find("Shadow") .GetComponent<SpriteRenderer>();

                if (SpriteSheetMap.Validate(spriteSheet))
                {
                    _bodyPartSpriteRenderers[SpriteRendererHeadIndex]   .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.Head1);
                    _bodyPartSpriteRenderers[SpriteRendererBodyIndex]   .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.Body1);
                    _bodyPartSpriteRenderers[SpriteRendererHandsIndex]  .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.BaseHands);
                    _bodyPartSpriteRenderers[SpriteRendererFeetIndex]   .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.BaseShoes);
                    _bodyPartSpriteRenderers[SpriteRendererShadowIndex] .sprite = spriteSheet.GetSprite<SpriteSheetMap>(SpriteSheetMap.Enum.Shadow);
                }
                else
                {
                    _bodyPartSpriteRenderers[SpriteRendererHeadIndex]   .sprite = null;
                    _bodyPartSpriteRenderers[SpriteRendererBodyIndex]   .sprite = null;
                    _bodyPartSpriteRenderers[SpriteRendererHandsIndex]  .sprite = null;
                    _bodyPartSpriteRenderers[SpriteRendererFeetIndex]   .sprite = null;
                    _bodyPartSpriteRenderers[SpriteRendererShadowIndex] .sprite = null;
                }
            }
        }
        /// <summary>
        /// Serialized property holding the spritesheet to get default sprites from.
        /// </summary>
        private SerializedProperty _battleSpriteSheetProperty;

        /// <summary>
        /// Serialized property holding parent objects to get the body part SpriteRenderers from.
        /// </summary>
        private SerializedProperty _characterGameObjectsProperty;

        /// <summary>
        /// Array that holds the SpriteRenderer components of each body part gameobject.
        /// </summary>
        private readonly SpriteRenderer[] _bodyPartSpriteRenderers = new SpriteRenderer[5];

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
            _characterGameObjectsProperty = serializedObject.FindProperty("_characterGameObjects");
            _spriteDisableProperty = serializedObject.FindProperty("_autoSpriteDisable");
        }
    }
}
#endif
