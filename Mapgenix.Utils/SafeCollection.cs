using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Mapgenix.Utils
{
    [Serializable]
    public class SafeCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly Dictionary<string, T> _dictionary;
        private Dictionary<int, string> _mappingCache;

        public event EventHandler<ItemEventArgs> Inserting;
        public event EventHandler<ItemEventArgs> Inserted;
        public event EventHandler<ItemEventArgs> Removing;
        public event EventHandler<ItemEventArgs> Removed;
        public event EventHandler<ItemEventArgs> Adding;
        public event EventHandler<ItemEventArgs> Added;
        public event EventHandler<EventArgs> ClearingItems;
        public event EventHandler<EventArgs> ClearedItems;

       
        public SafeCollection()
        {
            _dictionary = new Dictionary<string, T>();
            _mappingCache = new Dictionary<int, string>();
        }

        protected virtual void OnInserting(ItemEventArgs e)
        {
            EventHandler<ItemEventArgs> handler = Inserting;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnInserted(ItemEventArgs e)
        {
            EventHandler<ItemEventArgs> handler = Inserted;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRemoving(ItemEventArgs e)
        {
            EventHandler<ItemEventArgs> handler = Removing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRemoved(ItemEventArgs e)
        {
            EventHandler<ItemEventArgs> handler = Removed;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAdding(ItemEventArgs e)
        {
            EventHandler<ItemEventArgs> handler = Adding;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAdded(ItemEventArgs e)
        {
            EventHandler<ItemEventArgs> handler = Added;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnClearingItems(EventArgs e)
        {
            EventHandler<EventArgs> handler = ClearingItems;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnClearedItems(EventArgs e)
        {
            EventHandler<EventArgs> handler = ClearedItems;

            if (handler != null)
            {
                handler(this, e);
            }
        }

      
        public T this[string key]
        {
            get { return _dictionary[key]; }
            set
            {
                Validators.CheckParameterIsNotNull(value, "value");

                _dictionary[key] = value;

                foreach (int index in _mappingCache.Keys)
                {
                    if (_mappingCache[index] == key)
                    {
                        base[index] = value;
                    }
                }
            }
        }

       
        public Collection<string> GetKeys()
        {
            Collection<string> newCollection = new Collection<string>();

            foreach (string name in _mappingCache.Values)
            {
                newCollection.Add(name);
            }

            return newCollection;
        }

      
        public void Add(string key, T item)
        {
            Validators.CheckParameterIsNotNull(key, "key");
            Validators.CheckParameterIsNotNull(item, "item");

            OnAdding(new ItemEventArgs(item));
            _dictionary.Add(key, item);
            base.Add(item);

            _mappingCache.Add(_dictionary.Count - 1, key);
            OnAdded(new ItemEventArgs(item));
        }

       
        public new void Add(T item)
        {
            Validators.CheckParameterIsNotNull(item, "item");

            Add(Guid.NewGuid().ToString(), item);
        }

        
        public new void Insert(int index, T item)
        {
            Validators.CheckParameterIsNotNull(item, "item");
            Validators.CheckIfInputValueIsInRange(index, "index", 0, Items.Count);
            
            string key = Guid.NewGuid().ToString();
            Insert(index, key, item);
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

       
        public void Insert(int index, string key, T item)
        {
            Validators.CheckParameterIsNotNull(item, "item");
            Validators.CheckIfInputValueIsInRange(index, "index", 0, Items.Count);
            Validators.CheckParameterIsNotNull(key, "key");

            OnInserting(new ItemEventArgs(item));
            base.Insert(index, item);

            _dictionary.Add(key, item);

            Dictionary<int, string> newMappingCache = new Dictionary<int, string>();
            for (int i = 0; i < Items.Count; i++)
            {
                if (i < index)
                {
                    newMappingCache.Add(i, _mappingCache[i]);
                }
                else if (i == index)
                {
                    newMappingCache.Add(index, key);
                }
                else
                {
                    newMappingCache.Add(i, _mappingCache[i - 1]);
                }
            }

            _mappingCache.Clear();
            _mappingCache = newMappingCache;
            OnInserted(new ItemEventArgs(item));
        }

      
        public void Remove(string key)
        {
            Validators.CheckParameterIsNotNull(key, "key");
            Validators.CheckIEnumerableIsEmptyOrNull(_mappingCache);

            foreach (int index in _mappingCache.Keys)
            {
                if (_mappingCache[index] == key)
                {
                    RemoveItem(index);
                    break;
                }
            }
        }

        
        protected override void RemoveItem(int index)
        {
            Validators.CheckIEnumerableIsEmptyOrNull(_mappingCache);
            Validators.CheckIfInputValueIsInRange(index, "index", 0, Items.Count - 1);

            T item = _dictionary[_mappingCache[index]];
            OnRemoving(new ItemEventArgs(item));

            Dictionary<int, string> newMappingCache = new Dictionary<int, string>();
            for (int i = 0; i < Items.Count; i++)
            {
                if (_mappingCache.ContainsKey(i))
                {
                    if (i > index)
                    {
                        newMappingCache.Add(i - 1, _mappingCache[i]);
                    }
                    else if (i != index)
                    {
                        newMappingCache.Add(i, _mappingCache[i]);
                    }
                }
            }

            if (_mappingCache.ContainsKey(index))
            {
                _dictionary.Remove(_mappingCache[index]);
            }

            _mappingCache.Clear();
            _mappingCache = newMappingCache;

            base.RemoveItem(index);
            OnRemoved(new ItemEventArgs(item));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        
        
        public bool Contains(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        protected override void ClearItems()
        {
            OnClearingItems(new EventArgs());
            base.ClearItems();
            _dictionary.Clear();
            _mappingCache.Clear();
            OnClearedItems(new EventArgs());
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        
        protected override void SetItem(int index, T item)
        {
            Validators.CheckIfInputValueIsInRange(index, "index", 0, Items.Count - 1);
            Validators.CheckParameterIsNotNull(item, "item");

            base.SetItem(index, item);

            if (_mappingCache.ContainsKey(index))
            {
                _dictionary[_mappingCache[index]] = item;
            }
        }



        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}
