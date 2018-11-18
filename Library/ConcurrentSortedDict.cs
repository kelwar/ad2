using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Library
{
    public class ConcurrentSortedDict<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region field

        private readonly ConcurrentDictionary<TKey, TValue> _dict;

        #endregion



        #region ctor

        public ConcurrentSortedDict()
        {
            _dict = new ConcurrentDictionary<TKey, TValue>();
        }

        public ConcurrentSortedDict(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            _dict = new ConcurrentDictionary<TKey, TValue>(collection);
        }

        public ConcurrentSortedDict(IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TValue>(comparer);
        }

        public ConcurrentSortedDict(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TValue>(collection, comparer);
        }

        #endregion




        public TValue this[TKey key]
        {
            get { return _dict[key]; }
            set { _dict[key] = value; }
        }

        public int Count => _dict.Count;
        public bool IsReadOnly => true;
        public ICollection<TKey> Keys
        {
            get
            {
                return SortDictionaryByKey().Select(payer => payer.Key).ToList();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return SortDictionaryByKey().Select(payer => payer.Value).ToList();
            }
        }


        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (!TryAdd(item.Key, item.Value))
                throw new Exception($"key={item.Key} alredy exist. Could not added");
        }

        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new Exception($"key={key} alredy exist. Could not added");
        }

        /// <summary>
        ///  Если элемент с таким ключем уже есть то возвращаетя false и элемент не добавляется
        /// </summary>
        public bool TryAdd(TKey key, TValue value)
        {
            return _dict.TryAdd(key, value);
        }



        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return _dict.AddOrUpdate(key, addValue, updateValueFactory);
        }


        /// <summary>
        /// Если ключ уже сушествует, то вызывается делегат, в котором не происходит никаких проверок, просто возвращается новый объект, который и записывается по ключу.
        /// </summary>
        public TValue AddOrUpdate(TKey key, TValue addValue)
        {
            return _dict.AddOrUpdate(key, addValue, (k, existingVal) => addValue);
        }


        /// <summary>
        /// Если ключ уже сушествует, то вызывается делегат, в котором проверяется изменяется текущий объект словаря или новый. Проверятся addValue.
        /// Если новый, то выкидывается исключение.
        /// Возвращается добавленное или обновленное значение.
        /// </summary>
        public TValue AddOrUpdateWithoutRetrieving(TKey key, TValue addValue)
        {
            _dict.AddOrUpdate(key, addValue,
                (k, existingVal) =>
                {
                    // Если объект новый, под существующим ключем. (не допускается дубликата) 
                    if (!addValue.Equals(existingVal))
                        throw new ArgumentException($"Duplicate city object are not allowed: {k}.");

                    return existingVal;
                });
            return addValue;
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dict.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue outValue;
            return _dict.TryRemove(item.Key, out outValue);
        }

        public bool Remove(TKey key)
        {
            TValue outValue;
            return _dict.TryRemove(key, out outValue);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            return _dict.TryRemove(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        /// <summary>
        /// Вернуть упорядоченное значение
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return SortDictionaryByKey().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SortDictionaryByKey().GetEnumerator();
        }

        private Dictionary<TKey, TValue> SortDictionaryByKey()
        {
            var snapshot = _dict.ToArray(); //Перечисляем копию, иначе при использовании OrderBy возможно исключение внутри OrderBy в методе CopyTo()
            var sorted = snapshot.OrderBy(c => c.Key);
            return sorted.ToDictionary(item => item.Key, item => item.Value);
        }
    }



}