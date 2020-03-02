using System.Windows;

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
