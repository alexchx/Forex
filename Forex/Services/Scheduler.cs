using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Forex.Services
{
    public class Scheduler : NotifyPropertyChanged
    {
        private static readonly TimeSpan Interval = new TimeSpan(0, 1, 0);
        private static readonly object _LOCK = new object();
        public static readonly Scheduler Current = new Scheduler();

        private DispatcherTimer _timer;

        #region Notify Properties

        private bool _isSyncRunning;
        public bool IsSyncRunning
        {
            get
            {
                return _isSyncRunning;
            }
            private set
            {
                if (value == _isSyncRunning)
                {
                    return;
                }

                _isSyncRunning = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event EventHandler<SyncUpdatedEventArgs> SyncUpdated;
        public event EventHandler<SyncFailedEventArgs> SyncFailed;

        public Scheduler()
        {
            _timer = new DispatcherTimer { Interval = Interval };
            _timer.Tick += _timer_Tick;
        }

        private async void _timer_Tick(object sender, EventArgs e)
        {
            await RunNowAsync();
        }

        public void Start()
        {
            RunNowAsync().GetAwaiter();

            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        public void Stop()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        public async Task RunNowAsync()
        {
            lock (_LOCK)
            {
                if (IsSyncRunning)
                {
                    return;
                }

                IsSyncRunning = true;
            }

            try
            {
                var lastUpdateTime = await DbService.GetNewestTimeAsync();
                DateTime currentDay = lastUpdateTime == null ? DateTime.Now.Date.AddDays(-100) : lastUpdateTime.Value.Date;

                while (currentDay <= DateTime.Now)
                {
                    var items = await DataService.GetExchangeRateAsync(currentDay, lastUpdateTime);
                    if (items.Count > 0)
                    {
                        await DbService.SyncRateItemsAsync(items);

                        SyncUpdated?.Invoke(this, new SyncUpdatedEventArgs { LastUpdate = items.Max(o => o.Time) });
                    }

                    currentDay = currentDay.AddDays(1);
                    lastUpdateTime = null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Forex data sync error");

                SyncFailed?.Invoke(this, new SyncFailedEventArgs { Message = ex.Message });
            }
            finally
            {
                lock (_LOCK)
                {
                    IsSyncRunning = false;
                }
            }
        }
    }

    public class SyncUpdatedEventArgs
    {
        public DateTime LastUpdate { get; set; }
    }

    public class SyncFailedEventArgs
    {
        public string Message { get; set; }
    }
}
