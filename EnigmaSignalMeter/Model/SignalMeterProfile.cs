using System;
using Krkadoni.Enigma;

namespace Com.Krkadoni.App.SignalMeter.Model
{
    public class SignalMeterProfile : Profile
    {
        public SignalMeterProfile()
        {
            streaming = false;
            transcodingPort = 8002;
            transcoding = false;
        }

        bool streaming;
        public bool Streaming
        {
            get
            {
                return streaming;
            }
            set
            {
                if (value.Equals(streaming)) return;
                streaming = value;
                OnPropertyChanged();
            }
        }

        bool transcoding;
        public bool Transcoding
        {
            get
            {
                return transcoding;
            }
            set
            {
                if (value.Equals(transcoding)) return;
                transcoding = value;
                OnPropertyChanged();
            }
        }
            
        int transcodingPort;
        public int TranscodingPort
        {
            get
            {
                return transcodingPort;
            }
            set
            {
                if (value.Equals(transcodingPort)) return;
                transcodingPort = value;
                OnPropertyChanged();
            }
        }
    }
}

