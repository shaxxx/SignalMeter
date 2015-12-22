using Android.Support.V4.App;
using Com.Krkadoni.Interfaces;
using Android.Support.V7.Widget;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using Com.Krkadoni.Utils;

namespace Com.Krkadoni.App.SignalMeter.Layout
{
    public abstract class RecyclerListFragment<TItemObject> : Fragment 
        where TItemObject : class
    {
        protected int ListItemResource { get; set; }

        protected int ListItemTextResource { get; set; }

        protected int ListItemColor { get; set; }

        protected int ListItemSelectedColor { get; set; }

        public string FragmentName { get; protected set; }

        public RecyclerView ListView
        {
            get;
            private set;
        }

        public GenericRecyclerViewAdapter<TItemObject> Adapter
        {
            get
            {
                if (ListView == null)
                    return default( GenericRecyclerViewAdapter<TItemObject>);
                return (GenericRecyclerViewAdapter<TItemObject>)ListView.GetAdapter();
            }
        }

        public delegate void ListItemClickedHandler(object sender,ListItemClickedEventArgs<TItemObject> e);

        public event ListItemClickedHandler ListItemClicked;

        protected virtual void OnListItemClicked(ListItemClickedEventArgs<TItemObject> e)
        {
            if (ListItemClicked != null)
                ListItemClicked(this, e);
        }


        public delegate void ListItemLongClickedHandler(object sender,ListItemClickedEventArgs<TItemObject> e);

        public event ListItemLongClickedHandler ListItemLongClicked;

        protected virtual void OnListItemLongClicked(ListItemClickedEventArgs<TItemObject> e)
        {
            if (ListItemLongClicked != null)
                ListItemLongClicked(this, e);
        }


        protected virtual IOnClickListener CreateClickListener(GenericRecyclerViewAdapter<TItemObject> adapter)
        {
            return new GenericOnClickListener((view, position) =>
                {
                    if (adapter != null && adapter.Data != null)
                    {
                        adapter.ClearSelections();
                        adapter.ToggleSelection(position);
                        OnListItemClicked(new ListItemClickedEventArgs<TItemObject>(position, view, adapter.GetItem(position)));
                    }
                },
                (view, position) =>
                {
                    if (adapter != null && adapter.Data != null)
                    {
                        OnListItemLongClicked(new ListItemClickedEventArgs<TItemObject>(position, view, adapter.GetItem(position)));
                    }
                }
            );
        }

        protected virtual GenericRecyclerViewAdapter<TItemObject> CreateAdapter()
        {
            var adapter = new GenericRecyclerViewAdapter<TItemObject>();
            adapter.FuncOnCreateViewHolder = (ViewGroup parent, int viewType) =>
            {
                View view = Activity.LayoutInflater.Inflate(ListItemResource, parent, false);
                var holder = new SimpleRecyclerViewHolder<TextView>(view, ListItemTextResource);
                return holder;   
            };

            adapter.ActionOnBindViewHolder = (RecyclerView.ViewHolder holder, int position) =>
            {
                if (adapter.Data != null && adapter.ItemCount - 1 >= position)
                {
                    TItemObject item = adapter.GetItem(position);
                    holder.ItemView.SetBackgroundResource(
                        adapter.SelectedItems.Contains(position) ? 
                            ListItemSelectedColor
                            : ListItemColor);    
                    BindData(holder, position, item);
                }                  
            };
           
            return adapter;
        }

        protected RecyclerListFragment()
        {

//            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.IceCreamSandwich)
//            {
//                ListItemResource = Android.Resource.Layout.SimpleListItemActivated1;
//                ListItemSelectedColor = Resource.Color.listSelectedItem;
//                ListItemTextResource = Android.Resource.Id.Text1;
//            }
//            else
//            {
            ListItemResource = Resource.Layout.simple_list_item_activated;
            ListItemSelectedColor = Resource.Color.listSelectedItem;
            ListItemTextResource = Resource.Id.text1;
//            }
           
           
            ListItemColor = Resource.Color.listItem;

        }

        protected RecyclerListFragment(int listItemResource, int listItemTextResource)
        {
            ListItemResource = listItemResource;
            ListItemTextResource = listItemTextResource;
            ListItemColor = Resource.Color.listItem;
            ListItemSelectedColor = Resource.Color.listSelectedItem;
        }

        protected abstract void BindData(RecyclerView.ViewHolder holder, int position, TItemObject item);

        protected void InitializeList(RecyclerView listView)
        {
            if (listView != null)
            {
                ListView = listView;
                var adapter = CreateAdapter();
                var clickListener = CreateClickListener(adapter);
                listView.AddOnItemTouchListener(new RecyclerTouchListener(Activity, listView, clickListener));
                listView.SetAdapter(adapter);
                listView.SetLayoutManager(new LinearLayoutManager(Activity)); 
            }
           
        }

        public void SetData(List<TItemObject> data)
        {           
            if (Adapter != null)
            {
                //Adapter.Clear();
                if (data != null)
                {
                    Adapter.Data = data;
                }
            }
        }
            
    }
}

