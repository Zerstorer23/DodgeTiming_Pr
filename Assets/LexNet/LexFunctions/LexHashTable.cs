namespace Lex
{

    using Lex;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    public class LexHashTable
    {
        public Dictionary<int, object> lexHash = new Dictionary<int, object>();

        public object this[int i]
        {
            get { return lexHash[i] ; }
        }
        public object this[Property i]
        {
            get { return  lexHash[(int)i] ; }
        }
        LexPlayer owner = null;
        public LexHashTable()
        {
            owner = null;
        }
        public LexHashTable(LexPlayer owner)
        {
            this.owner = owner;
        }

        public void Add(Property key, object value) => Add((int)key, value);
        public void Add(int key, object value)
        {
            if (value.GetType().IsEnum) {
                value = (int)value;
            }
            lexHash.Add(key, value);
        }

        public T Get<T>(Property key, T value) => Get((int)key, value);
        public T Get<T>(int key, T value)
        {
            if (lexHash.ContainsKey(key))
            {
                return (T)lexHash[key];
            }
            return value;
        }

        public T Get<T>(Property playerLives)
        {
            if (lexHash.ContainsKey((int)playerLives))
            {
                return (T)lexHash[(int)playerLives];
            }
            return default;
        }

        public bool ContainsKey(int key) => lexHash.ContainsKey(key);
        public void UpdateProperties(LexHashTable hash)
        {
            foreach (var entry in hash.lexHash)
            {
     //           Debug.LogWarning("Received hash " + (Property)entry.Key + " ;" + entry.Value);
                if (!lexHash.ContainsKey(entry.Key))
                {
                    lexHash.Add(entry.Key, entry.Value);
                }
                else
                {
                    lexHash[entry.Key] = entry.Value;
                }
            }
        }
        public void UpdateProperties(int key, object value)
        {

            if (!lexHash.ContainsKey(key))
            {
                lexHash.Add(key, value);
            }
            else
            {
                lexHash[key] = value;
            }

        }
        public void Clear() {
            lexHash.Clear();
            
        }
    }
}