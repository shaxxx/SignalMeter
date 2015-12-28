using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Krkadoni.Enigma.Enums;
using Krkadoni.Enigma.Properties;
using Krkadoni.Enigma;
using ModernHttpClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;

namespace Com.Krkadoni.App.SignalMeter.Model
{
    public class ModernWebRequester : IWebRequester, INotifyPropertyChanged
    {
        private readonly ILog _log;
        private readonly object objectLock;
        private readonly object headersLock;
        private readonly object uriLock;
        private readonly List<HttpClient> WorkingClients;
        private readonly List<HttpClient> FreeClients;
        private readonly Dictionary<string, AuthenticationHeaderValue> authHeaders;
        private readonly MediaTypeWithQualityHeaderValue enigma1MediaTypeHeader;
        private readonly MediaTypeWithQualityHeaderValue enigma2MediaTypeHeader;
        private readonly Dictionary<string, Uri> uriCache;

        public ModernWebRequester([NotNull] ILog log)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            _log = log;
            WorkingClients = new List<HttpClient>();
            FreeClients = new List<HttpClient>();
            objectLock = new object();
            headersLock = new object();
            authHeaders = new Dictionary<string, AuthenticationHeaderValue>();
            enigma1MediaTypeHeader = new MediaTypeWithQualityHeaderValue("text/html");
            enigma2MediaTypeHeader = new MediaTypeWithQualityHeaderValue("text/xml");
            uriCache = new Dictionary<string, Uri>();
            uriLock = new object();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task<string> GetResponseAsync(string url, IProfile profile, CancellationToken token)
        {
            try
            {  
                var sb = new StringBuilder();
                if (profile.UseSsl)
                    sb.AppendFormat("https://{0}:{1}/{2}", profile.Address, profile.HttpPort, url);
                else
                    sb.AppendFormat("http://{0}:{1}/{2}", profile.Address, profile.HttpPort, url);

                url = sb.ToString();

                _log.DebugFormat("Initializing HttpWebRequest to {0}", url);
                var st = new Stopwatch();
               
                var httpClient = GetClient();
                var uri = GetUri(url);

                httpClient.BaseAddress = uri;
                if (!string.IsNullOrEmpty(profile.Password))
                {
                    httpClient.DefaultRequestHeaders.Authorization = GetAuthHeader(profile.Username, profile.Password);
                }
               
                httpClient.DefaultRequestHeaders.Accept.Add(profile.Enigma == EnigmaType.Enigma1 ? enigma1MediaTypeHeader : enigma2MediaTypeHeader);
                st.Restart();

                string result = null;

                using (var task = httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token))
                {
                    var response = await task;
                    token.ThrowIfCancellationRequested();
                    if (task.Status == TaskStatus.Faulted && task.Exception != null)
                        throw task.Exception;
                    if (!response.IsSuccessStatusCode)
                        throw new FailedStatusCodeException(url, response.StatusCode);
                    token.ThrowIfCancellationRequested();
                    result = await response.Content.ReadAsStringAsync();
                }
                  
                FreeUpClient(httpClient);
                sb.Clear();

                st.Stop();

                token.ThrowIfCancellationRequested();

                if (result != null)
                {
                    _log.DebugFormat("{0} response is", url);
                    _log.DebugFormat(result);   
                }
                else
                {
                    _log.WarnFormat("Response to {0} is null!", url);
                }

                _log.DebugFormat("HttpWebRequest to {0} took {1} ms", url, st.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                if (ex is KnownException || ex is OperationCanceledException)
                    throw;

                if (ex is AggregateException)
                {
                    Exception exToThrow = ((AggregateException)ex).Flatten();
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

                throw new WebRequestException(string.Format("HttpWebRequest for {0} failed!", url), ex);
            }
        }

        public async Task<byte[]> GetBinaryResponseAsync(string url, IProfile profile, CancellationToken token)
        {
            try
            {
                var sb = new StringBuilder();
                if (profile.UseSsl)
                    sb.AppendFormat("https://{0}:{1}/{2}", profile.Address, profile.HttpPort, url);
                else
                    sb.AppendFormat("http://{0}:{1}/{2}", profile.Address, profile.HttpPort, url);

                var fullUrl = sb.ToString();

                _log.DebugFormat("Initializing HttpWebRequest to {0}", fullUrl);
                var st = new Stopwatch();

                var httpClient = GetClient();
                var uri = GetUri(fullUrl);

                httpClient.BaseAddress = uri;
                if (!string.IsNullOrEmpty(profile.Password))
                {
                    httpClient.DefaultRequestHeaders.Authorization = GetAuthHeader(profile.Username, profile.Password);
                }

                httpClient.DefaultRequestHeaders.Accept.Add(profile.Enigma == EnigmaType.Enigma1 ? enigma1MediaTypeHeader : enigma2MediaTypeHeader);
                st.Restart();

                byte[] result = null;
                using (var task = httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token))
                {
                    var response = await task;
                    token.ThrowIfCancellationRequested();
                    if (task.Status == TaskStatus.Faulted && task.Exception != null)
                        throw task.Exception;
                    if (!response.IsSuccessStatusCode)
                        throw new FailedStatusCodeException(url, response.StatusCode);
                    token.ThrowIfCancellationRequested();
                    result = await response.Content.ReadAsByteArrayAsync();
                }

                FreeUpClient(httpClient);
                sb.Clear();
                fullUrl = null;
                st.Stop();

                token.ThrowIfCancellationRequested();

                if (result == null)
                {
                    _log.WarnFormat("Response to {0} is null!", url);
                }

                _log.DebugFormat("HttpWebRequest to {0} took {1} ms", url, st.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                if (ex is KnownException || ex is OperationCanceledException)
                    throw;

                if (ex is AggregateException)
                {
                    Exception exToThrow = ((AggregateException)ex).Flatten();
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

                throw new WebRequestException(string.Format("HttpWebRequest for {0} failed!", url), ex);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private HttpClient GetClient()
        {
            lock (objectLock)
            {
                if (FreeClients.Count == 0)
                {
                    var httpClient = new HttpClient(new NativeMessageHandler());
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
                    FreeClients.Add(httpClient);
                }
                var client = FreeClients.First();
                FreeClients.Remove(client);
                WorkingClients.Add(client);
                return client;
            }
        }

        private void FreeUpClient(HttpClient client)
        {
            lock (objectLock)
            {
                if (client != null)
                {
                    client.CancelPendingRequests();
                    client.DefaultRequestHeaders.Clear();
                    WorkingClients.Remove(client);
                    FreeClients.Add(client);
                }
            }
        }

        private AuthenticationHeaderValue GetAuthHeader(string username, string password)
        {
            lock (headersLock)
            {
                var key = string.Format("{0}:{1}", username, password);
                if (authHeaders.ContainsKey(key))
                    return authHeaders[key];
                var byteArray = Encoding.ASCII.GetBytes(key);
                var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                authHeaders.Add(key, header);
                return header;
            }
        }

        private Uri GetUri(string url)
        {
            lock (uriLock)
            {
                if (uriCache.ContainsKey(url))
                    return uriCache[url];
                var uri = new Uri(url);
                uriCache.Add(url, uri);
                return uri;
            }
        }

    }
}

