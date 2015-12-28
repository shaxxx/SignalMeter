using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Support.V4.Widget;
using Android.Widget;
using Com.Krkadoni.App.SignalMeter.Layout;
using Com.Krkadoni.App.SignalMeter.Model;
using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using Android.Content;
using Com.Krkadoni.App.SignalMeter.Utils;
using Com.Krkadoni.Utils;
using Xamarin;
using Android.Util;
using System.Threading.Tasks;


namespace Com.Krkadoni.App.SignalMeter.Layout
{
    [Activity(Name = "com.krkadoni.app.signalmeter.layout.MainActivity", 
        Label = "@string/AppName", 
        MainLauncher = true, 
        Icon = "@drawable/icon", 
        LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    public class MainActivity : AppCompatActivity
    {
        
        #region "Private Declarations"

        private const string selectedViewKey = "selectedView";

        private const string TAG = "MainActivity";

        GlobalApp app;

        private Android.Support.V7.Widget.Toolbar mToolbar;

        private FragmentDrawer drawerFragment;

        private ProfilesFragment profilesFragment;

        private BouquetsFragment bouquetsFragment;

        private ServicesFragment servicesFragment;

        private SignalFragment signalFragment;

        private MainEventHandlers.ViewsEnum selectedView = MainEventHandlers.ViewsEnum.Profiles;

        private bool backButtonPressed;

        private IMenuItem screenShotMenu;

        private IMenuItem streamMenu;

        private IMenuItem restartMenu;

        private IMenuItem sleepMenu;

        private IMenuItem aboutMenu;

        private IMenuItem coinsMenu;

        private bool IsTabletLandscapeLayout;

        private StreamManager streamManager;

        private MainEventHandlers eventHandlers;

        private void ResetFragmentPositions()
        {
            var fragment = SupportFragmentManager.FindFragmentById(Resource.Id.container_body);
            if (fragment != null)
            {
                SupportFragmentManager.BeginTransaction().Remove(fragment).Commit();
            }
            if (fragment != null)
            {
                bool matched = false;
                var tProfiles = fragment as ProfilesFragment;
                if (tProfiles != null)
                {
                    profilesFragment = tProfiles;
                    matched = true;
                }
                if (!matched)
                {
                    var tBouquets = fragment as BouquetsFragment;
                    if (tBouquets != null)
                    {
                        bouquetsFragment = tBouquets;
                        matched = true;
                    }
                }
                if (!matched)
                {
                    var tServices = fragment as ServicesFragment;
                    if (tServices != null)
                    {
                        servicesFragment = tServices;
                        matched = true;
                    }
                }
                if (!matched)
                {
                    var tSignal = fragment as SignalFragment;
                    if (tSignal != null)
                    {
                        signalFragment = tSignal;
                    }
                }
            }
            if (profilesFragment == null)
            {
                profilesFragment = (ProfilesFragment)SupportFragmentManager.FindFragmentById(Resource.Id.profiles_layout);
                if (profilesFragment != null)
                {
                    SupportFragmentManager.BeginTransaction().Remove(profilesFragment).Commit();
                }
            }
            if (bouquetsFragment == null)
            {
                bouquetsFragment = (BouquetsFragment)SupportFragmentManager.FindFragmentById(Resource.Id.bouquets_layout);
                if (bouquetsFragment != null)
                {
                    SupportFragmentManager.BeginTransaction().Remove(bouquetsFragment).Commit();
                }
            }
            if (servicesFragment == null)
            {
                servicesFragment = (ServicesFragment)SupportFragmentManager.FindFragmentById(Resource.Id.services_layout);
                if (servicesFragment != null)
                {
                    SupportFragmentManager.BeginTransaction().Remove(servicesFragment).Commit();
                }
            }
            if (signalFragment == null)
            {
                signalFragment = (SignalFragment)SupportFragmentManager.FindFragmentById(Resource.Id.signal_layout);
                if (signalFragment != null)
                {
                    SupportFragmentManager.BeginTransaction().Remove(signalFragment).Commit();
                }
            }
            SupportFragmentManager.ExecutePendingTransactions();
            if (profilesFragment == null)
                profilesFragment = new ProfilesFragment();
            if (bouquetsFragment == null)
                bouquetsFragment = new BouquetsFragment();
            if (servicesFragment == null)
                servicesFragment = new ServicesFragment();
            if (signalFragment == null)
                signalFragment = new SignalFragment();
        }

        private bool savedInstanceState;

        #endregion

        #region "Activity LifeCycle"

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            InitializeInsights();

            //Read application wide values
            app = ((GlobalApp)this.ApplicationContext);
            app.Settings = ((PreferenceManager)app.PreferenceManager).LoadSettings(this);
            savedInstanceState = false;
            if (bundle != null)
            {
                selectedView = (MainEventHandlers.ViewsEnum)bundle.GetInt(selectedViewKey);
            }

            var tapjoyPayload = Intent.GetStringExtra(Tapjoy.Tapjoy.IntentExtraPushPayload);
            if (tapjoyPayload != null)
            {
                TapjoyManager.HandlePushPayload(tapjoyPayload);
            }

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main_material);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(mToolbar);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            IsTabletLandscapeLayout = (FindViewById<LinearLayout>(Resource.Id.tablet_layout) != null);
            ResetFragmentPositions();
            if (!IsTabletLandscapeLayout && drawerFragment == null)
            {
                drawerFragment = (FragmentDrawer)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_navigation_drawer);
                drawerFragment.SetUpDrawer(Resource.Id.fragment_navigation_drawer, FindViewById<DrawerLayout>(Resource.Id.drawer_layout), mToolbar);
                drawerFragment.ListItemClicked += (sender, e) =>
                {
                    if (ConnectionManager.Connected)
                        DisplayView((MainEventHandlers.ViewsEnum)e.Position); 
                };
            }

            ReadSatellitesXml();

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            streamMenu = menu.FindItem(Resource.Id.action_stream);
            screenShotMenu = menu.FindItem(Resource.Id.action_screenshot);
            sleepMenu = menu.FindItem(Resource.Id.action_sleep);
            restartMenu = menu.FindItem(Resource.Id.action_restart);
            aboutMenu = menu.FindItem(Resource.Id.action_about);
            coinsMenu = menu.FindItem(Resource.Id.action_coins);
            SetMenuVisibility();
            return true;
        }

