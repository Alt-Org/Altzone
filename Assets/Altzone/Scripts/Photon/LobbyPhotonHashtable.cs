using System.Collections;
using System.Collections.Generic;
using Photon.Client;
using Photon.Realtime;
using UnityEngine;

namespace Altzone.Scripts.Lobby.Wrappers
{
    public class LobbyPhotonHashtable
    {
        private PhotonHashtable _hashtable;
        public object this[object key] => _hashtable[key];

        public object this[byte key] => _hashtable[key];

        public object this[int key] => _hashtable[key];

        public static object GetBoxedByte(byte value)
        {
            return PhotonHashtable.GetBoxedByte(value);
        }

        public LobbyPhotonHashtable()
        {
            _hashtable = new PhotonHashtable();
        }

        public LobbyPhotonHashtable(int x)
        {
            _hashtable = new PhotonHashtable(x);
        }

        public LobbyPhotonHashtable(Dictionary<object, object> dictionary)
        {
            _hashtable = new PhotonHashtable();
            foreach (KeyValuePair<object, object> pair in dictionary)
            {
                _hashtable[pair.Key] = pair.Value;
            }
        }

        public LobbyPhotonHashtable(PhotonHashtable hashtable)
        {
            _hashtable = hashtable;
        }

        public void Add(byte k, object v)
        {
            _hashtable.Add(k, v);
        }

        public void Add(int k, object v)
        {
            _hashtable.Add(k, v);
        }

        public void Remove(byte k)
        {
            _hashtable.Remove(k);
        }

        public void Remove(int k)
        {
            _hashtable.Remove(k);
        }

        //
        // Summary:
        //     Translates the byte key into the pre-boxed byte before doing the lookup.
        //
        // Parameters:
        //   key:
        public bool ContainsKey(byte key)
        {
            return _hashtable.ContainsKey(key);
        }

        public DictionaryEntryEnumerator GetEnumerator()
        {
            return _hashtable.GetEnumerator();
        }

        public override string ToString()
        {
           return _hashtable.ToString();
        }

        public void Clear()
        {
            _hashtable.Clear();
        }

        //
        // Summary:
        //     Creates a shallow copy of the PhotonHashtable.
        //
        // Returns:
        //     Shallow copy of the PhotonHashtable.
        //
        // Remarks:
        //     A shallow copy of a collection copies only the elements of the collection, whether
        //     they are reference types or value types, but it does not copy the objects that
        //     the references refer to. The references in the new collection point to the same
        //     objects that the references in the original collection point to.
        public object Clone()
        {
            return _hashtable.Clone();
        }

        public string ToStringFull()
        {
            return _hashtable.ToStringFull();
        }

        public PhotonHashtable GetOriginal()
        {
            return _hashtable;
        }
    }
}
