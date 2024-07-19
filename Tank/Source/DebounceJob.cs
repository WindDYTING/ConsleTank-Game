using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tank
{
    internal class DebounceJob
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private object _locker = new object();
        private DateTime? _firstRunTime;
        private TimeSpan _delay;
        private TimeSpan _maxDelay;

        public DebounceJob(TimeSpan delay, TimeSpan? maxDelay = null)
        {
            _delay = delay;
            _maxDelay = maxDelay ?? _delay * 2;
        }

        public void Run(Action action)
        {
            lock (_locker)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (_firstRunTime == null)
            {
                _firstRunTime = DateTime.Now;
            } else if (DateTime.Now - _firstRunTime > _maxDelay)
            {
                _firstRunTime = null;
                action();
                return;
            }

            Task.Delay(_delay, token)
                .ContinueWith(_ =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        _firstRunTime = null;
                        action();
                    }
                });
        }
    }
}
