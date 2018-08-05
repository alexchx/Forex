using Forex.Models;
using Forex.Services;
using Forex.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WinForms = System.Windows.Forms;

namespace Forex
{
    public partial class MainWindow : Window
    {
        #region Dependency Properties

        public static readonly DependencyProperty LatestRateProperty = DependencyProperty.Register(
            nameof(LatestRate),
            typeof(double),
            typeof(MainWindow),
            new PropertyMetadata());

        public static readonly DependencyProperty LatestMaxRateProperty = DependencyProperty.Register(
            nameof(LatestMaxRate),
            typeof(double),
            typeof(MainWindow),
            new PropertyMetadata());

        public static readonly DependencyProperty LatestMinRateProperty = DependencyProperty.Register(
            nameof(LatestMinRate),
            typeof(double),
            typeof(MainWindow),
            new PropertyMetadata());

        public static readonly DependencyProperty LastUpdatedByProperty = DependencyProperty.Register(
            nameof(LastUpdatedBy),
            typeof(DateTime?),
            typeof(MainWindow),
            new PropertyMetadata());

        public static readonly DependencyProperty PagedRatesProperty = DependencyProperty.Register(
            nameof(PagedRates),
            typeof(PagedResult<RateItem>),
            typeof(MainWindow),
            new PropertyMetadata());

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(MainWindow),
            new PropertyMetadata());

        public static readonly DependencyProperty IsDetailsViewVisibleProperty = DependencyProperty.Register(
            nameof(IsDetailsViewVisible),
            typeof(bool),
            typeof(MainWindow),
            new PropertyMetadata(false));

        public double LatestRate
        {
            get { return (double)GetValue(LatestRateProperty); }
            set { SetValue(LatestRateProperty, value); }
        }

        public double LatestMaxRate
        {
            get { return (double)GetValue(LatestMaxRateProperty); }
            set { SetValue(LatestMaxRateProperty, value); }
        }

        public double LatestMinRate
        {
            get { return (double)GetValue(LatestMinRateProperty); }
            set { SetValue(LatestMinRateProperty, value); }
        }

        public DateTime? LastUpdatedBy
        {
            get { return (DateTime?)GetValue(LastUpdatedByProperty); }
            set { SetValue(LastUpdatedByProperty, value); }
        }

