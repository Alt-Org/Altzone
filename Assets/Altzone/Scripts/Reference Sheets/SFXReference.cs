using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "SFXReference", menuName = "ScriptableObjects/SFXReferenceScriptableObject")]
public class SFXReference : ScriptableObject
{
    [SerializeField] private List<SoundCategory> _soundCategories = new List<SoundCategory>();

    public List<SoundCategory> SoundCategories { get => _soundCategories; }

    public SoundEffect Get(string SoundName)
    {
        foreach (SoundCategory category in SoundCategories)
            foreach (SoundEffect sfx in category.SoundEffects)
                if (sfx.Name.ToLower() == SoundName.ToLower())
                    return sfx;

        return null;
    }

    public SoundEffect Get(string CategoryName, string SoundName)
    {
        foreach (SoundCategory category in SoundCategories)
            if (category.Name.ToLower() == CategoryName.ToLower())
                foreach (SoundEffect sfx in category.SoundEffects)
                    if (sfx.Name.ToLower() == SoundName.ToLower())
                        return sfx;

        return null;
    }
}

public enum SoundCategoryType
{
    OneShot,
    OneShotWithStop,
    Loop
}

[System.Serializable]
public class SoundCategory
{
    public string Name;
    public List<SoundEffect> SoundEffects;
}

[System.Serializable]
public class SoundEffect
{
    public string Name;
    public SoundCategoryType Type;
    public AudioClip Audio;
}
