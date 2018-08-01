using Forex.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Forex
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DbService.Initialize();

            Scheduler.Current.Start();
        }

        /// <summary>
        /// Occurs when a Windows session ends (Logging off, Shutting down, Restarting, Hibernating), which would trigger the Application_Exit event (is this really the truth???)
        /// https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/application-management-overview#session-ending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Cleanup();
        }

        /// <summary>
        /// Occurs when the user exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Cleanup();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception baseEx = e.Exception.GetBaseException();

            // fallback to native message box
            MessageBox.Show(baseEx.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }

        private void Cleanup()
        {
            Scheduler.Current.Stop();

            // TODO: can login out successfully??
            //DataService.LogoutAsync().Wait();

            DbService.Cleanup();
        }
    }
}
