using System;
using Krkadoni.Enigma;
using Krkadoni.Enigma.Commands;
using System.Threading;
using System.Collections.Generic;
using Android.Util;
using System.Threading.Tasks;
using Krkadoni.Enigma.Responses;
using System.ComponentModel;
using System.Diagnostics;
using Krkadoni.Enigma.Enums;


namespace Com.Krkadoni.App.SignalMeter.Model
{
    public class ConnectionManager : INotifyPropertyChanged
    {

        private ConnectionManager()
        {
            factory = new ModernClientFactory();
            ((ConsoleLog)factory.Log()).SetDebug(Debugger.IsAttached);
            CurrentServiceMonitor = new CommandMonitor<IGetCurrentServiceResponse>(GetCurrentServiceForCurrentProfile);
            SignalMonitor = new CommandMonitor<ISignalResponse>(SignalForCurrentProfile);
            CurrentServiceMonitor.Delay = TimeSpan.FromSeconds(ServiceMonitorDelayInSeconds);
            CurrentServiceMonitor.MonitorCommandFinished += (sender, e) =>
            {
                if (Connected)
                {
                    CurrentService = e.Item1.CurrentService;
                }
            };
        }

        private const string TAG = "ConnectionManager";
        private static IFactory factory;
        private const int ServiceMonitorDelayInSeconds = 15;
        private const int commandTimeOutInSeconds = 15;

        public enum ConnectionStatusEnum
        {
            Disconnected = 0,
            Connecting = 1,
            Connected = 2,
            Errored = 3,
            Disconnecting = 4
        }

        #region "Singleton"

        private static ConnectionManager instance;
        private static object instanceLock = new System.Object();

        public static ConnectionManager GetInstance()
        {           
            if (instance == null)
            {
                lock (instanceLock)
                {
                    instance = new ConnectionManager();       
                }             
            }
            return instance;
        }

        #endregion

        #region "Locks"

        private static object profileLock = new object();

        private static object selectedProfileLock = new object();

        private static object bouquetLock = new object();

        private static object serviceLock = new object();

        private static object bouquetsLock = new object();

        private static object connectLock = new object();

        private static object disconnectLock = new object();

        private static object cachedBouquetItemsLock = new object();

        private static object bouquetItemsLock = new object();

        private static object satellitesLock = new object();

        #endregion

        #region "Properties"

        static Dictionary<IBouquetItemBouquet, IList<IBouquetItem>> cachedBouquetItems;

        private static Dictionary<IBouquetItemBouquet, IList<IBouquetItem>> CachedBouquetItems
        {
            get
            {
                return cachedBouquetItems;
            }
            set
            {
                cachedBouquetItems = value;
            }
        }

        private static Dictionary<int, string> satellites;

        public static Dictionary<int, string> Satellites
        {
            get
            {
                return satellites;
            }
            set
            {
                lock (satellitesLock)
                {
                    if (!Equals(satellites, value))
                    {
                        satellites = value;
                        OnPropertyChanged("Satellites");
                    }
                }
            }
        }

        static ConnectionStatusEnum connectionStatus;

        public static ConnectionStatusEnum ConnectionStatus
        {
            get
            {
                return connectionStatus;
            }
            set
            {
                if (!Equals(value, connectionStatus))
                {
                    connectionStatus = value;
                    OnPropertyChanged("ConnectionStatus");
                    if (!Connected)
                    {
                        CurrentServiceMonitor.Stop();
                    }
                    else
                    {
                        CurrentServiceMonitor.Start();
                    }
                }
            }
        }

        public static bool Connected
        {
            get
            {
                return ConnectionStatus == ConnectionStatusEnum.Connected;
            }
        }

        private static CancellationTokenSource tokenSource;

        private static SignalMeterProfile selectedProfile;

        public static SignalMeterProfile SelectedProfile
        {
            get
            {
                return selectedProfile;
            }
            set
            {
                lock (selectedProfileLock)
                {
                    if (!Equals(value, selectedProfile))
                    {
                        selectedProfile = value;
                        OnPropertyChanged("SelectedProfile");
                    }
                }
            }
        }

        static IBouquetItemBouquet selectedBouquet;

        public static IBouquetItemBouquet SelectedBouquet
        {
            get
            {
                return selectedBouquet;
            }
            set
            {
                lock (bouquetLock)
                {
                    if (!Equals(value, selectedBouquet))
                    {
                        selectedBouquet = value;
                        OnPropertyChanged("SelectedBouquet");
                    }
                }
            }
        }

