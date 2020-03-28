using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
        public MainWindowViewController(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox,RoutedEventHandler removeChildContextMenu_Click) 
        {
            this.LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.LeftNewObjectViewController = new ObjectViewController(xmlOutputBox, removeChildContextMenu_Click);
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
                        "The was an error in the file for " + selectedObject + ". Please check the xml file as it is probably malformed xml. For a detailed error check the log.",
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
                nextTreeView.Header =  selectedObject;
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
            string[] xuiFiles = Directory.GetFiles(Path.Combine( XmlFileManager._LoadedFilesPath, XmlFileManager.Xui_Folder_Name));
            if(xuiFiles.Length > 0) LoadFilesPathWrappers(xuiFiles, searchTreeLoadedFilesComboBox, newObjectViewLoadedFilesComboBox);
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
        public void LoadDirectoryViewControl(ComboBox loadedModsSearchViewComboBox, ComboBox loadedModsCenterViewFilesComboBox, ComboBox currentModLoadedFilesCenterViewComboBox)
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
                    Properties.Settings.Default.CustomTagName = currentModName;
                    Properties.Settings.Default.Save();
                    //Copy the files to the output path at Output/Mods/ModName
                    string appOutputPath = Path.Combine(XmlFileManager._fileOutputPath, "Mods", currentModName);
                    XmlFileManager.CopyAllFilesToPath(fullSelectedPath, appOutputPath);
                    loadedModsCenterViewFilesComboBox.AddUniqueValueTo(currentModName);
                    loadedModsSearchViewComboBox.AddUniqueValueTo(currentModName);
                    LoadCustomTagWrappers(currentModName, currentModLoadedFilesCenterViewComboBox);
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
        internal void ModifySearchViewFont(int fontChange, List<TreeViewItem> treesToSearch)
        {
            foreach (TreeViewItem nextTreeViewItem in treesToSearch) 
            {
                if (fontChange < 0 && nextTreeViewItem.FontSize == 1) continue;
                nextTreeViewItem.FontSize += fontChange;
                if (nextTreeViewItem.Header.GetType() == typeof(MyComboBox)) (nextTreeViewItem.Header as MyComboBox).FontSize += fontChange;
                List<TreeViewItem> children = nextTreeViewItem.GetTreeViewChildren();
                if (children != null && children.Count > 0) ModifySearchViewFont(fontChange, children);
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
                string newOutputLocation = Path.Combine( XmlFileManager._LoadedFilesPath, nextModTag);
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
                    if (nextModTag.Equals(Properties.Settings.Default.CustomTagName)) currentModLoadedFilesCenterViewComboBox.AddUniqueValueTo(loadedModFilesCenterViewItem);
                    if (!Directory.Exists(newOutputLocation)) Directory.CreateDirectory(newOutputLocation);
                    if (!File.Exists(Path.Combine(newOutputLocation, parentPath, wrapper.xmlFile.FileName)))
                    {
                        Directory.CreateDirectory(Path.Combine(newOutputLocation, parentPath));
                        File.Copy(nextModFile, Path.Combine(newOutputLocation, parentPath,  wrapper.xmlFile.FileName));
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
    }
}