        protected override void OnResume()
        {
            base.OnResume();
            app.ActivityResumed = true;
        }

        protected override void OnStart()
        {
            base.OnStart();
            app.ActivityStarted = true;

            if (eventHandlers == null)
            {
                eventHandlers = new MainEventHandlers(this);
                eventHandlers.DisplayView = this.DisplayView;
                eventHandlers.SetMenuVisibility = () => this.SetMenuVisibility();
                profilesFragment.ListItemClicked += eventHandlers.ProfileSelectedHandler;
                bouquetsFragment.ListItemClicked += eventHandlers.BouquetSelectedHandler;
                servicesFragment.ListItemClicked += eventHandlers.ServiceSelectedHandler;
                servicesFragment.ListItemLongClicked += eventHandlers.ServiceLongClickedHandler;
                ConnectionManager.GetInstance().PropertyChanged -= eventHandlers.ConnectionManager_PropertyChanged;
                ConnectionManager.GetInstance().PropertyChanged += eventHandlers.ConnectionManager_PropertyChanged;
                ConnectionManager.GetInstance().ExceptionRaised -= eventHandlers.ConnectionManager_ExceptionRaised;
                ConnectionManager.GetInstance().ExceptionRaised += eventHandlers.ConnectionManager_ExceptionRaised;
            }

            if (streamManager == null)
                streamManager = new StreamManager(this);

            Tapjoy.Tapjoy.OnActivityStart(this);

            if (!ConnectionManager.Connected)
                DisplayView(MainEventHandlers.ViewsEnum.Profiles);
            else
                DisplayView(selectedView);
            if (ConnectionManager.Connected)
                ConnectionManager.CurrentServiceMonitor.Start();

            if (ConnectionManager.ConnectionStatus == ConnectionManager.ConnectionStatusEnum.Connecting)
                eventHandlers.ShowProgressDialog();
        }

