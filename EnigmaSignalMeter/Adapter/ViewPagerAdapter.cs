using System;
using Android.Content;
using Android.Support.V4.App;
using Com.Krkadoni.App.SignalMeter.Layout;

namespace Com.Krkadoni.App.SignalMeter.Adapter
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
        const int PAGE_COUNT = 4;
        Context context;

        public ViewPagerAdapter(FragmentManager fm, Context c)
            : base(fm)
        {
            context = c;
        }
            
        public override int Count
        {
            get { return PAGE_COUNT; }
        }

        public override Fragment GetItem(int position)
        {
            switch (position)
            {

            // Open ProfileTab
                case 0:
                    ProfilesFragment profileTabFragment = new ProfilesFragment();
                  
                    return profileTabFragment;

            // Open BouquetsTab
                case 1:
                    BouquetsFragment bouquetsTabFragment = new BouquetsFragment();
                    return bouquetsTabFragment;

            // Open ServicesTab
                case 2:
                    ServicesFragment servicesTabFragment = new ServicesFragment();
                    return servicesTabFragment;

            // Open SignalTab
                case 3:
                    SignalFragment signalTabFragment = new SignalFragment();
                    return signalTabFragment;
            }
            return null;
        }


        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            switch (position)
            {
            
            // Open ProfileTab
                case 0:
                    return  new Java.Lang.String(context.GetString(Resource.String.ProfileTitle));
            
            // Open BouquetsTab
                case 1:
                    return new Java.Lang.String(context.GetString(Resource.String.BouquetsTitle));
            
            // Open ServiceTab
                case 2:
                    return new Java.Lang.String(context.GetString(Resource.String.ServicesTitle));
            
            //Open SignalTab
                case 3:
                    return new Java.Lang.String(context.GetString(Resource.String.SignalTitle));

            }
            return null;   
        }                        
      
    }
}

