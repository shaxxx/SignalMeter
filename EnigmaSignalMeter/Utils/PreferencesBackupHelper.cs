using Android.App.Backup;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class PreferencesBackupHelper : BackupAgentHelper
    {
        const string PROFILE_FILE_KEY = "com.krkadoni.app.signalmeter.Profiles";
        const string PREFERENCES_FILE_KEY = "com.krkadoni.app.signalmeter.Preferences";
        const string PREFERENCES_BACKUP_KEY = "com.krkadoni.app.signalmeter.Backup";

        public PreferencesBackupHelper()
        {
           
            //PROFILE_FILE_KEY = GetString(Resource.String.profile_file_key);
            //PREFERENCES_FILE_KEY = GetString(Resource.String.preferences_file_key);

//            // An arbitrary string used within the BackupAgentHelper implementation to
//            // identify the SharedPreferenceBackupHelper's data.
            //PREFERENCES_BACKUP_KEY = GetString(Resource.String.preferences_backup_key);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            SharedPreferencesBackupHelper helper = new SharedPreferencesBackupHelper(this, PROFILE_FILE_KEY, PREFERENCES_FILE_KEY);
            AddHelper(PREFERENCES_BACKUP_KEY, helper);
        }     
    }        
}