        protected override void OnStop()
        {
            app.ActivityStopped = true;
            ConnectionManager.CurrentServiceMonitor.Stop();
            Tapjoy.Tapjoy.OnActivityStop(this);
            base.OnStop();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            savedInstanceState = true;
            base.OnSaveInstanceState(outState);
            outState.PutInt(selectedViewKey, (int)selectedView);
        }

        protected override void OnDestroy()
        {
            if (eventHandlers != null)
            {
                eventHandlers.HideProgressDialog();
            }
            if (backButtonPressed)
            {
                backButtonPressed = false;
                ConnectionManager.Disconnect();
                ConnectionManager.SelectedProfile = null;
                Finish();
                Process.KillProcess(Process.MyPid());
            }
            base.OnDestroy();
        }

        public override void OnBackPressed()
        {
            if (selectedView == MainEventHandlers.ViewsEnum.Profiles || IsTabletLandscapeLayout)
            {
                backButtonPressed = true;
                base.OnBackPressed();
            }
            else
            {
                DisplayView(MainEventHandlers.ViewsEnum.Profiles);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            var payload = intent.GetStringExtra(Tapjoy.Tapjoy.IntentExtraPushPayload);
            if (payload != null)
            {
                TapjoyManager.HandlePushPayload(payload);
            }
        }

        #endregion

        #region "Event Handlers"

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.
            int id = item.ItemId;
            if (id == Resource.Id.action_stream)
            {
                streamManager.InitStream(ConnectionManager.CurrentService);
                return true;
            }
            else if (id == Resource.Id.action_screenshot)
            {
                ;
                InitScreenShot();
                return true;
            }
            else if (id == Resource.Id.action_sleep)
            {
                SleepProfile();
                return true;
            }
            else if (id == Resource.Id.action_restart)
            {
                RestartGui();
                return true;
            }
            else if (id == Resource.Id.action_coins)
            {
                if (TapjoyManager.StreamPlacementAvailable)
                    TapjoyManager.ShowStreamPlacement();
            }
            else if (id == Resource.Id.action_about)
            {
                ShowAbout();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void SetMenuVisibility(bool forceHide = false)
        {
            if (!app.ActivityStarted)
                return;
            if (ConnectionManager.GetInstance() == null)
                return;
            if (TapjoyManager.GetInstance() == null)
                return;
            if (screenShotMenu != null)
                screenShotMenu.SetVisible(ConnectionManager.Connected && !forceHide);
            if (streamMenu != null && screenShotMenu != null)
                streamMenu.SetVisible(screenShotMenu.IsVisible && ConnectionManager.CurrentProfile != null && ConnectionManager.CurrentProfile.Streaming && !TapjoyManager.ConnectFailed);
            if (screenShotMenu != null && sleepMenu != null)
                sleepMenu.SetVisible(screenShotMenu.IsVisible);
            if (screenShotMenu != null && restartMenu != null)
                restartMenu.SetVisible(screenShotMenu.IsVisible);
            if (coinsMenu != null)
                coinsMenu.SetVisible(TapjoyManager.StreamPlacementAvailable);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            SetMenuVisibility();
        }

        #endregion

        #region "Private Methods"

        private async void InitializeInsights()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                return;

            await Task.Delay(TimeSpan.FromSeconds(2));

            try
            {
                var key = FileSystem.ReadStringAsset(this, GetString(Resource.String.insights_key));
                if (!string.IsNullOrEmpty(key))
                    Insights.Initialize(key, this.ApplicationContext); 
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("Failed to initialize Xamarin Insights! {0}", ex.Message));
            }
        }

        private async void SleepProfile()
        {
            if (!ConnectionManager.Connected || ConnectionManager.CurrentProfile == null)
                return;
            await ConnectionManager.SendToSleepAndDisconnectCurrentProfile();
            DisplayView(MainEventHandlers.ViewsEnum.Profiles);
            Toast.MakeText(this, GetString(Resource.String.inf_disconnected), ToastLength.Short).Show();

        }

