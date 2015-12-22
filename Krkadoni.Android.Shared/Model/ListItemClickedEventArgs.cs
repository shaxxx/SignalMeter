using System;
using Android.Views;

namespace Com.Krkadoni.Utils
{
    public class ListItemClickedEventArgs<T> : EventArgs where T : class
    {
        public int Position { get; private set;}
        public T Item { get; private set;}
        public View View { get; private set;}

        public ListItemClickedEventArgs(int position, View view, T item)
        {
            Position = position;
            Item = item;
            this.View = view;
        }
    }
}

