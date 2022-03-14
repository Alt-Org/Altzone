namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Simple cache for string objects to avoid garbage collection.
    /// </summary>
    /// <remarks>
    /// https://gamedev.center/array-vs-dictionary-lookup-in-net/
    /// </remarks>
    public class StringCache
    {
        private readonly string[] _phrases;

        public string this[int index] => _phrases[index];

        public StringCache(string[] phrases)
        {
            _phrases = phrases;
        }
    }
}