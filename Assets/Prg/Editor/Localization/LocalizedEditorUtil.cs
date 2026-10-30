using System.Collections.Generic;
using Prg;
using Prg.Localization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if PRG_DEBUG
using Debug = Prg.Debug;
#else
using Debug = UnityEngine.Debug;
#endif

namespace Editor.Prg.Localization
{
    /// <summary>
    /// Editor utility for localization process operational support for selected prefab(s).
    /// </summary>
    public static class LocalizedEditorUtil
    {
        public static void UpdateLocalization(GameObject prefab)
        {
            Debug.Log($"{prefab.GetFullPath()}", prefab);

            if (prefab.IsSceneObject())
            {
                Debug.Log($"{prefab.name} must be selected in Project Window");
                return;
            }
            var assetPath = AssetDatabase.GetAssetPath(prefab);
            Debug.Log($"{prefab.name} assetPath {assetPath}", prefab);
            using var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath);

            var updateCount = 0;
            foreach (var localized in prefab.GetComponentsInChildren<Localized>(includeInactive: true))
            {
                if (localized._isManualUpdate)
                {
                    continue;
                }
                var updated = LocalizedEditor.AutoUpdate(localized.gameObject, localized);
                updateCount += updated;
                var status = updated == 0 ? RichText.White("OK") : RichText.Yellow("update");
                Debug.Log($"{status} {localized.GetFullPath()}");
            }
            Debug.Log($"updateCount {updateCount}");
        }

        public static void AddLocalization(GameObject prefab)
        {
            if (prefab.IsSceneObject())
            {
                Debug.Log($"{prefab.name} must be selected in Project Window");
                return;
            }
            var assetPath = AssetDatabase.GetAssetPath(prefab);
            Debug.Log($"{prefab.name} assetPath {assetPath}", prefab);
            using var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath);

            var prefabRoot = editingScope.prefabContentsRoot;
            var textComponents = prefabRoot.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            if (textComponents.Length == 0)
            {
                Debug.Log($"{RichText.Yellow("No")} textComponents found");
                return;
            }
            Debug.Log($"textComponents {textComponents.Length}");
            var knownKeys = new HashSet<string>();
            var newTextComponentCount = 0;
            foreach (var textComponent in textComponents)
            {
                if (textComponent.gameObject.CompareTag(Localized.NoLocalizeTag))
                {
                    continue;
                }
                var parentTransform = textComponent.transform.parent;
                var hasParent = parentTransform != null;
                var button = hasParent ? parentTransform.GetComponent<Button>() : null;
                var hasButton = button != null;
                var toggle = hasParent ? parentTransform.GetComponent<Toggle>() : null;
                var hasToggle = toggle != null;
                Localized localized;
                if (hasButton)
                {
                    localized = button.GetComponent<Localized>();
                }
                else if (hasToggle)
                {
                    localized = toggle.GetComponent<Localized>();
                }
                else
                {
                    localized = textComponent.GetComponent<Localized>();
                }
                var hasLocalized = localized != null;
                var type = hasButton ? "Button" : hasToggle ? "Toggle" : "Text";
                if (hasLocalized)
                {
                    Debug.Log(
                        $"{RichText.White("OK")} {type} {textComponent.GetFullPath()} | {localized._defaultText}");
                    continue;
                }
                if (hasButton)
                {
                    newTextComponentCount += 1;
                    localized = AddLocalized(button.gameObject);
                    Debug.Log($"{RichText.Yellow("Add")} {type} {button.GetFullPath()} | {localized._defaultText}");
                }
                else if (hasToggle)
                {
                    newTextComponentCount += 1;
                    localized = AddLocalized(toggle.gameObject);
                    Debug.Log($"{RichText.Yellow("Add")} {type} {toggle.GetFullPath()} | {localized._defaultText}");
                }
                else
                {
                    newTextComponentCount += 1;
                    localized = AddLocalized(textComponent.gameObject);
                    Debug.Log(
                        $"{RichText.Yellow("Add")} {type} {textComponent.GetFullPath()} | {localized._defaultText}");
                }
            }
            Debug.Log(newTextComponentCount == 0
                ? $"{RichText.Magenta("No")} textComponents added"
                : $"{RichText.Green("New")} textComponents {newTextComponentCount}");
            return;

