using System;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Views;
using System.Linq;

namespace Com.Krkadoni.Utils
{
    public class GenericRecyclerViewAdapter<T> : RecyclerView.Adapter
    {

        List<T> data;

        public List<T> Data
        {
            get { return data; }
            set
            { 
                data = value;
                selectedItems.Clear();
                NotifyDataSetChanged();
            }
        }

        internal HashSet<int> selectedItems;

        public Func<ViewGroup, int, RecyclerView.ViewHolder> FuncOnCreateViewHolder { get; set; }

        public Action<RecyclerView.ViewHolder, int> ActionOnBindViewHolder { get; set; }

        public GenericRecyclerViewAdapter()
        {
            selectedItems = new HashSet<int>();
            Data = new List<T>();
        }

        public GenericRecyclerViewAdapter(List<T> data)
        {
            selectedItems = new HashSet<int>();
            this.Data = data;
        }

        public GenericRecyclerViewAdapter(List<T> data, 
                                          Func<ViewGroup, int, RecyclerView.ViewHolder> funcOnCreateViewHolder, 
                                          Action<RecyclerView.ViewHolder, int> actionOnBindViewHolder
        )
        {
            selectedItems = new HashSet<int>();
            this.Data = data;
            FuncOnCreateViewHolder = funcOnCreateViewHolder;
            ActionOnBindViewHolder = actionOnBindViewHolder;
        }

        public virtual void Insert(int position, T item)
        {
            Data.Insert(position, item);
            NotifyItemInserted(position);
            NotifyDataSetChanged();
        }

        public virtual void InsertRange(int position, T[] items)
        {
            Data.AddRange(items);
            NotifyItemRangeInserted(position, items.Length);
            NotifyDataSetChanged();
        }

        public virtual void Add(T item)
        {
            var count = Data.Count;
            Data.Add(item);
            NotifyItemInserted(count);
            NotifyDataSetChanged();
        }

        public virtual void AddRange(T[] items)
        {
            var count = Data.Count;
            Data.AddRange(items);
            NotifyItemRangeInserted(count, items.Length);
            NotifyDataSetChanged();
        }

        public virtual void Delete(int position)
        {
            Data.RemoveAt(position);
            NotifyItemRemoved(position);
            NotifyDataSetChanged();
        }

        public virtual void Clear()
        {
            if (Data != null)
            {
                Data.Clear();
                NotifyDataSetChanged();
            }
        }

        public override int ItemCount
        {
            get { return Data != null ? Data.Count : 0; }
        }

        public T GetItem(int position)
        {
            if (Data != null && Data.Count > position && position >= 0)
            {
                return Data[position];
            }
            return default(T);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (FuncOnCreateViewHolder != null)
            {
                return FuncOnCreateViewHolder.Invoke(parent, viewType);
            }
            return null;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (ActionOnBindViewHolder != null)
            {
                ActionOnBindViewHolder.Invoke(holder, position);
            }
        }

        public void ToggleSelection(int pos)
        {
            if (selectedItems.Contains(pos))
            {
                selectedItems.Remove(pos);
            }
            else
            {
                if (data.Count > pos)
                    selectedItems.Add(pos);
            }
            NotifyItemChanged(pos);
        }

        public void ClearSelections()
        {
            selectedItems.Clear();
            NotifyDataSetChanged();
        }

        public int SelectedItemCount
        {
            get { return selectedItems.Count; }
        }

        public List<int> SelectedItems
        {
            get
            {
                int[] items = new int[selectedItems.Count];
                selectedItems.CopyTo(items);
                return  items.ToList();
            }
        }

    }
}

