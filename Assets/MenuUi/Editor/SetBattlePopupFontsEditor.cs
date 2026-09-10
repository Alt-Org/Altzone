using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

/// <summary>
/// Editor window to find prefabs containing `BattlePopupPanelManager` and set
/// their TextMeshProUGUI/TMPro text components to the selected AllowedFonts TMP asset.
///
/// Usage: Place AllowedFonts asset in a `Resources` folder (Resources/AllowedFonts) or
/// assign it in the window, pick the TMP font, then click "Find & Update...".
/// </summary>
public class SetBattlePopupFontsEditor : EditorWindow
{
    private ScriptableObject _allowedFontsObj;
    private int _selectedTmpIndex;
    private Font _selectedLegacyFont;
    private GameObject _targetPrefabAsset;

    [MenuItem("ALT-Zone/Tools/Set Battle Popup Fonts")]
    public static void ShowWindow()
    {
        GetWindow<SetBattlePopupFontsEditor>("Set Battle Popup Fonts");
    }

    private void OnEnable()
    {
        if (_allowedFontsObj == null)
        {
            _allowedFontsObj = Resources.Load("AllowedFonts") as ScriptableObject;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Battle Popup Font Setter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _allowedFontsObj = (ScriptableObject)EditorGUILayout.ObjectField("AllowedFonts (Resources)", _allowedFontsObj, typeof(ScriptableObject), false);

        EditorGUILayout.Space();
        _targetPrefabAsset = (GameObject)EditorGUILayout.ObjectField("Target Prefab (optional)", _targetPrefabAsset, typeof(GameObject), false);
        EditorGUILayout.HelpBox("If a Target Prefab is assigned, only that prefab will be updated. Otherwise the tool scans all project prefabs.", MessageType.None);

        if (_allowedFontsObj == null)
        {
            EditorGUILayout.HelpBox("Assign the AllowedFonts asset (can be placed in a Resources folder).", MessageType.Info);
        }
        else
        {
            // Read _tmpFonts via reflection so we don't need a compile-time dependency on TextMeshPro assembly
            var field = _allowedFontsObj.GetType().GetField("_tmpFonts", BindingFlags.Public | BindingFlags.Instance);
            UnityEngine.Object[] tmpList = null;
            if (field != null)
            {
                tmpList = field.GetValue(_allowedFontsObj) as UnityEngine.Object[];
            }
            tmpList = tmpList ?? new UnityEngine.Object[0];
            string[] tmpNames = tmpList.Select(t => t == null ? "<null>" : t.name).ToArray();
            if (tmpNames.Length == 0)
            {
                EditorGUILayout.HelpBox("No TMP fonts found in AllowedFonts._tmpFonts.", MessageType.Warning);
            }
            else
            {
                _selectedTmpIndex = EditorGUILayout.Popup("TMP Font (Allowed)", Mathf.Clamp(_selectedTmpIndex, 0, tmpNames.Length - 1), tmpNames);
            }

            _selectedLegacyFont = (Font)EditorGUILayout.ObjectField("Legacy Font (optional)", _selectedLegacyFont, typeof(Font), false);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Find & Update Battle Popup Prefabs"))
        {
            UpdateBattlePopupFonts();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("This searches all project prefabs and updates those containing a BattlePopupPanelManager component.");
        EditorGUILayout.LabelField("Changes are saved into the prefab assets.");
    }

    private void UpdateBattlePopupFonts()
    {
        if (_allowedFontsObj == null)
        {
            Debug.LogError("AllowedFonts asset not assigned. Place it in Resources and/or assign it in the window.");
            return;
        }

        UnityEngine.Object chosenTmp = null;
        var field = _allowedFontsObj.GetType().GetField("_tmpFonts", BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            var arr = field.GetValue(_allowedFontsObj) as UnityEngine.Object[];
            if (arr != null && arr.Length > 0)
            {
                _selectedTmpIndex = Mathf.Clamp(_selectedTmpIndex, 0, arr.Length - 1);
                chosenTmp = arr[_selectedTmpIndex];
            }
        }

        Font chosenLegacy = _selectedLegacyFont;

        string[] guids;
        if (_targetPrefabAsset != null)
        {
            var p = AssetDatabase.GetAssetPath(_targetPrefabAsset);
            guids = new[] { AssetDatabase.AssetPathToGUID(p) };
        }
        else
        {
            guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        }
        int updatedCount = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                var manager = root.GetComponentInChildren<BattlePopupPanelManager>(true);
                if (manager == null) continue;

                bool changed = false;

                // Update TMP texts via reflection (so editor script doesn't require TMPro assembly)
                var tmpTextType = Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
                if (tmpTextType != null && chosenTmp != null)
                {
                    var comps = root.GetComponentsInChildren(tmpTextType, true);
                    foreach (var comp in comps)
                    {
                        var fontProp = comp.GetType().GetProperty("font");
                        if (fontProp != null)
                        {
                            var current = fontProp.GetValue(comp) as UnityEngine.Object;
                            if (current != chosenTmp)
                            {
                                fontProp.SetValue(comp, chosenTmp);
                                changed = true;
                            }
                        }
                    }
                }

                var texts = root.GetComponentsInChildren<Text>(true);
                foreach (var t in texts)
                {
                    if (chosenLegacy != null && t.font != chosenLegacy)
                    {
                        t.font = chosenLegacy;
                        changed = true;
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    updatedCount++;
                    Debug.Log($"SetBattlePopupFonts: Updated prefab '{path}'");
                }
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        Debug.Log($"SetBattlePopupFonts: Completed. Updated {updatedCount} prefab(s). Choose 'Acme Regular SDF' in AllowedFonts before running.");
    }
}
