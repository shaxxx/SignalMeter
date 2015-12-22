using System;
using Android.Widget;
using System.Threading.Tasks;
using Krkadoni.Enigma.Responses;
using Krkadoni.Enigma;
using Com.Krkadoni.App.SignalMeter.Model;
using Android.Util;
using Com.Krkadoni.Utils;
using Krkadoni.Enigma.Enums;
using Android.Content;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class StreamManager
    {
        private const string TAG = "StreamManager";
        Context context;
        GlobalApp app;
        private Toast StreamStatusToast;

       public StreamManager(Context context)
        {

            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
            StreamStatusToast = Toast.MakeText(context, string.Empty, ToastLength.Long);
            app = ((GlobalApp)context.ApplicationContext);
        }

        private void ShowStreamStatusToast(string message)
        {
            if (StreamStatusToast == null || !app.ActivityStarted)
                return;
            StreamStatusToast.SetText(message);
            StreamStatusToast.Show();
        }

        public async void InitStream(IBouquetItemService service)
        {
            TapjoyManager.StreamAvailable((result) =>
                {
                    if (result == TapjoyManager.FeatureBuyResult.FeatureAvailable)
                        InitStreamAvailable(service);
                });
        }

        private async void InitStreamAvailable(IBouquetItemService service)
        {
            try
            {
                //check if we have all the info
                if (!ConnectionManager.Connected || service == null || service.Reference == null || ConnectionManager.CurrentProfile == null || !ConnectionManager.CurrentProfile.Streaming)
                    return;
                var profile = ConnectionManager.CurrentProfile;
                string link = null;
                bool portAvailable = false;
                int originalPort;
                int streamPort = 0;
                string enigma1ServiceParameters = string.Empty;
                if (profile.Transcoding)
                {
                    originalPort = profile.TranscodingPort;
                    //check if transcoding port is available
                    portAvailable = await Network.CheckPortOpen(profile.Address, originalPort);
                    if (portAvailable)
                        originalPort = profile.TranscodingPort;
                    else
                    {
                        portAvailable = await Network.CheckPortOpen(profile.Address, 8002);
                        if (portAvailable)
                            originalPort = 8002;
                        else
                            //transcoding port is not available, fallback to regular stream port
                            originalPort = profile.StreamingPort;
                    }
                }
                else
                {
                    originalPort = profile.StreamingPort;
                }
                //check if specified streaming port is available
                if (!portAvailable)
                    portAvailable = await Network.CheckPortOpen(profile.Address, originalPort);
                if (!portAvailable || profile.Enigma == EnigmaType.Enigma1)
                {
                    //we need original streaming parameters from the receiver
                    var streamParametersResponse = await ReadStreamParameters(service);
                    if (streamParametersResponse == null)
                    {
                        //failed to get original parameters
                        if (profile.Enigma == EnigmaType.Enigma1)
                        {
                            //we need additional parameters for Enigma1
                            ShowStreamStatusToast(context.GetString(Resource.String.err_failed_to_initialize_stream));
                            return;
                        }
                    }
                    else
                    {
                        Uri streamUri = new Uri(streamParametersResponse.StreamUrl);
                        enigma1ServiceParameters = streamUri.AbsolutePath;
                        //we have stream parameters from the receiver
                        if (!portAvailable)
                        {
                            //receiver specified different port for streaming
                            if (streamUri.Port != originalPort)
                            {
                                //check if alternative port is available
                                portAvailable = await Network.CheckPortOpen(profile.Address, streamUri.Port);
                                if (portAvailable)
                                {
                                    //notify user about port change
                                    streamPort = streamUri.Port;
                                    Toast.MakeText(context, string.Format(context.GetString(Resource.String.warn_using_alternative_port), streamUri.Port), ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                //port is not available, receiver returned the same port
                                ShowStreamStatusToast(string.Format("{0}:{1} {2}", profile.Address, originalPort, context.GetString(Resource.String.err_port_not_available)));
                                return;
                            }
                        }
                        else
                        {
                            //we have stream parameters, specified streaming port in profile is available
                            streamPort = originalPort;
                        }
                    }
                    if (!portAvailable)
                    {
                        ShowStreamStatusToast(string.Format("{0}:{1} {2}", profile.Address, originalPort, context.GetString(Resource.String.err_port_not_available)));
                        return;
                    }
                }
                else
                {
                    //port is avialable, it's Enigma2
                    streamPort = originalPort;
                    if (profile.Transcoding && streamPort != profile.TranscodingPort)
                        Toast.MakeText(context, string.Format(context.GetString(Resource.String.warn_using_alternative_port), streamPort), ToastLength.Short).Show();
                }
                if (profile.Enigma == EnigmaType.Enigma2)
                    //http://example.com:8001/
                    link = string.Format("http://{0}:{1}/{2}", profile.Address, streamPort, service.Reference);
                else
                    //http://dm600pvr:31339/0,61,1ff,200
                    link = string.Format("http://{0}:{1}{2}", profile.Address, streamPort, enigma1ServiceParameters);
                link = System.Uri.EscapeUriString(link);
                ShowStreamStatusToast(context.GetString(Resource.String.inf_initializing_stream));
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(Android.Net.Uri.Parse(link), "video/*");
                Log.Debug(TAG, "Requesting stream for link " + link);
                context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, string.Format("Failed to initialize stream! {0}", ex.Message));
                ShowStreamStatusToast(string.Format("{0} {1}", context.GetString(Resource.String.err_failed_to_initialize_stream), ex.Message));
            }
        }

        private async Task<IGetStreamParametersResponse> ReadStreamParameters(IBouquetItemService service)
        {
            //try to obtain stream URL from the receiver to get stream parameters
            var result = await ConnectionManager.GetStreamParametersForService(service);
            if (result != null && !string.IsNullOrEmpty(result.StreamUrl))
            {
                Uri streamUri;
                if (Uri.TryCreate(result.StreamUrl, UriKind.Absolute, out streamUri))
                {
                    return result;
                }
                //if stream url is not valid url return null
                return null;
            }
            return null;
        }
            
    }
}

