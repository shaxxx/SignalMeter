using System;
using Android.Support.V7.App;
using Android.Content;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public static class Dialogs
    {
        
        public static Android.Support.V7.App.AlertDialog.Builder BuildDialog(
            Context context, 
            string title, 
            string message, 
            Action<object, DialogClickEventArgs> positiveAction,
            Action<object, DialogClickEventArgs> negativeAction,
            Action<object, DialogClickEventArgs> neutralAction,
            int positiveButtonResource,
            int negativeButtonResource,
            int neutralButtonResource)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(context, Resource.Style.AppCompatAlertDialogStyle);
            builder.SetTitle(title);
            builder.SetMessage(message);
            if (positiveAction != null)
                builder.SetPositiveButton(positiveButtonResource, (sender,e) => positiveAction(sender,e));
            if (neutralAction != null)
                builder.SetNeutralButton(neutralButtonResource, (sender,e) => neutralAction(sender,e));
            if (negativeAction != null)
                builder.SetNegativeButton(negativeButtonResource, (sender,e) => negativeAction(sender,e));
            return builder;
        }

        public static Android.Support.V7.App.AlertDialog.Builder QuestionDialog(
            Context context, 
            string message, 
            Action<object, DialogClickEventArgs> positiveAction,
            Action<object, DialogClickEventArgs> negativeAction)
        {
            return BuildDialog(
                context, 
                context.GetString(Resource.String.title_question), 
                message, 
                positiveAction, 
                negativeAction, 
                null, 
                Android.Resource.String.Yes, 
                Android.Resource.String.No, 
                0);
        }

        public static Android.Support.V7.App.AlertDialog.Builder ErrorDialog(
            Context context, 
            string message, 
            Action<object, DialogClickEventArgs> buttonAction)
        {
            return BuildDialog(
                context, 
                context.GetString(Resource.String.title_error), 
                message, 
                buttonAction, 
                null, 
                null, 
                Android.Resource.String.Ok, 
                0, 
                0);
        }

        public static Android.Support.V7.App.AlertDialog.Builder WarningDialog(
            Context context, 
            string message, 
            Action<object, DialogClickEventArgs> buttonAction)
        {
            return BuildDialog(
                context, 
                context.GetString(Resource.String.title_warning), 
                message, 
                buttonAction, 
                null, 
                null, 
                Android.Resource.String.Ok, 
                0, 
                0);
        }

        public static Android.Support.V7.App.AlertDialog CreateFullScreenIndeterminateProgressDialog(Context context)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(context);
            builder.SetView(LayoutInflater.From(context).Inflate(Resource.Layout.loader, null));
            //builder.SetCancelable(false);
            builder.SetRecycleOnMeasureEnabled(true);
            builder.SetInverseBackgroundForced(true);
            var dialog = builder.Create();
            dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            return dialog;
        }
    }
}

