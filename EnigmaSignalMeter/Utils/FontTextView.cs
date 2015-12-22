using System;
using Android.Graphics;
using Android.Widget;
using Android.Content;
using Android.Util;
using Com.Krkadoni.Utils;
using Krkadoni.Enigma;
using Com.Krkadoni.App.SignalMeter.Model;

namespace Com.Krkadoni.App.SignalMeter.Utils
{
    public class FontTextView : TextView
    {
        public FontTextView(Context context)
            : base(context)
        {
            this.Typeface =  ((IGlobalApp<IProfile>)Context.ApplicationContext).DefaultFont; 
        }

        public FontTextView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            var attributes = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.FontTextView,0, 0);
            var face = attributes.GetString(Resource.Styleable.FontTextView_typeFace);
            this.Typeface = ((IGlobalApp<SignalMeterProfile>)Context.ApplicationContext).LoadTypeFace(face);
            attributes.Recycle();
        }

        public FontTextView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            var attributes = context.Theme.ObtainStyledAttributes(attrs,Resource.Styleable.FontTextView,0, 0);
            var face = attributes.GetString(Resource.Styleable.FontTextView_typeFace);
            this.Typeface = ((IGlobalApp<SignalMeterProfile>)Context.ApplicationContext).LoadTypeFace(face);
            attributes.Recycle();
        }

        protected void onDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
        }

    }
}