        static IBouquetItemService selectedService;

        public static IBouquetItemService SelectedService
        {
            get
            {
                return selectedService;
            }
            set
            {
                if (!Equals(value, selectedService))
                {
                    selectedService = value; 
                    OnPropertyChanged("SelectedService");
                }
            }
        }

        static IList<IBouquetItem> bouquetItems;

        public static IList<IBouquetItem> BouquetItems
        {
            get
            {
                return bouquetItems ?? new List<IBouquetItem>();
            }
            private set
            {
                lock (bouquetItemsLock)
                {
                    if (!Equals(value, bouquetItems))
                    {
                        bouquetItems = value;
                        OnPropertyChanged("BouquetItems");
                    }
                }
            }
        }

        private static SignalMeterProfile currentProfile;

        public static SignalMeterProfile CurrentProfile
        {
            get
            {
                return currentProfile;
            }
            private set
            {
                lock (profileLock)
                {
                    if (!Equals(value, currentProfile))
                    {
                        currentProfile = value;
                        OnPropertyChanged("CurrentProfile");
                    }
                }
            }
        }

        static IBouquetItemService currentService;

        public static IBouquetItemService CurrentService
        {
            get
            {
                return currentService;
            }
            private set
            {
                lock (serviceLock)
                {
                    if (currentService == null && value == null)
                        return;

                    if ((currentService != null && value == null) || (currentService == null && value != null))
                    {
                        currentService = value;
                        OnPropertyChanged("CurrentService");
                        return;
                    }

                    if (currentService.Reference != value.Reference || CurrentService.Name != value.Name)
                    {
                        currentService = value;
                        OnPropertyChanged("CurrentService");
                    }
                }
                UpdateSatellitePosition();
            }
        }

        static string currentSatellitePosition;

        public static string CurrentSatellitePosition
        {
            get
            {
                return currentSatellitePosition;
            }
            private set
            {
                if (!Equals(value, currentSatellitePosition))
                {
                    currentSatellitePosition = value;
                    OnPropertyChanged("CurrentSatellitePosition");
                }
            }
        }

        static IList<IBouquetItemBouquet> bouquets;

        public static IList<IBouquetItemBouquet> Bouquets
        {
            get
            {
                return bouquets;
            }
            private set
            {
                lock (bouquetsLock)
                {
                    if (!Equals(value, bouquets))
                    { 
                        bouquets = value;
                        OnPropertyChanged("Bouquets");
                    }
                }
            }
        }

        static List<SignalMeterProfile> profiles;

        public static List<SignalMeterProfile> Profiles
        {
            get
            {
                return profiles;
            }
            set
            {
                if (!Equals(profiles, value))
                {
                    profiles = value;
                    OnPropertyChanged("Profiles");
                }
            }
        }

        public static CommandMonitor<IGetCurrentServiceResponse> CurrentServiceMonitor{ get; private set; }

        public static CommandMonitor<ISignalResponse> SignalMonitor { get; private set; }

        #endregion

        #region "Public methods"

        public static async Task Connect(SignalMeterProfile profile)
        {
            lock (connectLock)
            {
                //we're alredy conntected to selected profile
                if (Connected && CurrentProfile == profile)
                    return;

                if (ConnectionStatus == ConnectionStatusEnum.Connecting)
                {
                    Log.Debug(TAG, "Already trying to connect, ignoring new requests");
                    return;
                }

                //disconnect current profile
                if (Connected)
                {
                    Disconnect();
                }
                ConnectionStatus = ConnectionStatusEnum.Connecting;
            }

            tokenSource = new CancellationTokenSource();  
            var result = await WakeUp(profile);

            if (!result)
            {
                ConnectionStatus = ConnectionStatusEnum.Errored; 
                return;  
            }

            CachedBouquetItems = new Dictionary<IBouquetItemBouquet, IList<IBouquetItem>>();
            CurrentProfile = profile;
            ConnectionStatus = ConnectionStatusEnum.Connected; 
        }

        public static async Task LoadBouquets(SignalMeterProfile profile)
        {
            if (!Connected)
            {
                Log.Debug(TAG, string.Format("Cannot load bouquets for profile {0}. Not connected!", profile.Name));
                return;
            }
            var response = await GetBouquets(profile);
            if (response != null)
                Bouquets = response.Bouquets;
        }

