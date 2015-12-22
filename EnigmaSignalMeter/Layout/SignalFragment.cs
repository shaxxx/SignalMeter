
using System;
using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Krkadoni.App.SignalMeter.Model;
using Krkadoni.Enigma.Enums;
using Krkadoni.Enigma.Responses;
using Android.Util;
using Krkadoni.Enigma;
using System.Threading.Tasks;
using Com.Krkadoni.App.SignalMeter.Utils;
using Android.Support.V7.Widget;

namespace Com.Krkadoni.App.SignalMeter.Layout
{
    public class SignalFragment : Fragment
    {
        public View signalView;
        private View signalLevelsView;
        public ProgressBar pbSNR;
        public ProgressBar pbdB;
        public ProgressBar pbBER;
        public ProgressBar pbAGC;
        public TextView lblCurrentService;
        public TextView lblCurrentSatellite;
        public TextView lblSNR;
        public TextView lbldB;
        public TextView lblAGC;
        public TextView lblBER;
        public CheckBox ceLOCK;
        public CheckBox ceSYNC;
        public TextView lblrqTime;
        private SwitchCompat switchVoice;
        private bool fragmentVisible = false;
        const string TAG = "SignalFragment";
        private bool startMonitor;
        private bool IsLandscape;
        private const string MONITOR_KEY = "MONITOR";
        private const string VOICE_KEY = "VOICE";
        Button btnMonitor;
        private TextToSpeechManager ttsManager;
        private bool SavedInstanceState;

        public SignalFragment()
        {
            Log.Debug(TAG, "SignalFragment initialized!");
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ConnectionManager.GetInstance().PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "CurrentProfile")
                {
                    SetVisibility(); 
                }
                else if (e.PropertyName == "ConnectionStatus")
                {
                    SetVisibility(); 
                }
                else if (e.PropertyName == "CurrentService")
                {
                    SetCurrentServiceText();
                }
                else if (e.PropertyName == "CurrentSatellitePosition")
                {
                    SetCurrentSatelliteText();
                }
                else if (e.PropertyName == "BouquetItems")
                {
                    SetCurrentSatelliteText();
                }
            };

            ConnectionManager.SignalMonitor.MonitorCommandFinished += (sender, e) => SetSignalLevels(e.Item1, e.Item2);
            ConnectionManager.SignalMonitor.MonitorCommandFinished += (sender, e) =>
            {
                if (ttsManager != null && !SavedInstanceState & switchVoice != null && switchVoice.Checked)
                    ttsManager.SpeakSignalLevels(e.Item1, e.Item2);
            };
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            //base.OnCreateView(inflater, container, savedInstanceState);

            // Get the view from fragment xml
            signalView = inflater.Inflate(Resource.Layout.signal_fragment, container, false);
            signalLevelsView = signalView.FindViewById<LinearLayout>(Resource.Id.signalLevels);

            IsLandscape = (Activity.FindViewById(Resource.Id.tablet_layout) != null);
            if (IsLandscape)
            {
                btnMonitor = signalView.FindViewById<Button>(Resource.Id.button_signal_monitor);
                btnMonitor.Click += async (sender, e) => StartStopMonitor();
            }

            lblCurrentService = signalView.FindViewById<TextView>(Resource.Id.lblCurrentService);   
            lblCurrentService.Text = string.Empty;
            lblCurrentService.Click += (sender, e) => ((MainActivity)Activity).DisplayView(MainEventHandlers.ViewsEnum.Services);
            lblCurrentSatellite = signalView.FindViewById<TextView>(Resource.Id.lblCurrentSatellite);   
            lblCurrentSatellite.Text = string.Empty;

            lblSNR = signalView.FindViewById<TextView>(Resource.Id.lblSNR); 
            pbSNR = signalView.FindViewById<ProgressBar>(Resource.Id.pbSNR);
            pbSNR.ProgressDrawable = Resources.GetDrawable(Resource.Drawable.progress);

            lbldB = signalView.FindViewById<TextView>(Resource.Id.lbldB);   
            pbdB = signalView.FindViewById<ProgressBar>(Resource.Id.pbdB);
            pbdB.ProgressDrawable = Resources.GetDrawable(Resource.Drawable.progress);

            lblAGC = signalView.FindViewById<TextView>(Resource.Id.lblAGC); 
            pbAGC = signalView.FindViewById<ProgressBar>(Resource.Id.pbAGC);
            pbAGC.ProgressDrawable = Resources.GetDrawable(Resource.Drawable.progress);

