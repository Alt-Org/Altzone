using System.Collections.Generic;

namespace Altzone.Scripts.Audio
{
    public class Chunk<T>
    {
        public string Name = "";
        public int AmountInUse = 0;
        public List<T> Pool = new List<T>();

        public Chunk() { }

        public Chunk(string name) { Name = name; }

        /// <summary>
        /// Adds given item to the pool.
        /// </summary>
        public void Add(T item) { Pool.Add(item); }

        /// <summary>
        /// Inserts given item into the pool to the given index.
        /// </summary>
        /// <returns>Default or Item that was pushed out of the pool.</returns>
        public T Insert(T item, int index)
        {
            Pool.Insert(index, item);

            int lastIndex = Pool.Count - 1;
            T extraItem = Pool[lastIndex];
            Pool.RemoveAt(lastIndex);

            return extraItem;
        }

        /// <summary>
        /// Clears the item pool and the chunk name.
        /// </summary>
        public void ClearAll()
        {
            Name = "";
            Clear();
        }

        /// <summary>
        /// Clears the item pool.
        /// </summary>
        public void Clear()
        {
            AmountInUse = 0;
            Pool.Clear();
        }
    }
}
