using Android.Content;
using Com.Krkadoni.Interfaces;
using Com.Krkadoni.Utils;
using Android.Support.V7.Widget;
using Android.Views;

namespace Com.Krkadoni.Utils
{

    public class RecyclerTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
    {
        
        private GestureDetector gestureDetector;
        private IOnClickListener clickListener;

        public RecyclerTouchListener(Context context, RecyclerView recyclerView, IOnClickListener clickListener)
        {
            this.clickListener = clickListener;
            var gestureListener = new GenericOnGestureListener();
            gestureListener.ActionOnLongPress = (e) =>
            {
                View child = recyclerView.FindChildViewUnder(e.GetX(), e.GetY());
                if (child != null && clickListener != null)
                {
                    clickListener.OnLongClick(child, recyclerView.GetChildPosition(child));
                }
            };       
            gestureListener.FuncOnSingleTapUp = (e) => true;
            gestureDetector = new GestureDetector(context, gestureListener);
        }
                
        #region IOnItemTouchListener implementation

        public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
        {
            View child = rv.FindChildViewUnder(e.GetX(), e.GetY());
            if (child != null && clickListener != null && gestureDetector.OnTouchEvent(e))
            {
                clickListener.OnClick(child, rv.GetChildPosition(child));
            }
            return false;
        }

        public void OnRequestDisallowInterceptTouchEvent(bool p0)
        {
            
        }

        public void OnTouchEvent(RecyclerView rv, MotionEvent e)
        {
            
        }

        #endregion

    }
}
