using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        SerializedProperty CharPhotoSeries;
        SerializedProperty BattleEntityPrototype;
        SerializedProperty BattleUiSprite;

        int _prevHp = 0;
        int _prevSpeed = 0;
        int _prevCharSize = 0;
        int _prevAttack = 0;
        int _prevDefence = 0;

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
            CharPhotoSeries= serializedObject.FindProperty(nameof(CharacterSpec.CharPhotoSeries));
            BattleEntityPrototype = serializedObject.FindProperty(nameof(CharacterSpec.BattleEntityPrototype));
            BattleUiSprite = serializedObject.FindProperty(nameof(CharacterSpec.BattleUiSprite));
        }
        public override void OnInspectorGUI()
        {
            CharacterSpec script = (CharacterSpec)target;
            serializedObject.Update();
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

            if (script.CharacterStats == null || script.CharacterStats.Id != script.CharacterId)
            {
                script.CharacterStats = CharacterStorage.Instance.CharacterList.FirstOrDefault(x => x.Id == script.CharacterId);
            }
            if (script.CharacterStats != null)
            {
                script.Hp.Level = script.CharacterStats.DefaultHp;
                script.Hp.Coefficient = script.CharacterStats.HpStrength;
                script.Speed.Level = script.CharacterStats.DefaultSpeed;
                script.Speed.Coefficient = script.CharacterStats.SpeedStrength;
                script.CharacterSize.Level = script.CharacterStats.DefaultCharacterSize;
                script.CharacterSize.Coefficient = script.CharacterStats.CharacterSizeStrength;
                script.Attack.Level = script.CharacterStats.DefaultAttack;
                script.Attack.Coefficient = script.CharacterStats.AttackStrength;
                script.Defence.Level = script.CharacterStats.DefaultDefence;
                script.Defence.Coefficient = script.CharacterStats.DefenceStrength;
            }

            /*if (PlayerCharacters.GetCharacter(((int)script.CharacterId).ToString()) == null)
            {
                script.Id = ((int)script.CharacterId).ToString();
            }
            else
            {
                script.CharacterId = 0;
            }*/
            _prevHp = script.Hp.Level;
            _prevSpeed = script.Speed.Level;
            _prevCharSize = script.CharacterSize.Level;
            _prevAttack = script.Attack.Level;
            _prevDefence = script.Defence.Level;
            serializedObject.ApplyModifiedProperties();
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
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Finnish");
            script.FinnishName = EditorGUILayout.TextField("Name", script.FinnishName);
            EditorGUILayout.LabelField("Character Description");
            GUIStyle style = EditorStyles.textField;
            style.wordWrap = true;
            script.CharacterDescriptionFinnish = EditorGUILayout.TextArea(script.CharacterDescriptionFinnish, style);
            script.CharacterShortDescriptionFinnish = EditorGUILayout.TextField("Character Short Description", script.CharacterShortDescriptionFinnish);
            EditorGUILayout.LabelField("Character Ability Description");
            script.CharacterAbilityDescriptionFinnish = EditorGUILayout.TextField(script.CharacterAbilityDescriptionFinnish, style);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("English");
            script.EnglishName = EditorGUILayout.TextField("Name", script.EnglishName);
            EditorGUILayout.LabelField("Character Description");
            script.CharacterDescriptionEnglish = EditorGUILayout.TextArea(script.CharacterDescriptionEnglish, style);
            script.CharacterShortDescriptionEnglish = EditorGUILayout.TextField("Character Short Description", script.CharacterShortDescriptionEnglish);
            EditorGUILayout.LabelField("Character Ability Description");
            script.CharacterAbilityDescriptionEnglish = EditorGUILayout.TextArea(script.CharacterAbilityDescriptionEnglish, style);
            EditorGUILayout.Space();
            script.ClassType = (CharacterClassType)EditorGUILayout.EnumPopup("Character Type", script.ClassType);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Special Attributes", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            script.CharacterStats = EditorGUILayout.ObjectField("Character Stats", script.CharacterStats, typeof(BaseCharacter),false) as BaseCharacter;
            EditorGUI.EndDisabledGroup();
            if (script.CharacterStats != null)
            {
                EditorGUILayout.PropertyField(Hp);
                if(_prevHp != script.Hp.Level && _prevHp > 0) script.CharacterStats.DefaultHp = script.Hp.Level;
                EditorGUILayout.PropertyField(Speed);
                if (_prevSpeed != script.Speed.Level && _prevSpeed> 0) script.CharacterStats.DefaultSpeed = script.Speed.Level;
                EditorGUILayout.PropertyField(CharacterSize);
                if (_prevCharSize != script.CharacterSize.Level && _prevCharSize > 0) script.CharacterStats.DefaultCharacterSize = script.CharacterSize.Level;
                EditorGUILayout.PropertyField(Attack);
                if (_prevAttack != script.Attack.Level && _prevAttack > 0) script.CharacterStats.DefaultAttack = script.Attack.Level;
                EditorGUILayout.PropertyField(Defence);
                if (_prevDefence != script.Defence.Level && _prevDefence > 0) script.CharacterStats.DefaultDefence = script.Defence.Level;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(GalleryImage);
            EditorGUILayout.PropertyField(GalleryHeadImage);
            EditorGUILayout.PropertyField(CharPhotoSeries);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(BattleEntityPrototype);
            EditorGUILayout.PropertyField(BattleUiSprite);

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
