using System;
using Android.Speech.Tts;
using Android.Util;
using Krkadoni.Enigma.Responses;
using Android.Content;
using Android.App;
using Android.Widget;
using System.Net;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class TextToSpeechManager: Java.Lang.Object, TextToSpeech.IOnInitListener
    {
        private TextToSpeech mTts = null;
        private bool ttsLoaded = false;
        private const string TAG = "TextToSpeechManager";
        private const int ttsErrorLimit = 3;
        private object lockObject;

        GlobalApp app;
        Context context;

        public TextToSpeechManager(Context context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
            app = ((GlobalApp)context.ApplicationContext);
            if (mTts == null)
                mTts = new TextToSpeech(context, this);
            lockObject = new object();
        }

        public void OnInit(OperationResult status)
        {
            if (status != OperationResult.Success)
            {
                Log.Error("TTS error", "Initialization Failed!");
                OnTtsFailed();
                return;
            }
            if (mTts.Language == null || string.IsNullOrEmpty(mTts.Language.Language))
            {
                try
                {
                    var result = mTts.SetLanguage(Java.Util.Locale.Default);
                    if (result == LanguageAvailableResult.MissingData || result == LanguageAvailableResult.NotSupported)
                    {
                        result = mTts.SetLanguage(Java.Util.Locale.Us);
                        if (result == LanguageAvailableResult.MissingData)
                        {
                            OnTtsError();
                        }
                        else if (result == LanguageAvailableResult.NotSupported)
                        {
                            OnTtsFailed();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, string.Format("Failed to set TTS langauage. {0}", ex.Message));
                    OnTtsFailed();
                }
            }

            ttsLoaded = mTts.Language != null && !string.IsNullOrEmpty(mTts.Language.Language);
        }

        private void OnTtsError()
        {
            if (app.Settings.LastAskedForTtsSettings.Date != DateTime.Today && !app.Settings.IgnoreTtsError)
            {
                var message = string.Format("{0} {1} {2}", 
                                  context.GetString(Resource.String.err_no_tts_language_available), 
                                  context.GetString(Resource.String.inf_signal_levels_will_not_be_spoken), 
                                  context.GetString(Resource.String.question_try_tts_language_download));
                
                Action<object, DialogClickEventArgs> positiveAction = (sender, args) =>
                {
                    app.Settings.TtsErrorCount = 0;
                    app.Settings.LastAskedForTtsSettings = DateTime.Now;
                    ((PreferenceManager)app.PreferenceManager).SaveSettings(context, app.Settings);
                    DownloadLanguageFiles();
                };
                Action<object, DialogClickEventArgs> negativeAction = (sender, args) =>
                {
                    app.Settings.TtsErrorCount += 1;
                    app.Settings.LastAskedForTtsSettings = DateTime.Now;
                    ((PreferenceManager)app.PreferenceManager).SaveSettings(context, app.Settings);

                    if (app.Settings.TtsErrorCount >= ttsErrorLimit)
                    {
                        AskIgnoreTtsError();
                    }
                };
                var dialog = Dialogs.QuestionDialog(context, message, positiveAction, negativeAction);
                dialog.SetTitle(Resource.String.title_warning);
                dialog.SetCancelable(false);
                dialog.Show();
            }
        }

        private void OnTtsFailed()
        {
            if (app.Settings.LastAskedForTtsSettings.Date != DateTime.Today && !app.Settings.IgnoreTtsError)
            {
                var message = string.Format("{0} {1} {2}", context.GetString(Resource.String.err_failed_to_initialize_tts_engine), context.GetString(Resource.String.inf_signal_levels_will_not_be_spoken), context.GetString(Resource.String.question_open_tts_settings));
                Action<object, DialogClickEventArgs> positiveAction = (sender, args) =>
                {
                    app.Settings.TtsErrorCount = 0;
                    app.Settings.LastAskedForTtsSettings = DateTime.Now;
                    ((PreferenceManager)app.PreferenceManager).SaveSettings(context, app.Settings);
                    LaunchTtsSettings();
                };
                Action<object, DialogClickEventArgs> negativeAction = (sender, args) =>
                {
                    app.Settings.TtsErrorCount += 1;
                    app.Settings.LastAskedForTtsSettings = DateTime.Now;
                    ((PreferenceManager)app.PreferenceManager).SaveSettings(context, app.Settings);
                    if (app.Settings.TtsErrorCount >= ttsErrorLimit)
                    {
                        AskIgnoreTtsError();
                    }
                };
                var dialog = Dialogs.QuestionDialog(context, message, positiveAction, negativeAction);
                dialog.SetTitle(Resource.String.title_warning);
                dialog.SetCancelable(false);
                dialog.Show();
            }
        }

        private void LaunchTtsSettings()
        {
            //Open Android Text-To-Speech Settings
            if (((int)Android.OS.Build.VERSION.SdkInt) >= 14)
            {
                Intent intent = new Intent();
                intent.SetAction("com.android.app.Settings.TTS_SETTINGS");
                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
            else
            {
                Intent intent = new Intent();
                intent.AddCategory(Intent.CategoryLauncher);
                intent.SetComponent(new ComponentName("com.android.settings", "com.android.app.Settings.TextToSpeechSettings"));
                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
        }

        private void AskIgnoreTtsError()
        {
            
            var message = context.GetString(Resource.String.question_ignore_further_tts_error);
            Action<object, DialogClickEventArgs> positiveAction = (sender, args) =>
            {
                app.Settings.IgnoreTtsError = true;
                app.Settings.LastAskedForTtsSettings = DateTime.Now;
                ((PreferenceManager)app.PreferenceManager).SaveSettings(context, app.Settings);
            };
            Action<object, DialogClickEventArgs> negativeAction = (sender, args) =>
            {
                app.Settings.TtsErrorCount = 0;
                app.Settings.LastAskedForTtsSettings = DateTime.Now;
                ((PreferenceManager)app.PreferenceManager).SaveSettings(context, app.Settings);
            };
            var dialog = Dialogs.QuestionDialog(context, message, positiveAction, negativeAction);
            dialog.SetCancelable(false);
            dialog.Show();
        }

        private void DownloadLanguageFiles()
        {
            var url = context.GetString(Resource.String.language_pack_apk_url);
            Uri uri = new Uri(url);
            var filename = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, System.IO.Path.GetFileName(uri.LocalPath));
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    System.IO.File.Delete(filename);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Language files APK exists and cannot be deleted");
                    Toast.MakeText(context, context.GetString(Resource.String.err_download_failed), ToastLength.Short).Show();
                    return;
                }
            }

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(filename)))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));   
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Download folder doesn't exist and cannot be created");
                    Toast.MakeText(context, context.GetString(Resource.String.err_download_failed), ToastLength.Short).Show();
                    return;
                } 
            }

            var progressDialog = new ProgressDialog(context);
            progressDialog.SetTitle(context.GetString(Resource.String.title_downloading));
            progressDialog.SetMessage(context.GetString(Resource.String.please_wait));
            progressDialog.Progress = 0;
            progressDialog.SetCancelable(false);
            progressDialog.Indeterminate = false;
            progressDialog.Max = 100;
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        progressDialog.Progress = e.ProgressPercentage;
                    };
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        progressDialog.Dismiss();
                        if (e.Error != null)
                        {
                            Toast.MakeText(context, context.GetString(Resource.String.err_download_failed) + " " + e.Error.Message, ToastLength.Short).Show();
                            System.Diagnostics.Debug.WriteLine("Failed to download language pack. " + e.Error.Message);
                            return;
                        }
                        System.Diagnostics.Debug.WriteLine("Downloaded file " + filename);
                        progressDialog.Dismiss();
                        InstallLanguageFiles(filename);
                    };
                    wc.DownloadFileAsync(uri, filename);
                }
                progressDialog.Show();
            }
            catch (Exception ex)
            {
                progressDialog.Dismiss();
                Toast.MakeText(context, context.GetString(Resource.String.err_download_failed) + " " + ex.Message, ToastLength.Short).Show();
                System.Diagnostics.Debug.WriteLine("Failed to download language pack. " + ex.Message);
            }
        }

        private void InstallLanguageFiles(string fileName)
        {
            try
            {
                var file = new Java.IO.File(fileName);
                file.SetReadable(true, false);
                Intent promptInstall = new Intent(Intent.ActionView).SetDataAndType(Android.Net.Uri.FromFile(file), "application/vnd.android.package-archive");
                context.StartActivity(promptInstall);
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, context.GetString(Resource.String.err_install_failed) + " " + ex.Message, ToastLength.Short).Show();
                System.Diagnostics.Debug.WriteLine("Failed to install language pack. " + ex.Message);
            }
        }

        public void SpeakSignalLevels(ISignalResponse response, TimeSpan requestTime)
        {  
            lock (lockObject)
            {
                if (!ttsLoaded)
                    return;
                
                if (!app.ActivityStarted)
                    return;
                
                try
                {
                    if (response != null && response.Signal != null && response.Signal.Snr >= 0 && !mTts.IsSpeaking)
                    {
                        Log.Debug(TAG,"Speaking signal");
                        mTts.Speak(response.Signal.Snr.ToString(), QueueMode.Add, null);
                        Log.Debug(TAG,"Speak finished");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, string.Format("SpeakSignalLevels failed with error {0}", ex.Message));
                }   
            }
        }

        public void Stop()
        {
            if (mTts != null)
            {
                lock (lockObject)
                {
                    ttsLoaded = false;
                    mTts.Stop();
                    mTts.Shutdown(); 
                    mTts.Dispose();
                }
            }
        }

    }
}

