using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using MenuUI.Scripts.SoulHome;
using UnityEditor;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    //[CreateAssetMenu(menuName = "ALT-Zone/SoulhomeFurnitureReference", fileName = "SoulhomeFurnitureReference")]
    public class SoulHomeFurnitureReference : ScriptableObject
    {
        [SerializeField] private StorageFurnitureReference _sheet;
        [SerializeField] private List<SoulhomeFurnitureSetInfo> _info;

        public List<SoulhomeFurnitureSetInfo> Info => _info; // Public accessor for _info

        public GameObject GetSoulHomeFurnitureObject(string name)
        {
            SoulhomeFurnitureInfoObject data = GetFurnitureData(name);
            return data.FurnitureHandling.gameObject;
        }

        public GameObject GetSoulHomeTrayFurnitureObject(string name)
        {
            SoulhomeFurnitureInfoObject data = GetFurnitureData(name);
            return data?.TrayFurniture?.gameObject;
        }


        public SoulhomeFurnitureInfoObject GetFurnitureData(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string[] parts = name.Split("_");

            if (parts.Length != 2) { return null; }

            foreach(SoulhomeFurnitureSetInfo info in _info)
            {
                if(info.SetName == parts[1])
                {
                    foreach(SoulhomeFurnitureInfoObject info2 in info.list)
                    {
                        if(info2.Name == parts[0]) return info2;
                    }
                }
            }
            return null;
        }

        public void UpdateSheet()
        {
            List<FurnitureSetInfo> furnitureList = _sheet.Info;
            List<SoulhomeFurnitureSetInfo> oldSheet = _info;
            List<SoulhomeFurnitureSetInfo> newSheet = new();

            foreach (FurnitureSetInfo setInfo in furnitureList)
            {
                SoulhomeFurnitureSetInfo newSetInfo = new();
                newSetInfo.SetName = setInfo.SetName;
                newSetInfo.list = new();
                foreach (FurnitureInfoObject infoObject in setInfo.list)
                {
                    SoulhomeFurnitureInfoObject newInfo = new();
                    newInfo.Name = infoObject.Name;
                    newSetInfo.list.Add(newInfo);
                }
                newSheet.Add(newSetInfo);
            }

            foreach (SoulhomeFurnitureSetInfo newSetInfo in newSheet)
            {
                foreach (SoulhomeFurnitureSetInfo oldSetInfo in oldSheet)
                {
                    if (!newSetInfo.SetName.Equals(oldSetInfo.SetName))
                    {
                        continue;
                    }
                    foreach (SoulhomeFurnitureInfoObject newInfo in newSetInfo.list)
                    {
                        foreach (SoulhomeFurnitureInfoObject oldInfo in oldSetInfo.list)
                        {
                            if (!newInfo.Name.Equals(oldInfo.Name))
                            {
                                continue;
                            }
                            newInfo.FurnitureHandling = oldInfo.FurnitureHandling;
                            newInfo.TrayFurniture = oldInfo.TrayFurniture;
                        }
                    }
                }
            }
            _info = newSheet;
        }

        public bool AddFurniture(SoulhomeFurnitureSetInfo setInfo)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Don't try to add furniture while the game is running.");
                return false;
            }

            SoulhomeFurnitureSetInfo localSet = _info.FirstOrDefault(x => x.SetName == setInfo.SetName);
            if (localSet == null)
            {
                Debug.LogError("Furniture not found in SoulHomeReference");
                return false;
            }
            else
            {
                bool added = false;
                foreach (SoulhomeFurnitureInfoObject furnitureInfo in setInfo.list)
                {
                    SoulhomeFurnitureInfoObject localFurniture = localSet.list.FirstOrDefault(x => x.Name == furnitureInfo.Name);
                    if (localFurniture == null)
                    {
                        Debug.LogError("Furniture not found in SoulHomeReference");
                    }
                    else
                    {
                        localFurniture.FurnitureHandling = furnitureInfo.FurnitureHandling;
                        localFurniture.TrayFurniture = furnitureInfo.TrayFurniture;
                        added = true;
#if UNITY_EDITOR
                        AssetDatabase.Refresh();
                        EditorUtility.SetDirty(this);
                        AssetDatabase.SaveAssets();
#endif
                    }
                }
                return added;
            }
        }
    }

    [Serializable]
    public class SoulhomeFurnitureInfoObject
    {
        public string Name;
        public FurnitureHandling FurnitureHandling;
        public TrayFurniture TrayFurniture;
    }

    [Serializable]
    public class SoulhomeFurnitureSetInfo
    {
        public string SetName;

        public List<SoulhomeFurnitureInfoObject> list;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SoulHomeFurnitureReference))]
    public class SoulHomeFurnitureReferenceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SoulHomeFurnitureReference script = (SoulHomeFurnitureReference)target;
            if (GUILayout.Button("Update Sheet"))
            {
                script.UpdateSheet();
            }
        }
    }
#endif
}
