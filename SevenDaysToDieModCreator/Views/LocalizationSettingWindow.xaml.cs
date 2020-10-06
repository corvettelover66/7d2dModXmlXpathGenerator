using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
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
        private LocalizationView ModLocalizationGrid { get; set; }
        public LocalizationSettingWindow()
        {
            InitializeComponent(); 
            string pathToModLocalizationFile = XmlFileManager._ModConfigOutputPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            ModLocalizationGrid = new LocalizationView(pathToModLocalizationFile);
            MainLocalStackPanel.Content = ModLocalizationGrid;

            string pathToGameLocalizationFile = XmlFileManager._LoadedFilesPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            LocalizationFileObject gameLocalizationFile = new LocalizationFileObject(pathToGameLocalizationFile);
            
            LocalizationPreviewBox.ShowLineNumbers = true;
            SearchPanel.Install(LocalizationPreviewBox);
            TextEditorOptions newOptions = new TextEditorOptions
            {
                EnableRectangularSelection = true,
                EnableTextDragDrop = true,
                HighlightCurrentLine = true,
                ShowTabs = true
            };
            this.LocalizationPreviewBox.TextArea.Options = newOptions;
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();

            ModLocalizationGrid.Maingrid.GotFocus += Maingrid_GotFocus;
        }

        private void Maingrid_GotFocus(object sender, RoutedEventArgs e)
        {
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
        }

        private void CopyGameRecord_Click(object sender, RoutedEventArgs e)
        {
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
        }

        private void SaveLocalizationTableButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