        public PagedResult<RateItem> PagedRates
        {
            get { return (PagedResult<RateItem>)GetValue(PagedRatesProperty); }
            set { SetValue(PagedRatesProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public bool IsDetailsViewVisible
        {
            get { return (bool)GetValue(IsDetailsViewVisibleProperty); }
            set { SetValue(IsDetailsViewVisibleProperty, value); }
        }

        #endregion

        // Notes: Value changes will be applied automatically in Live Charts as long as the object reference is still the same, dependency property is not required
        public SeriesCollection Series { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private WinForms.NotifyIcon _notifier;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += Home_Loaded;
            PreviewMouseLeftButtonUp += MainWindow_PreviewMouseLeftButtonUp;
            DetailsViewTrigger.MouseLeftButtonUp += DetailsViewTrigger_MouseLeftButtonUp;
            Scheduler.Current.SyncUpdated += Current_SyncUpdated;
            Scheduler.Current.SyncFailed += Current_SyncFailed;

            Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "MAX",
                    Stroke = Brushes.Red,
                    Values = new ChartValues<double>()
                },
                new LineSeries
                {
                    Title = "MIN",
                    Stroke = Brushes.Green,
                    Values = new ChartValues<double>()
                }
            };
            Labels = new List<string>();
            //YFormatter = value => value.ToString("C");

            InitSystemTray();

            DataContext = this;
        }

        private async void Home_Loaded(object sender, RoutedEventArgs e)
        {
            // fetch data for the latest 100 days initially
            await FetchDataAsync(100);
            await Scheduler.Current.RunNowAsync();
            (FindResource("RotateStoryboard") as Storyboard).Begin();
        }

        private async void Current_SyncUpdated(object sender, SyncUpdatedEventArgs e)
        {
            await FetchDataAsync(1);
        }

        private void Current_SyncFailed(object sender, SyncFailedEventArgs e)
        {
            Message = e.Message;

            NotifyMessage("Forex data sync error", e.Message);
        }

        private async Task FetchDataAsync(int maxDays)
        {
            var lastUpdateTime = await DbService.GetNewestTimeAsync();

            var results = await DbService.GetDailyDetailsAsync(lastUpdateTime);

            PagedRates = results;

            var latestRate = results.Items.OrderByDescending(o => o.Time).FirstOrDefault();
            if (latestRate != null)
            {
                LastUpdatedBy = latestRate.Time;
                LatestRate = latestRate.Rate;
            }
            else
            {
                LastUpdatedBy = null;
                LatestRate = 0;
            }

            var summaries = (await DbService.GetDailySummaryAsync(maxDays))
                .OrderBy(o => o.Date)
                .ToList();

            var latestSummary = summaries.OrderByDescending(o => o.Date).FirstOrDefault();
            if (latestSummary != null)
            {
                LatestMaxRate = latestSummary.MaxRate;
                LatestMinRate = latestSummary.MinRate;
            }
            else
            {
                LatestMaxRate = 0;
                LatestMinRate = 0;
            }

            // merge summaries
            foreach (var summary in summaries)
            {
                var label = summary.Date.ToString("MM-dd");
                var index = Labels.IndexOf(label);

                if (index == -1)
                {
                    Series[0].Values.Add(summary.MaxRate);
                    Series[1].Values.Add(summary.MinRate);
                    Labels.Add(label);
                }
                else
                {
                    Series[0].Values[index] = summary.MaxRate;
                    Series[1].Values[index] = summary.MinRate;
                    Labels[index] = label;
                }
            }
        }

        #region Pager

        private async void PagerFirst_Click(object sender, RoutedEventArgs e)
        {
            if (PagedRates.Pager.CurrentPage > 1)
            {
                PagedRates = await DbService.GetDailyDetailsAsync(LastUpdatedBy.Value.Date, 1);
            }
        }

        private async void PagerPrev_Click(object sender, RoutedEventArgs e)
        {
            if (PagedRates.Pager.CurrentPage > 1)
            {
                PagedRates = await DbService.GetDailyDetailsAsync(LastUpdatedBy.Value.Date, PagedRates.Pager.CurrentPage - 1);
            }
        }

        private async void PagerNext_Click(object sender, RoutedEventArgs e)
        {
            if (PagedRates.Pager.CurrentPage < PagedRates.Pager.PageCount)
            {
                PagedRates = await DbService.GetDailyDetailsAsync(LastUpdatedBy.Value.Date, PagedRates.Pager.CurrentPage + 1);
            }
        }

        private async void PagerLast_Click(object sender, RoutedEventArgs e)
        {
            if (PagedRates.Pager.CurrentPage < PagedRates.Pager.PageCount)
            {
                PagedRates = await DbService.GetDailyDetailsAsync(LastUpdatedBy.Value.Date, PagedRates.Pager.PageCount);
            }
        }

        #endregion

        private void MainWindow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement elem)
            {
                //// hide when click any other places outside of the DetailsView
                //if (elem != DetailsView && elem.FindAscendantByName("DetailsView") == null)
                //{
                //    IsDetailsViewVisible = false;

                //    e.Handled = true;
                //}

                // hide when click any other places outside of the DetailsView
                if (elem != DetailsView
                    && elem != DetailsViewTrigger
                    && elem.FindAscendantByName("DetailsView") == null
                    && elem.FindAscendantByName("DetailsViewTrigger") == null)
                {
                    IsDetailsViewVisible = false;
                }
            }
        }

        private void DetailsViewTrigger_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsDetailsViewVisible = !IsDetailsViewVisible;
        }

        #region System Tray

        private void InitSystemTray()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Forex.Assets.logo.ico"))
            {
                _notifier = new WinForms.NotifyIcon { Visible = true, Icon = new System.Drawing.Icon(stream) };
            }

            Closing += (sender, e) =>
            {
                Hide();
                e.Cancel = true;
            };

            var menu = new WinForms.ContextMenuStrip();
            menu.Items.Add("Open", null, (sender, args) => Show());
            menu.Items.Add("Quit", null, (sender, args) => Application.Current.Shutdown());
            _notifier.ContextMenuStrip = menu;

            _notifier.MouseMove += (sender, e) =>
            {
                _notifier.Text = $"USD/CNY Rate: {LatestRate}\r\nUpdated By: {LastUpdatedBy?.ToString("yyyy-MM-dd HH:mm:ss")}"
            };
            _notifier.MouseDown += (sender, e) =>
            {
                if (e.Button == WinForms.MouseButtons.Left)
                {
                    Show();
                }
            };

            _notifier.BalloonTipClicked += (sender, e) => Show();
        }

        private void NotifyMessage(string title, string message)
        {
            _notifier.BalloonTipTitle = title;
            _notifier.BalloonTipText = message;
            _notifier.ShowBalloonTip(2000);
        }

        #endregion
    }
}
