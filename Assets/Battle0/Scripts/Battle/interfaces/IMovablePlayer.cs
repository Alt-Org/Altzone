using UnityEngine;

namespace Battle0.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface to move player towards given position with given speed.
    /// </summary>
    public interface IMovablePlayer
    {
        Camera Camera { get; }
        Transform Transform { get; }
        float Speed { get; set; }
        void MoveTo(Vector2 position);
    }
}