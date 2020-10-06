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
        private LocalizationGridUserControl ModLocalizationGrid { get; set; }

        //Reference to the loaded list warppers in the main window
        private Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; set; }

        private string GridAsCSVOnStart { get; set; }
        private string GridAsCSVAfterUpdate { get; set; }
        private const string TITLE_POSTPEND_MESSAGE = "_" + LocalizationFileObject.LOCALIZATION_FILE_NAME;
        private string WindowTitle { get; set; }
        private string GetTitleForWindow() 
        {
            return WindowTitle + TITLE_POSTPEND_MESSAGE;
        }
        private string StartingMod { get; set; }
        public LocalizationSettingWindow(Dictionary<string, XmlObjectsListWrapper> loadedListWrappers)
        {
            InitializeComponent();
            StartingMod = Properties.Settings.Default.ModTagSetting;
            WindowTitle = StartingMod.ToString();
            this.Title = GetTitleForWindow();
            this.LoadedListWrappers = loadedListWrappers;

            string pathToModLocalizationFile = XmlFileManager._ModConfigOutputPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            ModLocalizationGrid = new LocalizationGridUserControl(pathToModLocalizationFile);
            GridAsCSVOnStart = ModLocalizationGrid.Maingrid.GridAsCSV();
            GridAsCSVAfterUpdate = GridAsCSVOnStart;
            List<string> allCustomTagDirectories = XmlFileManager.GetCustomModFoldersInOutput();
            foreach (string nextModTag in allCustomTagDirectories)
            {
                ModSelectionComboBox.AddUniqueValueTo(nextModTag);
            }
            ModSelectionComboBox.SelectedItem = Properties.Settings.Default.ModTagSetting;
            
            ModSelectionComboBox.DropDownClosed += ModSelectionComboBox_DropDownClosed;
            ModLocalizationScrollViewer.Content = ModLocalizationGrid;

            string pathToGameLocalizationFile = XmlFileManager._LoadedFilesPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            LocalizationFileObject gameLocalizationFile = new LocalizationFileObject(pathToGameLocalizationFile);

            TextEditorOptions newOptions = new TextEditorOptions
            {
                EnableRectangularSelection = true,
                EnableTextDragDrop = true,
                HighlightCurrentLine = true,
                ShowTabs = true
            };

            GameRecordOutputBox.ShowLineNumbers = true;
            GameRecordOutputBox.TextArea.Options = newOptions;
            LocalizationPreviewBox.ShowLineNumbers = true;
            LocalizationPreviewBox.TextArea.Options = newOptions;
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
            LocalizationPreviewBox.LostFocus += LocalizationPreviewBox_LostFocus;
            SearchPanel.Install(LocalizationPreviewBox);
            ModLocalizationScrollViewer.GotFocus += Maingrid_GotFocus;
            ModLocalizationScrollViewer.LostFocus += Maingrid_GotFocus;
        }
        private void LocalizationPreviewBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HasLocalizatioWindowChanged())
            {
                string message = "You have modified the localization directly in the output window.\n" +
                     "Would you like to update the grid below with those changes?\n\n" +
                     "if you don't these changes will be lost.";
                string caption = "Update Grid";

                MessageBoxResult results = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (results)
                {
                    case MessageBoxResult.Yes:
                        string pathToTempFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TMP_" + LocalizationFileObject.LOCALIZATION_FILE_NAME);
                        XmlFileManager.WriteStringToFile(Directory.GetCurrentDirectory(), "TMP_" + LocalizationFileObject.LOCALIZATION_FILE_NAME, LocalizationPreviewBox.Text);
                        ReloadModLocalizationGrid(pathToTempFile);
                        GridAsCSVAfterUpdate = LocalizationPreviewBox.Text;
                        break;
                }
            }
        }
        private void SaveLocalization()
        {
            XmlFileManager.WriteStringToFile(XmlFileManager.Get_ModOutputPath(ModSelectionComboBox.SelectedItem.ToString()), LocalizationFileObject.LOCALIZATION_FILE_NAME, LocalizationPreviewBox.Text);
            GridAsCSVOnStart = ModLocalizationGrid.Maingrid.GridAsCSV();
        }
        public bool HasGridChanged() 
        {
            return !ModLocalizationGrid.Maingrid.GridAsCSV().Equals(GridAsCSVOnStart);
        }
        public bool HasLocalizatioWindowChanged()
        {
            return !LocalizationPreviewBox.Text.Equals(GridAsCSVAfterUpdate);
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
        private void ReloadModLocalizationGrid(string pathToLocalizatioFile, bool deleteFileAfterLoadingGrid = true)
        {
            ModLocalizationGrid = new LocalizationGridUserControl(pathToLocalizatioFile);
            ModLocalizationScrollViewer.Content = ModLocalizationGrid;
            if(deleteFileAfterLoadingGrid) File.Delete(pathToLocalizatioFile);
        }
        private void ReloadModLocalizationGrid(ComboBox modSelectionComboBox)
        {
            string modOutptPath = XmlFileManager.Get_ModOutputPath(modSelectionComboBox.SelectedItem.ToString());
            string pathToModLocalizationFile = modOutptPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            ModLocalizationGrid = new LocalizationGridUserControl(pathToModLocalizationFile);
            ModLocalizationScrollViewer.Content = ModLocalizationGrid;
            GridAsCSVOnStart = ModLocalizationGrid.Maingrid.GridAsCSV();
            LocalizationPreviewBox.Text = GridAsCSVOnStart;
            StartingMod = ModSelectionComboBox.SelectedItem.ToString();
            WindowTitle = StartingMod.ToString();
            this.Title = GetTitleForWindow();
        }
        private void Maingrid_GotFocus(object sender, RoutedEventArgs e)
        {
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
            GridAsCSVAfterUpdate = LocalizationPreviewBox.Text;
        }
        private void CopyGameRecord_Click(object sender, RoutedEventArgs e)
        {
            LocalizationPreviewBox.Text = ModLocalizationGrid.Maingrid.GridAsCSV();
        }
        private void SaveLocalizationTableButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLocalization();
            string message = "Successfully saved changes to: \n\n " + XmlFileManager.Get_ModOutputPath(ModSelectionComboBox.SelectedItem.ToString()) + LocalizationFileObject.LOCALIZATION_FILE_NAME;
            string caption = "Save Grid to Localization.txt";
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddEmptyRow_Click(object sender, RoutedEventArgs e)
        {
            //Hard code the wrapper to the items.xml for the mod
            XmlObjectsListWrapper selectedModItemsWrapper = LoadedListWrappers.GetValueOrDefault(ModSelectionComboBox.SelectedItem.ToString() + "_items");
            //Hard code the wrapper to the blocks.xml for the mod
            XmlObjectsListWrapper selectedModBlocksWrapper = LoadedListWrappers.GetValueOrDefault(ModSelectionComboBox.SelectedItem.ToString() + "_blocks");

            ModLocalizationGrid.AddEmptyRow(selectedModItemsWrapper, selectedModBlocksWrapper);
        }
    }
}