            lblBER = signalView.FindViewById<TextView>(Resource.Id.lblBER); 
            pbBER = signalView.FindViewById<ProgressBar>(Resource.Id.pbBER);
            pbBER.ProgressDrawable = Resources.GetDrawable(Resource.Drawable.progress_reverse);

            ceLOCK = signalView.FindViewById<CheckBox>(Resource.Id.ceLock);
            ceSYNC = signalView.FindViewById<CheckBox>(Resource.Id.ceSync); 

            lblrqTime = signalView.FindViewById<TextView>(Resource.Id.lblrqTime);
            switchVoice = signalView.FindViewById<SwitchCompat>(Resource.Id.switch_voice);
            switchVoice.CheckedChange += (sender, e) => SwitchTts();

            return signalView;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            if (savedInstanceState != null)
            {
                if (savedInstanceState.ContainsKey(MONITOR_KEY))
                    startMonitor = savedInstanceState.GetBoolean(MONITOR_KEY);
                if (savedInstanceState.ContainsKey(VOICE_KEY))
                    switchVoice.Checked = savedInstanceState.GetBoolean(VOICE_KEY);
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            ConnectionManager.SignalMonitor.Stop();
            outState.PutBoolean(MONITOR_KEY, startMonitor);
            outState.PutBoolean(VOICE_KEY, switchVoice.Checked);
            SavedInstanceState = true;
            if (ttsManager != null)
            {
                ttsManager.Stop();
                ttsManager.Dispose();
                ttsManager = null;
            }
        }

        public override async void OnStart()
        {
            base.OnStart();
            fragmentVisible = true;

            if (!IsLandscape || startMonitor)
            {
                await StartMonitor();
            }
            ResetControls();
            SetVisibility();
            SetButtonText();
            SetCurrentServiceText();
            SetCurrentSatelliteText();
        }

        public override void OnStop()
        {
            base.OnPause();
            fragmentVisible = false;
            ConnectionManager.SignalMonitor.Stop();
            if (Activity != null)
                Activity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
        }

        private void ResetControls()
        {
            if (this.View != null)
            {
                try
                {
                    lblSNR.Text = "SNR";
                    pbSNR.Progress = 0;
                    lbldB.Text = "dB";
                    pbdB.Progress = 0;
                    lblBER.Text = "BER";
                    pbBER.Progress = 0;
                    lblAGC.Text = "AGC";
                    pbAGC.Progress = 0;
                    ceLOCK.Checked = false;
                    ceSYNC.Checked = false;
                    lblrqTime.Text = string.Empty;
                    lblCurrentService.Text = string.Empty;
                    lblCurrentSatellite.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Error("SignalFragment", string.Format("ResetControls failed with error {0}", ex.Message));
                }
            }
        }

