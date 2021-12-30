using SevenDaysToDieModCreator.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SevenDaysToDieModCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupExceptionHandling();
            //Show splash screen
            MessageBox.Show("Killing zombies or making mods?", "Making Mods", MessageBoxButton.YesNo, MessageBoxImage.Information);

            MainWindow mwd = new MainWindow
            {
                Title = "7 Days to Die Mod Edit"
            };
            mwd.Show();
        }
        private void SetupExceptionHandling()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs exception)
        {
            XmlFileManager.WriteStringToLog("ERROR MESSAGE: " + exception.Exception.Message, true);
            XmlFileManager.WriteStringToLog("ERROR TRACE: " + exception.Exception.StackTrace);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs exception)
        {
            XmlFileManager.WriteStringToLog("ERROR MESSAGE: " + exception.Exception.Message, true);
            XmlFileManager.WriteStringToLog("ERROR TRACE: " + exception.Exception.StackTrace);
        }

        //Global Error Processing. Catch any errors and send them to the log, let application shutdown
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs exception)
        {
            Exception objectAsException = (Exception)exception.ExceptionObject;
            // Process unhandled exception
            XmlFileManager.WriteStringToLog("ERROR MESSAGE: " + objectAsException.Message, true);
            XmlFileManager.WriteStringToLog("ERROR TRACE: " + objectAsException.StackTrace);
        }
    }
}
