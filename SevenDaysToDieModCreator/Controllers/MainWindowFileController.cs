using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Controllers
{
    class MainWindowFileController
    {
        //A dictionary for finding XmlListWrappers by filename
        //Key file name without .xml i.e. recipes, progressions, items
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        public MainWindowFileController(Dictionary<string, XmlObjectsListWrapper> loadedListWrappers)
        {
            this.LoadedListWrappers = loadedListWrappers;
        }
        public void LoadStartingDirectory(ComboBox searchTreeLoadedFilesComboBox, ComboBox newObjectViewLoadedFilesComboBox, ComboBox currentModLoadedFilesCenterViewComboBox, ComboBox loadedModsSearchViewComboBox, ComboBox CurrentGameFilesCenterViewComboBox)
        {
            Directory.CreateDirectory(XmlFileManager.LoadedFilesPath);
            Directory.CreateDirectory(Path.Combine(XmlFileManager.LoadedFilesPath, XmlFileManager.Xui_Folder_Name));
            Directory.CreateDirectory(Path.Combine(XmlFileManager.LoadedFilesPath, XmlFileManager.Xui_Menu_Folder_Name));
            //Check normal files
            string[] files = Directory.GetFiles(XmlFileManager.LoadedFilesPath, "*.xml");
            LoadFilesPathWrappers(files, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox, CurrentGameFilesCenterViewComboBox);
            //Check for Xui files
            string[] xuiFiles = Directory.GetFiles(Path.Combine(XmlFileManager.LoadedFilesPath, XmlFileManager.Xui_Folder_Name));
            if (xuiFiles.Length > 0) LoadFilesPathWrappers(xuiFiles, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox, CurrentGameFilesCenterViewComboBox);
            //Check for Xui menu files
            string[] xuiMenuFiles = Directory.GetFiles(Path.Combine(XmlFileManager.LoadedFilesPath, XmlFileManager.Xui_Menu_Folder_Name));
            if (xuiMenuFiles.Length > 0) LoadFilesPathWrappers(xuiMenuFiles, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox, CurrentGameFilesCenterViewComboBox);

            List<string> allCustomTagDirectories = XmlFileManager.GetCustomModFoldersInOutput();
            loadedModsSearchViewComboBox.AddUniqueValueTo("");
            foreach (string nextModTag in allCustomTagDirectories)
            {
                loadedModsSearchViewComboBox.AddUniqueValueTo(nextModTag);
                LoadCustomTagWrappers(nextModTag, currentModLoadedFilesCenterViewComboBox);
            }
        }
        private void LoadFilesPathWrappers(string[] files, ComboBox searchTreeLoadedFilesComboBox, ComboBox newObjectViewLoadedFilesComboBox, ComboBox currentGameFilesCenterViewComboBox)
        {
            foreach (string file in files)
            {
                XmlObjectsListWrapper wrapper = LoadWrapperFromFile(file);
                string parentPath = wrapper.XmlFile.ParentPath ?? "";
                if (wrapper != null && !File.Exists(Path.Combine(XmlFileManager.LoadedFilesPath, parentPath, wrapper.XmlFile.FileName)))
                    File.Copy(file, Path.Combine(XmlFileManager.LoadedFilesPath, parentPath, wrapper.XmlFile.FileName));
                if (wrapper != null)
                {
                    string wrapperDictionaryKey = wrapper.GenerateDictionaryKey(); 

                    UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                    searchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                    newObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                    currentGameFilesCenterViewComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                }
            }
        }
        public void LoadModFolder(ComboBox loadedModsSearchViewComboBox, ComboBox loadedModsCenterViewComboBox, ComboBox currentModFilesCenterViewComboBox)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string fullSelectedPath = openFileDialog.FileName;
                string currentModName = Path.GetFileName(openFileDialog.FileName);

                //If the selected path DOES NOT contain config it's the correct folder.
                if (!fullSelectedPath.ToLower().Contains("config"))
                {
                    bool hasXmlFiles = XmlFileManager.CheckLoadedModFolderForXmlFiles(fullSelectedPath);
                    if (hasXmlFiles) 
                    {
                        currentModFilesCenterViewComboBox.SetComboBox(new List<string>());
                        Properties.Settings.Default.ModTagSetting = currentModName;
                        Properties.Settings.Default.Save();
                        loadedModsSearchViewComboBox.SelectedItem = currentModName;
                        //Copy the files to the output path at Output/Mods/ModName
                        string appOutputPath = Path.Combine(XmlFileManager.AllModsOutputPath, "Mods", currentModName);
                        bool overwriteLocalAppFiles = false;
                        if (Directory.Exists(appOutputPath))
                        {
                            MessageBoxResult messageBoxResult = MessageBox.Show(
                                "The mod is already loaded. Do you want to OVERWRITE the local, application files?\n\n" +
                                "WARNING: This feature does not merge the files! If you have changes in the files, they will be overwritten.",
                                "Overwrite Application Mod Files",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);
                            switch (messageBoxResult)
                            {
                                case MessageBoxResult.Yes:
                                    overwriteLocalAppFiles = true;
                                    break;
                            }
                        }
                        XmlFileManager.CopyAllFilesToPath(fullSelectedPath, appOutputPath, overwriteLocalAppFiles);
                        loadedModsCenterViewComboBox.AddUniqueValueTo(currentModName);
                        loadedModsSearchViewComboBox.AddUniqueValueTo(currentModName);
                        LoadCustomTagWrappers(currentModName, currentModFilesCenterViewComboBox);                    
                    }
                    else
                    {
                        MessageBox.Show(
                            "The was an error loading the mod at " + openFileDialog.FileName + ".\n\nThere was no xml found in the Config folder of the mod. Please check the folder for xml files.",
                            "Missing XML Files!",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "The was an error loading the mod at " + openFileDialog.FileName + ".\n\nIf you selected the Config folder, please try again and select the ModFolder",
                        "Error Loading Directory",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        private void HandleFilesWithProblems(List<string> unloadedFiles)
        {
            //There were files with problems
            if (unloadedFiles.Count > 0)
            {
                string allFilesString = "";
                foreach (string nextFile in unloadedFiles)
                {
                    allFilesString += nextFile + "\n";
                }
                string messageBoxText = "Some files did not load correctly! \nFiles:\n" + allFilesString + "Only xml files can be loaded, and the file should only be loaded once.";
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }
        public void LoadFilesViewControl(ComboBox searchTreeLoadedFilesComboBox, ComboBox newObjectViewLoadedFilesComboBox, ComboBox currentGameFilesCenterViewComboBox)
        {
            List<string> unloadedFiles = new List<string>();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Length > 1)
                {
                    foreach (string nextFileName in openFileDialog.FileNames)
                    {
                        if (nextFileName.EndsWith(".xml")) AddWrapperDataToUIAndCopyFiles(nextFileName, unloadedFiles, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox, currentGameFilesCenterViewComboBox);
                        else if (nextFileName.EndsWith(".txt")) CopyFileToLoadedFilesPath(nextFileName);
                    }
                }
                else
                {
                    if (openFileDialog.FileName.EndsWith(".xml")) AddWrapperDataToUIAndCopyFiles(openFileDialog.FileName, unloadedFiles, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox, currentGameFilesCenterViewComboBox);
                    else if (openFileDialog.FileName.EndsWith(".txt")) CopyFileToLoadedFilesPath(openFileDialog.FileName);

                }
            }
            HandleFilesWithProblems(unloadedFiles);
        }
        private void CopyFileToLoadedFilesPath(string originalFilePath)
        {
            string loadedFilesPathWithFile = Path.Combine(XmlFileManager.LoadedFilesPath, Path.GetFileName(originalFilePath));
            if (File.Exists(loadedFilesPathWithFile)) OverwriteFilePrompt(loadedFilesPathWithFile, originalFilePath);
            else File.Copy(originalFilePath, loadedFilesPathWithFile);
        }
        private void OverwriteFilePrompt(string loadedFilesPathWithFile, string originalFilePath) 
        {
            string message = "You have already loaded the file: \n\n " 
                + Path.GetFileName(loadedFilesPathWithFile) + "\n\n" +
                "Would you like to reload the current file with this one?";
            string caption = "Overwrite Loaded File";
            MessageBoxResult results = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch ( results) 
            {
                case MessageBoxResult.Yes:
                    File.Delete(loadedFilesPathWithFile);
                    File.Copy(originalFilePath, loadedFilesPathWithFile);
                    break;
            }
        }
        private void AddWrapperDataToUIAndCopyFiles(string nextFileName, List<string> unloadedFiles, ComboBox searchTreeLoadedFilesComboBox, ComboBox newObjectViewLoadedFilesComboBox, ComboBox currentGameFilesCenterViewComboBox)
        {
            XmlObjectsListWrapper wrapper = LoadWrapperFromFile(nextFileName);
            string parentPath = wrapper.XmlFile.ParentPath ?? "";
            string loadedFilePathToFile =  Path.Combine(XmlFileManager.LoadedFilesPath, parentPath, wrapper.XmlFile.FileName);
            if (wrapper == null) unloadedFiles.Add(nextFileName);
            else if (File.Exists(loadedFilePathToFile)) OverwriteFilePrompt(loadedFilePathToFile, nextFileName);
            else File.Copy(nextFileName, Path.Combine(XmlFileManager.LoadedFilesPath, parentPath, wrapper.XmlFile.FileName));

            if (wrapper != null)
            {
                string wrapperDictionaryKey = wrapper.GenerateDictionaryKey();
                UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                searchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                newObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                currentGameFilesCenterViewComboBox.AddUniqueValueTo(wrapperDictionaryKey);
            }
        }

        public void LoadCustomTagWrappers(string nextModTag, ComboBox currentModLoadedFilesCenterViewComboBox)
        {
            string modOutputPath = XmlFileManager.Get_ModOutputPath(nextModTag);
            if (Directory.Exists(modOutputPath)) 
            {
                string[] modOutputFiles = Directory.GetFiles(modOutputPath);
                SetCustomTagWrapper(modOutputFiles, nextModTag, currentModLoadedFilesCenterViewComboBox);

                if (Directory.Exists(Path.Combine(modOutputPath, XmlFileManager.Xui_Folder_Name)))
                {
                    string[] modXuiOutputFiles = Directory.GetFiles(Path.Combine(modOutputPath, XmlFileManager.Xui_Folder_Name));
                    if (modXuiOutputFiles.Length > 0) SetCustomTagWrapper(modXuiOutputFiles, nextModTag, currentModLoadedFilesCenterViewComboBox);
                }
                if (Directory.Exists(Path.Combine(modOutputPath, XmlFileManager.Xui_Menu_Folder_Name)))
                {
                    string[] modXuiMenuOutputFiles = Directory.GetFiles(Path.Combine(modOutputPath, XmlFileManager.Xui_Menu_Folder_Name));
                    if (modXuiMenuOutputFiles.Length > 0) SetCustomTagWrapper(modXuiMenuOutputFiles, nextModTag, currentModLoadedFilesCenterViewComboBox);
                }            
            }
        }
        private void SetCustomTagWrapper(string[] modOutputFiles, string nextModTag, ComboBox currentModLoadedFilesCenterViewComboBox)
        {
            foreach (string nextModFile in modOutputFiles)
            {
                string newOutputLocation = Path.Combine(XmlFileManager.LoadedFilesPath, nextModTag);
                XmlObjectsListWrapper wrapper = LoadWrapperFromFile(nextModFile);
                if (wrapper != null)
                {
                    string wrapperDictionaryKey = nextModTag + "_" + wrapper.GenerateDictionaryKey();

                    UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                    if (nextModTag.Equals(Properties.Settings.Default.ModTagSetting)) currentModLoadedFilesCenterViewComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                    if (!Directory.Exists(newOutputLocation)) Directory.CreateDirectory(newOutputLocation);
                    string parentPath = wrapper.XmlFile.ParentPath ?? "";
                    if (!File.Exists(Path.Combine(newOutputLocation, parentPath, wrapper.XmlFile.FileName)))
                    {
                        Directory.CreateDirectory(Path.Combine(newOutputLocation, parentPath));
                        File.Copy(nextModFile, Path.Combine(newOutputLocation, parentPath, wrapper.XmlFile.FileName));
                    }
                }
            }
        }
        private void UpdateWrapperInDictionary(string key, XmlObjectsListWrapper updatedWrapper)
        {
            this.LoadedListWrappers.Remove(key);
            this.LoadedListWrappers.Add(key, updatedWrapper);
        }
        private XmlObjectsListWrapper LoadWrapperFromFile(string fileName)
        {
            long fileSize = new System.IO.FileInfo(fileName).Length;
            if (fileSize < 1) return null;

            XmlObjectsListWrapper wrapper = null;
            if (fileName.EndsWith(".xml"))
            {
                try
                {
                    wrapper = new XmlObjectsListWrapper(new XmlFileObject(fileName));
                }
                catch (Exception exception)
                {
                    XmlFileManager.WriteStringToLog("Failed to load file with exception:\n" + exception);
                    wrapper = null;
                }
            }
            return wrapper;
        }
        internal void RefreshMainUIComboboxes(ComboBox currentModLoadedFilesCenterViewComboBox, ComboBox loadedModsCenterViewComboBox, ComboBox loadedModsSearchViewComboBox)
        {
            currentModLoadedFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(Properties.Settings.Default.ModTagSetting, Properties.Settings.Default.ModTagSetting + "_"));
            loadedModsCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFoldersInOutput());
            loadedModsSearchViewComboBox.SetComboBox(XmlFileManager.GetCustomModFoldersInOutput());
            loadedModsCenterViewComboBox.Text = Properties.Settings.Default.ModTagSetting;
        }
        internal bool DeleteModFile(string modTagSetting, string comboBoxTextUnparsed)
        {
            bool didDelete = false;
            //Make sure the strings are available.
            if (String.IsNullOrEmpty(modTagSetting) || String.IsNullOrEmpty(comboBoxTextUnparsed)) 
            {
                XmlFileManager.WriteStringToLog("The inputs for the delete setting were incorrect. Did not delete file.");
                return didDelete;
            }
            //Split the string from the box it should be in the form modname_possibledir_filename
            string fileName = "";
            string[] comboBoxTextSplit = comboBoxTextUnparsed.Split("_");
            if (comboBoxTextSplit.Length > 1) 
            {
                //Get the very last element, should be the filename without the .xml extention 
                //This gets the last one essentially length - 1
                fileName = comboBoxTextSplit[^1] ;
                //Add the xml extention
                fileName += ".xml";
            }
            string promptMessage = "Are you absolutely sure you want to delete the file " + fileName + " for the mod " + modTagSetting + "?\n\n This cannot be UNDONE!";
            string promptCaption = "Delete file " + fileName;
            MessageBoxResult messageBoxResult = MessageBox.Show(promptMessage, promptCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (messageBoxResult) 
            {
                case MessageBoxResult.Yes:
                    XmlFileManager.DeleteModFile(modTagSetting, fileName);
                    didDelete = true;
                break;
            }
            return didDelete;
        }
        public void HandleLocalizationFile() 
        {
            string pathToLocalization = XmlFileManager.LoadedFilesPath + LocalizationFileObject.LOCALIZATION_FILE_NAME;

            if (File.Exists(pathToLocalization))
            {
                LocalizationSettingWindow localizationView = new LocalizationSettingWindow(this.LoadedListWrappers);
                localizationView.Show();
            }
            else 
            {
                string message = "You cannot use this feature without the 7d2d Game Localization.txt file loaded. To do this, in the main application, " +
                    "go File -> Load Game File(s) and select the Localizaton.txt file from the 7d2d main Game Directory Data/Config folder.";
                string caption = "Missing Game Localization File";
                MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void PreviewModFileInXmlOutput(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox, string wrapperKey)
        {
            if (!String.IsNullOrEmpty(wrapperKey))
            {
                XmlObjectsListWrapper xmlObjectsListWrapper = this.LoadedListWrappers.GetValueOrDefault(wrapperKey);
                xmlObjectsListWrapper ??= this.LoadedListWrappers.GetValueOrDefault(Properties.Settings.Default.ModTagSetting + "_" + wrapperKey);
                if (xmlObjectsListWrapper == null)
                {
                    MessageBox.Show(
                        "The was an error in the file for " + Properties.Settings.Default.ModTagSetting + "_" + wrapperKey + ".\n\n" +
                        "It is probably malformed xml, to check this, switch to the mod, open the \"File\" menu and click \"Validate Mod files\".",
                        "File Loading Error!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                string parentPath = xmlObjectsListWrapper.XmlFile.ParentPath ?? "";

                xmlOutputBox.Text = XmlFileManager.ReadExistingFile(Path.Combine(parentPath, xmlObjectsListWrapper.XmlFile.FileName));
            }
        }
    }
}
//public void LoadGameModDirectoryViewControl(ComboBox loadedModsSearchViewComboBox, ComboBox loadedModsCenterViewComboBox, ComboBox currentModFilesCenterViewComboBox)
//{
//    CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
//    openFileDialog.IsFolderPicker = true;
//    if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
//    {
//        string fullSelectedPath = openFileDialog.FileName;
//        //The problem is overwriting the existing directories directories
//        Directory.Move(fullSelectedPath, XmlFileManager._fileOutputPath);
//        List<string> allCustomTagDirectories = XmlFileManager.GetCustomModFoldersInOutput();
//        loadedModsSearchViewComboBox.AddUniqueValueTo("");
//        StringBuilder builder = new StringBuilder();
//        foreach (string nextModTag in allCustomTagDirectories)
//        {
//            bool hasXmlFiles = XmlFileManager.CheckLoadedModFolderForXmlFiles(Path.Combine(fullSelectedPath, nextModTag));
//            if (hasXmlFiles)
//            { 
//                loadedModsSearchViewComboBox.AddUniqueValueTo(nextModTag);
//                loadedModsCenterViewComboBox.AddUniqueValueTo(nextModTag);
//                LoadCustomTagWrappers(nextModTag, currentModFilesCenterViewComboBox);
//            }
//            else
//            {
//                builder.AppendLine("File: \n" + Path.Combine(openFileDialog.FileName, nextModTag) + "\n\n");
//            }
//        }
//        if (builder.Length > 0) 
//        {
//            builder.Insert(0, "There were some mods that did not load correctly. " +
//                "This would be because the xml files were not found or the directory for the mod is not well formed.\n\n" +
//                "Failed files:\n\n");
//            MessageBox.Show(builder.ToString(), "Failed Mod Directories", MessageBoxButton.OK, MessageBoxImage.Error);
//        }
//    }
//}