using Android.Views.Animations;
using Com.Krkadoni.App.SignalMeter;
using Com.Krkadoni.Interfaces;
using Com.Krkadoni.Utils;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.Widget;
using Com.Krkadoni.App.SignalMeter.Utils;
using Com.Krkadoni.App.SignalMeter.Model;

namespace Com.Krkadoni.App.SignalMeter.Layout
{
    public class FragmentDrawer : RecyclerListFragment<NavDrawerItem>
    {

        private static string TAG = typeof(FragmentDrawer).Name;

        private Android.Support.V7.App.ActionBarDrawerToggle mDrawerToggle;
        private DrawerLayout mDrawerLayout;
        private View containerView;
        private static string[] titles = null;

        public FragmentDrawer()
        {
            FragmentName = typeof(FragmentDrawer).Name; 
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // drawer labels
            titles = Activity.Resources.GetStringArray(Resource.Array.nav_drawer_labels);
            ConnectionManager.GetInstance().PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "ConnectionStatus")
                {

                }
            };
        }

        public static List<NavDrawerItem> GetData()
        {
            List<NavDrawerItem> data = new List<NavDrawerItem>();

            // preparing navigation drawer items
            for (int i = 0; i < titles.Length; i++)
            {
                NavDrawerItem navItem = new NavDrawerItem();
                navItem.Title = titles[i];
                data.Add(navItem);
            }
            return data;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Inflating view layout
            View layout = inflater.Inflate(Resource.Layout.fragment_navigation_drawer, container, false);
            var drawerList = layout.FindViewById<RecyclerView>(Resource.Id.drawerList);

            ListItemResource = Resource.Layout.nav_drawer_row;
            ListItemTextResource = Resource.Id.title;
            ListItemSelectedColor = Resource.Color.navigationBarSelectedItem;
            ListItemColor = Resource.Color.navigationBarItem;

            InitializeList(drawerList);

            SetData(GetData());
            return layout;
        }

        protected override GenericRecyclerViewAdapter<NavDrawerItem> CreateAdapter()
        {
            var adapter = new GenericRecyclerViewAdapter<NavDrawerItem>();
            adapter.FuncOnCreateViewHolder = (ViewGroup parent, int viewType) =>
            {
                if (Activity == null || Activity.LayoutInflater == null)
                    return null;
                View view = Activity.LayoutInflater.Inflate(ListItemResource, parent, false);
                var holder = new TwoItemsRecyclerViewHolder<FontTextView, TextView>(view, Resource.Id.list_item_icon, ListItemTextResource);
                return holder;   
            };

            adapter.ActionOnBindViewHolder = (RecyclerView.ViewHolder holder, int position) =>
            {
                if (adapter.Data != null && adapter.ItemCount - 1 >= position)
                {
                    NavDrawerItem item = adapter.GetItem(position);
                    holder.ItemView.SetBackgroundResource(
                        adapter.SelectedItems.Contains(position) ? 
                            ListItemSelectedColor
                            : ListItemColor);    
                    BindData(holder, position, item);
                }                  
            };

            return adapter;
        }

        protected override void BindData(RecyclerView.ViewHolder holder, int position, NavDrawerItem item)
        {
            if (item != null)
            {
                FontTextView textView = ((TwoItemsRecyclerViewHolder<FontTextView, TextView>)holder).Item1;

                switch (position)
                {
                    
                    case 0:
                        textView.Text = GetString(Resource.String.fb_monitor);
                        break;
                    case 1:
                        textView.Text = GetString(Resource.String.fb_list);
                        break;
                    case 2:
                        textView.Text = GetString(Resource.String.fb_film2);
                        break;
                    case 3:
                        textView.Text = GetString(Resource.String.fb_chart4);
                        break;
                    default:
                        textView.Text = string.Empty;
                        break;
                }
                ((TwoItemsRecyclerViewHolder<FontTextView, TextView>)holder).Item2.Text = item.Title;
            }
        }

        protected override IOnClickListener CreateClickListener(GenericRecyclerViewAdapter<NavDrawerItem> adapter)
        {
            return new GenericOnClickListener((view, position) =>
                {
                    if (adapter != null && adapter.Data != null && adapter.Data.Count > position && position >= 0)
                    {
                        mDrawerLayout.CloseDrawer(containerView);
                        adapter.ClearSelections();
                        adapter.ToggleSelection(position);
                        OnListItemClicked(new ListItemClickedEventArgs<NavDrawerItem>(position, view, adapter.GetItem(position)));
                    }
                });
        }

        public void SetUpDrawer(int fragmentId, DrawerLayout drawerLayout, Android.Support.V7.Widget.Toolbar toolbar)
        {
            containerView = Activity.FindViewById(fragmentId);
            mDrawerLayout = drawerLayout;
            mDrawerToggle = new GenericActionBarDrawerToggle(Activity, drawerLayout, toolbar, Resource.String.drawer_open, Resource.String.drawer_close)
            {

                ActionOnDrawerOpened = (view) => Activity.SupportInvalidateOptionsMenu(),
                ActionOnDrawerClosed = (view) => Activity.SupportInvalidateOptionsMenu(),
                ActionOnDrawerSlide = (view, slideOffset) =>
                { 
                    if (((int)Android.OS.Build.VERSION.SdkInt) >= 11)
                    {
                        toolbar.Alpha = (1 - slideOffset / 2);
                    }
                    else
                    {
                        SetAlphaForView(toolbar, (1 - slideOffset / 2));   
                    }                
                }
            };

            mDrawerLayout.SetDrawerListener(mDrawerToggle);
            mDrawerLayout.Post(new GenericRunnable(mDrawerToggle.SyncState));

        }

        private void SetAlphaForView(View v, float alpha)
        {
            AlphaAnimation animation = new AlphaAnimation(alpha, alpha);
            animation.Duration = 0; 
            animation.FillAfter = true; 
            v.StartAnimation(animation);
        }
            
    }
}

