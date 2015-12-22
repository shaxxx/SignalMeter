using System;
using Android.Views;

namespace Com.Krkadoni.Utils
{
    public class GenericOnGestureListener : Java.Lang.Object, GestureDetector.IOnGestureListener
    {
        public GenericOnGestureListener()
        {
            
        }

        public Func<MotionEvent, bool> FuncOnDown { get; set; }

        public Func<MotionEvent,MotionEvent, float, float, bool> FuncOnFling { get; set; }

        public Action<MotionEvent> ActionOnLongPress { get; set; }

        public Func<MotionEvent,MotionEvent, float, float, bool> FuncOnScroll { get; set; }

        public Action<MotionEvent> ActionOnShowPress { get; set; }

        public Func<MotionEvent, bool> FuncOnSingleTapUp { get; set; }

        #region IOnGestureListener implementation

        public bool OnDown(MotionEvent e)
        {
            if (FuncOnDown != null)
            {
                return FuncOnDown.Invoke(e);
            }
            return false;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if (FuncOnFling != null)
            {
                return FuncOnFling.Invoke(e1,e2, velocityX, velocityY);
            }
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {
            if (ActionOnLongPress != null)
            {
                ActionOnLongPress.Invoke(e);
            }
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            if (FuncOnScroll != null)
            {
                return FuncOnScroll.Invoke(e1,e2, distanceX, distanceY);
            }
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {
            if (ActionOnShowPress != null)
            {
                ActionOnShowPress.Invoke(e);
            }
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            if (FuncOnSingleTapUp != null)
            {
                return FuncOnSingleTapUp.Invoke(e);
            }
            return false;
        }

        #endregion

    }
}

