using System;
using Android.Telephony;
using Android.Util;
using Android.Content;
using Android.Provider;
using Android.Net.Wifi;

namespace Com.Krkadoni.Utils.Model
{
    public static class DeviceInfo
    {
        const string TAG = "DeviceInfo";

        public static void ObtainInfo(Context context)
        {
            AndroidSerial = GetAndroidSerial();
            AndroidID = GetAndroidID(context);
            Manufacturer = Android.OS.Build.Manufacturer;
            DeviceModel = Android.OS.Build.Model;
            AndroidVersion = Android.OS.Build.VERSION.Release;
            MacAddress = GetMacAddress(context);
        }

        static string iMEIorMEDI;
        public static string IMEIorMEDI
        {
            get
            {
                return iMEIorMEDI;
            }
            set
            {
                iMEIorMEDI = value;
            }
        }

        public static string AndroidSerial { get; private set;}

        public static string AndroidID { get; private set; }

        public static string Manufacturer { get; private set; }

        public static string DeviceModel { get; private set; }

        public static string AndroidVersion { get; private set; }

        public static string MacAddress { get; private set; }

        public static string GetIMEIorMEID (Context context)
        {
            string deviceIMEIorMEID = "";

            try {
                
                TelephonyManager telephonyManager = (TelephonyManager)context.GetSystemService (Context.TelephonyService);

                if (telephonyManager != null) {
                    deviceIMEIorMEID = telephonyManager.DeviceId;
                }
            } catch (Exception e) {
                Log.Error (TAG, "e: " + e.ToString ());
            }

            return deviceIMEIorMEID;
        }

        public static string GetAndroidSerial ()
        {
            string androidSerial = "";

            try {
                // Is there no IMEI or MEID?
                // Is this at least Android 2.3+?
                // Then let's get the serial.
                if (int.Parse (Android.OS.Build.VERSION.Sdk) >= 9) {
                    // THIS CLASS IS ONLY LOADED FOR ANDROID 2.3+
                    androidSerial = GetSerial ();
                }
            } catch (Exception e) {
                Log.Error (TAG, "e: " + e.ToString ());
            }

            return androidSerial;
        }

        public static string GetAndroidID (Context context)
        {
            // ANDROID_ID
            return Android.Provider.Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
        }

        private static string GetSerial ()
        {
            string serial = null;
            try {
                serial = Android.OS.Build.Serial;
                Log.Debug  (TAG, "serial: " + serial);
            } catch (Exception e) {
                Log.Error(TAG, e.ToString ());
            }

            return serial;
        }

        public static string GetMacAddress(Context context)
        {
            string address = string.Empty;
            try
            {
                WifiManager manager = (WifiManager) context.GetSystemService(Context.WifiService);
                WifiInfo info = manager.ConnectionInfo;
                address = info.MacAddress;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex.ToString ());  
            }
            return address;
        }


    }
}
