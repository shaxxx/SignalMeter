using System;

namespace Com.Krkadoni.Utils
{
    public class NavDrawerItem {
       
        public NavDrawerItem() {

        }

        public NavDrawerItem(bool showNotify, String title) {
            this.ShowNotify = showNotify;
            this.Title = title;
        }
            
        public bool ShowNotify { get; set; }

        public String Title { get; set; }
       
    }
}

