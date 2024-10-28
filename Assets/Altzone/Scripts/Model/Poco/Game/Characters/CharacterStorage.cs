using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/CharacterStorage", fileName = "CharacterStorage")]
    public class CharacterStorage{

        [SerializeField] private List<BaseCharacter> _characterList;

        public List<BaseCharacter> CharacterList { get => _characterList;}

        public CharacterStorage()
        {
            _characterList = new List<BaseCharacter>();
            //This finds every class that inherits the BaseCharacter class (and isn't abstract class like the CharacterClass classes)
            //and calls their constructor followed by adding them to the characterlist.
            foreach ( Type type in Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BaseCharacter))))
            {
                _characterList.Add((BaseCharacter)Activator.CreateInstance(type));
            }
            _characterList.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

    }
}