        private void SetVisibility()
        {
            if (this.View != null)
            {
                try
                {
                    if (ConnectionManager.CurrentProfile == null || !ConnectionManager.Connected)
                    {
                        signalView.Visibility = ViewStates.Invisible;
                    }
                    else
                    {
                        signalView.Visibility = ViewStates.Visible;
                        if (ConnectionManager.SignalMonitor.Status == CommandMonitor<ISignalResponse>.MonitorStatus.Stopped)
                        {
                            signalLevelsView.Visibility = ViewStates.Invisible;

                        }
                        else
                        {
                            signalLevelsView.Visibility = ViewStates.Visible;
                            if (ConnectionManager.CurrentProfile.Enigma == EnigmaType.Enigma2)
                            {
                                ceLOCK.Visibility = ViewStates.Invisible;
                                ceSYNC.Visibility = ViewStates.Invisible;
                            }
                            else
                            {
                                ceLOCK.Visibility = ViewStates.Visible;
                                ceSYNC.Visibility = ViewStates.Visible;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Error("SignalFragment", string.Format("SetVisibility failed with error {0}", ex.Message));
                }
            }
        }

        private void StartStopMonitor()
        {
            if (ConnectionManager.SignalMonitor.Status == CommandMonitor<ISignalResponse>.MonitorStatus.Stopped)
                StartMonitor();
            else
                StopMonitor();
        }

        private async Task StartMonitor()
        {
            startMonitor = true;
            await ConnectionManager.SignalMonitor.Start();
            Activity.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            SetButtonText();
            SetVisibility();
            Log.Debug(TAG, "SignalFragment StartMonitor!");

        }

        private void StopMonitor()
        {
            startMonitor = false;
            ConnectionManager.SignalMonitor.Stop();
            Activity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            SetButtonText();
            SetVisibility();
            Log.Debug(TAG, "SignalFragment StopMonitor!");
        }

        private void SetSignalLevels(ISignalResponse response, TimeSpan requestTime)
        {
            if (!fragmentVisible || this.View == null)
                return;
            
            try
            {
                if (response != null)
                {
                    lblSNR.Text = string.Format("SNR: {0} %", response.Signal.Snr > 0 ? response.Signal.Snr : 0);
                    lblAGC.Text = string.Format("ACG: {0} %", response.Signal.Acg > 0 ? response.Signal.Acg : 0);
                    lblBER.Text = string.Format("BER: {0}", response.Signal.Ber > 0 ? response.Signal.Ber : 0);

                    pbSNR.Progress = response.Signal.Snr;
                    pbAGC.Progress = response.Signal.Acg;
                    pbBER.Progress = response.Signal.Ber;
                    lblrqTime.Text = string.Format("{0}: {1} ms", GetString(Resource.String.request_time), (int)requestTime.TotalMilliseconds);

                    if (response.Signal is IE2Signal)
                    {
                        var db = ((IE2Signal)response.Signal).Db;
                        db = db > 0 ? db : 0;
                        lbldB.Text = string.Format("dB: {0}", db);
                        db = db * 100 > 1500 ? 1500 : db * 100;
                        pbdB.Progress = (int)db;
                    }
                    else
                    {
                        var locked = ((IE1Signal)response.Signal).Lock;
                        var synced = ((IE1Signal)response.Signal).Sync;
                        ceLOCK.Checked = locked;
                        ceSYNC.Checked = synced;
                    }
                }
                else
                {
                    ResetControls();
                }
            }
            catch (Exception ex)
            {
                Log.Error("SignalFragment", string.Format("SetSignalLevels failed with error {0}", ex.Message));
            }

        }

        private void SetCurrentServiceText()
        {
            try
            {
                if (this.View != null && ConnectionManager.CurrentService != null)
                {
                    lblCurrentService.Text = ConnectionManager.CurrentService.Name;
                    return;
                }
                lblCurrentService.Text = string.Empty; 
            }
            catch (Exception ex)
            {
                Log.Error("SignalFragment", string.Format("SetCurrentServiceText failed with error {0}", ex.Message));  
            }
        }

        private void SetCurrentSatelliteText()
        {
            try
            {
                if (this.View != null && ConnectionManager.CurrentSatellitePosition != null && ConnectionManager.CurrentSatellitePosition.Length > 0)
                {
                    lblCurrentSatellite.Text = ConnectionManager.CurrentSatellitePosition;
                    return;
                }

                if (this.View != null && ConnectionManager.CurrentService != null && !string.IsNullOrEmpty(ConnectionManager.CurrentService.Reference))
                {
                    string[] sData = ConnectionManager.CurrentService.Reference.Trim().Split(':');
                    var nameSpc = ConnectionManager.GetNamespaceFromReference(ConnectionManager.CurrentService);
                    if (nameSpc.ToLower().StartsWith("eeee"))
                        lblCurrentSatellite.Text = GetString(Resource.String.dvbt_service);
                    else if (nameSpc.ToLower().StartsWith("ffff"))
                        lblCurrentSatellite.Text = GetString(Resource.String.dvbc_service);
                    else if (sData.Length >= 10 &&
                             (
                                 sData[0] == "4097" ||
                                 sData[10].IndexOf("//", StringComparison.CurrentCulture) > -1 ||
                                 (sData.Length == 12 && sData[11] != null)
                             ))
                        lblCurrentSatellite.Text = GetString(Resource.String.stream_service);
                    else
                        lblCurrentSatellite.Text = GetString(Resource.String.stream_service);                    
                }
                else
                {
                    lblCurrentSatellite.Text = string.Empty; 
                }

            }
            catch (Exception ex)
            {
                Log.Error("SignalFragment", string.Format("SetCurrentSatelliteText failed with error {0}", ex.Message));  
            }
        }

        private void SetButtonText()
        {
            if (btnMonitor == null)
                return;
            
            if (ConnectionManager.SignalMonitor.Status == CommandMonitor<ISignalResponse>.MonitorStatus.Stopped)
                btnMonitor.SetText(Resource.String.start_monitor);
            else
                btnMonitor.SetText(Resource.String.stop_monitor);
        }

        private void SwitchTts()
        {
            if (ttsManager == null)
                ttsManager = new TextToSpeechManager(Activity);
        }
            
    }
}