        private async void InitScreenShot()
        {
            var screenShotActivity = new Intent(this, typeof(ScreenShotActivity));
            StartActivity(screenShotActivity);
        }

        private void RestartGui()
        {
            
            if (!ConnectionManager.Connected || ConnectionManager.CurrentProfile == null)
                return;

            var message = string.Format(GetString(Resource.String.question_restart_gui), ConnectionManager.CurrentProfile.Name);
            Action<object, DialogClickEventArgs> positiveAction = (sender, e) =>
            {
                ConnectionManager.RestartAndDisconnectCurrentProfile();
                SetMenuVisibility(true);
                DisplayView(MainEventHandlers.ViewsEnum.Profiles);
                Toast.MakeText(this, GetString(Resource.String.inf_disconnected), ToastLength.Short).Show();
            };
            Dialogs.QuestionDialog(this, message, positiveAction, (sender, e) =>
                {
                }).Show();
        }

        private void ReadSatellitesXml()
        {
            string xml = string.Empty;
            Dictionary<int, string> satellites = new Dictionary<int, string>();
            ConnectionManager.Satellites = satellites;
            xml = FileSystem.ReadStringAsset(this, GetString(Resource.String.satellites_xml_path));
            if (!string.IsNullOrEmpty(xml))
            {
                using (var reader = XmlReader.Create(new StringReader(xml)))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "sat" && reader.AttributeCount >= 2)
                                {
                                    var satName = reader.GetAttribute("name");
                                    var satPosition = reader.GetAttribute("position");
                                    var positionInt = 0;
                                    if (int.TryParse(satPosition, out positionInt))
                                    {
                                        if (!satellites.ContainsKey(positionInt))
                                            satellites.Add(positionInt, satName);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void ShowAbout()
        {
            var aboutActivity = new Intent(this, typeof(AboutActivity));
            StartActivity(aboutActivity);
        }

        #endregion

        public void DisplayView(MainEventHandlers.ViewsEnum position)
        {
            string title = GetString(Resource.String.AppName);
            Android.Support.V4.App.Fragment fragment;

            if (!app.ActivityStarted)
            {
                selectedView = position;
                return;
            }

            if (savedInstanceState)
            {
                return;
            }

            if (!IsTabletLandscapeLayout)
            {
                switch (position)
                {
                    case MainEventHandlers.ViewsEnum.Profiles:
                        fragment = profilesFragment;
                        title = GetString(Resource.String.ProfileTitle);
                        break;
                    case MainEventHandlers.ViewsEnum.Bouquets:
                        fragment = bouquetsFragment;
                        title = GetString(Resource.String.BouquetsTitle);
                        break;
                    case MainEventHandlers.ViewsEnum.Services:
                        fragment = servicesFragment;
                        title = GetString(Resource.String.ServicesTitle);
                        break;
                    case MainEventHandlers.ViewsEnum.Signal:
                        fragment = signalFragment;
                        title = GetString(Resource.String.SignalTitle);
                        break;
                    default:
                        fragment = profilesFragment;
                        title = GetString(Resource.String.ProfileTitle);
                        break;
                }
                selectedView = position;

                if (savedInstanceState)
                    return;
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.container_body, fragment).CommitAllowingStateLoss();
                SupportFragmentManager.ExecutePendingTransactions();
                drawerFragment.Adapter.ClearSelections();
                drawerFragment.Adapter.ToggleSelection((int)position);
                SupportActionBar.Title = title;
            }
            else
            {
                if (savedInstanceState)
                    return;   
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.profiles_layout, profilesFragment).CommitAllowingStateLoss();
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.bouquets_layout, bouquetsFragment).CommitAllowingStateLoss();
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.services_layout, servicesFragment).CommitAllowingStateLoss();
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.signal_layout, signalFragment).CommitAllowingStateLoss();
                SupportFragmentManager.ExecutePendingTransactions();
                SupportActionBar.Title = title;

            }
        }

    }
}