        public static void Disconnect()
        {
            lock (disconnectLock)
            {
                if (ConnectionStatus == ConnectionStatusEnum.Disconnecting)
                    return;

                ConnectionStatus = ConnectionStatusEnum.Disconnecting;
                Log.Debug(TAG, "Disconnecting...");

                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                }

                CurrentProfile = null;
                SelectedBouquet = null;
                SelectedService = null;
                CurrentService = null;
                CurrentSatellitePosition = string.Empty;
                if (BouquetItems != null)
                    BouquetItems.Clear();
                BouquetItems = null;
                if (Bouquets != null)
                    Bouquets.Clear();
                Bouquets = null;
                if (CachedBouquetItems != null)
                    CachedBouquetItems.Clear();
                CachedBouquetItems = null;


                ConnectionStatus = ConnectionStatusEnum.Disconnected;
                Log.Debug(TAG, "Disconnected!");   
            }               
        }

        public static async Task LoadBouquetItems(IBouquetItemBouquet bouquet)
        {
            if (bouquet == null)
            {
                SelectedBouquet = null;
                BouquetItems = null;            
                return;
            }

            if (CachedBouquetItems.ContainsKey(bouquet))
            {
                BouquetItems = CachedBouquetItems[bouquet];
                return;
            }

            var result = await GetBouquetItems(CurrentProfile, bouquet);

            if (Connected)
            {
                lock (CachedBouquetItems)
                {
                    if (CachedBouquetItems.ContainsKey(bouquet))
                    {
                        CachedBouquetItems[bouquet] = result.Items;
                    }
                    else
                    {
                        CachedBouquetItems.Add(bouquet, result.Items);
                        CachedBouquetItems[bouquet] = result.Items;
                    }
                }
                BouquetItems = CachedBouquetItems[bouquet];
            }
        }

        public async static Task ChangeService(IBouquetItemService service)
        {
            if (service == null)
            {
                SelectedService = null;
                return;
            }

            await Zap(CurrentProfile, service);
            await ReadCurrentService(CurrentProfile);
        }

        public async static Task ReadCurrentService(SignalMeterProfile profile)
        {
            if (!Connected)
            {
                Log.Error("Unable to read current service for profile {0}. Not connected.", profile.Name);
                return;
            }

            var response = (await GetCurrentService(profile));

            if (response != null)
                CurrentService = response.CurrentService;
        }

