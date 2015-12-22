using System;
using System.Collections.Generic;
using System.Linq;
using Krkadoni.Enigma.Enums;
using Com.Krkadoni.Interfaces;
using Com.Krkadoni.App.SignalMeter.Model;
using Android.Content;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class PreferenceManager : IPreferenceManager<SignalMeterProfile>
    {
        public PreferenceManager()
        {
            _preferencesLock = new object();
        }

        const string _profilePrefix = "PROFILE_";
        const string _profileAddressPrefix = "_ADDRESS";
        const string _profileEnigmaPrefix = "_ENIGMA";
        const string _profilePasswordPrefix = "_PASSWORD";
        const string _profileSslPrefix = "_SSL";
        const string _profileUsernamePrefix = "_USERNAME";
        const string _profileStreamPortPrefix = "_STREAMPORT";
        const string _profileHttpPortPrefix = "_HTTPPORT";
        const string _profileTranscodingPortPrefix = "_TRANSCODINGPORT";
        const string _profileStreamingPrefix = "_STREAMING";
        const string _profileTranscodingPrefix = "_TRANSCODING";
        const string _lastAskedVoiceSettingsKey = "LAST_ASKED_FOR_TTS_SETTINGS";
        const string _streamActivatedKey = "STREAM_ACTIVATED";
        const string _ignoreTtsError = "IGNORE_TTS_ERROR";
        const string _ttsErrorCount = "TTS_ERROR_COUNT";

        readonly object _preferencesLock;

        #region IPreferenceManager implementation

        public List<SignalMeterProfile> LoadItems(Android.Content.Context context)
        {
            lock (_preferencesLock)
            {
                var sharedPref = context.GetSharedPreferences(context.GetString(Resource.String.profile_file_key), Android.Content.FileCreationMode.Private);
                IDictionary<string,object> preferences = sharedPref.All;

                var profiles = new List<SignalMeterProfile>();
                var profileKeys = preferences.ToList()
                    .Where(x => x.Key.StartsWith(_profilePrefix) && x.Key.Split('_').Length - 1 == 1)
                    .Select(x => int.Parse(x.Key.Substring(_profilePrefix.Length)))
                    .OrderBy(x => x)
                    .ToList();
                profileKeys.ForEach(x =>
                    {
                        var profile = new SignalMeterProfile();
                        var id = _profilePrefix + x.ToString();
                        profile.Name = (string)preferences[id];
                        profile.Address = (string)preferences[id + _profileAddressPrefix];
                        profile.Enigma = (EnigmaType)((int)preferences[id + _profileEnigmaPrefix]);
                        profile.Password = (string)preferences[id + _profilePasswordPrefix];
                        profile.UseSsl = (bool)preferences[id + _profileSslPrefix];
                        profile.Username = (string)preferences[id + _profileUsernamePrefix];
                        profile.HttpPort = (int)preferences[id + _profileHttpPortPrefix];
                        profile.StreamingPort = (int)preferences[id + _profileStreamPortPrefix];
                        profile.TranscodingPort = (int)preferences[id + _profileTranscodingPortPrefix];
                        profile.Streaming = (bool)preferences[id + _profileStreamingPrefix];
                        profile.Transcoding = (bool)preferences[id + _profileTranscodingPrefix];
                        profiles.Add(profile);
                    }
                );
                return profiles;
            }        
        }

        public SignalMeterProfile LoadItem(Android.Content.Context context, String key)
        {
            lock (_preferencesLock)
            {
                var sharedPref = context.GetSharedPreferences(context.GetString(Resource.String.profile_file_key), Android.Content.FileCreationMode.Private);
                IDictionary<string,object> preferences = sharedPref.All;
                SignalMeterProfile profile;
                var profiles = preferences.ToList().Where(x => x.Key.StartsWith(_profilePrefix) && x.Key.Split('_').Length - 1 == 1).ToList();
                foreach (var pr in profiles)
                {
                    if (pr.Value.ToString() == key)
                    {
                        profile = new SignalMeterProfile();
                        var id = pr.Key;
                        profile.Name = (string)pr.Value;
                        profile.Address = (string)preferences[id + _profileAddressPrefix];
                        profile.Enigma = (EnigmaType)((int)preferences[id + _profileEnigmaPrefix]);
                        profile.Password = (string)preferences[id + _profilePasswordPrefix];
                        profile.UseSsl = (bool)preferences[id + _profileSslPrefix];
                        profile.Username = (string)preferences[id + _profileUsernamePrefix];
                        profile.HttpPort = (int)preferences[id + _profileHttpPortPrefix];
                        profile.StreamingPort = (int)preferences[id + _profileStreamPortPrefix];
                        profile.TranscodingPort = (int)preferences[id + _profileTranscodingPortPrefix];
                        profile.Streaming = (bool)preferences[id + _profileStreamingPrefix];
                        profile.Transcoding = (bool)preferences[id + _profileTranscodingPrefix];
                        return profile;
                    }
                }              
                return null;   
            }
        }

        public void SaveItem(Android.Content.Context context, SignalMeterProfile item)
        {   
            lock (_preferencesLock)
            {
                var sharedPref = context.GetSharedPreferences(context.GetString(Resource.String.profile_file_key), Android.Content.FileCreationMode.Private);
                var editor = sharedPref.Edit();
                IDictionary<string,object> preferences = sharedPref.All;
                var profiles = preferences.ToList().Where(x => x.Key.StartsWith(_profilePrefix) && x.Key.Split('_').Length - 1 == 1).ToList();
                string id;

                string existingProfile = profiles.SingleOrDefault(x => x.Value.ToString() == item.Name).Key;
                if (existingProfile == null)
                {
                    //add new profile
                    id = _profilePrefix + (profiles.Count + 1).ToString();
                }
                else
                {
                    //update existing profile
                    id = existingProfile;
                }

                editor.PutString(id, item.Name);
                editor.PutString(id + _profileAddressPrefix, item.Address);
                editor.PutInt(id + _profileEnigmaPrefix, (int)item.Enigma);
                editor.PutString(id + _profilePasswordPrefix, item.Password);
                editor.PutBoolean(id + _profileSslPrefix, item.UseSsl);
                editor.PutString(id + _profileUsernamePrefix, item.Username);
                editor.PutInt(id + _profileHttpPortPrefix, item.HttpPort);
                editor.PutInt(id + _profileStreamPortPrefix, item.StreamingPort);
                editor.PutBoolean(id + _profileStreamingPrefix, item.Streaming);
                editor.PutBoolean(id + _profileTranscodingPrefix, item.Transcoding);
                editor.PutInt(id + _profileTranscodingPortPrefix, item.TranscodingPort);
                editor.Commit();
            }
        }

        public void SaveItems(Android.Content.Context context, List<SignalMeterProfile> profiles)
        {
            profiles.ToList().ForEach(x => SaveItem(context, x));
        }

        public void DeleteItem(Android.Content.Context context, string key)
        {
            lock (_preferencesLock)
            {
                var sharedPref = context.GetSharedPreferences(context.GetString(Resource.String.profile_file_key), Android.Content.FileCreationMode.Private);
                var editor = sharedPref.Edit();
                IDictionary<string,object> preferences = sharedPref.All;
                var profiles = preferences.ToList().Where(x => x.Key.StartsWith(_profilePrefix) && x.Key.Split('_').Length - 1 == 1).ToList();
                string id;

                string existingProfile = profiles.SingleOrDefault(x => x.Value.ToString() == key).Key;

                if (existingProfile == null)
                    return;
                //update existing profile
                id = existingProfile;

                editor.Remove(id);
                editor.Remove(id + _profileAddressPrefix);
                editor.Remove(id + _profileEnigmaPrefix);
                editor.Remove(id + _profilePasswordPrefix);
                editor.Remove(id + _profileSslPrefix);
                editor.Remove(id + _profileUsernamePrefix);
                editor.Remove(id + _profileHttpPortPrefix);
                editor.Remove(id + _profileStreamPortPrefix);
                editor.Remove(id + _profileStreamingPrefix);
                editor.Remove(id + _profileTranscodingPrefix);
                editor.Remove(id + _profileTranscodingPortPrefix);
                editor.Apply();
            }
        }


        public ApplicationSettings LoadSettings(Context context)
        {
            lock (_preferencesLock)
            {
                IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
                var sharedPref = context.GetSharedPreferences(context.GetString(Resource.String.preferences_file_key), Android.Content.FileCreationMode.Private);
                IDictionary<string,object> preferences = sharedPref.All;
                var settings = new ApplicationSettings();
                settings.StreamActivated = preferences.ContainsKey(_streamActivatedKey) ? (bool)preferences[_streamActivatedKey] : false;
                settings.LastAskedForTtsSettings = preferences.ContainsKey(_lastAskedVoiceSettingsKey) ? Convert.ToDateTime(preferences[_lastAskedVoiceSettingsKey],culture) : DateTime.MinValue;
                settings.IgnoreTtsError = preferences.ContainsKey(_ignoreTtsError) ? (bool)preferences[_ignoreTtsError] : false;
                settings.TtsErrorCount = preferences.ContainsKey(_ttsErrorCount) ? (int)preferences[_ttsErrorCount] : 0;
                return settings;
            }
        }

        public void SaveSettings(Context context, ApplicationSettings settings)
        {
            lock (_preferencesLock)
            {
                IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
                var sharedPref = context.GetSharedPreferences(context.GetString(Resource.String.preferences_file_key), Android.Content.FileCreationMode.Private);
                var editor = sharedPref.Edit();
                editor.PutBoolean(_streamActivatedKey, settings.StreamActivated);
                editor.PutString(_lastAskedVoiceSettingsKey, settings.LastAskedForTtsSettings.ToString(culture));
                editor.PutBoolean(_ignoreTtsError, settings.IgnoreTtsError);
                editor.PutInt(_ttsErrorCount, settings.TtsErrorCount);
                editor.Commit();
            }
        }

        #endregion
    }
}

