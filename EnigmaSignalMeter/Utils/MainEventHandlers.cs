using System;
using Android.Content;
using Com.Krkadoni.App.SignalMeter.Model;
using Krkadoni.Enigma.Commands;
using Krkadoni.Enigma;
using System.Linq;
using Android.Util;
using System.Net;
using System.Net.Http;
using Android.Widget;
using Com.Krkadoni.Utils;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class MainEventHandlers
    {

        public enum ViewsEnum
        {
            None = -1,
            Profiles = 0,
            Bouquets = 1,
            Services = 2,
            Signal = 3,
        }

        Context context;
        private Android.Support.V7.App.AlertDialog progressDialog;
        GlobalApp app;

        private Toast ConnectStatusToast { get; set; }

        private const string TAG = "MainEventHandlers";

        public MainEventHandlers(Context context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
            app = ((GlobalApp)context.ApplicationContext);
            ConnectStatusToast = Toast.MakeText(context, string.Empty, ToastLength.Long);
            TapjoyManager.Initialize(context);
            TapjoyManager.Connect(delegate
                {
                    if (SetMenuVisibility != null)
                        SetMenuVisibility.Invoke();
                });
        }

        public void ConnectionManager_ExceptionRaised(object sender, ExceptionRaisedEventArgs e)
        {
            HideProgressDialog();
            if (!app.ActivityStarted)
                return;
            var message = string.Empty;
            DisplayView(ViewsEnum.Profiles);
            if (e.Command is IRestartCommand)
            {
                return;
            }
            if (e.Command is IWakeUpCommand && e.Exception is System.OperationCanceledException)
            {
                message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name);
                message += " " + context.GetString(Resource.String.err_operation_timed_out);
            }
            else
            {
                var command = e.Command.GetType().Name.Replace("Command", "");
                var commandFailed = string.Format(context.GetString(Resource.String.err_command_failed), command);
                message = commandFailed;

                var firstEx = e.Exception;
                if (e.Exception is AggregateException)
                {
                    firstEx = ((AggregateException)e.Exception).Flatten();
                }

                while (firstEx.InnerException != null)
                {
                    firstEx = firstEx.InnerException;
                    if (firstEx is KnownException)
                        break;
                }

                Log.Error(TAG, firstEx.Message);
                Log.Error(TAG, firstEx.StackTrace);
                if (firstEx is WebException)
                {
                    WebException webEx = (WebException)firstEx;
                    if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        if (e.Command is IWakeUpCommand)
                            message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name) + " " + context.GetString(Resource.String.err_check_your_settings);
                        else
                            message += " " + context.GetString(Resource.String.err_check_your_settings);
                    }
                    else if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (e.Command is IWakeUpCommand)
                            message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name) + " " + context.GetString(Resource.String.err_check_your_credentials);
                        else
                            message += " " + context.GetString(Resource.String.err_check_your_settings);
                    }
                    else if (webEx.Status == WebExceptionStatus.ConnectFailure)
                    {
                        if (e.Command is IWakeUpCommand)
                            message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name);
                        else
                            message += " " + context.GetString(Resource.String.err_check_your_connection);
                    }
                    else
                        message += " " + string.Format(context.GetString(Resource.String.err_request_failed_with_status_code), webEx.Status.ToString());
                }
                else if (firstEx is Java.Net.ConnectException)
                {
                    if (e.Command is IWakeUpCommand)
                        message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name);
                    else
                        message += " " + context.GetString(Resource.String.err_check_your_connection);
                    Log.Debug(TAG, "Java.Net.ConnectException");
                }
                else if (firstEx is Java.Lang.RuntimeException)
                {
                    if (e.Command is IWakeUpCommand && firstEx.Message.ToUpper().IndexOf("EAI_NODATA") > -1)
                    {
                        message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name);
                        message += " " + context.GetString(Resource.String.err_invalid_address);
                    }
                    else
                        message += " " + context.GetString(Resource.String.err_check_your_connection);
                    Log.Debug(TAG, "Java.Net.ConnectException");
                }
                else if (firstEx is Java.Lang.Throwable)
                {
                    var javaMessage = ((Java.Lang.Throwable)firstEx).Message;
                    if (javaMessage.IndexOf('(') > -1 && javaMessage.IndexOf(')') > -1)
                    {
                        var beginIndex = javaMessage.LastIndexOf('(');
                        var endIndex = javaMessage.LastIndexOf(')');
                        if (endIndex > beginIndex)
                        {
                            javaMessage = javaMessage.Substring(beginIndex+1);
                            javaMessage = javaMessage.Substring(0,endIndex - beginIndex -1);
                        }
                        else
                            javaMessage = string.Empty;
                    }
                    else
                        javaMessage = string.Empty; 
                    
                    if (e.Command is IWakeUpCommand)
                        message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name) + " " + javaMessage;
                    else
                        message += " " + context.GetString(Resource.String.err_check_your_connection);
                    Log.Debug(TAG, "Java.Lang.Exception");
                }
                else if (firstEx is FailedStatusCodeException)
                {
                    var ex = (FailedStatusCodeException)firstEx;
                    if (ex.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        message += " " + string.Format(context.GetString(Resource.String.err_server_error), ex.Message);
                        Dialogs.ErrorDialog(context, message, (obj, arg) =>
                            {
                            }).Show();
                        message = string.Empty;
                    }
                    else if (ex.StatusCode == HttpStatusCode.NotFound)
                        message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name) + " " + context.GetString(Resource.String.err_invalid_enigma_type_or_not_enigma);
                    else if (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (e.Command is IWakeUpCommand)
                            message = string.Format(context.GetString(Resource.String.err_failed_connect), e.Profile.Name) + " " + context.GetString(Resource.String.err_check_your_credentials);
                        else
                            message = commandFailed + " " + string.Format(context.GetString(Resource.String.err_request_failed_with_status_code), ex.StatusCode);
                    }
                    else
                        message = commandFailed + " " + string.Format(context.GetString(Resource.String.err_request_failed_with_status_code), ex.StatusCode);
                }
                else if (firstEx is OperationCanceledException)
                {
                    message = commandFailed + " " + context.GetString(Resource.String.err_operation_timed_out);
                }
                else if (firstEx is KnownException)
                {
                    message = commandFailed + " " + firstEx.Message;
                }
                else
                {
                    message = commandFailed + " " + firstEx.Message;
                }
            }
            if (message.Length > 0)
                ShowConnectStatusToast(message);
        }

        public async void ProfileSelectedHandler(object sender, ListItemClickedEventArgs<SignalMeterProfile> e)
        {
            if (e.Item == null || ConnectionManager.ConnectionStatus == ConnectionManager.ConnectionStatusEnum.Connecting || ConnectionManager.ConnectionStatus == ConnectionManager.ConnectionStatusEnum.Disconnecting)
            {
                return;
            }
            if (ConnectionManager.CurrentProfile == e.Item)
            {
                if (ConnectionManager.Connected)
                    DisplayView(ViewsEnum.Bouquets);
                return;
            }
            ConnectionManager.SelectedProfile = e.Item;
            ShowConnectStatusToast(string.Format(context.GetString(Resource.String.inf_connecting_to), e.Item.Name));
            ShowProgressDialog();
            await ConnectionManager.Connect(e.Item);
            if (ConnectionManager.Connected)
            {
                ShowConnectStatusToast(string.Format(context.GetString(Resource.String.inf_connected_to), e.Item.Name));
                await ConnectionManager.ReadCurrentService(e.Item);
                await ConnectionManager.LoadBouquets(e.Item);
                if (ConnectionManager.Bouquets != null && ConnectionManager.Bouquets.Count > 0)
                {
                    ConnectionManager.SelectedBouquet = ConnectionManager.Bouquets.First();
                    DisplayView(ViewsEnum.Bouquets);
                    HideProgressDialog();
                    await ConnectionManager.LoadBouquetItems(ConnectionManager.Bouquets.First());
                }
                else
                {
                    HideProgressDialog();
                }
            }
        }

        public async void BouquetSelectedHandler(object sender, ListItemClickedEventArgs<IBouquetItemBouquet> e)
        {
            ConnectionManager.SelectedBouquet = e.Item;
            await ConnectionManager.LoadBouquetItems(e.Item);
            if (ConnectionManager.Connected)
            {
                DisplayView(ViewsEnum.Services);
            }
        }

        public async void ServiceSelectedHandler(object sender, ListItemClickedEventArgs<IBouquetItem> e)
        {
            if (e.Item is IBouquetItemService)
            {
                IBouquetItemService service = (IBouquetItemService)e.Item;
                ConnectionManager.SelectedService = service;
                await ConnectionManager.ChangeService(service);
                DisplayView(ViewsEnum.Signal);
            }
        }

        public async void ServiceLongClickedHandler(object sender, ListItemClickedEventArgs<IBouquetItem> e)
        {
            if (e.Item is IBouquetItemService)
            {
                IBouquetItemService service = (IBouquetItemService)e.Item;
                ConnectionManager.SelectedService = service;
                await ConnectionManager.ChangeService(service);
            }
        }

        public void ShowConnectStatusToast(string message)
        {
            if (ConnectStatusToast == null || !app.ActivityStarted)
                return;
            ConnectStatusToast.SetText(message);
            ConnectStatusToast.Show();
        }

        public void ConnectionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionStatus")
            {
                SetMenuVisibility();
            }
        }

        public void ShowProgressDialog()
        {
            if (progressDialog == null)
            {
                progressDialog = Dialogs.CreateFullScreenIndeterminateProgressDialog(context);
            }
            progressDialog.Show();
        }

        public void HideProgressDialog()
        {
            if (progressDialog == null)
                return;
            progressDialog.Dismiss();
        }

        public Action<ViewsEnum> DisplayView { get; set; }

        public Action SetMenuVisibility { get; set; }

    }
}

