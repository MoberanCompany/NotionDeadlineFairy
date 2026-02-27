using System;
using System.Threading;
using NotionDeadlineFairy.Abstractions;
using NotionDeadlineFairy.Utils;

namespace NotionDeadlineFairy.Services
{
    public class PollingService : IDisposable
    {
        private static readonly Lazy<PollingService> _instance =
            new(() => new PollingService());

        public static PollingService Instance => _instance.Value;

        private readonly object _sync = new();
        private System.Threading.Timer? _timer;
        private int _intervalSeconds = 300;
        private int _isTicking;

        private PollingService() { }

        public void Start(int intervalSeconds)
        {
            lock (_sync)
            {
                _intervalSeconds = NormalizeInterval(intervalSeconds);
                _timer?.Dispose();
                _timer = new System.Threading.Timer(
                    _ => Tick(),
                    null,
                    TimeSpan.FromSeconds(_intervalSeconds),
                    TimeSpan.FromSeconds(_intervalSeconds));
            }
        }

        public void UpdateInterval(int intervalSeconds)
        {
            Start(intervalSeconds);
        }

        public void Stop()
        {
            lock (_sync)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }

        private void Tick()
        {
            if (Interlocked.CompareExchange(ref _isTicking, 1, 0) != 0)
            {
                return;
            }

            try
            {
                DispatcherHelper.Invoke(() =>
                {
                    var widgets = ServiceLocator.Instance.GetService<IWidget>();
                    if (widgets is null || widgets.Count == 0)
                    {
                        return;
                    }

                    foreach (var widget in widgets)
                    {
                        try
                        {
                            widget.Refresh();
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                });
            }
            finally
            {
                Interlocked.Exchange(ref _isTicking, 0);
            }
        }

        private static int NormalizeInterval(int intervalSeconds)
        {
            return intervalSeconds > 0 ? intervalSeconds : 300;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
