using System;
using Krkadoni.Enigma;
using Krkadoni.Enigma.Commands;

namespace Com.Krkadoni.App.SignalMeter.Model
{
    public class ExceptionRaisedEventArgs : EventArgs 
    {
        public Exception Exception { get; private set;}

        public IProfile Profile { get; private set;}

        public ICommand Command { get; private set;}

        public bool Handled { get; set;}

        public ExceptionRaisedEventArgs(IProfile profile, ICommand command, Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException("ex");
            if (command == null)
                throw new ArgumentNullException("command");
            if (profile == null)
                throw new ArgumentNullException("profile");

            Exception = ex;
            Command = command;
            Profile = profile;
        }
    }
}

