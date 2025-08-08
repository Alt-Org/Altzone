using System.Collections.Generic;

namespace Altzone.Scripts.Audio
{
    public class Chunk<T>
    {
        public int AmountInUse = 0;
        public List<T> Pool = new List<T>();
    }
}
