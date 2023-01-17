namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Public interface for <c>CustomCharacterModel</c>.
    /// </summary>
    /// <remarks>
    /// See https://github.com/Alt-Org/Altzone/wiki/Battle-Pelihahmo
    /// </remarks>
    public interface ICustomCharacterModel
    {
        public int Id { get; }
        public int CharacterModelId { get; }
        int PlayerPrefabId { get; set; }
        public string Name { get; set; }
        public int Speed { get; set; }
        public int Resistance { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
    }
}