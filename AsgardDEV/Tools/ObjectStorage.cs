using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AsgardDEV.Tools
{
    /// <summary>
    ///   Class to store objects. Completely threadsafe using locks. Does not allow duplicates!
    /// </summary>
    public class ObjectStorage
    {
        private readonly Dictionary<object, object> Storage = new Dictionary<object, object>();

        public object GetValue(object key)
        {
            lock (Storage)
            {
                return !Storage.ContainsKey(key) ? null : Storage[key];
            }
        }

        public void RemoveValue(object key)
        {
            lock (Storage)
            {
                if (Storage.ContainsKey(key))
                {
                    Storage.Remove(key);
                }
            }
        }

        public void AddValue(object key, object value)
        {
            lock (Storage)
            {
                if (!Storage.ContainsKey(value))
                {
                    Storage.Add(key, value);
                }
                else
                {
                    Console.WriteLine("[Warning] Trying to insert duplicate entry in dictionary: " + key);
                }
            }
        }

        public object[] GetAllValues()
        {
            lock (Storage)
            {
                object[] objs = new object[Storage.Count];
                Storage.Values.CopyTo(objs, 0);
                return objs;
            }
        }
    }
}