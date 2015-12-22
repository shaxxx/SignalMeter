using Android.Support.V7.App;
using Android.Views;
using System;

namespace Com.Krkadoni.Utils
{
    public class GenericActionBarDrawerToggle : ActionBarDrawerToggle
    {
        
        public GenericActionBarDrawerToggle(Android.App.Activity activity, Android.Support.V4.Widget.DrawerLayout drawerLayout, Android.Support.V7.Widget.Toolbar toolbar, int openDrawerContentDescRes, int closeDrawerContentDescRes)
            : base(activity, drawerLayout, toolbar, openDrawerContentDescRes, closeDrawerContentDescRes)
        {
          
        }
            
        public Action<View> ActionOnDrawerOpened { get; set;}

        public Action<View> ActionOnDrawerClosed { get; set;}

        public Action<View, float> ActionOnDrawerSlide { get; set;}

        public override void OnDrawerOpened(View drawerView)
        {
            base.OnDrawerOpened(drawerView);
            if (ActionOnDrawerOpened != null)
            {
                ActionOnDrawerOpened.Invoke(drawerView);
            }   
        }

        public override void OnDrawerClosed(View drawerView)
        {
            base.OnDrawerClosed(drawerView);
            if (ActionOnDrawerClosed != null)
            {
                ActionOnDrawerClosed.Invoke(drawerView);
            }  
        }

        public override void OnDrawerSlide(View drawerView, float slideOffset)
        {
            base.OnDrawerSlide(drawerView, slideOffset);
            if (ActionOnDrawerSlide != null)
            {
                ActionOnDrawerSlide.Invoke(drawerView, slideOffset);
            }  
        }

    }
}

