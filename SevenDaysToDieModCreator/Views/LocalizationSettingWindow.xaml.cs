using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class LocalizationSettingWindow : Window
    {
        private LocalizationView ModLocalizationGrid { get; set; }
            
        private string GridAsCSVOnStart { get; set; }

        private string StartingMod { get; set; }
        public LocalizationSettingWindow()
        {
            InitializeComponent(); 
            string pathToModLocalizationFile = XmlFileManager._ModConfigOutputPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            ModLocalizationGrid = new LocalizationView(pathToModLocalizationFile);
            GridAsCSVOnStart = ModLocalizationGrid.Maingrid.GridAsCSV();
            List<string> allCustomTagDirectories = XmlFileManager.GetCustomModFoldersInOutput();
            foreach (string nextModTag in allCustomTagDirectories)
            {
                ModSelectionComboBox.AddUniqueValueTo(nextModTag);
            }
            ModSelectionComboBox.SelectedItem = Properties.Settings.Default.ModTagSetting;
            StartingMod = Properties.Settings.Default.ModTagSetting;
            ModSelectionComboBox.DropDownClosed += ModSelectionComboBox_DropDownClosed;
            ModLocalizationScrollViewer.Content = ModLocalizationGrid;

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
            LocalizationPreviewBox.TextArea.Options = newOptions;
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
            LocalizationPreviewBox.LostFocus += LocalizationPreviewBox_LostFocus;
            ModLocalizationScrollViewer.GotFocus += Maingrid_GotFocus;
            ModLocalizationScrollViewer.LostFocus += Maingrid_GotFocus;
        }
        private void LocalizationPreviewBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HasLocalizatioWindowChanged())
            {
                string localizationPath = XmlFileManager.Get_ModOutputPath(ModSelectionComboBox.SelectedItem.ToString()) + LocalizationFileObject.LOCALIZATION_FILE_NAME;
                string message = "You have modified the localization directly in the output window.\n" +
                     "Would you like to save those changes and update the grid?\n\n" +
                     "WARNING: This will overwrite the file at \n\n" + localizationPath +"\nand cannot be undone!";
                string caption = "Save Localization and Update Grid";

                MessageBoxResult results = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (results)
                {
                    case MessageBoxResult.Yes:
                        SaveLocalization();
                        ReloadModLocalizationGrid(ModSelectionComboBox);
                        break;
                }
            }
        }
        private void SaveLocalization()
        {
            XmlFileManager.WriteStringToFile(XmlFileManager.Get_ModOutputPath(ModSelectionComboBox.SelectedItem.ToString()), LocalizationFileObject.LOCALIZATION_FILE_NAME, LocalizationPreviewBox.Text);
        }
        public bool HasGridChanged() 
        {
            return !ModLocalizationGrid.Maingrid.GridAsCSV().Equals(GridAsCSVOnStart);
        }
        public bool HasLocalizatioWindowChanged()
        {
            return !LocalizationPreviewBox.Text.Equals(GridAsCSVOnStart);
        }
        private void ModSelectionComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (sender is ComboBox modSelectionComboBox) 
            {
                string message = "You have unsaved changes below, for the localization of " + StartingMod + ".\n" +
                    "Are you sure you want to change mods, and LOSE any work made on the current localization?";
                string caption = "Lose Unsaved Changes";

                //If the grid has changed and is unsaved, make sure they are okay losing changes.
                if (HasGridChanged())
                {
                    MessageBoxResult results = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch (results)
                    {
                        case MessageBoxResult.Yes:
                            ReloadModLocalizationGrid(modSelectionComboBox);
                            break;
                        case MessageBoxResult.No:
                            modSelectionComboBox.SelectedItem = StartingMod;
                            break;
                    }
                }
                else 
                {
                    ReloadModLocalizationGrid(modSelectionComboBox);
                }

            }
        }

        private void ReloadModLocalizationGrid(ComboBox modSelectionComboBox)
        {
            string modOutptPath = XmlFileManager.Get_ModOutputPath(modSelectionComboBox.SelectedItem.ToString());
            string pathToModLocalizationFile = modOutptPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            ModLocalizationGrid = new LocalizationView(pathToModLocalizationFile);
            ModLocalizationScrollViewer.Content = ModLocalizationGrid;
            GridAsCSVOnStart = ModLocalizationGrid.Maingrid.GridAsCSV();
            StartingMod = ModSelectionComboBox.SelectedItem.ToString();
        }

        private void Maingrid_GotFocus(object sender, RoutedEventArgs e)
        {
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
            GridAsCSVOnStart = LocalizationPreviewBox.Text;
        }
        private void CopyGameRecord_Click(object sender, RoutedEventArgs e)
        {
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
        }
        private void SaveLocalizationTableButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLocalization();
        }
    }
}
