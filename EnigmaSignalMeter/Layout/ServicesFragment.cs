using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Krkadoni.App.SignalMeter;
using Krkadoni.Enigma;
using Android.Support.V7.Widget;
using Com.Krkadoni.Utils;
using Com.Krkadoni.App.SignalMeter.Model;
using System.Collections.Generic;
using System.Linq;
using Com.Krkadoni.App.SignalMeter.Utils;
using Android.Graphics;
using Android.Util;

namespace Com.Krkadoni.App.SignalMeter.Layout
{
    public class ServicesFragment : RecyclerListFragment<IBouquetItem>
    {
        private View view;
        private TextView txtCurrentBouquet;
        private const string LIST_STATE = "listState";
        private IParcelable mListState;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ConnectionManager.GetInstance().PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "BouquetItems")
                {
                    UpdateData();
                    SetService();
                }
                else if (e.PropertyName == "CurrentService")
                {
                    SetService();
                }
                else if (e.PropertyName == "SelectedBouquet")
                {
                    SetCurrentBouquetText();
                }
            };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            base.OnCreateView(inflater, container, savedInstanceState);

            view = inflater.Inflate(Resource.Layout.services_fragment, container, false);
            txtCurrentBouquet = view.FindViewById<TextView>(Resource.Id.lblCurrentBouquet);
            txtCurrentBouquet.Text = string.Empty;
            txtCurrentBouquet.Click += (sender, e) => ((MainActivity)Activity).DisplayView(MainEventHandlers.ViewsEnum.Bouquets);
            var servicesList = view.FindViewById<RecyclerView>(Resource.Id.ServicesList);
            servicesList.AddItemDecoration(new DividerItemDecoration(Activity));
            ListItemResource = Resource.Layout.marker_row;
            ListItemTextResource = Resource.Id.marker_title;
            InitializeList(servicesList);
            if (ConnectionManager.BouquetItems != null)
            {
                UpdateData();
                SetService();
            }
            return view;   
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            if (mListState != null)
                ListView.GetLayoutManager().OnRestoreInstanceState(mListState);
        }

        public override void OnStart()
        {
            base.OnStart();
            SetCurrentBouquetText();
        }

        public override void OnPause()
        {
            base.OnPause();
            if (ListView != null)
                mListState = ListView.GetLayoutManager().OnSaveInstanceState();
        }

        private void UpdateData()
        {
            SetData((List<IBouquetItem>)ConnectionManager.BouquetItems);
        }

        private void SetService()
        {
            if (Adapter == null || Adapter.Data == null)
                return;
            
            Adapter.ClearSelections();
            if (ConnectionManager.CurrentService != null && Adapter.ItemCount > 0)
            {
                var currentAdapterService = Adapter.Data.OfType<IBouquetItemService>().SingleOrDefault(x => x.Reference == ConnectionManager.CurrentService.Reference);
                if (currentAdapterService != null)
                {
                    var index = Adapter.Data.IndexOf(currentAdapterService);
                    if (index > -1 && !Adapter.SelectedItems.Contains(index))
                        Adapter.ToggleSelection(index);
                }
            }
        }

        protected override GenericRecyclerViewAdapter<IBouquetItem> CreateAdapter()
        {
            var adapter = new GenericRecyclerViewAdapter<IBouquetItem>();
            adapter.FuncOnCreateViewHolder = (ViewGroup parent, int viewType) =>
            {
                View view = Activity.LayoutInflater.Inflate(ListItemResource, parent, false);
                var holder = new TwoItemsRecyclerViewHolder<FontTextView, TextView>(view, Resource.Id.marker_icon, ListItemTextResource);
                return holder;   
            };

            adapter.ActionOnBindViewHolder = (RecyclerView.ViewHolder holder, int position) =>
            {
                if (adapter.Data != null && adapter.ItemCount - 1 >= position && holder != null)
                {
                    IBouquetItem item = adapter.GetItem(position);
                    if (item != null)
                    {
                        holder.ItemView.SetBackgroundResource(
                            adapter.SelectedItems.Contains(position) ? 
                                ListItemSelectedColor
                                : ListItemColor);    
                        BindData(holder, position, item); 
                    }
                }                  
            };

            return adapter;
        }

        protected override Com.Krkadoni.Interfaces.IOnClickListener CreateClickListener(GenericRecyclerViewAdapter<IBouquetItem> adapter)
        {
            return new GenericOnClickListener((view, position) =>
                {
                    if (adapter != null && adapter.Data != null && adapter.Data.Count > position && position >= 0)
                    {
                        var item = adapter.GetItem(position);
                        if (item is IBouquetItemService)
                        {
                            adapter.ClearSelections();
                            adapter.ToggleSelection(position);
                            OnListItemClicked(new ListItemClickedEventArgs<IBouquetItem>(position, view, adapter.GetItem(position)));
                        }
                    }
                },
                (view, position) =>
                {
                    if (adapter != null && adapter.Data != null)
                    {
                        OnListItemLongClicked(new ListItemClickedEventArgs<IBouquetItem>(position, view, adapter.GetItem(position)));
                    }
                }
            );
        }

        protected override void BindData(RecyclerView.ViewHolder holder, int position, IBouquetItem item)
        {
            if (item != null && holder != null)
            {
                var twoHold = (TwoItemsRecyclerViewHolder<FontTextView, TextView>)holder;
                if (twoHold.Item1 != null && twoHold.Item2 != null)
                {
                    twoHold.Item2.Text = item.Name;         
                    twoHold.Item1.Visibility = item is IBouquetItemMarker ? ViewStates.Visible : ViewStates.Invisible;
                    twoHold.Item2.SetTypeface(null, item is IBouquetItemMarker ? TypefaceStyle.Bold : TypefaceStyle.Normal);  
                }
            }
        }

        private void SetCurrentBouquetText()
        {
            try
            {
                if (this.View != null && ConnectionManager.SelectedBouquet != null && txtCurrentBouquet != null && ConnectionManager.SelectedBouquet.Name != null)
                {
                    txtCurrentBouquet.Text = ConnectionManager.SelectedBouquet.Name;
                    return;
                }
                if (txtCurrentBouquet != null)
                    txtCurrentBouquet.Text = string.Empty; 
            }
            catch (Exception ex)
            {
                Log.Error("ServicesFragment", string.Format("SetCurrentBouquetText failed with error {0}", ex.Message));  
            }
        }
    }
}

