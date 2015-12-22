using System;
using Android.Content;
using Com.Tapjoy;
using Android.Widget;
using Android.Util;
using System.ComponentModel;
using Com.Krkadoni.Utils;
using System.Threading.Tasks;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class TapjoyManager : Java.Lang.Object, ITJGetCurrencyBalanceListener
    {
        private const string TAG = "TapjoyManager";
        private const string streamPlacementTitle = "StreamFeature";
        private const string streamRewardPlacementTitle = "StreamReward";

        Context context;
        GlobalApp app;
        Toast TapjoyToast;
        private bool connected = false;

        public enum FeatureBuyResult
        {
            FeatureAvailable = 0,
            NotConnected = 1,
            BalanceTooLow = 2,
            SpendingError = 3
        }

        #region "Singleton"

        private static TapjoyManager instance;
        private static object instanceLock = new System.Object();
        private CurrencySpentListener streamSpentListener;
        private ITJPlacementListener streamPlacementListener;
        private TJPlacement streamPlacement;
        private ConnectListener connectListener;
        private ITJEarnedCurrencyListener earnedListener;
        private ITJVideoListener videoListener;
        private TJPlacement streamRewardPlacement;

        public static int StreamPrice { get; private set; }

        private TapjoyManager(Context context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
            TapjoyToast = Toast.MakeText(context, string.Empty, ToastLength.Short);
            app = ((GlobalApp)context.ApplicationContext);
            StreamPrice = context.Resources.GetInteger(Resource.Integer.tapjoy_stream_price);
            connectListener = new ConnectListener();
            streamSpentListener = new CurrencySpentListener();
            streamPlacementListener = new PlacementListener();
            earnedListener = new CurrencyEarnedListener();
            videoListener = new VideoListener();
        }

        public static void Initialize(Context context)
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    instance = new TapjoyManager(context);       
                }             
            }
        }

        public static TapjoyManager GetInstance()
        {           
            if (instance == null)
                throw new InvalidOperationException("TapjoyManager is not initialized!");          
            return instance;
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged(string propertyName)
        {
            if (TapjoyManager.GetInstance().PropertyChanged != null)
            {
                TapjoyManager.GetInstance().PropertyChanged(TapjoyManager.GetInstance(), new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public async static void Connect(Action<bool> resultCallback)
        {
            if (instance == null)
                return;

            await Task.Delay(TimeSpan.FromSeconds(5));

            if (Tapjoy.Tapjoy.IsConnected)
            {
                Log.Debug(TAG, "Skipping Tapjoy connect, already connected!"); 
                if (resultCallback != null)
                    resultCallback.Invoke(true);
                return;
            }
            else if (ConnectFailed)
            {
                Log.Debug(TAG, "Skipping Tapjoy connect, previous attempt failed!"); 
                if (resultCallback != null)
                    resultCallback.Invoke(false);
                return;   
            }

            // Bug workaround https://code.google.com/p/android/issues/detail?id=12987
            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.GingerbreadMr1)
            {
                var detector = new JavaScriptEngineDetect(GetInstance().context);
                var result = detector.Detect();
                if (result != JavaScriptEngineDetect.JavaScriptEngine.V8)
                {
                    Log.Debug(TAG, "Not initializing Tapjoy on Gingerbread JSC engine. JavaScriptInterface bug workaround!"); 
                    EnableStreamFeature();
                    if (resultCallback != null)
                        resultCallback.Invoke(true);
                    return;
                }
            }

            var key = FileSystem.ReadStringAsset(GetInstance().context, GetInstance().context.GetString(Resource.String.tapjoy_key));
            if (string.IsNullOrEmpty(key))
            {
                Log.Error(TAG, "Failed to connect to Tapjoy service! Key is empty!");  
                if (resultCallback != null)
                    resultCallback.Invoke(false);
                return;
            }

            var gcmSenderId = FileSystem.ReadStringAsset(GetInstance().context, GetInstance().context.GetString(Resource.String.gcmsenderid_key));
            if (string.IsNullOrEmpty(key))
            {
                Log.Error(TAG, "Failed to connect to Tapjoy service! GCM Sender key is empy!");  
                if (resultCallback != null)
                    resultCallback.Invoke(false);
                return;
            }

            var parameters = new Java.Util.Hashtable();
            //parameters.Put(TapjoyConnectFlag.EnableLogging, "true");
            GetInstance().connectListener.ResultCallback = resultCallback;
            Tapjoy.Tapjoy.SetDebugEnabled(System.Diagnostics.Debugger.IsAttached);
            Tapjoy.Tapjoy.SetGcmSender(gcmSenderId);
            Tapjoy.Tapjoy.Connect(GetInstance().context, key, parameters, GetInstance().connectListener);
        }

        public static void Connect()
        {
            Connect(null);
        }

        public static bool ConnectFailed { get; internal set; }

        int balance;

        private static int Balance
        {
            get
            {
                return instance == null ? 0 : GetInstance().balance;
            }
            set
            {
                if (!Equals(TapjoyManager.Balance, value))
                {
                    GetInstance().balance = value;
                    OnPropertyChanged("Balance");
                }
            }
        }

        private static void UpdateBalance()
        {
            if (instance == null)
                return;
            
            try
            {
                if (!Tapjoy.Tapjoy.IsConnected)
                {
                    Log.Debug(TAG, string.Format("UpdateBalance failed! Not connected to TapJoy!"));
                    return;
                }
                Tapjoy.Tapjoy.GetCurrencyBalance(GetInstance());
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("UpdateBalance failed! {0}", ex.Message));
            } 
        }

        private static void BuyStream(Action<FeatureBuyResult> resultCallback)
        {
            if (instance == null)
                return;
            
            if (GetInstance().app.Settings.StreamActivated)
            {
                Log.Debug(TAG, string.Format("BuyStream is exiting! Stream is already available!"));  
                if (resultCallback != null)
                    resultCallback.Invoke(FeatureBuyResult.FeatureAvailable);
                return;
            }

            if (!Tapjoy.Tapjoy.IsConnected)
            {
                Log.Debug(TAG, string.Format("BuyStream failed! Not connected to TapJoy!"));  
                if (resultCallback != null)
                    resultCallback.Invoke(FeatureBuyResult.NotConnected);
                return;
            }

            if (Balance < StreamPrice)
            {
                Log.Debug(TAG, string.Format("BuyStream failed! Balance {0} is less than {1}!", Balance, StreamPrice));  
                if (resultCallback != null)
                    resultCallback.Invoke(FeatureBuyResult.BalanceTooLow);
                return;
            }

            GetInstance().streamSpentListener.ResultCallback = resultCallback;
            try
            {
                Tapjoy.Tapjoy.SpendCurrency(StreamPrice, GetInstance().streamSpentListener); 
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("BuyStream failed! {0}", ex.Message));  
                if (resultCallback != null)
                    resultCallback.Invoke(FeatureBuyResult.SpendingError);
                return;
            }
        }

        public static async void StreamAvailable(Action<FeatureBuyResult> resultCallback)
        {

            if (instance == null)
            {
                if (resultCallback != null)
                    resultCallback.Invoke(FeatureBuyResult.NotConnected);
                return;
            }

            try
            {
                if (GetInstance().app.Settings.StreamActivated)
                {
                    Log.Debug(TAG, string.Format("Stream feature is activated!"));  
                    if (resultCallback != null)
                        resultCallback.Invoke(FeatureBuyResult.FeatureAvailable);
                    return;
                }

                if (!Tapjoy.Tapjoy.IsConnected)
                {
                    //check if internet is available
                    if (await Network.CheckPortOpen("www.google.com", 80))
                    {
                        //internet is available, tapjoy is blocked
                        ShowAdBlockDialog();
                    }
                    else
                    {
                        ShowNotConnectedDialog();
                    }

                    if (resultCallback != null)
                        resultCallback.Invoke(FeatureBuyResult.NotConnected);
                    return;
                }    

                if (Balance == 0)
                {
                    ShowFeatureRequiresActionsDialog();
                    if (resultCallback != null)
                        resultCallback.Invoke(FeatureBuyResult.BalanceTooLow);
                    return;
                }
                else if (Balance < StreamPrice)
                {
                    ShowInsufficientBalanceDialog();
                    if (resultCallback != null)
                        resultCallback.Invoke(FeatureBuyResult.NotConnected);
                    return;
                }
                else
                {
                    BuyStream((result) =>
                        {  
                            if (result == FeatureBuyResult.FeatureAvailable)
                                EnableStreamFeature();
                            if (resultCallback != null)
                                resultCallback.Invoke(result);
                        });
                } 
            }
            catch (Exception ex)
            {
                Dialogs.ErrorDialog(GetInstance().context, ex.Message, (sender, e) =>
                    {
                    });     
            }
        }

        public static void ShowStreamPlacement()
        {
            if (StreamPlacementAvailable)
            {
                if (GetInstance().streamPlacement.IsContentAvailable)
                {
                    GetInstance().streamPlacement.ShowContent(); 
                }
                else
                    Toast.MakeText(GetInstance().context, Resource.String.content_not_available, ToastLength.Short).Show();
            }
        }

        public static bool StreamPlacementAvailable
        {
            get
            {
                return instance != null && GetInstance().connected;
            }
        }

        public static void HandlePushPayload(string payload)
        {
            if (instance == null)
                return;

            if (payload == null)
            {
                Log.Debug(TAG, "HandlePushPayload received null payload!");
                return;
            }

            Log.Debug(TAG, "HandlePushPayload received payload " + payload);

            if (GetInstance().context == null)
            {
                Log.Debug(TAG, "HandlePushPayload not handling payload. Context is not initialized");
                return;
            }

            if (payload.ToLower().IndexOf("stream=1") > -1)
            {
                EnableStreamFeature();
            }
            if (payload.ToLower().IndexOf("stream=0") > -1)
            {
                DisableStreamFeature();
            }

            //GetInstance().TapjoyToast.SetText("Tapjoy push payload: " + payload);
            //GetInstance().TapjoyToast.Show();

        }

        public void OnGetCurrencyBalanceResponse(string currency, int amount)
        {
            if (instance == null)
                return;
            
            Log.Debug(TAG, string.Format("Received {0} balance: {1}", currency, amount));
            Balance = amount;
        }

        public void OnGetCurrencyBalanceResponseFailure(string currency)
        {
            if (instance == null)
                return;
            
            Balance = 0;
            Log.Error(TAG, string.Format("Failed to get balance for currency {0}", currency));
        }

        private static void EnableStreamFeature()
        {
            GetInstance().app.Settings.StreamActivated = true;
            ((PreferenceManager)GetInstance().app.PreferenceManager).SaveSettings(GetInstance().context, GetInstance().app.Settings);
            if (Tapjoy.Tapjoy.IsConnected)
                Tapjoy.Tapjoy.SetUserLevel(1);
            Log.Debug(TAG, "Stream function is now enabled");
        }

        private static void DisableStreamFeature()
        {
            GetInstance().app.Settings.StreamActivated = false;
            ((PreferenceManager)GetInstance().app.PreferenceManager).SaveSettings(GetInstance().context, GetInstance().app.Settings);
            if (Tapjoy.Tapjoy.IsConnected)
                Tapjoy.Tapjoy.SetUserLevel(0);
            Log.Debug(TAG, "Stream function is now DISABLED");
        }

        private static void ShowNotConnectedDialog()
        {
            if (!GetInstance().app.ActivityStarted)
                return;
            
            //internet is not available
            var message = string.Format("{0} {1} {2}", 
                              GetInstance().context.GetString(Resource.String.tapjoy_failed_connect),
                              GetInstance().context.GetString(Resource.String.err_check_your_connection),
                              GetInstance().context.GetString(Resource.String.inf_streaming_not_available));
            Dialogs.WarningDialog(GetInstance().context, message, (sender, e) =>
                {
                }).Show();
        }

        private static void ShowAdBlockDialog()
        {
            if (!GetInstance().app.ActivityStarted)
                return;
            
            //internet is available, tapjoy is blocked
            var message = string.Format("{0} {1} {2}", 
                              GetInstance().context.GetString(Resource.String.tapjoy_failed_connect),
                              GetInstance().context.GetString(Resource.String.please_disable_ad_blocker),
                              GetInstance().context.GetString(Resource.String.inf_streaming_not_available));
            Dialogs.WarningDialog(GetInstance().context, message, (sender, e) =>
                {
                }).Show();
        }

        private static void ShowInsufficientBalanceDialog()
        {
            if (!GetInstance().app.ActivityStarted)
                return;

            //user needs more coins
            var message = string.Format("{0} {1}", 
                              string.Format(GetInstance().context.GetString(Resource.String.tapjoy_not_enough), StreamPrice, Balance),
                              GetInstance().context.GetString(Resource.String.tapjoy_would_you_like_to_earn_more_coins));
            Action<object, DialogClickEventArgs> positiveAction = (sender, e) =>
            {
                GetInstance().streamPlacement.ShowContent();   
            };
            Dialogs.QuestionDialog(GetInstance().context, message, positiveAction, (sender, e) =>
                {
                }).Show();   
        }

        private static void ShowFeatureRequiresActionsDialog()
        {
            if (!GetInstance().app.ActivityStarted)
                return;

            //user needs more coins
            var message = GetInstance().context.GetString(Resource.String.tapjoy_feature_not_available);
            Action<object, DialogClickEventArgs> positiveAction = (sender, e) =>
            {
                if (!GetInstance().streamPlacement.IsContentAvailable)
                {
                    GetInstance().streamPlacement.RequestContent();
                    return;
                }
     
                GetInstance().streamPlacement.ShowContent();   
            };
            Dialogs.QuestionDialog(GetInstance().context, message, positiveAction, (sender, e) =>
                {
                }).Show();   
        }

        private class ConnectListener : Java.Lang.Object, ITJConnectListener
        {
            public async void OnConnectFailure()
            {
                Log.Error(TAG, "Failed to connect to Tapjoy service!");

                //we don't care about failure if we already have stream feature
                if (!ConnectFailed && !GetInstance().app.Settings.StreamActivated)
                {
                    //check if internet is available
                    if (await Network.CheckPortOpen("www.google.com", 80))
                    {
                        //internet is available, tapjoy is blocked
                        ShowAdBlockDialog();
                    }
                    else
                    {
                        ShowNotConnectedDialog();
                    }
                }

                ConnectFailed = true;

                if (ResultCallback != null)
                    ResultCallback.Invoke(false);
            }

            public void OnConnectSuccess()
            {
                Log.Debug(TAG, "Connected to Tapjoy service!");

                ConnectFailed = false;
                GetInstance().connected = true;
                UpdateBalance();

                GetInstance().streamPlacement = new TJPlacement(GetInstance().context, streamPlacementTitle, GetInstance().streamPlacementListener);
                GetInstance().streamPlacement.RequestContent();

                Tapjoy.Tapjoy.SetUserCohortVariable(1, GetInstance().app.ReleaseName);

                if (!GetInstance().app.Settings.StreamActivated)
                {
                    GetInstance().streamRewardPlacement = new TJPlacement(GetInstance().context, streamRewardPlacementTitle, new StreamRewardPlacementListener());
                    GetInstance().streamRewardPlacement.RequestContent();
                }

                Tapjoy.Tapjoy.SetEarnedCurrencyListener(GetInstance().earnedListener);
                Tapjoy.Tapjoy.SetVideoListener(GetInstance().videoListener);

                if (ResultCallback != null)
                    ResultCallback.Invoke(true);
            }

            public Action<bool> ResultCallback { get; set; }
        }

        private class CurrencySpentListener : Java.Lang.Object, ITJSpendCurrencyListener
        {
            public void OnSpendCurrencyResponse(string currency, int amount)
            {
                Log.Debug(TAG, string.Format("Spent {1} {0} on stream feature", currency, amount));
                UpdateBalance();
                if (ResultCallback != null)
                {
                    ResultCallback.Invoke(FeatureBuyResult.FeatureAvailable);
                }
            }

            public void OnSpendCurrencyResponseFailure(string currency)
            {
                Log.Error(TAG, string.Format("Failed to spend {0} on Stream feature}", currency));
                UpdateBalance();
                if (ResultCallback != null)
                {
                    ResultCallback.Invoke(FeatureBuyResult.SpendingError);
                }
            }

            public Action<FeatureBuyResult> ResultCallback { get; set; }

        }

        private class PlacementListener : Java.Lang.Object, ITJPlacementListener
        {

            public virtual void OnContentDismiss(TJPlacement p0)
            {
                Log.Debug(TAG, string.Format("OnContentDismiss: {0}", p0.Name));
                UpdateBalance();
                p0.RequestContent();
            }

            public virtual void OnContentReady(TJPlacement p0)
            {
                Log.Debug(TAG, string.Format("OnContentReady: {0}", p0.Name));
            }

            public virtual void OnContentShow(TJPlacement p0)
            {
                Log.Debug(TAG, string.Format("OnContentShow: {0}", p0.Name));
            }

            public virtual void OnPurchaseRequest(TJPlacement p0, ITJActionRequest p1, string p2)
            {
                Log.Debug(TAG, string.Format("OnPurchaseRequest: {0}", p0.Name));
            }

            public virtual void OnRequestFailure(TJPlacement p0, TJError p1)
            {
                Log.Error(TAG, string.Format("OnRequestFailure: {0}", p1.Message));
            }

            public virtual void OnRequestSuccess(TJPlacement p0)
            {
                Log.Debug(TAG, string.Format("OnRequestSuccess: {0}", p0.Name));
            }

            public virtual void OnRewardRequest(TJPlacement p0, ITJActionRequest request, string p2, int p3)
            {
                Log.Debug(TAG, string.Format("OnRewardRequest: {0}", p0.Name));
                request.Completed();
            }
            
        }

        private class StreamRewardPlacementListener : PlacementListener
        {
            public override void OnContentReady(TJPlacement p0)
            {
                base.OnContentReady(p0);
                if (!GetInstance().app.Settings.StreamActivated)
                {
                    p0.ShowContent();
                }
            }

            public override void OnRewardRequest(TJPlacement p0, ITJActionRequest request, string p2, int p3)
            {
                Log.Debug(TAG, string.Format("OnStreamRewardRequest: {0}", p0.Name));
                EnableStreamFeature();
                request.Completed();
            }

            public override void OnContentDismiss(TJPlacement p0)
            {
                Log.Debug(TAG, string.Format("OnStreamContentDismiss: {0}", p0.Name));

            }
        }

        private class CurrencyEarnedListener : Java.Lang.Object, ITJEarnedCurrencyListener
        {
            public void OnEarnedCurrency(string currency, int amount)
            {
                UpdateBalance();
                //GetInstance().TapjoyToast.SetText(string.Format(GetInstance().context.GetString(Resource.String.tapjoy_earned_coins), amount));
                //GetInstance().TapjoyToast.Show();
            }
            
        }

        private class VideoListener : Java.Lang.Object, ITJVideoListener
        {
            public void OnVideoComplete()
            {
                Log.Debug(TAG, "OnVideoComplete");
                UpdateBalance();
            }

            public void OnVideoError(int p0)
            {
                Log.Debug(TAG, string.Format("OnVideoError: {0}", p0));
            }

            public void OnVideoStart()
            {
                Log.Debug(TAG, "OnVideoStart");
            }
            
        }

    }
}

