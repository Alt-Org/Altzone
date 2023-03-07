using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Factory class for <c>IStorefront</c>.
    /// </summary>
    public static class Storefront
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            Model.Store.Storefront.Instance = null;
        }

        public static IStorefront Get()
        {
            return Model.Store.Storefront.Instance ??= new Model.Store.Storefront();
        }
    }
}