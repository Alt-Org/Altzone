using UnityEngine.Assertions;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Abstract base class for models, if we might need something that is common to all, like persisting data?
    /// </summary>
    public abstract class AbstractModel
    {
        public readonly int Id;

        protected AbstractModel(int id)
        {
            Assert.IsTrue(id > 0, "id > 0");
            Id = id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}";
        }
    }
}
