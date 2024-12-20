﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            while (values.Count < keys.Count)
            {
				values.Add(default);
			}

            for (int i = 0; i < keys.Count; i++)
			{
				if (this.ContainsKey(keys[i]))
				{
					this.Add(keys[i] is string ? (TKey)Convert.ChangeType("", typeof(TKey)) : default, values[i]);
				}
				else
				{
					this.Add(keys[i], values[i]);
				}
			}
        }
    }

}