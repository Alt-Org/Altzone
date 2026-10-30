using System;
using System.Linq;
using Prg;
using Prg.Localization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if PRG_DEBUG
using Debug = Prg.Debug;
#else
using Debug = UnityEngine.Debug;
#endif

namespace Editor.Prg.Localization
{
    /// <summary>
    /// Custom Editor for <c>Localized</c> text component.<br />
    /// Localization key and default value can be set here, in addition to runtime details.
    /// </summary>
    [CustomEditor(typeof(Localized)), CanEditMultipleObjects]
    public class LocalizedEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var bgColorBefore = GUI.backgroundColor;
            if (serializedObject.isEditingMultipleObjects)
            {
                GUI.backgroundColor = Color.magenta;
                if (GUILayout.Button($"Update All ({targets.Length})"))
                {
                    foreach (var targetObject in targets)
                    {
                        if (targetObject is not Localized localized)
                        {
                            continue;
                        }
                        var parent = localized.gameObject;
                        if (Update(parent, localized) != 0)
                        {
                            EditorUtility.SetDirty(parent);
                            EditorUtility.SetDirty(targetObject);
                            Debug.Log($"CHANGED {localized}");
                        }
                    }
                }
                GUI.backgroundColor = bgColorBefore;
                GUILayout.Space(20);
                DrawDefaultInspector();
                return;
            }
            GUILayout.Space(20);
            if (GetComponentType(serializedObject) == LocalizedComponentType.Unknown)
            {
                if (Update(Selection.activeGameObject, serializedObject.targetObject as Localized) != 0)
                {
                    EditorUtility.SetDirty(Selection.activeGameObject);
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    Debug.Log($"CHANGED {serializedObject.targetObject}");
                }
            }
            if (GUILayout.Button("Copy Key+Value to Clipboard"))
            {
                CopyKeyToClipboard(serializedObject);
            }
            var isManualUpdate = IsManualUpdate(serializedObject);
            if (isManualUpdate)
            {
                GUI.backgroundColor = Color.magenta;
                GUILayout.Button("Manual Update (Code) Enabled");
                GUI.backgroundColor = bgColorBefore;
                if (ReloadText(Selection.activeGameObject, serializedObject.targetObject as Localized))
                {
                    EditorUtility.SetDirty(Selection.activeGameObject);
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    Debug.Log($"CHANGED {serializedObject.targetObject}");
                }
            }
            else
            {
                if (GUILayout.Button("Reload Localization Key"))
                {
                    if (ReloadKey(Selection.activeGameObject, serializedObject.targetObject as Localized))
                    {
                        EditorUtility.SetDirty(Selection.activeGameObject);
                        EditorUtility.SetDirty(serializedObject.targetObject);
                        Debug.Log($"CHANGED {serializedObject.targetObject}");
                    }
                }
                if (GUILayout.Button("Reload Default Text"))
                {
                    if (Update(Selection.activeGameObject, serializedObject.targetObject as Localized) != 0)
                    {
                        EditorUtility.SetDirty(Selection.activeGameObject);
                        EditorUtility.SetDirty(serializedObject.targetObject);
                        Debug.Log($"CHANGED {serializedObject.targetObject}");
                    }
                }
            }
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Find Keys that Contains this Key"))
            {
                var serializedProperty = serializedObject.FindProperty(nameof(Localized._key));
                FindAllSimilarKeys(serializedProperty.stringValue, serializedObject.targetObject);
            }
            GUI.backgroundColor = bgColorBefore;
            GUILayout.Space(20);
            DrawDefaultInspector();
        }

        private static void CopyKeyToClipboard(SerializedObject serializedObject)
        {
            var serializedProperty1 = serializedObject.FindProperty(nameof(Localized._key));
            var serializedProperty2 = serializedObject.FindProperty(nameof(Localized._defaultText));
            EditorGUIUtility.systemCopyBuffer = $"{serializedProperty1.stringValue}\t{serializedProperty2.stringValue}";
        }

        private static bool IsManualUpdate(SerializedObject serializedObject)
        {
            var serializedProperty = serializedObject.FindProperty(nameof(Localized._isManualUpdate));
            return serializedProperty.boolValue;
        }

        private static LocalizedComponentType GetComponentType(SerializedObject serializedObject)
        {
            var serializedProperty = serializedObject.FindProperty(nameof(Localized._componentType));
            return (LocalizedComponentType)serializedProperty.intValue;
        }

        public static int AutoUpdate(GameObject gameObject, Localized localized)
        {
            Assert.IsFalse(localized._isManualUpdate);
            return Update(gameObject, localized);
        }

        private static int Update(GameObject gameObject, Localized localized)
        {
            var changed1 = ReloadKey(gameObject, localized);
            var changed2 = ReloadText(gameObject, localized);
            return changed1 || changed2 ? 1 : 0;
        }

        private static bool ReloadKey(GameObject gameObject, Localized localized)
        {
            var changed = false;
            if (localized._textMeshPro == null)
            {
                changed = ReloadTextComponent(gameObject, localized);
            }
            var prev1 = localized._key;
            localized._key = Localized.KeyFrom(localized);
            return changed || prev1 != localized._key;
        }

        private static bool ReloadText(GameObject gameObject, Localized localized)
        {
            var changed = false;
            var prev1 = localized.SafeText;
            if (localized._textMeshPro == null)
            {
                changed = ReloadTextComponent(gameObject, localized);
            }
            localized._defaultText = localized.SafeText;
            return changed || prev1 != localized._defaultText;
        }

        private static bool ReloadTextComponent(GameObject gameObject, Localized localized)
        {
            var prev1 = localized._textMeshPro;
            var prev2 = localized._componentType;
            localized._textMeshPro = GetTextComponent(gameObject, out var componentType);
            localized._componentType = componentType;
            return prev1 != localized._textMeshPro || prev2 != localized._componentType;
        }

        private static TMP_Text GetTextComponent(GameObject parent, out LocalizedComponentType componentType)
        {
            {
                var button = parent.GetComponent<Button>();
                if (button != null)
                {
                    componentType = LocalizedComponentType.Button;
                    return button.GetComponentInChildren<TMP_Text>();
                }
            }
            {
                var toggle = parent.GetComponentInParent<Toggle>();
                if (toggle != null)
                {
                    componentType = LocalizedComponentType.Toggle;
                    return toggle.GetComponentInChildren<TMP_Text>();
                }
            }
            {
                var text = parent.GetComponent<TMP_Text>();
                if (text != null)
                {
                    componentType = LocalizedComponentType.Text;
                    return text;
                }
            }
            componentType = LocalizedComponentType.Unknown;
            return null;
        }

        private static void FindAllSimilarKeys(string localizationKey, Object context)
        {
            Debug.Log($"find key={localizationKey}", context);
            CheckComponentsInPrefabs<Localized>(localized =>
            {
                if (localized._key.Contains(localizationKey))
                {
                    Key(localized);
                }
            });
        }

        private static void Key(Localized localized)
        {
            Debug.Log($"{localized} path={localized.GetFullPath()}", localized);
        }

        private static void CheckComponentsInPrefabs<T>(Action<T> callback) where T : MonoBehaviour
        {
            var allPrefabs = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") && path.EndsWith(".prefab"))
                .OrderBy(x => x);
            var gameObjects = allPrefabs.Select(a =>
                AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null);
            foreach (var gameObject in gameObjects)
            {
                var components = gameObject.GetComponentsInChildren<T>(includeInactive: true);
                foreach (var component in components)
                {
                    callback(component);
                }
            }
        }
    }
}
