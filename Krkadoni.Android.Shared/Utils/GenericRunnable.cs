using System;
using Android.Content;

namespace Com.Krkadoni.Utils
{
    public class GenericRunnable : Java.Lang.Object, Java.Lang.IRunnable
    {
        
        public GenericRunnable(Action runAction)
        {
            ActionRun = runAction;
        }

        public GenericRunnable(Context context, Action runAction)
        {
            Context = context;
            ActionRun = runAction;
        }

        public Context Context { get; set; }

        public Action ActionRun { get; set; }

        public void Run()
        {
            if (ActionRun != null)
            {
                ActionRun.Invoke();
            }
        }

    }
}
    
