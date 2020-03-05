using SevenDaysToDieModCreator.Models;
using System;
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
			MessageBox.Show("Killing zombies or making mods?", "Making Mods", MessageBoxButton.OK, MessageBoxImage.Information);

			MainWindow mwd = new MainWindow
			{
				Title = "7 Days To Die Mod Editor"
			};
			mwd.Show();
		}
		private void SetupExceptionHandling()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += CurrentDomain_UnhandledException;
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
