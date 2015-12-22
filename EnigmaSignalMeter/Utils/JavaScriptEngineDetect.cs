using System;
using Android.Webkit;
using Android.Content;
using Android.Util;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class JavaScriptEngineDetect
    {
        private readonly WebView webView;
        public static string content;
        public static Action<JavaScriptEngine> _resultCallback;
        private const string TAG = "JavaScriptEngineDetect";
        private const string cacheFile = "jsengine.html";
        private string cache;
        private bool cacheFailed = false;
        private Context context;

        const string html = @"
        <!DOCTYPE html>
        <html>
            <head>
                <script>
                    window.onload = function() 
                    {
                        var v8string = 'function%20javaEnabled%28%29%20%7B%20%5Bnative%20code%5D%20%7D';
  
                        if (window.devicePixelRatio)  //If WebKit browser
                            {
                                if (escape(navigator.javaEnabled.toString()) === v8string)
                                    {
                                        document.title = ""V099787 detected"";
                                    }
                                else
                                    {
                                        document.title = ""JSC detected"";
                                    }
                            }
                        else 
                            {
                                document.title = ""Not a WebKit browser"";
                            }
    
                        function display(msg) 
                            {
                                var p = document.createElement('p');
                                p.innerHTML = msg;
                                document.body.appendChild(p);
                            }
        
                   };
            </script>
            <meta charset=utf-8 />
            <title>JavaScript Engine</title>
        </head>
    <body>
    </body>
</html>";


        public JavaScriptEngineDetect(Context context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
            webView = new WebView(context);
            webView.Settings.JavaScriptEnabled = true;
            var cacheDir = context.CacheDir.AbsolutePath;
            var cache = System.IO.Path.Combine(cacheDir, cacheFile);

            if (System.IO.File.Exists(cache))
            {
                try
                {
                    System.IO.File.Delete(cache);
                }
                catch (Exception ex)
                {
                    Log.Error(TAG,"Failed to delete " + cacheFile);
                }
            }

            try
            {
                System.IO.File.WriteAllText(cache, html);
                cacheFailed = false;
            }
            catch (Exception ex)
            {
                cacheFailed = true;
                Log.Error(TAG,"Failed to write " + cacheFile);
            }
        }

        public enum JavaScriptEngine
        {
            V8 = 0,
            JSC = 1,
            Unknown = 2
        }

        public JavaScriptEngine Detect()
        {
            if (cacheFailed)
                return JavaScriptEngine.Unknown;

            var cache = System.IO.Path.Combine(context.CacheDir.AbsolutePath, cacheFile);

            if (!System.IO.File.Exists(cache))
                return JavaScriptEngine.Unknown;
            
            try
            {
                webView.LoadUrl("file:///" + cache);
                //webView.LoadData(html,"text/html", null);
                if (string.IsNullOrEmpty(webView.Title))
                {
                    if (webView.Title.ToLower().IndexOf("jsc") > -1)
                        return JavaScriptEngine.JSC;
                    else if (webView.Title.ToLower().StartsWith("v0"))
                        return JavaScriptEngine.V8;
                    else
                        return JavaScriptEngine.Unknown;
                }
                return JavaScriptEngine.Unknown;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex.Message);
                return JavaScriptEngine.Unknown;
            }
        }
    }
}
