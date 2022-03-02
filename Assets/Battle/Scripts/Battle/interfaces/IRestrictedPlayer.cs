using UnityEngine;

namespace Battle.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface to restrict player movement to given area.
    /// </summary>
    public interface IRestrictedPlayer
    {
        void SetPlayArea(Rect area);
        bool CanMove { get; set; }
    }
}