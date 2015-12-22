using System;
using System.Threading.Tasks;
using Com.Krkadoni.Utils.Model;
using System.Diagnostics;


namespace Com.Krkadoni.App.SignalMeter.Model
{
    public class CommandMonitor<T>
    {
        const string TAG = "CommandMonitor";

        public Func<Task<T>> Action { get; private set; }

        private DateTime StartTime { get; set; }

        public enum MonitorStatus
        {
            Stopped = 0,
            Running = 1,
            PerformingAction = 2
        }

        MonitorStatus status;

        public MonitorStatus Status
        {
            get
            {
                return status;
            }
            private set
            {
                if (status != value)
                {
                    status = value;
                    OnMonitorStatusChangedEvent(status);   
                }
            }
        }

        public CommandMonitor(Func<Task<T>> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            this.Action = action;
        }

        public async Task Start()
        {
            if (Status != MonitorStatus.Stopped)
                return;

            StartTime = DateTime.Now;
            Status = MonitorStatus.Running;

            PerformAction(StartTime);

        }

        public void Stop()
        {
            if (Status == MonitorStatus.Stopped)
                return;

            StartTime = DateTime.Now;
            Status = MonitorStatus.Stopped;
        }


        private async Task PerformAction(DateTime time)
        {
            var st = new Stopwatch();
            var stTime = time;

            while (Status == MonitorStatus.Running)
            {
                Status = MonitorStatus.PerformingAction;
                st.Restart();
                var result = await Action.Invoke();
                st.Stop();
                if (Status == MonitorStatus.PerformingAction && StartTime == stTime)
                {
                    OnMonitorCommandFinishedEvent(new GenericEventArgs<T, TimeSpan>(result, st.Elapsed));
                    Status = MonitorStatus.Running;
                    if (Delay.TotalMilliseconds > 0 && StartTime == stTime)
                    {
                        await Task.Delay(Delay);
                    }
                }

                if (StartTime != stTime)
                    break;
            }
        }

        public TimeSpan Delay { get; set; }

        public delegate void MonitorCommandFinishedEventHandler(object sender,GenericEventArgs<T, TimeSpan> e);

        public event MonitorCommandFinishedEventHandler MonitorCommandFinished;

        protected void OnMonitorCommandFinishedEvent(GenericEventArgs<T, TimeSpan> args)
        {
            if (MonitorCommandFinished != null)
            {
                this.MonitorCommandFinished(this, args);
            }
        }


        public delegate void MonitorStatusChangedEventHandler(object sender,GenericEventArgs<MonitorStatus> args);

        public event MonitorStatusChangedEventHandler MonitorStatusChanged;

        protected void OnMonitorStatusChangedEvent(MonitorStatus status)
        {
            //if (System.Diagnostics.Debugger.IsAttached)
                //Log.Debug(TAG, string.Format("Monitor status for {0} command changed to {1}", typeof(T).Name, status));
//            if (MonitorStatusChanged != null)
//            {
//                this.MonitorStatusChanged(this, new GenericEventArgs<MonitorStatus>(status));
//            }
        }

    }
}

