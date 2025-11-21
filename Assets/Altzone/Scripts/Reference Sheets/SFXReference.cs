using System.Collections.Generic;
using Altzone.Scripts.Audio;
using Assets.Altzone.Scripts.Reference_Sheets;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(fileName = "SFXReference", menuName = "ScriptableObjects/SFXReferenceScriptableObject")]
    public class SFXReference : ScriptableObject
    {
        [SerializeField] private List<SoundCategory> _soundCategories = new List<SoundCategory>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
            _hasInstance = false;
        }

        private static SFXReference _instance;
        private static bool _hasInstance;

        public static SFXReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<SFXReference>($"Audio/SFXDataReference");
                    _hasInstance = _instance != null;
                }
                return _instance;
            }
        }

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

        /// <returns>First SoundEffect that is found in given SoundCategory name.</returns>
        public SoundEffect Get(AudioCategoryType CategoryType, string SoundName)
        {
            foreach (SoundCategory category in _soundCategories)
                if (category.Type == CategoryType)
                    foreach (SoundEffect sfx in category.SoundEffects)
                        if (sfx.Name.ToLower() == SoundName.ToLower())
                            return sfx;

            return null;
        }

        /// <returns>First SoundEffect that is found in given SoundCategory name.</returns>
        public SoundEffect Get(AudioCategoryType CategoryType, BattleSFXNameTypes SoundTypeName)
        {
            foreach (SoundCategory category in _soundCategories)
                if (category.Type == CategoryType)
                    foreach (SoundEffect sfx in category.SoundEffects)
                        if (sfx.BattleName == SoundTypeName)
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
        public AudioCategoryType Type;
        public List<SoundEffect> SoundEffects;
    }

    [System.Serializable]
    public class SoundEffect
    {
        public string Name;
        public BattleSFXNameTypes BattleName;
        public SoundPlayType Type;
        public AudioClip Audio;
        [Range(0f, 10f)]
        public float Volume = 1;
    }
}
