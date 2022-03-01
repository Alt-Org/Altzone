using UnityEngine;

namespace Battle0.Scripts.interfaces
{
    /// <summary>
    /// Interface to restrict player movement to given area.
    /// </summary>
    public interface IRestrictedPlayer
    {
        void setPlayArea(Rect area);
        bool canMove { get; set; }
    }
}