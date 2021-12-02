namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Enum values for Defence "attribute".
    /// </summary>
    /// <remarks>
    /// This can be serialized so do not change or remove enum values.
    /// </remarks>
    public enum Defence
    {
        None = 0,
        Desensitisation = 1,
        Deflection = 2,
        Introjection = 3,
        Projection = 4,
        Retroflection = 5,
        Egotism = 6,
        Confluence = 7,
    }

    /// <summary>
    /// Player Defence model.
    /// </summary>
    public class DefenceModel : AbstractModel
    {
        public readonly Defence Defence;

        public DefenceModel(int id, Defence defence) : base(id)
        {
            Defence = defence;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Defence)}: {Defence}";
        }
    }
}