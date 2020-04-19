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
using System.Xml;

namespace SevenDaysToDieModCreator.Controllers
{
    class MainWindowViewController
    {
        private long FILE_SIZE_THRESHOLD = 1000000;
        //A dictionary for finding XmlListWrappers by filename
        //Key file name without .xml i.e. recipes, progressions, items
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        public ObjectViewController LeftNewObjectViewController { get; private set; }
        public MainWindowViewController(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox)
        {
            this.LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.LeftNewObjectViewController = new ObjectViewController(xmlOutputBox);
        }
        public void AddSearchTree(MyStackPanel searchTreeFormsPanel, ComboBox SearchTreeLoadedFilesComboBox, bool doAddContextMenu = true)
        {
            string selectedObject = SearchTreeLoadedFilesComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = this.LoadedListWrappers.GetWrapperFromDictionary(selectedObject);
            if (selectedObject.Split("_").Length > 1)
            {
                selectedWrapper = this.LoadedListWrappers.GetValueOrDefault(selectedObject);
                if (selectedWrapper == null)
                {
                    MessageBox.Show(
                        "The was an error in the file for " + selectedObject + ".\n\n" +
                        "It is probably malformed xml, to check this, switch to the mod, open the \"File\" menu and click \"Validate Mod files\".",
                        "File Loading Error!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            XmlObjectsListWrapper leftObjectWrapper = searchTreeFormsPanel.LoadedListWrappers.GetValueOrDefault(selectedObject);
            if (leftObjectWrapper == null || leftObjectWrapper.xmlFile.FileSize < this.FILE_SIZE_THRESHOLD)
            {
                TreeViewItem nextTreeView = this.LeftNewObjectViewController.GetSearchTreeViewRecursive(selectedWrapper, selectedObject, doAddContextMenu);
                nextTreeView.Header = selectedObject;
                searchTreeFormsPanel.Children.Add(nextTreeView);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "That is a large file and consumes a considerable amount of resources, you already have one of those objects in the view. Are you sure you want another? ",
                    "Add Another Large Search Tree",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        TreeViewItem nextTreeView = LeftNewObjectViewController.GetSearchTreeViewRecursive(selectedWrapper, selectedObject, doAddContextMenu);
                        nextTreeView.Header = selectedObject;
                        searchTreeFormsPanel.Children.Add(nextTreeView);
                        break;
                }
            }
        }
        public void LoadStartingDirectory(ComboBox searchTreeLoadedFilesComboBox, ComboBox newObjectViewLoadedFilesComboBox, ComboBox currentModLoadedFilesCenterViewComboBox, ComboBox loadedModsSearchViewComboBox)
        {
            Directory.CreateDirectory(XmlFileManager._LoadedFilesPath);
            Directory.CreateDirectory(Path.Combine(XmlFileManager._LoadedFilesPath, XmlFileManager.Xui_Folder_Name));
            Directory.CreateDirectory(Path.Combine(XmlFileManager._LoadedFilesPath, XmlFileManager.Xui_Menu_Folder_Name));
            //Check normal files
            string[] files = Directory.GetFiles(XmlFileManager._LoadedFilesPath);
            LoadFilesPathWrappers(files, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox);
            //Check for Xui files
            string[] xuiFiles = Directory.GetFiles(Path.Combine(XmlFileManager._LoadedFilesPath, XmlFileManager.Xui_Folder_Name));
            if (xuiFiles.Length > 0) LoadFilesPathWrappers(xuiFiles, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox);
            //Check for Xui menu files
            string[] xuiMenuFiles = Directory.GetFiles(Path.Combine(XmlFileManager._LoadedFilesPath, XmlFileManager.Xui_Menu_Folder_Name));
            if (xuiMenuFiles.Length > 0) LoadFilesPathWrappers(xuiMenuFiles, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox);

            List<string> allCustomTagDirectories = XmlFileManager.GetCustomModFoldersInOutput();
            loadedModsSearchViewComboBox.AddUniqueValueTo("");
            foreach (string nextModTag in allCustomTagDirectories)
            {
                loadedModsSearchViewComboBox.AddUniqueValueTo(nextModTag);
                LoadCustomTagWrappers(nextModTag, currentModLoadedFilesCenterViewComboBox);
            }
        }
        private void LoadFilesPathWrappers(string[] files, ComboBox searchTreeLoadedFilesComboBox, ComboBox newObjectViewLoadedFilesComboBox)
        {
            foreach (string file in files)
            {
                XmlObjectsListWrapper wrapper = LoadWrapperFromFile(file);
                string parentPath = wrapper.xmlFile.ParentPath == null ? "" : wrapper.xmlFile.ParentPath;
                if (wrapper != null && !File.Exists(Path.Combine(XmlFileManager._LoadedFilesPath, parentPath, wrapper.xmlFile.FileName)))
                    File.Copy(file, Path.Combine(XmlFileManager._LoadedFilesPath, parentPath, wrapper.xmlFile.FileName));
                if (wrapper != null)
                {
                    string wrapperDictionaryKey = wrapper.xmlFile.ParentPath == null
                        ? wrapper.xmlFile.GetFileNameWithoutExtension()
                        : wrapper.xmlFile.ParentPath + "_" + wrapper.xmlFile.GetFileNameWithoutExtension();

                    UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                    searchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                    newObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                }
            }
        }
        public void LoadDirectoryViewControl(ComboBox loadedModsSearchViewComboBox, ComboBox loadedModsCenterViewComboBox, ComboBox currentModFilesCenterViewComboBox)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string fullSelectedPath = openFileDialog.FileName;
                string currentModName = Path.GetFileName(openFileDialog.FileName);
                bool hasXmlFiles = XmlFileManager.CheckLoadedModFolderForXmlFiles(fullSelectedPath);

                if (hasXmlFiles)
                {
                    Properties.Settings.Default.ModTagSetting = currentModName;
                    Properties.Settings.Default.Save();
                    //Copy the files to the output path at Output/Mods/ModName
                    string appOutputPath = Path.Combine(XmlFileManager._fileOutputPath, "Mods", currentModName);
                    XmlFileManager.CopyAllFilesToPath(fullSelectedPath, appOutputPath);
                    loadedModsCenterViewComboBox.AddUniqueValueTo(currentModName);
                    loadedModsSearchViewComboBox.AddUniqueValueTo(currentModName);
                    LoadCustomTagWrappers(currentModName, currentModFilesCenterViewComboBox);
                }
                else
                {
                    MessageBox.Show(
                        "The was an error loading the mod at " + openFileDialog.FileName + ". There was no xml found in the Config folder of the mod. Please check the folder for xml files.",
                        "Missing XML Files!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
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

        public void LoadFilesViewControl(ComboBox SearchTreeLoadedFilesComboBox, ComboBox NewObjectViewLoadedFilesComboBox)
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
                        XmlObjectsListWrapper wrapper = LoadWrapperFromFile(nextFileName);
                        string parentPath = wrapper.xmlFile.ParentPath == null ? "" : wrapper.xmlFile.ParentPath;
                        if (wrapper == null) unloadedFiles.Add(nextFileName);
                        else if (!File.Exists(Path.Combine(XmlFileManager._LoadedFilesPath, parentPath, wrapper.xmlFile.FileName)))
                            File.Copy(nextFileName, Path.Combine(XmlFileManager._LoadedFilesPath, parentPath, wrapper.xmlFile.FileName));

                        if (wrapper != null)
                        {
                            string wrapperDictionaryKey = wrapper.xmlFile.ParentPath == null ? wrapper.xmlFile.GetFileNameWithoutExtension() : wrapper.xmlFile.ParentPath + "_" + wrapper.xmlFile.GetFileNameWithoutExtension();
                            UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                            SearchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                            NewObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                        }
                    }
                }
                else
                {
                    XmlObjectsListWrapper wrapper = LoadWrapperFromFile(openFileDialog.FileName);
                    string parentPath = wrapper.xmlFile.ParentPath == null ? "" : wrapper.xmlFile.ParentPath;
                    if (wrapper == null) unloadedFiles.Add(openFileDialog.FileName);
                    else if (!File.Exists(Path.Combine(XmlFileManager._LoadedFilesPath, parentPath, wrapper.xmlFile.FileName)))
                        File.Copy(openFileDialog.FileName, Path.Combine(XmlFileManager._LoadedFilesPath, parentPath, wrapper.xmlFile.FileName));

                    if (wrapper != null)
                    {
                        string wrapperDictionaryKey = wrapper.xmlFile.ParentPath == null ? wrapper.xmlFile.GetFileNameWithoutExtension() : wrapper.xmlFile.ParentPath + "_" + wrapper.xmlFile.GetFileNameWithoutExtension();
                        UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                        SearchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                        NewObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                    }
                }
            }
            HandleFilesWithProblems(unloadedFiles);
        }
        internal void ModifySearchViewFont(int fontChange, UIElementCollection allChildren)
        {
            foreach (Control nextControl in allChildren)
            {
                if (fontChange < 0 && nextControl.FontSize < 6) continue;
                if (nextControl is TreeViewItem nextTreeViewItem)
                {
                    if (nextTreeViewItem.Header is MyComboBox myComboBox)
                    {
                        if (myComboBox.FontSize > 4 || fontChange > 0) myComboBox.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header is Button button)
                    {
                        if (button.FontSize > 4 || fontChange > 0) button.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header.GetType() == typeof(string))
                    {
                        if (nextTreeViewItem.FontSize > 4 || fontChange > 0) nextTreeViewItem.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.HasItems) ModifySearchViewFontTreeView(fontChange, nextTreeViewItem.Items);
                }
                else nextControl.FontSize += fontChange;
            }
        }
        private void ModifySearchViewFontTreeView(int fontChange, ItemCollection allChildren)
        {
            foreach (Control nextControl in allChildren)
            {
                if (fontChange < 0 && nextControl.FontSize < 6) continue;
                if (nextControl is TreeViewItem nextTreeViewItem)
                {
                    if (nextTreeViewItem.Header is MyComboBox myComboBox) 
                    {
                        if(myComboBox.FontSize > 4 || fontChange > 0) myComboBox.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header is Button button)
                    {
                        if(button.FontSize > 4 || fontChange > 0) button.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header.GetType() == typeof(string))
                    {
                        if (nextTreeViewItem.FontSize > 4 || fontChange > 0) nextTreeViewItem.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.HasItems) ModifySearchViewFontTreeView(fontChange, nextTreeViewItem.Items);
                }
                else nextControl.FontSize += fontChange;
            }
        }
        public void LoadCustomTagWrappers(string nextModTag, ComboBox currentModLoadedFilesCenterViewComboBox)
        {
            string modOutputPath = XmlFileManager.Get_ModOutputPath(nextModTag);
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
        private void SetCustomTagWrapper(string[] modOutputFiles, string nextModTag, ComboBox currentModLoadedFilesCenterViewComboBox)
        {
            foreach (string nextModFile in modOutputFiles)
            {
                string newOutputLocation = Path.Combine(XmlFileManager._LoadedFilesPath, nextModTag);
                XmlObjectsListWrapper wrapper = LoadWrapperFromFile(nextModFile);
                if (wrapper != null)
                {
                    string parentPath = wrapper.xmlFile.ParentPath == null ? "" : wrapper.xmlFile.ParentPath;
                    string wrapperDictionaryKey = String.IsNullOrEmpty(parentPath)
                        ? nextModTag + "_" + wrapper.xmlFile.GetFileNameWithoutExtension()
                        : nextModTag + "_" + wrapper.xmlFile.ParentPath + "_" + wrapper.xmlFile.GetFileNameWithoutExtension();

                    UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                    string loadedModFilesCenterViewItem = String.IsNullOrEmpty(wrapper.xmlFile.ParentPath)
                        ? wrapper.xmlFile.GetFileNameWithoutExtension()
                        : parentPath + "_" + wrapper.xmlFile.GetFileNameWithoutExtension();
                    if (nextModTag.Equals(Properties.Settings.Default.ModTagSetting)) currentModLoadedFilesCenterViewComboBox.AddUniqueValueTo(loadedModFilesCenterViewItem);
                    if (!Directory.Exists(newOutputLocation)) Directory.CreateDirectory(newOutputLocation);
                    if (!File.Exists(Path.Combine(newOutputLocation, parentPath, wrapper.xmlFile.FileName)))
                    {
                        Directory.CreateDirectory(Path.Combine(newOutputLocation, parentPath));
                        File.Copy(nextModFile, Path.Combine(newOutputLocation, parentPath, wrapper.xmlFile.FileName));
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
        internal void SetNewCustomTag(Views.CustomDialogBox dialog, ComboBox currentModLoadedFilesCenterViewComboBox, ComboBox loadedModsCenterViewComboBox)
        {
            string name = XmlConvert.VerifyName(dialog.ResponseText);
            Properties.Settings.Default.ModTagSetting = name;
            Properties.Settings.Default.Save();
            currentModLoadedFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(name));
            loadedModsCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFoldersInOutput());
            loadedModsCenterViewComboBox.Text = name;
        }
        internal void ChangeCustomTagName(CustomDialogBox dialog, ComboBox currentModLoadedFilesCenterViewComboBox, ComboBox loadedModsCenterViewComboBox, ComboBox loadedModsSearchViewComboBox)
        {
            string newModName = XmlConvert.VerifyName(dialog.ResponseText);
            string oldModName = Properties.Settings.Default.ModTagSetting;
            XmlFileManager.RenameModDirectory(oldModName, newModName);
            Properties.Settings.Default.ModTagSetting = newModName;
            Properties.Settings.Default.Save();
            currentModLoadedFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(newModName));
            loadedModsCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFoldersInOutput());
            loadedModsSearchViewComboBox.SetComboBox(XmlFileManager.GetCustomModFoldersInOutput());
            loadedModsCenterViewComboBox.Text = newModName;
        }
    }
}