        public static string GetNamespaceFromReference(IBouquetItemService service)
        {

            if (service == null || service.Reference == null || service.Reference.Length == 0)
                return string.Empty;
            try
            {
                string[] sData = service.Reference.Trim().Split(':');
                string mNameSpc = string.Empty;

                if (sData.Length >= 10)
                    mNameSpc = sData[6].TrimStart('0');
                else
                    mNameSpc = sData[1].TrimStart('0');

                return mNameSpc;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("GetNamespaceFromReference failed with error {0}", ex.Message));
                return string.Empty;
            }
        }

        public static async Task<IGetStreamParametersResponse> GetStreamParametersForService(IBouquetItemService service)
        {
            if (!Connected || service == null)
                return null;

            return await GetStreamParameters(CurrentProfile, service);
        }

        public static async Task<IScreenshotResponse> GetScreenShotOfCurrentService(ScreenshotType screenshotType)
        {
            if (!Connected || CurrentService == null || CurrentProfile == null)
                return null;

            return await GetScreenShot(CurrentProfile, screenshotType);
        }

        public static async Task<bool> SendToSleepAndDisconnectCurrentProfile()
        {
            if (!Connected || CurrentProfile == null)
                return false;

            ConnectionManager.SignalMonitor.Stop();
            ConnectionManager.CurrentServiceMonitor.Stop();

            var result = await SendToSleep(CurrentProfile);
            Disconnect();
            return result;
        }

        public static async Task<bool> RestartAndDisconnectCurrentProfile()
        {
            if (!Connected || CurrentProfile == null)
                return false;

            ConnectionManager.SignalMonitor.Stop();
            ConnectionManager.CurrentServiceMonitor.Stop();

            var result = await Restart(CurrentProfile);
            Disconnect();
            return result;
        }

        #endregion

        #region "Command invokers"

        private static async Task<bool> WakeUp(SignalMeterProfile profile)
        {
            var command = factory.WakeUpCommand();
            var task = command.ExecuteAsync(profile, new CancellationToken());
            await PerformTask<IWakeUpCommand,IResponse<IWakeUpCommand>>(profile, command, task);
            return !task.IsFaulted && !task.IsCanceled && task.IsCompleted;
        }

        private static async Task<IGetBouquetsResponse> GetBouquets(SignalMeterProfile profile)
        {
            var command = factory.GetBouquetsCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token);
            var success = await PerformTask<IGetBouquetsCommand,IGetBouquetsResponse>(profile, command, task);      
            return  success ? task.Result : factory.GetBouquetsResponse();
        }

        private static async Task<IGetBouquetItemsResponse> GetBouquetItems(SignalMeterProfile profile, IBouquetItemBouquet bouquet)
        {
            var command = factory.GetBouquetItemsCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token, bouquet);
            var success = await PerformTask<IGetBouquetItemsCommand,IGetBouquetItemsResponse>(profile, command, task);
            return success ? task.Result : factory.GetBouquetItemsResponse();
        }

        private async static Task<bool> Zap(SignalMeterProfile profile, IBouquetItemService service)
        {
            var command = factory.ZapCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token, service);
            var success = await PerformTask<IZapCommand,IResponse<IZapCommand>>(profile, command, task);
            return success;
        }

        private async static Task<IGetCurrentServiceResponse> GetCurrentService(SignalMeterProfile profile)
        {
            var command = factory.GetCurrentServiceCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token);
            var success = await PerformTask<IGetCurrentServiceCommand,IGetCurrentServiceResponse>(profile, command, task);
            return success ? task.Result : null;
        }

        public async static Task<ISignalResponse> GetSignalLevels(SignalMeterProfile profile)
        {
            var command = factory.SignalCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token);
            var success = await PerformTask<ISignalCommand,ISignalResponse>(profile, command, task);      
            return  success ? task.Result : null;
        }

        private async static Task<IScreenshotResponse> GetScreenShot(SignalMeterProfile profile, ScreenshotType screenshotType)
        {
            var command = factory.ScreenshotCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token, screenshotType);
            var success = await PerformTask<IScreenshotCommand,IScreenshotResponse>(profile, command, task);      
            return  success ? task.Result : null;
        }

        private async static Task<bool> SendToSleep(SignalMeterProfile profile)
        {
            var command = factory.SleepCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token);
            var success = await PerformTask<ISleepCommand,IResponse<ISleepCommand>>(profile, command, task);
            return success;
        }

        private async static Task<bool> Restart(SignalMeterProfile profile)
        {
            var command = factory.RestartCommand();
            var task = command.ExecuteAsync(profile, tokenSource.Token);
            var success = await PerformTask<IRestartCommand,IResponse<IRestartCommand>>(profile, command, task, 1);
            return success;
        }

        private async static Task<IGetStreamParametersResponse> GetStreamParameters(SignalMeterProfile profile, IBouquetItemService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            
            if (profile == null)
                throw new ArgumentNullException("profile");
     
            var command = factory.GetStreamParametersCommand();
            var task = command.ExecuteAsync(profile, service, tokenSource.Token);
            var success = await PerformTask<IGetStreamParametersCommand, IGetStreamParametersResponse>(profile, command, task);
            return success ? task.Result : null;
        }

        private async static Task<bool> PerformTask<TCommand, TResponse>(SignalMeterProfile profile, TCommand command, Task<TResponse> commandTask, int timeout = commandTimeOutInSeconds) 
            where TCommand : class, ICommand
            where TResponse : class, IResponse<TCommand>
        {
            //don't perform command if not connected and not trying to wake the box up
            if (!Connected && typeof(TCommand) != typeof(IWakeUpCommand))
            {
                Log.Error(TAG, string.Format("Not performing command {0}. Not connected", typeof(TCommand).Name));
                return false;
            }

            if (commandTask.Status != TaskStatus.WaitingForActivation)
            {
                Log.Error(TAG, string.Format("Not performing command {0}. Task has already ran.", typeof(TCommand).Name));
                return false;
            }

            try
            {
                if (tokenSource != null)
                    tokenSource.Token.ThrowIfCancellationRequested();
                if (await Task.WhenAny(commandTask, Task.Delay(TimeSpan.FromSeconds(timeout), tokenSource.Token)) != commandTask)
                { 
                    commandTask.ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                var aggException = t.Exception.Flatten();
                                System.Diagnostics.Debug.WriteLine(string.Format("{0} failed after being canceled. {1}", typeof(TCommand).Name, aggException.Message));  
                            }
                        }, 
                        TaskContinuationOptions.OnlyOnFaulted);
                    if (tokenSource != null)
                        tokenSource.Cancel();
                    throw new OperationCanceledException();
                }

                if (tokenSource != null)
                    tokenSource.Token.ThrowIfCancellationRequested();

                if (Connected && !commandTask.IsCanceled && !commandTask.IsFaulted && commandTask.IsCompleted)
                    return true;

                if (commandTask.Exception != null)
                {                    
                    Exception exToThrow = commandTask.Exception.Flatten();
                    while (exToThrow.InnerException != null)
                    {
                        if (exToThrow is KnownException)
                        {
                            break;   
                        }
                        exToThrow = exToThrow.InnerException;
                    }
                    throw exToThrow;
                }
                    
                if (tokenSource != null)
                    tokenSource.Token.ThrowIfCancellationRequested();

                if (!Connected)
                    Log.Error(TAG, String.Format("Command {0} executed but we're not connected anymore.", typeof(TCommand).Name));
                return true;
            }
            catch (System.OperationCanceledException ex)
            {
                Log.Error(TAG, String.Format("Command is cancelled. {0} ", ex.Message));
                if (ConnectionStatus == ConnectionStatusEnum.Errored || ConnectionStatus == ConnectionStatusEnum.Disconnecting)
                    return false;
                ConnectionStatus = ConnectionStatusEnum.Errored;
                OnExceptionRaised(new ExceptionRaisedEventArgs(profile, command, ex));
                Disconnect();
                ConnectionStatus = ConnectionStatusEnum.Errored;
                return false;
            }
            catch (KnownException ex)
            {
                Log.Error(TAG, String.Format("Known exception: {0} ", ex.Message));
                if (ConnectionStatus == ConnectionStatusEnum.Errored || ConnectionStatus == ConnectionStatusEnum.Disconnecting)
                    return false;
                ConnectionStatus = ConnectionStatusEnum.Errored;
                OnExceptionRaised(new ExceptionRaisedEventArgs(profile, command, ex));
                Disconnect();
                ConnectionStatus = ConnectionStatusEnum.Errored;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, String.Format("Unknown exception: {0} ", ex.Message));
                if (ConnectionStatus == ConnectionStatusEnum.Errored || ConnectionStatus == ConnectionStatusEnum.Disconnecting)
                    return false;
                ConnectionStatus = ConnectionStatusEnum.Errored;
                OnExceptionRaised(new ExceptionRaisedEventArgs(profile, command, ex));
                Disconnect();
                ConnectionStatus = ConnectionStatusEnum.Errored;
                return false;
            }
        }

        #endregion

        #region "Events"

        public delegate void ExceptionRaisedHandler(object sender,ExceptionRaisedEventArgs e);

        public event ExceptionRaisedHandler ExceptionRaised;

        protected static void OnExceptionRaised(ExceptionRaisedEventArgs args)
        {
            if (ConnectionManager.GetInstance().ExceptionRaised != null)
            {
                ConnectionManager.GetInstance().ExceptionRaised(ConnectionManager.GetInstance(), args);
            }

        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged(string propertyName)
        {
            if (ConnectionManager.GetInstance().PropertyChanged != null)
            {
                ConnectionManager.GetInstance().PropertyChanged(ConnectionManager.GetInstance(), new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        private static async Task<IGetCurrentServiceResponse> GetCurrentServiceForCurrentProfile()
        {
            if (!Connected || CurrentProfile == null)
                return null;
            return await GetCurrentService(CurrentProfile);
        }

        private static async Task<ISignalResponse> SignalForCurrentProfile()
        {
            if (!Connected || CurrentProfile == null || CurrentService == null)
                return null;
            return await GetSignalLevels(CurrentProfile);
        }

        private static int GetSatellitePosition(IBouquetItemService service)
        {
            if (service == null || service.Reference == null || service.Reference.Length == 0)
                return 0;

            string mNameSpc = GetNamespaceFromReference(service);

            if (mNameSpc.Length < 5)
                return 0;

            mNameSpc = mNameSpc.Substring(0, mNameSpc.Length - 4);
            try
            {
                int decValue = int.Parse(mNameSpc, System.Globalization.NumberStyles.HexNumber); 
                return decValue;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex.Message);
                return 0;
            }

        }

        private static void UpdateSatellitePosition()
        {
            int position = GetSatellitePosition(CurrentService);
            lock (satellitesLock)
            {
                if (satellites != null && satellites.ContainsKey(position))
                    CurrentSatellitePosition = satellites[position];
                else
                    CurrentSatellitePosition = string.Empty;    
            }
        }

    }
}

