using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Krkadoni.App.SignalMeter;
using Android.Support.V7.Widget;
using Krkadoni.Enigma;
using Com.Krkadoni.Utils;
using Com.Krkadoni.App.SignalMeter.Model;
using System.Collections.Generic;
using Android.Util;
using Com.Krkadoni.App.SignalMeter.Utils;


namespace Com.Krkadoni.App.SignalMeter.Layout
{
    public class BouquetsFragment :  RecyclerListFragment<IBouquetItemBouquet>
    {
        private View view;
        private TextView lblCurrentProfile;
        private const string LIST_STATE = "listState";
        private IParcelable mListState;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ConnectionManager.GetInstance().PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "Bouquets")
                {
                    UpdateData();
                }
                else if (e.PropertyName == "SelectedBouquet")
                {
                    UpdateData();   
                }
                else if (e.PropertyName == "CurrentProfile")
                {
                    SetCurrentProfile();   
                }
            };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            base.OnCreateView(inflater, container, savedInstanceState);

            // Get the view from fragment xml
            view = inflater.Inflate(Resource.Layout.bouquets_fragment, container, false);
            lblCurrentProfile = view.FindViewById<TextView>(Resource.Id.lblCurrentProfile);
            lblCurrentProfile.Text = string.Empty;
            lblCurrentProfile.Click += (sender, e) => ((MainActivity)Activity).DisplayView(MainEventHandlers.ViewsEnum.Profiles);
            var bouquetsList = view.FindViewById<RecyclerView>(Resource.Id.BouquetsList);
            bouquetsList.AddItemDecoration(new DividerItemDecoration(Activity));
            InitializeList(bouquetsList);
            if (ConnectionManager.Bouquets != null)
                UpdateData();
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
            SetCurrentProfile();
        }

        public override void OnPause()
        {
            base.OnPause();
            if (ListView != null)
                mListState = ListView.GetLayoutManager().OnSaveInstanceState();
        }

        private void UpdateData()
        {
            if (Adapter == null || Adapter.Data == null)
                return;
            SetData((List<IBouquetItemBouquet>)ConnectionManager.Bouquets);
            var index = Adapter.Data.IndexOf(ConnectionManager.SelectedBouquet);
            if (index > -1 && !Adapter.SelectedItems.Contains(index))
                Adapter.ToggleSelection(index);
        }

        protected override void BindData(RecyclerView.ViewHolder holder, int position, IBouquetItemBouquet item)
        {
            if (item != null)
            {
                ((SimpleRecyclerViewHolder<TextView>)holder).Item.Text = item.Name;
            }
        }

        private void SetCurrentProfile()
        {
            try
            {
                if (this.View != null && ConnectionManager.CurrentProfile != null && lblCurrentProfile != null && ConnectionManager.CurrentProfile.Name != null)
                {
                    lblCurrentProfile.Text = ConnectionManager.CurrentProfile.Name;
                    return;
                }
                if (lblCurrentProfile != null)
                    lblCurrentProfile.Text = string.Empty; 
            }
            catch (Exception ex)
            {
                Log.Error("BouquetsFragment", string.Format("SetCurrentProfile failed with error {0}", ex.Message));  
            }
        }
    }
}

