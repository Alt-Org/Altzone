using System;
using System.Collections.ObjectModel;
using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ModelV2.Internal;
using UnityEditor;
using UnityEngine;

namespace Altzone.Editor.CustomEditors
{
    [CustomEditor(typeof(CharacterSpec)), CanEditMultipleObjects]
    public class CharacterSpecEditor : UnityEditor.Editor
    {
        private CharacterID _prevID = CharacterID.None;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ReadOnlyCollection<PlayerCharacterPrototype> characters =
                (ReadOnlyCollection<PlayerCharacterPrototype>) CharacterSpecConfig.Instance.Prototypes;

            CharacterSpec script = (CharacterSpec)target;

            if (_prevID != script.CharacterId)
            {
                _prevID = script.CharacterId;
                script.Id = ((int)script.CharacterId).ToString();
            }

            if (int.TryParse(script.Id, out int result) && Enum.IsDefined(typeof(CharacterID), result))
            {
                script.CharacterId = (CharacterID)int.Parse(script.Id);
                _prevID = script.CharacterId;
            }

            /*if (PlayerCharacters.GetCharacter(((int)script.CharacterId).ToString()) == null)
            {
                script.Id = ((int)script.CharacterId).ToString();
            }
            else
            {
                script.CharacterId = 0;
            }*/
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
