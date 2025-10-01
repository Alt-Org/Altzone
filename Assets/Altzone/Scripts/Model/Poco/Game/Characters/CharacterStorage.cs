using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/CharacterStorage", fileName = "CharacterStorage")]
    public class CharacterStorage : ScriptableObject
    {

        [SerializeField] private List<BaseCharacter> _characterList;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
            _hasInstance = false;
        }

        private static CharacterStorage _instance;
        private static bool _hasInstance;

        public static CharacterStorage Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<CharacterStorage>(nameof(CharacterStorage));
                    _hasInstance = _instance != null;
                }
                return _instance;
            }
        }

        public List<BaseCharacter> CharacterList {
            get
            {
                UpdateList();
                return _characterList;
            }
        }

        public CharacterStorage()
        {
            //UpdateList();
        }

        internal void UpdateList()
        {
            if(_characterList == null)_characterList = new List<BaseCharacter>();

            for(int i = _characterList.Count-1; i>=0;  i--)
            {
                if(_characterList[i] == null) _characterList.RemoveAt(i);
            }

            //This finds every class that inherits the BaseCharacter class (and isn't abstract class like the CharacterClass classes)
            //and calls their constructor followed by adding them to the characterlist.
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BaseCharacter))))
            {
                BaseCharacter character = Resources.Load<BaseCharacter>("Characters/Stats/" + type.Name + "Stats");
                bool isNew = false;
                if (character == null)
                {
                    character = (BaseCharacter)ScriptableObject.CreateInstance(type);
                    isNew = true;
                }
                //BaseCharacter character = (BaseCharacter)Activator.CreateInstance(type);
                if (!_characterList.Exists(x => x.Id == character.Id))
                _characterList.Add(character);
                else
                {
                    if(isNew)Destroy(character);
                }
            }
            _characterList.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterStorage))]
    public class CharacterStorageEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CharacterStorage script = (CharacterStorage)target;

            if (GUILayout.Button("Update CharacterStat list"))
            {
                script.UpdateList();
            }
        }
    }
#endif
}
