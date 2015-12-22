using Android.App;
using System;
using Android.Runtime;
using Android.App.Backup;
using Com.Krkadoni.Interfaces;
using Android.Graphics;
using System.Collections.Generic;
using Krkadoni.Enigma;
using Com.Krkadoni.App.SignalMeter.Utils;
using Com.Krkadoni.Utils;
using Com.Krkadoni.App.SignalMeter.Model;


namespace Com.Krkadoni.App.SignalMeter
{
    /// <summary>
    /// Custom extension of the base Android.App.Application class, in order to add properties which
    /// are global to the application, but not persisted as settings.
    /// </summary>
    [Application(AllowBackup = true, BackupAgent = typeof(PreferencesBackupHelper), RestoreAnyVersion = true)]
    public class GlobalApp : Android.App.Application, IGlobalApp<SignalMeterProfile>
    {
        private readonly Dictionary<string, Typeface> _loadedTypeFaces;

        public GlobalApp()
            : base()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += HandleUnhandledException;

            PreferenceManager = new Com.Krkadoni.App.SignalMeter.Utils.PreferenceManager();
            _loadedTypeFaces = new Dictionary<string, Typeface>();
            DefaultFont = LoadTypeFace(GetString(Resource.String.default_font_path));
            ReleaseName = GetString(Resource.String.ReleaseName);
        }

        public GlobalApp(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += HandleUnhandledException;

            PreferenceManager = new PreferenceManager();
            _loadedTypeFaces = new Dictionary<string, Typeface>();
            DefaultFont = LoadTypeFace(GetString(Resource.String.default_font_path));
            ReleaseName = GetString(Resource.String.ReleaseName);
        }

        public string ReleaseName { get; set; }

        public IPreferenceManager<SignalMeterProfile> PreferenceManager { get; set; }

        public void RequestBackup()
        {
            BackupManager bm = new BackupManager(this);
            bm.DataChanged();
        }

        public Typeface DefaultFont { get; set; }

        public Typeface LoadTypeFace(string fontPath)
        {
            if (_loadedTypeFaces.ContainsKey(fontPath))
            {
                return _loadedTypeFaces[fontPath];
            }
            var face = Typeface.CreateFromAsset(Assets, fontPath);
            _loadedTypeFaces.Add(fontPath, face);
            return face;
        }

        /// <summary>
        /// When app-wide unhandled exceptions are hit, this will handle them. Be aware however, that typically
        /// android will be destroying the process, so there's not a lot you can do on the android side of things,
        /// but your xamarin code should still be able to work. so if you have a custom err logging manager or 
        /// something, you can call that here. You _won't_ be able to call Android.Util.Log, because Dalvik
        /// will destroy the java side of the process.
        /// </summary>
        protected void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            // log won't be available, because dalvik is destroying the process
            //Log.Debug (logTag, "MyHandler caught : " + e.Message);
            // instead, your err handling code shoudl be run:
            Console.Error.Write("========= UnhandledException caught : " + e.Message);
        }

        private bool activityResumed;
        public bool ActivityResumed { 
            get
            {
                return activityResumed;
            }
            set 
            { 
                if (value)
                {
                    ActivityPaused = false;
                    ActivityStopped = false;
                }
                activityResumed = value;
            }
        }

        bool activityStarted;
        public bool ActivityStarted
        {
            get
            {
                return activityStarted;
            }
            set
            {
                if (value)
                {
                    ActivityPaused = false;
                    ActivityStopped = false;
                    ActivityResumed = true;
                }
                activityStarted = value;
            }
        }

        bool activityPaused;
        public bool ActivityPaused
        {
            get
            {
                return activityPaused;
            }
            set
            {
                if (value)
                {
                    ActivityResumed = false;
                    ActivityStarted = false;
                }
                activityPaused = value;
            }
        }

        bool activityStopped;
        public bool ActivityStopped
        {
            get
            {
                return activityStopped;
            }
            set
            {
                if (value)
                {
                    ActivityResumed = false;
                    ActivityStarted = false;
                    ActivityPaused = true;
                }
                activityStopped = value;
            }
        }

        public ApplicationSettings Settings { get; set;}
                  
    }
}

