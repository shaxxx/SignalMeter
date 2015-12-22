using RecyclerView = Android.Support.V7.Widget.RecyclerView;
using Android.Views;

namespace Com.Krkadoni.Utils
{
    public class TwoItemsRecyclerViewHolder<TItem1, TItem2> : RecyclerView.ViewHolder 
        where TItem1 : View
        where TItem2 : View
    {
        public TItem1 Item1 { get; private set; }

        public TItem2 Item2 { get; private set; }

        public TwoItemsRecyclerViewHolder(View itemView, int item1ResourceId, int item2ResourceId)
            : base(itemView)
        {
            if (itemView != null)
            {
                Item1 = itemView.FindViewById<TItem1>(item1ResourceId);
                Item2 = itemView.FindViewById<TItem2>(item2ResourceId);
            }
        }
    }

}
