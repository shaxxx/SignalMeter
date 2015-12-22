using System;
using Android.Graphics.Drawables;
using Android.Content;
using Android.Content.Res;
using Android.Support.V4.Content;
using Android.Graphics;
using Android.Support.V7.Widget;

namespace Com.Krkadoni.Utils
{

    public class DividerItemDecoration : RecyclerView.ItemDecoration {

        private static int[] ATTRS = new int[]{Android.Resource.Attribute.ListDivider};

        private readonly Drawable mDivider;

        public DividerItemDecoration(Context context) {
             var styledAttributes = context.ObtainStyledAttributes(ATTRS);
            mDivider = styledAttributes.GetDrawable(0);
            styledAttributes.Recycle();
        }
            
        public DividerItemDecoration(Context context, int resId) {
            mDivider = ContextCompat.GetDrawable(context, resId);
        }

        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state) {
            int left = parent.PaddingLeft;
            int right = parent.Width - parent.PaddingRight;

            int childCount = parent.ChildCount;
            for (int i = 0; i < childCount; i++) {
                var child = parent.GetChildAt(i);
//                if (child.LayoutParameters is RecyclerView.LayoutParams)
//                {
//                    RecyclerView.LayoutParams a = (RecyclerView.LayoutParams)child.LayoutParameters;
//                }
//                var mparams = ( RecyclerView.LayoutParams)child.LayoutParameters;

                int top = child.Bottom + 0;
                int bottom = top + mDivider.IntrinsicHeight;

                mDivider.SetBounds(left, top, right, bottom);
                mDivider.Draw(c);
            }
        }
    }
}

