using Com.Krkadoni.Interfaces;
using Android.Graphics;

namespace Com.Krkadoni.Utils
{
    public interface IGlobalApp<T>
    {
        string ReleaseName { get; set; }

        IPreferenceManager<T> PreferenceManager { get; set;}

        void RequestBackup();

        Typeface DefaultFont { get; set; }

        Typeface LoadTypeFace(string fontPath);

        bool ActivityResumed { get; set; }

        bool ActivityStarted { get; set; }

        bool ActivityPaused { get; set; }

        bool ActivityStopped { get; set; }

    }
}

