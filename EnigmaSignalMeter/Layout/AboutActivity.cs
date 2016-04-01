using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V4.App;
using Java.Lang;
using Android.Content;
using Java.Lang.Reflect;
using Android.Util;
using System.Threading.Tasks;
using Com.Krkadoni.Utils.Model;
using Android.Content.PM;
using Com.Krkadoni.App.SignalMeter.Utils;

namespace Com.Krkadoni.App.SignalMeter.Layout
{
    [Activity(Name = "com.krkadoni.app.signalmeter.layout.AboutActivity", Label = "About", ParentActivity = typeof(MainActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "com.krkadoni.app.signalmeter.layout.MainActivity")]
    public class AboutActivity : AppCompatActivity
    {

        private Android.Support.V7.Widget.Toolbar mToolbar;
        private const string TAG = "AboutActivity";
        private LinearLayout layoutAbout = null;
        private GlobalApp app;

        private TextView lbVersion;
        private TextView lbRelaseName;
        private TextView lbAndroidSerial;
        private TextView lbAndroidId;
        private TextView lbAndroidVersion;
        private TextView lbManufacturer;
        private TextView lbDeviceModel;
        private TextView lbMac;
        private TextView lbAdvertisingId;
        private Button btnCoins;


        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.about_activity);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(mToolbar);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = GetString(Resource.String.AboutTitle);
            layoutAbout = FindViewById<LinearLayout>(Resource.Id.layoutAboutSecondary);
            app = (GlobalApp)ApplicationContext;
            lbVersion = FindViewById<TextView>(Resource.Id.lbVersion);
            lbRelaseName = FindViewById<TextView>(Resource.Id.lbRelaseName);
            lbAndroidSerial = FindViewById<TextView>(Resource.Id.lbAndroidSerial);
            lbAndroidId = FindViewById<TextView>(Resource.Id.lbAndroidId);
            lbAndroidVersion = FindViewById<TextView>(Resource.Id.lbAndroidVersion);
            lbManufacturer = FindViewById<TextView>(Resource.Id.lbManufacturer);
            lbDeviceModel = FindViewById<TextView>(Resource.Id.lbDeviceModel);
            //lbMac = FindViewById<TextView>(Resource.Id.lbMac);
            lbAdvertisingId = FindViewById<TextView>(Resource.Id.lbAdvertisingId);
            btnCoins = FindViewById<Button>(Resource.Id.btnCoins);
            btnCoins.Click += (sender, e) =>
            Dialogs.BuildDialog(this, "Info", GetString(Resource.String.coins_explanation), (args, ed) =>
                {
                }, null, null, Android.Resource.String.Ok, 0, 0).Show();
            lbRelaseName.Text += ":  " + app.ReleaseName;
            try
            {
                PackageManager manager = this.PackageManager;
                PackageInfo info = manager.GetPackageInfo(this.PackageName, 0);
                lbVersion.Text += ":  " + info.VersionName;
            }
            catch (Exception)
            {
                Log.Debug(TAG, "Failed to obtain current application version name and version code");
            }

            DeviceInfo.ObtainInfo(this);

            lbAndroidSerial.Text += ":  " + DeviceInfo.AndroidSerial;
            lbAndroidId.Text += ":  " + DeviceInfo.AndroidID;
            lbAndroidVersion.Text += ":  " + DeviceInfo.AndroidVersion;
            lbManufacturer.Text += ":  " + DeviceInfo.Manufacturer;
            lbDeviceModel.Text += ":  " + DeviceInfo.DeviceModel;
            //lbMac.Text += ":  " + DeviceInfo.MacAddress;

            var id = await GetAdvertisingId();
            lbAdvertisingId.Text += ":  " + id;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                NavUtils.NavigateUpFromSameTask(this);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private async Task<string> GetAdvertisingId()
        {
            string id = string.Empty;
            try
            {
                Class localClass = Class.ForName("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                Class[] arrayOfClass;
                (arrayOfClass = new Class[1])[0] = Java.Lang.Class.FromType(typeof(Context));
                Method getAdvertisingIdInfo = localClass.GetMethod("getAdvertisingIdInfo", arrayOfClass);
                Class adInfoClass = Class.ForName("com.google.android.gms.ads.identifier.AdvertisingIdClient$Info");
                Method getId = null;
                Object adInfo = null;
                var task = Task.Factory.StartNew(() =>
                    {
                        adInfo = getAdvertisingIdInfo.Invoke(localClass, new Object[] { this });
                        getId = adInfoClass.GetMethod("getId", new Class[0]);
                        id = (string)getId.Invoke(adInfo, new Object[0]);
                    });
                await task;
                if (task.Exception != null)
                    Log.Debug(TAG, "Failed to obtain Google AdvertisingId");
            }
            catch (System.Exception)
            {
                Log.Debug(TAG, "Failed to obtain Google AdvertisingId");
            }
            return id;
        }

    }
}

