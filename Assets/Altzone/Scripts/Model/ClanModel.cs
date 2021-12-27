namespace Altzone.Scripts.Model
{
    public class ClanModel : AbstractModel
    {
        public readonly string Name;
        public readonly string Tag;
        public readonly bool IsSpectator;

        public ClanModel(int id, string name, string tag, bool isSpectator = false) : base(id)
        {
            Name = name;
            Tag = tag;
            IsSpectator = isSpectator;
        }
    }
}