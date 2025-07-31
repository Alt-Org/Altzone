using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(fileName = "SFXReference", menuName = "ScriptableObjects/SFXReferenceScriptableObject")]
    public class SFXReference : ScriptableObject
    {
        [SerializeField] private List<SoundCategory> _soundCategories = new List<SoundCategory>();

        public List<SoundCategory> SoundCategories { get => _soundCategories; }

        /// <returns>First SoundEffect that is found in any sound category.</returns>
        public SoundEffect Get(string SoundName)
        {
            foreach (SoundCategory category in _soundCategories)
                foreach (SoundEffect sfx in category.SoundEffects)
                    if (sfx.Name.ToLower() == SoundName.ToLower())
                        return sfx;

            return null;
        }

        /// <returns>First SoundEffect that is found in given SoundCategory name.</returns>
        public SoundEffect Get(string CategoryName, string SoundName)
        {
            foreach (SoundCategory category in _soundCategories)
                if (category.Name.ToLower() == CategoryName.ToLower())
                    foreach (SoundEffect sfx in category.SoundEffects)
                        if (sfx.Name.ToLower() == SoundName.ToLower())
                            return sfx;

            return null;
        }
    }

    public enum SoundPlayType
    {
        Default,
        OneShot,
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
        public SoundPlayType Type;
        public AudioClip Audio;
    }
}
