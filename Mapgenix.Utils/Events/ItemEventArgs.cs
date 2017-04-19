using System;

namespace Mapgenix.Utils
{
    public class ItemEventArgs : EventArgs
    {
        private object _item;

        public ItemEventArgs()
            : this(null)
        { }

        public ItemEventArgs(object item)
        {
            this._item = item;
        }

        public object Item
        {
            get { return _item; }
            set { _item = value; }
        }
    }
}
