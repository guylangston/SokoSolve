using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleZ.Win32
{
    public class IndexedDictionary<TA, TB> : IDictionary<TA, TB>
    {
        private int                   size;
        private TB[]                  indexedLookup;
        private Func<TA, int>         getIndex;
        private Func<int, TA>         getValue;
        private TB                    valueOfNotAssigned;
        private IEqualityComparer<TB> isEqual;

        public IndexedDictionary(int size, Func<TA, int> index, TB valueOfNotAssigned, IEqualityComparer<TB> isEqual, Func<int, TA> getValue)
        {
            this.size = size;
            getIndex = index;
            this.valueOfNotAssigned = valueOfNotAssigned;
            this.isEqual = isEqual;
            this.getValue = getValue;

            this.indexedLookup = new TB[size];
        }

        public TB this[TA key]
        {
            get => indexedLookup[getIndex(key)];
            set => indexedLookup[getIndex(key)] = value;
        }

        public void Add(TA                   key, TB value) => this[key] = value;
        public void Add(KeyValuePair<TA, TB> item) => this[item.Key] = item.Value;

        public void Clear() => ArrayHelper.Fill(indexedLookup, valueOfNotAssigned);

        public bool Contains(KeyValuePair<TA, TB> item) => isEqual.Equals(this[item.Key], item.Value);

        public void CopyTo(KeyValuePair<TA, TB>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TA, TB> item)
        {
            this[item.Key] = valueOfNotAssigned;
            return true;
        }

        public int  Count      => indexedLookup.Count(x => !isEqual.Equals(x, valueOfNotAssigned));
        public bool IsReadOnly => false;


        public bool ContainsKey(TA key) => !isEqual.Equals(this[key], valueOfNotAssigned);

        public bool Remove(TA key)
        {
            this[key] = valueOfNotAssigned;
            return true;
        }

        public bool TryGetValue(TA key, out TB value)
        {
            value = this[key];
            return  !isEqual.Equals(this[key], valueOfNotAssigned);
        }

        public ICollection<TA> Keys => this.Select(x => x.Key).ToList(); 
        public ICollection<TB> Values => this.Select(x => x.Value).ToList();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<KeyValuePair<TA, TB>> GetEnumerator()
        {
            for (int i = 0; i < size; i++)
            {
                var vv = indexedLookup[i];
                if (!isEqual.Equals(vv, valueOfNotAssigned))
                {
                    yield return new KeyValuePair<TA, TB>(getValue(i), vv);    
                }
            }
            
        }

        
    }
}