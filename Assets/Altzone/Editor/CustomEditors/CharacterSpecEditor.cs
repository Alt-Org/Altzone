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
        SerializedProperty IsApproved;
        SerializedProperty Hp;
        SerializedProperty Speed;
        SerializedProperty CharacterSize;
        SerializedProperty Attack;
        SerializedProperty Defence;
        SerializedProperty GalleryImage;
        SerializedProperty GalleryHeadImage;
        SerializedProperty BattleSprite;

        private void OnEnable()
        {
            IsApproved = serializedObject.FindProperty(nameof(CharacterSpec.IsApproved));
            Hp = serializedObject.FindProperty(nameof(CharacterSpec.Hp));
            Speed = serializedObject.FindProperty(nameof(CharacterSpec.Speed));
            CharacterSize = serializedObject.FindProperty(nameof(CharacterSpec.CharacterSize));
            Attack = serializedObject.FindProperty(nameof(CharacterSpec.Attack));
            Defence = serializedObject.FindProperty(nameof(CharacterSpec.Defence));
            GalleryImage = serializedObject.FindProperty(nameof(CharacterSpec.GalleryImage));
            GalleryHeadImage = serializedObject.FindProperty(nameof(CharacterSpec.GalleryHeadImage));
            BattleSprite = serializedObject.FindProperty(nameof(CharacterSpec.BattleSprite));
        }
        public override void OnInspectorGUI()
        {
            CharacterSpec script = (CharacterSpec)target;
            DrawEditor(script);
            //DrawDefaultInspector();

            ReadOnlyCollection<PlayerCharacterPrototype> characters =
                (ReadOnlyCollection<PlayerCharacterPrototype>) CharacterSpecConfig.Instance.Prototypes;

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

        private void DrawEditor(CharacterSpec script)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Character Basic Data", EditorStyles.boldLabel);
            script.Id = EditorGUILayout.TextField("Id", script.Id);
            script.CharacterId = (CharacterID)EditorGUILayout.EnumPopup("Character Id" , script.CharacterId);
            EditorGUILayout.PropertyField(IsApproved);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General Attributes", EditorStyles.boldLabel);
            script.Name = EditorGUILayout.TextField("Name", script.Name);
            script.ClassType = (CharacterClassID)EditorGUILayout.EnumPopup("Character Type", script.ClassType);
            EditorGUILayout.LabelField("Character Description");
            GUIStyle style = EditorStyles.textField;
            style.wordWrap = true;
            script.CharacterDescription = EditorGUILayout.TextArea(script.CharacterDescription, style);
            script.CharacterShortDescription = EditorGUILayout.TextField("Character Short Description", script.CharacterShortDescription);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Special Attributes", EditorStyles.boldLabel);
            script.CharacterStats = EditorGUILayout.ObjectField("Character Stats", script.CharacterStats, typeof(BaseCharacter),false) as BaseCharacter;
            if (script.CharacterStats != null)
            {
                EditorGUILayout.PropertyField(Hp);
                EditorGUILayout.PropertyField(Speed);
                EditorGUILayout.PropertyField(CharacterSize);
                EditorGUILayout.PropertyField(Attack);
                EditorGUILayout.PropertyField(Defence);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(GalleryImage);
            EditorGUILayout.PropertyField(GalleryHeadImage);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(BattleSprite);

            EditorGUILayout.LabelField("");
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
