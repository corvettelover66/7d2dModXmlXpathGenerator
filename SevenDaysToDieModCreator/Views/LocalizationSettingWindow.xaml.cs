using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for LocalizationSettingWindow.xaml
    /// </summary>
    public partial class LocalizationSettingWindow : Window
    {
        public LocalizationSettingWindow()
        {
            InitializeComponent(); 
            string pathToModLocalizationFile = XmlFileManager._ModConfigOutputPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            MainLocalStackPanel.Content = new LocalizationView(pathToModLocalizationFile);

            //GridRow0

            //string pathToGameLocalizationFile = XmlFileManager._LoadedFilesPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            //LocalizationFileObject localizationFileObject = new LocalizationFileObject(pathToGameLocalizationFile);
            //foreach (string headerField in localizationFileObject.HeaderValuesMap.Keys) 
            //{
            //    List<string> commonValues = localizationFileObject.HeaderValuesMap.GetValueOrDefault(headerField);
            //    ComboBox gameValues = commonValues.CreateComboBoxFromList();
            //    GameValuesSP.Children.Add(gameValues);
            //}
        }

        private void CopyGameRecord_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveLocalizationTableButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
