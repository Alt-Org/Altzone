using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToggleButton : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Toggle Button", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/ToggleButton"));
        go.name = "ToggleButton";
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif
}
