using RecyclerView = Android.Support.V7.Widget.RecyclerView;
using Android.Views;

namespace Com.Krkadoni.Utils
{

    public class SimpleRecyclerViewHolder<T> : RecyclerView.ViewHolder where T : View
    {
        public T Item { get; private set; }

        public SimpleRecyclerViewHolder(View itemView, int resourceId)
            : base(itemView)
        {
            if (itemView != null)
                Item = (T)itemView.FindViewById(resourceId);
        }
    }

}
