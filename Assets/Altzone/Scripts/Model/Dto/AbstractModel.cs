namespace Altzone.Scripts.Model.Dto
{
    /// <summary>
    /// Base class for all generic models.
    /// </summary>
    public abstract class AbstractModel
    {
        public int Id { get; }

        protected AbstractModel(int id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}";
        }
    }
}
