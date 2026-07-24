using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using UnityEditor;
using UnityEngine;

//pikku scripti vaan nopeesti lis‰m‰‰n StorageFurnitureReference.asset iin sivuperspektiivin kuvat
public static class FurnitureSidewaysImageAutoFill
{
    [MenuItem("Tools/Furniture/Fill Sideways Images")]
    public static void FillSidewaysImages()
    {
        StorageFurnitureReference reference =
            Resources.Load<StorageFurnitureReference>("StorageFurnitureReference");

        if (reference == null)
        {
            Debug.LogError("StorageFurnitureReference.asset not found.");
            return;
        }

        List<string> missingSprites = new();
        int assigned = 0;

        foreach (FurnitureSetInfo set in reference.Info)
        {
            foreach (FurnitureInfoObject furniture in set.list)
            {
                if (furniture.Image == null)
                    continue;

                if (furniture.SidewaysImage != null)
                    continue;

                string imageName = furniture.Image.name;

                if (!imageName.EndsWith("_0"))
                    continue;

                string sidewaysName =
                    imageName.Substring(0, imageName.Length - 2) + "_1";

                string spritePath =
                    AssetDatabase.GetAssetPath(furniture.Image);

                Object[] assets =
                    AssetDatabase.LoadAllAssetsAtPath(spritePath);

                bool found = false;

                foreach (Object asset in assets)
                {
                    if (asset is Sprite sprite &&
                        sprite.name == sidewaysName)
                    {
                        //furniture.SidewaysImage = sprite;
                        Debug.Log($"Would assign {imageName} -> {sidewaysName}"); //test
                        assigned++;
                        found = true;

                        Debug.Log(
                            $"Assigned {imageName} -> {sidewaysName}");

                        break;
                    }
                }

                if (!found)
                {
                    missingSprites.Add(
                        $"{set.SetName}/{furniture.Name} (expected '{sidewaysName}')");
                }
            }
        }

        EditorUtility.SetDirty(reference);
        AssetDatabase.SaveAssets();

        Debug.Log($"Finished. Assigned {assigned} sprites.");
        if (missingSprites.Count > 0)
        {
            Debug.LogWarning(
                $"Could not find sideways sprites for {missingSprites.Count} furniture items:");

            foreach (string item in missingSprites)
            {
                Debug.LogWarning(item);
            }
        }
        else
        {
            Debug.Log("All furniture entries have a SidewaysImage assigned.");
        }
    }
}
