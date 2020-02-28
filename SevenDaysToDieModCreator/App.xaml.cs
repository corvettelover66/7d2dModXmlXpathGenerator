using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SevenDaysToDieModCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			//Show splash screen
			MessageBox.Show("Killing zombies or making mods?", "Making Mods", MessageBoxButton.OK, MessageBoxImage.Information);

			MainWindow mwd = new MainWindow
			{
				Title = "7 Days To Die Mod Editor"
			};
			mwd.Show();
		}
    }
}
