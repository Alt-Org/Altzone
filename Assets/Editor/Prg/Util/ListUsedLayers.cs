using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class ListUsedLayers : MonoBehaviour
    {
        private const string Untagged = "Untagged";

        [MenuItem("Window/ALT-Zone/Util/List Used layers in Scene")]
        private static void _ListUsedLayers()
        {
            Debug.Log("*");
            ListObjectsInLayer(GetSceneObjects());
        }

        [MenuItem("Window/ALT-Zone/Util/List Used tags in Scene")]
        private static void _ListUsedTags()
        {
            Debug.Log("*");
            ListObjectsWithTag(GetSceneObjects());
        }

        private static void ListObjectsInLayer(IEnumerable<GameObject> gameObjects)
        {
            var layerObjects = new Dictionary<int, List<string>>();
            foreach (var go in gameObjects)
            {
                if (go.layer == 0)
                {
                    continue;
                }
                var name = GetFullName(go);
                if (!layerObjects.TryGetValue(go.layer, out var objectList))
                {
                    objectList = new List<string>();
                    layerObjects.Add(go.layer, objectList);
                }
                objectList.Add(name);
            }
            var usedLayers = layerObjects.Keys.ToList();
            usedLayers.Sort();
            foreach (var usedLayer in usedLayers)
            {
                var layerName = LayerMask.LayerToName(usedLayer);
                var objectList = layerObjects[usedLayer];
                Debug.Log($"Layer {usedLayer:D2} : {layerName,-16} is used in {objectList.Count} GameObject(s)");
            }
        }

        private static void ListObjectsWithTag(IEnumerable<GameObject> gameObjects)
        {
            var taggedObjects = new Dictionary<string, List<string>>();
            foreach (var go in gameObjects)
            {
                if (go.CompareTag(Untagged))
                {
                    continue;
                }
                var name = GetFullName(go);
                if (!taggedObjects.TryGetValue(go.tag, out var objectList))
                {
                    objectList = new List<string>();
                    taggedObjects.Add(go.tag, objectList);
                }
                objectList.Add(name);
            }
            var usedTags = taggedObjects.Keys.ToList();
            usedTags.Sort();
            foreach (var usedTag in usedTags)
            {
                var objectList = taggedObjects[usedTag];
                Debug.Log($"Tag {usedTag,-16} is used in {objectList.Count} GameObject(s)");
            }
        }

        private static IEnumerable<GameObject> GetSceneObjects()
        {
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.hideFlags == HideFlags.None);
        }

        private static string GetFullName(GameObject go)
        {
            var name = go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                name = go.name + "/" + name;
            }
            return name;
        }
    }
}