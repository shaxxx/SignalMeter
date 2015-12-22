using System;

namespace Com.Krkadoni.App.SignalMeter.Model
{
    public class ApplicationSettings
    {
        public ApplicationSettings()
        {
            LastAskedForTtsSettings = DateTime.MinValue;
        }

        public DateTime LastAskedForTtsSettings { get; set; }

        public bool StreamActivated { get; set; }

        public bool IgnoreTtsError { get; set; }

        public int TtsErrorCount { get; set; }

    }
}

