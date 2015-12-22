using System;
using Android.Views;
using Com.Krkadoni.Interfaces;


namespace Com.Krkadoni.Utils
{
    public class GenericOnClickListener : IOnClickListener
    {

        public GenericOnClickListener(Action<View,int> onClickAction, Action<View,int> onLongClickAction){
            OnClickAction = onClickAction;
            OnLongClickAction = onLongClickAction;
        }

        public GenericOnClickListener(Action<View,int> onClickAction){
            OnClickAction = onClickAction;
        }

        #region IOnClickListener implementation

        public void OnClick(Android.Views.View view, int position)
        {
            if (OnClickAction != null)
                OnClickAction.Invoke(view,position);
        }

        public void OnLongClick(Android.Views.View view, int position)
        {
            if (OnLongClickAction != null)
                OnLongClickAction.Invoke(view, position);
        }

        public Action<View,int> OnClickAction { get; set;}

        public Action<View,int> OnLongClickAction { get; set;}

        #endregion
    }
}

