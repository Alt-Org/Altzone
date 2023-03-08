using UnityEngine.Assertions;

namespace Altzone.Scripts.Temp
{
    /// <summary>
    /// Base class for all generic models.
    /// </summary>
    public abstract class AbstractModel
    {
        public int Id { get; private set; }

        protected AbstractModel(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Sets our ID after remote object has been created (with this new id) for this model instance.
        /// </summary>
        public void SetId(int id)
        {
            Assert.IsTrue(Id == 0);
            Assert.IsTrue(id != 0);
            Id = id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}";
        }
    }
}