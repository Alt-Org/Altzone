using UnityEngine;

namespace Battle.Scripts.interfaces
{
    /// <summary>
    /// Game arena interface for Battle arena.
    /// </summary>
    public interface IGameArena
    {
        /// <summary>
        /// Creates walls around game arena.
        /// </summary>
        void makeWalls();

        /// <summary>
        /// Outer area of game arena.
        /// </summary>
        /// <remarks>
        /// Anything outside this area could be considered an error if it should stay inside game area!
        /// </remarks>
        Rect outerArea { get; }
    }
}