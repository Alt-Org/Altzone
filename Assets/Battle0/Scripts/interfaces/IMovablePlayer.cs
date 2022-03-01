using UnityEngine;

namespace Battle0.Scripts.interfaces
{
    /// <summary>
    /// Interface to move player towards given position.
    /// </summary>
    public interface IMovablePlayer
    {
        void moveTo(Vector3 position);
    }
}