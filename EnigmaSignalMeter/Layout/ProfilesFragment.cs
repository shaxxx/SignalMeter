using System;
using Android.Support.V7.Widget;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Com.Krkadoni.App.SignalMeter;
using Com.Krkadoni.Utils;
using Com.Krkadoni.App.SignalMeter.Model;
using Com.Krkadoni.Interfaces;
using Android.Content;
using System.IO;
using Krkadoni.Enigma.Enums;
using Com.Krkadoni.App.SignalMeter.Utils;


namespace Com.Krkadoni.App.SignalMeter.Layout
{
    public sealed class ProfilesFragment : RecyclerListFragment<SignalMeterProfile>
    {
        private View layout;
        private const string LIST_STATE = "listState";
        private IParcelable mListState;
        private Android.Support.V7.Widget.Toolbar bottomToolbar;
        private IMenuItem addProfile;
        private IMenuItem editProfile;
        private IMenuItem deleteProfile;
        private IMenuItem sleepProfile;
        public const string profileNameKey = "EDIT_PROFILE_NAME";
        public const int addProfileRequestCode = 1;
        public const int editProfileRequestCode = 2;
        private const string testProfilesFileName = "profiles.txt";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ConnectionManager.GetInstance().PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "SelectedProfile")
                {
                    SetSelected();
                    UpdateMenuVisibility();
                }
                else if (e.PropertyName == "Profiles")
                {
                    SetData(ConnectionManager.Profiles);    
                    SetSelected();   
                    UpdateMenuVisibility();
                }
                else if (e.PropertyName == "ConnectionStatus")
                {
                    UpdateMenuVisibility();
                }
            };
        }

        private void SetSelected()
        {
            if (Adapter == null || Adapter.Data == null)
                return;

            Adapter.ClearSelections();

            if (ConnectionManager.SelectedProfile == null)
            {
                return;
            }

            var index = Adapter.Data.IndexOf(ConnectionManager.SelectedProfile);
            if (index > -1 && !Adapter.SelectedItems.Contains(index))
                Adapter.ToggleSelection(index);             
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            layout = (View)inflater.Inflate(Resource.Layout.profile_fragment, container, false);

            var profilesList = layout.FindViewById<RecyclerView>(Resource.Id.ProfileList);
            profilesList.AddItemDecoration(new DividerItemDecoration(Activity));
            InitializeList(profilesList);
            ListItemLongClicked += (sender, e) => RemoveProfile(e.Item);
            SetData(ConnectionManager.Profiles);
            bottomToolbar = layout.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_bottom);
            bottomToolbar.InflateMenu(Resource.Menu.menu_profiles);
            addProfile = bottomToolbar.Menu.FindItem(Resource.Id.action_add_profile);
            editProfile = bottomToolbar.Menu.FindItem(Resource.Id.action_edit_profile);
            deleteProfile = bottomToolbar.Menu.FindItem(Resource.Id.action_delete_profile);
            sleepProfile = bottomToolbar.Menu.FindItem(Resource.Id.action_sleep);

            SetSelected();
            UpdateMenuVisibility();

            bottomToolbar.MenuItemClick += (sender, e) =>
            {
                if (e.Item == addProfile)
                {
                    AddProfile();
                    e.Handled = true;
                }
                else if (e.Item == editProfile)
                {

                    EditProfile();
                    e.Handled = true;
                }
                else if (e.Item == deleteProfile)
                {
                    RemoveProfile(ConnectionManager.SelectedProfile);
                    e.Handled = true;
                }
            };

            return layout;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            if (mListState != null)
                ListView.GetLayoutManager().OnRestoreInstanceState(mListState);
            
            if (ConnectionManager.Profiles == null)
            {
                var app = ((GlobalApp)Activity.ApplicationContext);
                var profiles = app.PreferenceManager.LoadItems(Activity);
                profiles.AddRange(LoadTestProfiles());
                ConnectionManager.Profiles = profiles;
            }
        }

        public override void OnPause()
        {
            base.OnPause();
            if (ListView != null)
                mListState = ListView.GetLayoutManager().OnSaveInstanceState();
        }

        protected override IOnClickListener CreateClickListener(GenericRecyclerViewAdapter<SignalMeterProfile> adapter)
        {
            return new GenericOnClickListener((view, position) =>
                {
                    if (adapter != null && adapter.Data != null &&
                        (ConnectionManager.ConnectionStatus == ConnectionManager.ConnectionStatusEnum.Connected) ||
                        (ConnectionManager.ConnectionStatus == ConnectionManager.ConnectionStatusEnum.Disconnected) ||
                        (ConnectionManager.ConnectionStatus == ConnectionManager.ConnectionStatusEnum.Errored))
                    {
                        adapter.ClearSelections();
                        adapter.ToggleSelection(position);
                        OnListItemClicked(new ListItemClickedEventArgs<SignalMeterProfile>(position, view, adapter.GetItem(position)));
                    }
                },
                (view, position) =>
                {
                    if (adapter != null && adapter.Data != null)
                    {
                        OnListItemLongClicked(new ListItemClickedEventArgs<SignalMeterProfile>(position, view, adapter.GetItem(position)));
                    }
                });
        }

        protected override void BindData(RecyclerView.ViewHolder holder, int position, SignalMeterProfile item)
        {
            if (item != null && holder != null)
            {
                ((SimpleRecyclerViewHolder<TextView>)holder).Item.Text = item.Name;
            }
        }

        private void UpdateMenuVisibility()
        {
            if (addProfile != null)
            addProfile.SetVisible(true);

            if (editProfile == null || deleteProfile == null)
                return;

            if (Adapter != null && Adapter.Data != null && Adapter.SelectedItems.Count > 0)
            {
                editProfile.SetVisible(true);
                deleteProfile.SetVisible(true); 
            }
            else
            {
                editProfile.SetVisible(false);
                deleteProfile.SetVisible(false); 
            }
        }

        private void AddProfile()
        {
            var profileEditActivity = new Intent(Activity, typeof(ProfileEditActivity));
            StartActivityForResult(profileEditActivity, addProfileRequestCode);
        }

        private void EditProfile()
        {
            var profileEditActivity = new Intent(Activity, typeof(ProfileEditActivity));
            profileEditActivity.PutExtra(profileNameKey, ConnectionManager.SelectedProfile.Name);
            profileEditActivity.PutExtra(profileNameKey, ConnectionManager.SelectedProfile.Name);
            StartActivityForResult(profileEditActivity, editProfileRequestCode);
        }

        private void RemoveProfile(SignalMeterProfile profile)
        {
            if (Adapter == null || Adapter.Data == null)
                return;           

            if (profile == null)
                return;

            var position = Adapter.Data.IndexOf(profile);

            if (position < 0)
                return;
            var message = string.Format(GetString(Resource.String.question_delete_profile), profile);
            Action<object, DialogClickEventArgs> positiveAction = (sender, args) =>
                {
                    if (ConnectionManager.Connected && ConnectionManager.CurrentProfile == profile)
                    {
                        ConnectionManager.Disconnect();
                        Toast.MakeText(Activity, GetString(Resource.String.inf_disconnected), ToastLength.Short).Show();
                    }
                    var app = (GlobalApp)Activity.ApplicationContext;
                    app.PreferenceManager.DeleteItem(Activity, profile.Name);
                    Adapter.Delete(position);
                    Adapter.SelectedItems.Clear();
                    ConnectionManager.SelectedProfile = null;
                };
            Dialogs.QuestionDialog(Activity,message,positiveAction, (sender, args) => {}).Show();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {

            if (requestCode == addProfileRequestCode)
            {
                if ((Android.App.Result)resultCode == Android.App.Result.Ok)
                {
                    Adapter.NotifyDataSetChanged();
                }
            }
            else if (requestCode == editProfileRequestCode)
            {
                if ((Android.App.Result)resultCode == Android.App.Result.Ok)
                {
                    Adapter.NotifyDataSetChanged();
                }
            }
        }

        private List<SignalMeterProfile> LoadTestProfiles()
        {
            var list = new List<SignalMeterProfile>();
            if (FileSystem.IsExternalStorageReadable())
            {
                var sdPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                var testFilePath = Path.Combine(sdPath, testProfilesFileName);
                if (File.Exists(testFilePath))
                {
                    try
                    {
                        var lines = File.ReadAllLines(testFilePath);
                        SignalMeterProfile profile = null;
                        foreach (var line in lines)
                        {
                            if (line.ToLower().StartsWith("name;"))
                            {
                                profile = new SignalMeterProfile();
                                list.Add(profile);
                                var currentDate = DateTime.Now;
                                profile.Name = line.Substring(5) + "  " +
                                    string.Format("{0}.{1}.{2} {3}:{4}:{5}", 
                                        currentDate.Day, 
                                        currentDate.Month, 
                                        currentDate.Year, 
                                        currentDate.Hour, 
                                        currentDate.Minute, 
                                        currentDate.Second);
                            }

                            if (profile != null)
                            {
                                if (line.ToLower().StartsWith("address;"))
                                {
                                    profile.Address = line.Substring(8);
                                }
                                else if (line.ToLower().StartsWith("enigma;"))
                                {
                                    profile.Enigma = (EnigmaType)int.Parse(line.Substring(7).Trim());
                                }
                                else if (line.ToLower().StartsWith("httpport;"))
                                {
                                    profile.HttpPort = int.Parse(line.Substring(9).Trim());
                                }
                                else if (line.ToLower().StartsWith("password;"))
                                {
                                    profile.Password = line.Substring(9);
                                }
                                else if (line.ToLower().StartsWith("streaming;"))
                                {
                                    profile.Streaming = line.Substring(10).Trim() == "1";
                                }
                                else if (line.ToLower().StartsWith("streamingport;"))
                                {
                                    profile.StreamingPort = int.Parse(line.Substring(14).Trim());
                                }
                                else if (line.ToLower().StartsWith("transcoding;"))
                                {
                                    profile.Transcoding = line.Substring(12).Trim() == "1";
                                }
                                else if (line.ToLower().StartsWith("transcodingport;"))
                                {
                                    profile.TranscodingPort = int.Parse(line.Substring(16).Trim());
                                }
                                else if (line.ToLower().StartsWith("username;"))
                                {
                                    profile.Username = line.Substring(9).Trim();
                                }
                                else if (line.ToLower().StartsWith("usessl;"))
                                {
                                    profile.UseSsl = line.Substring(7).Trim() == "1";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Failed to load test profiles! {0}", ex.Message));
                    }
                }
            }
            return list;
        }

    }
}

