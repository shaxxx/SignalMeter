using System;
using Krkadoni.Enigma;


namespace Com.Krkadoni.App.SignalMeter.Model
{
    public class ModernClientFactory : Factory
    {
        ModernWebRequester requester;
        
        public ModernClientFactory() : base()
        {
            
        }

        private ILog _log;

        public override ILog Log()
        {
            return _log ?? (_log = new ConsoleLog());
        }

        public override IWebRequester WebRequester()
        {
            if (requester == null)
                requester = new ModernWebRequester(_log);
            return requester;
        }


    }
}

