using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Support.V4.App;
using Android.Widget;
using Android.Support.V7.Widget;
using Com.Krkadoni.App.SignalMeter.Model;
using System.Linq;
using Android.Content;
using Krkadoni.Enigma.Enums;
using Com.Krkadoni.Utils;
using System;
using Com.Krkadoni.App.SignalMeter.Utils;
using System.Threading.Tasks;
using System.Threading;

namespace Com.Krkadoni.App.SignalMeter.Layout
{
    [Activity(Name = "com.krkadoni.app.signalmeter.layout.ProfileEditActivity", Label = "Profile", ParentActivity = typeof(MainActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "com.krkadoni.app.signalmeter.layout.MainActivity")]
    public class ProfileEditActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private const string TAG = "ProfileEditActivity";
        private IMenuItem saveMenu;
        private IMenuItem cancelMenu;
        private EditText txtName;
        private EditText txtAddress;
        private EditText txtUsername;
        private EditText txtPassword;
        private EditText txtPort;
        private RadioButton radioEnigma1;
        private RadioButton radioEnigma2;
        private SwitchCompat switchStreaming;
        private EditText txtStreamingPort;
        private SwitchCompat switchTranscoding;
        private EditText txtTranscodingPort;
        private CheckBox cbUseSsl;
        private LinearLayout layoutStreaming;
        private LinearLayout layoutTranscoding;
        private string editProfileName;
        private ScrollView svProfile;
        private bool isDirty;

        private const string profileNameKey = "PROFILE_NAME";
        private const string profileAddressKey = "PROFILE_ADDRESS";
        private const string profileUsernameKey = "PROFILE_USERNAME";
        private const string profilePasswordKey = "PROFILE_PASSWORD";
        private const string profilePortKey = "PROFILE_PORT";
        private const string profileEnigma1Key = "PROFILE_ENIGMA1";
        private const string profileEnigma2Key = "PROFILE_ENIGMA2";
        private const string profileStreamingKey = "PROFILE_STREAMING";
        private const string profileStreamingPortKey = "PROFILE_STREAMING_PORT";
        private const string profileTranscodingKey = "PROFILE_TRANSCODING";
        private const string profileTranscodingPortKey = "PROFILE_TRANSCODING_PORT";
        private const string profileUseSslKey = "PROFILE_USE_SSL";
        private const string profileOriginalNameKey = "ORIGINAL_PROFILE_NAME";
        private const string isDirtyKey = "IS_DIRTY";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.profile_edit_activity);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);       
            SetSupportActionBar(mToolbar);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            svProfile = FindViewById<ScrollView>(Resource.Id.svProfile);
            txtName = FindViewById<EditText>(Resource.Id.edit_profile_name);
            txtAddress = FindViewById<EditText>(Resource.Id.edit_profile_address);
            txtUsername = FindViewById<EditText>(Resource.Id.edit_profile_username);
            txtPassword = FindViewById<EditText>(Resource.Id.edit_profile_password);
            txtPort = FindViewById<EditText>(Resource.Id.edit_profile_port);
            radioEnigma1 = FindViewById<RadioButton>(Resource.Id.rbEnigma1);
            radioEnigma2 = FindViewById<RadioButton>(Resource.Id.rbEnigma2);
            switchStreaming = FindViewById<SwitchCompat>(Resource.Id.switch_streaming);
            txtStreamingPort = FindViewById<EditText>(Resource.Id.edit_streaming_port);
            switchTranscoding = FindViewById<SwitchCompat>(Resource.Id.switch_transcoding);
            txtTranscodingPort = FindViewById<EditText>(Resource.Id.edit_transcoding_port);
            cbUseSsl = FindViewById<CheckBox>(Resource.Id.cbUseSsl);
            layoutStreaming = FindViewById<LinearLayout>(Resource.Id.layout_Streaming);
            layoutTranscoding = FindViewById<LinearLayout>(Resource.Id.layout_Transcoding);

            //update controls content
            if (bundle != null && bundle.ContainsKey(profileNameKey))
            {
                //restore state
                editProfileName = bundle.GetString(profileOriginalNameKey);
                txtName.Text = bundle.GetString(profileNameKey);
                txtAddress.Text = bundle.GetString(profileAddressKey);
                txtUsername.Text = bundle.GetString(profileUsernameKey);
                txtPassword.Text = bundle.GetString(profilePasswordKey);
                txtPort.Text = bundle.GetString(profilePortKey);
                radioEnigma1.Checked = bundle.GetBoolean(profileEnigma1Key);
                radioEnigma2.Checked = bundle.GetBoolean(profileEnigma2Key);
                switchStreaming.Checked = bundle.GetBoolean(profileStreamingKey);
                txtStreamingPort.Text = bundle.GetString(profileStreamingPortKey);
                switchTranscoding.Checked = bundle.GetBoolean(profileTranscodingKey);
                txtTranscodingPort.Text = bundle.GetString(profileTranscodingPortKey);
                cbUseSsl.Checked = bundle.GetBoolean(profileUseSslKey);
                isDirty = bundle.GetBoolean(isDirtyKey);
            }
            else if (Intent != null && Intent.HasExtra(ProfilesFragment.profileNameKey))
            {
                //edit existing profile   
                SignalMeterProfile profile = null;
                editProfileName = Intent.GetStringExtra(ProfilesFragment.profileNameKey);
            
                if (ConnectionManager.Profiles != null)
                {
                    profile = ConnectionManager.Profiles.FirstOrDefault(x => x.Name == editProfileName);
                    if (profile == null)
                        profile = new SignalMeterProfile();
                }
                else
                    profile = new SignalMeterProfile();

                txtName.Text = profile.Name;
                txtAddress.Text = profile.Address;
                txtUsername.Text = profile.Username;
                txtPassword.Text = profile.Password;
                txtPort.Text = profile.HttpPort.ToString();
                radioEnigma1.Checked = profile.Enigma == global::Krkadoni.Enigma.Enums.EnigmaType.Enigma1;
                radioEnigma2.Checked = profile.Enigma == global::Krkadoni.Enigma.Enums.EnigmaType.Enigma2;
                switchStreaming.Checked = profile.Streaming;
                txtStreamingPort.Text = profile.StreamingPort == 0 ? string.Empty : profile.StreamingPort.ToString();
                switchTranscoding.Checked = profile.Transcoding;
                txtTranscodingPort.Text = profile.TranscodingPort == 0 ? string.Empty : profile.TranscodingPort.ToString();
                cbUseSsl.Checked = profile.UseSsl;

            }
            else
            {
                //create new profile

            }

            //add event handlers
            txtName.TextChanged += (sender, e) => isDirty = true;
            txtAddress.TextChanged += (sender, e) => isDirty = true;
            txtUsername.TextChanged += (sender, e) => isDirty = true;
            txtPassword.TextChanged += (sender, e) => isDirty = true;
            txtPort.TextChanged += (sender, e) => isDirty = true;
            radioEnigma1.CheckedChange += (sender, e) => isDirty = true;
            radioEnigma2.CheckedChange += (sender, e) => isDirty = true;
            txtStreamingPort.TextChanged += (sender, e) => isDirty = true;
            txtTranscodingPort.TextChanged += (sender, e) => isDirty = true;
            cbUseSsl.CheckedChange += (sender, e) => isDirty = true;
            switchStreaming.CheckedChange += (sender, e) =>
            {
                isDirty = true;
                layoutStreaming.Visibility = switchStreaming.Checked ? ViewStates.Visible : ViewStates.Gone;
                if (switchStreaming.Checked)
                    svProfile.Post(() => svProfile.ScrollTo(0, svProfile.Bottom));
            };
            switchTranscoding.CheckedChange += (sender, e) =>
            {
                isDirty = true;
                layoutTranscoding.Visibility = switchTranscoding.Checked ? ViewStates.Visible : ViewStates.Gone;
                if (switchTranscoding.Checked)
                    svProfile.Post(() => svProfile.ScrollTo(0, svProfile.Bottom));
            };

            SupportActionBar.Title = string.IsNullOrEmpty(editProfileName) ? 
                GetString(Resource.String.action_add_profile) : 
                editProfileName;
            layoutStreaming.Visibility = switchStreaming.Checked ? ViewStates.Visible : ViewStates.Gone;
            layoutTranscoding.Visibility = switchTranscoding.Checked ? ViewStates.Visible : ViewStates.Gone;

        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(profileNameKey, txtName.Text);
            outState.PutString(profileAddressKey, txtAddress.Text);
            outState.PutString(profileUsernameKey, txtUsername.Text);
            outState.PutString(profilePasswordKey, txtPassword.Text);
            outState.PutString(profilePortKey, txtPort.Text);
            outState.PutBoolean(profileEnigma1Key, radioEnigma1.Checked);
            outState.PutBoolean(profileEnigma2Key, radioEnigma2.Checked);
            outState.PutBoolean(profileStreamingKey, switchStreaming.Checked);
            outState.PutString(profileStreamingPortKey, txtStreamingPort.Text);
            outState.PutBoolean(profileTranscodingKey, switchTranscoding.Checked);
            outState.PutString(profileTranscodingPortKey, txtTranscodingPort.Text);
            outState.PutBoolean(profileUseSslKey, cbUseSsl.Checked);
            outState.PutString(profileOriginalNameKey, editProfileName);
            outState.PutBoolean(isDirtyKey, isDirty);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_edit_profile, menu);
            saveMenu = menu.FindItem(Resource.Id.action_save);
            cancelMenu = menu.FindItem(Resource.Id.action_cancel);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                if (isDirty)
                    ConfirmDirtyExit();
                else
                    NavUtils.NavigateUpFromSameTask(this);            
                return true;
            }
            else if (item.ItemId == saveMenu.ItemId)
            {
                ValidateData();
                return true;
            }
            else if (item.ItemId == cancelMenu.ItemId)
            {
                if (isDirty)
                    ConfirmDirtyExit();
                else
                    ExitWithoutSave();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void ConfirmDirtyExit()
        {
            if (ConnectionManager.CurrentProfile == null)
                return;

            var message = string.Format(GetString(Resource.String.question_save_changes), ConnectionManager.CurrentProfile.Name);
            Action<object, DialogClickEventArgs> positiveAction = ( sender, args) => ValidateData();
            Action<object, DialogClickEventArgs> negativeAction = ( sender, args) => ExitWithoutSave();
            var dialog = Dialogs.QuestionDialog(this, message, positiveAction, negativeAction);
            dialog.SetCancelable(false);
            dialog.Show();
        }

        private void SaveAndExit()
        { 
            SignalMeterProfile profile = null;
            if (!string.IsNullOrEmpty(editProfileName))
                profile = ConnectionManager.Profiles.FirstOrDefault(x => x.Name == editProfileName);

            if (profile == null)
            {
                profile = new SignalMeterProfile();
                ConnectionManager.Profiles.Add(profile);
            }

            profile.Name = txtName.Text.Trim();
            profile.Address = txtAddress.Text.Trim();
            profile.Username = txtUsername.Text.Trim();
            profile.HttpPort = int.Parse(txtPort.Text.Trim());
            profile.Password = txtPassword.Text.Trim();
            profile.Enigma = radioEnigma2.Checked ? EnigmaType.Enigma2 : EnigmaType.Enigma1;
            profile.Streaming = switchStreaming.Checked;
            profile.StreamingPort = Network.IsValidPort(txtStreamingPort.Text.Trim()) ? int.Parse(txtStreamingPort.Text.Trim()) : 0;
            profile.Transcoding = switchTranscoding.Checked;
            profile.TranscodingPort = Network.IsValidPort(txtTranscodingPort.Text.Trim()) ? int.Parse(txtTranscodingPort.Text.Trim()) : 0;
            profile.UseSsl = cbUseSsl.Checked;

            if (!string.IsNullOrEmpty(editProfileName) && editProfileName != profile.Name)
                ((GlobalApp)ApplicationContext).PreferenceManager.DeleteItem(this, editProfileName);

            ((GlobalApp)ApplicationContext).PreferenceManager.SaveItem(this, profile);

            Intent resultIntent = new Intent();
            resultIntent.PutExtra(ProfilesFragment.profileNameKey, profile.Name);
            SetResult(Result.Ok, resultIntent);
            Finish();

        }

        private void ExitWithoutSave()
        {
            Intent resultIntent = new Intent();
            SetResult(Result.Canceled, resultIntent);
            Finish();
        }

        private async void ValidateData()
        {
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_profile_name));
                txtName.RequestFocus();
            }
            else if (txtName.Text.Length < 3)
            {
                ShowMessage(GetString(Resource.String.err_invalid_profile_name));
                txtName.RequestFocus();
            }
            else if (ConnectionManager.Profiles.Any(x => 
                x.Name.ToLower() == txtName.Text.Trim().ToLower() &&
                         editProfileName != txtName.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_profile_name));
                txtName.RequestFocus();
            }
            else if (string.IsNullOrEmpty(txtAddress.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_address));
                txtAddress.RequestFocus();
            }
            else if (txtAddress.Text.Length < 3)
            {
                ShowMessage(GetString(Resource.String.err_invalid_address));
                txtAddress.RequestFocus();
            }
            else if (!Network.IsValidHostnameOrIpAddress(txtAddress.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_address));
                txtAddress.RequestFocus();
            }
            else if (string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_username));
                txtUsername.RequestFocus();
            }
            else if (string.IsNullOrEmpty(txtPort.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_http_port));
                txtPort.RequestFocus();
            }
            else if (!Network.IsValidPort(txtPort.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_http_port));
                txtPort.RequestFocus();
            }
            else if (radioEnigma1.Checked == false && radioEnigma2.Checked == false)
            {
                ShowMessage(GetString(Resource.String.err_invalid_enigma_type));
                radioEnigma2.RequestFocus();
            }
            else if (switchStreaming.Checked && string.IsNullOrEmpty(txtStreamingPort.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_streaming_port));
                txtStreamingPort.RequestFocus();
            }
            else if (switchStreaming.Checked && !Network.IsValidPort(txtStreamingPort.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_streaming_port));
                txtStreamingPort.RequestFocus();
            }
            else if (switchTranscoding.Checked && string.IsNullOrEmpty(txtTranscodingPort.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_transcoding_port));
                txtTranscodingPort.RequestFocus();
            }
            else if (switchTranscoding.Checked && !Network.IsValidPort(txtTranscodingPort.Text.Trim()))
            {
                ShowMessage(GetString(Resource.String.err_invalid_transcoding_port));
                txtTranscodingPort.RequestFocus();
            }
            else if (isDirty)
            {
                ShowMessage(GetString(Resource.String.checking_ports));
                await CheckHttp();
            }
            else
            {
                SaveAndExit();
            }
        }

        Toast messageToast;

        private void ShowMessage(string error)
        {
            if (messageToast == null)
            {
                messageToast = Toast.MakeText(this, string.Empty, ToastLength.Short);
            }
            messageToast.SetText(error);
            messageToast.Show();
        }

        private async Task CheckHttp()
        {
            if (!await Network.CheckPortOpen(txtAddress.Text.Trim(), int.Parse(txtPort.Text.Trim())))
            {
                var message = string.Format(GetString(Resource.String.warn_http_port_closed), txtAddress.Text.Trim(), txtPort.Text.Trim()); 
                message += " " + GetString(Resource.String.warn_save_the_profile_anyway);
                Dialogs.QuestionDialog(this, message, (sender, args) => SaveAndExit(), (sender, e) => {}).Show();
            }
            else
            {
                //Check further ports only if HTTP port is available
                await CheckStream();
            }
        }

        private async Task CheckStream()
        {
            if (switchStreaming.Checked && !await Network.CheckPortOpen(txtAddress.Text.Trim(), int.Parse(txtStreamingPort.Text.Trim())))
            {
                var message = string.Format(GetString(Resource.String.warn_streaming_port_closed), txtAddress.Text.Trim(), txtStreamingPort.Text.Trim()); 
                message += " " + GetString(Resource.String.warn_save_the_profile_anyway);
                Dialogs.QuestionDialog(this, message, (sender, args) => SaveAndExit(), (sender, e) => {}).Show();
            }
            else
            {
                //Check transcording port only if streaming port is available
                await CheckTranscoding();
            }
        }

        private async Task CheckTranscoding()
        {
            if (switchStreaming.Checked && switchTranscoding.Checked && !await Network.CheckPortOpen(txtAddress.Text.Trim(), int.Parse(txtTranscodingPort.Text.Trim())))
            {
                var message = string.Format(GetString(Resource.String.warn_transcoding_port_closed), txtAddress.Text.Trim(), txtTranscodingPort.Text.Trim()); 
                message += " " + GetString(Resource.String.warn_save_the_profile_anyway);
                Dialogs.QuestionDialog(this, message, (sender, args) => SaveAndExit(), (sender, e) => {}).Show();
            }
            else
            {
                //All ports are available
                SaveAndExit();
            }
        }


    }
}

