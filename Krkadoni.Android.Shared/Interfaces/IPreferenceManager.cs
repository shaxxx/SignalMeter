using System;
using Android.Content;
using System.Collections.Generic;

namespace Com.Krkadoni.Interfaces
{
    public interface IPreferenceManager<T>
    {
       List<T> LoadItems(Context context);

        T LoadItem(Context context, String key);

        void SaveItem(Context context, T Item);

        void SaveItems(Context context, List<T> Items);

        void DeleteItem(Context context, String key);

    }
}