            Localized AddLocalized(GameObject parent)
            {
                var instance = parent.AddComponent<Localized>();
                LocalizedEditor.AutoUpdate(parent, instance);
                if (!knownKeys.Add(instance._key))
                {
                    Debug.Log($"{RichText.Magenta("Duplicate")} key {instance._key}");
                }
                return instance;
            }
        }

        public static void RemoveLocalization(GameObject prefab)
        {
            if (prefab.IsSceneObject())
            {
                Debug.Log($"{prefab.name} must be selected in Project Window");
                return;
            }
            var assetPath = AssetDatabase.GetAssetPath(prefab);
            Debug.Log($"{prefab.name} assetPath {assetPath}", prefab);
            using var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath);
            var prefabRoot = editingScope.prefabContentsRoot;

            var textComponents = prefabRoot.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            Debug.Log($"textComponents {textComponents.Length}");
            if (textComponents.Length > 0)
            {
                foreach (var textComponent in textComponents)
                {
                    if (textComponent.gameObject.CompareTag(Localized.NoLocalizeTag))
                    {
                        Debug.Log(
                            $"{RichText.White("Remove NoLocalize Tag")} {textComponent.GetFullPath()} | {Localized.SanitizeText(textComponent.text)}",
                            textComponent);
                        textComponent.gameObject.tag = string.Empty;
                    }
                }
            }
            var localizedComponents = prefabRoot.GetComponentsInChildren<Localized>(includeInactive: true);
            Debug.Log($"localizedComponents {localizedComponents.Length}");
            if (localizedComponents.Length == 0)
            {
                return;
            }
            foreach (var localizedComponent in localizedComponents)
            {
                Debug.Log(
                    $"{RichText.White("Remove Localized Tag")} {localizedComponent.GetFullPath()} | {Localized.SanitizeText(localizedComponent._defaultText)}",
                    localizedComponent);
                PrefabUtility.RevertAddedComponent(localizedComponent, InteractionMode.AutomatedAction);
            }
        }

        public static void AddNoLocalizeTag(GameObject prefab)
        {
            if (prefab.IsSceneObject())
            {
                Debug.Log($"{prefab.name} must be selected in Project Window");
                return;
            }
            var assetPath = AssetDatabase.GetAssetPath(prefab);
            Debug.Log($"{prefab.name} assetPath {assetPath}", prefab);
            using var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath);
            var prefabRoot = editingScope.prefabContentsRoot;

            var textComponents = prefabRoot.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            Debug.Log($"textComponents {textComponents.Length}");
            if (textComponents.Length > 0)
            {
                foreach (var textComponent in textComponents)
                {
                    if (!textComponent.gameObject.CompareTag(Localized.NoLocalizeTag))
                    {
                        Debug.Log(
                            $"{RichText.White("Set NoLocalize Tag")} {textComponent.GetFullPath()} | {Localized.SanitizeText(textComponent.text)}",
                            textComponent);
                        textComponent.gameObject.tag = Localized.NoLocalizeTag;
                    }
                }
            }
            var localizedComponents = prefabRoot.GetComponentsInChildren<Localized>(includeInactive: true);
            Debug.Log($"localizedComponents {localizedComponents.Length}");
            if (localizedComponents.Length == 0)
            {
                return;
            }
            foreach (var localizedComponent in localizedComponents)
            {
                Debug.Log(
                    $"{RichText.White("Has Localized")} {localizedComponent.GetFullPath()} | {Localized.SanitizeText(localizedComponent._defaultText)}",
                    localizedComponent);
            }
        }
    }
}
