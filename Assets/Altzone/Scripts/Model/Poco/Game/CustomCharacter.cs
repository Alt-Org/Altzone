using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Attributes;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Player created custom 'game' character based on given <c>CharacterClass</c>.
    /// </summary>
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CustomCharacter
    {
        [PrimaryKey] public CharacterID Id;
        public string CharacterName => GetCharacterName(Id);
        public string CharacterClassAndName => GetCharacterClassAndName(Id);
        public CharacterClassID CharacterClassID => GetClassID(Id);
        public int InsideCharacterID => GetInsideCharacterID(Id);

        public BaseCharacter CharacterBase { get => _characterBase;}

        private BaseCharacter _characterBase;
        /// <summary>
        /// This can be used for example to load UNITY assets by name for UI at runtime.
        /// </summary>
        //[Optional] public int UnityKey = -1;

        [Mandatory] public string Name;
        public int Hp;
        public int Speed;
        public int Resistance;
        public int Attack;
        public int Defence;

        public CustomCharacter(CharacterID id, int hp, int speed, int resistance, int attack, int defence)
        {
            //Assert.AreNotEqual(CharacterID.None, id);
            //Assert.IsTrue(characterClassId.IsMandatory());
            //Assert.IsTrue(name.IsMandatory());
            Id = id;
            Name = GetCharacterName(id);
            Hp = hp;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public CustomCharacter(BaseCharacter character)
        {
            Assert.AreNotEqual(CharacterID.None, character.Id);
            _characterBase = character;
            Id = character.Id;
            Name = GetCharacterName(character.Id);
            Hp = character.Hp;
            Speed = character.Speed;
            Resistance = character.Resistance;
            Attack = character.Attack;
            Defence = character.Defence;
        }

        internal static CustomCharacter CreateEmpty()
        {
            //Assert.AreEqual(customCharacter.CharacterClassId, characterClass.Id, "CharacterClassId mismatch");
            return new CustomCharacter(
                CharacterID.None,
                0,
                0,
                0,
                0,
                0);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}" +
                   $", {nameof(Name)}: {Name}" +
                   $", {nameof(Hp)}: {Hp}, {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }

        private static string GetCharacterName(CharacterID id)
        {
            switch (id)
            {
                case CharacterID.DesensitizerBodybuilder:
                    return "Bodybuilder";
                case CharacterID.TricksterComedian:
                    return "Comedian";
                case CharacterID.TricksterConman:
                    return "Conman";
                case CharacterID.ObedientPreacher:
                    return "Preacher";
                case CharacterID.ProjectorGrafitiartist:
                    return "Grafitiartist";
                case CharacterID.RetroflectorOvereater:
                    return "Overeater";
                case CharacterID.RetroflectorAlcoholic:
                    return "Alcoholic";
                case CharacterID.ConfluentBesties:
                    return "Besties";
                case CharacterID.IntellectualizerResearcher:
                    return "Researcher";
                default:
                    return "Error";
            }
        }

        public static string GetCharacterClassAndName(CharacterID id)
        {
            CharacterClassID classId = GetClassID(id);

            string className = CharacterClass.GetClassName(classId);

            return className+GetCharacterName(id);
        }

        public static CharacterClassID GetClassID(CharacterID id)
        {
            CharacterClassID ClassId = (CharacterClassID)((int)id & 0b1111_1111__0000_0000);
            return ClassId;
        }

        public static int GetInsideCharacterID(CharacterID id)
        {
            int characterId = (int)id & 0b0000_0000__1111_1111;
            return characterId;
        }

    }
}
