using Microsoft.Win32;
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
            if (selectedObject.Split(":").Length > 1) 
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
                TreeViewItem nextTreeView = this.LeftNewObjectViewController.GetSearchTreeViewRecursive(selectedWrapper, doAddContextMenu);
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
                        TreeViewItem nextTreeView = LeftNewObjectViewController.GetSearchTreeViewRecursive(selectedWrapper, doAddContextMenu);
                        searchTreeFormsPanel.Children.Add(nextTreeView);
                        break;
                }
            }
        }
        public void LoadStartingDirectory(ComboBox SearchTreeLoadedFilesComboBox, ComboBox NewObjectViewLoadedFilesComboBox, ComboBox OpenDirectEditLoadedFilesComboBox, ComboBox LoadedModsComboBox)
        {
            if (!Directory.Exists(XmlFileManager._LoadedFilesPath)) Directory.CreateDirectory(XmlFileManager._LoadedFilesPath);
            string[] files = Directory.GetFiles(XmlFileManager._LoadedFilesPath);
            foreach (string file in files)
            {
                XmlObjectsListWrapper wrapper = LoadWrapperFromFile(file);
                if (wrapper != null && !File.Exists(XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName))
                    File.Copy(file, XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName);
                if (wrapper != null)
                {
                    string wrapperDictionaryKey = wrapper.xmlFile.GetFileNameWithoutExtension();
                    UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                    SearchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                    NewObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                }
            }
            List<string> allCustomTagDirectories = XmlFileManager.GetCustomModFoldersInOutput();
            LoadedModsComboBox.AddUniqueValueTo("");
            foreach (string nextModTag in allCustomTagDirectories)
            {
                LoadCustomTagWrappers(nextModTag, LoadedModsComboBox, OpenDirectEditLoadedFilesComboBox);
            }
        }
        public void LoadDirectoryViewControl(ComboBox SearchTreeLoadedFilesComboBox)
        {
            List<string> unloadedFiles = new List<string>();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (XmlFileManager.IsDirectory(openFileDialog.FileName)) 
                {
                
                }
                //XmlObjectsListWrapper wrapper = LoadFile(SearchTreeLoadedFilesComboBox, openFileDialog.FileName, NewObjectViewLoadedFilesComboBox, openDirectEditViewComboBox);
                //if (wrapper == null) unloadedFiles.Add(openFileDialog.FileName);
            }
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
                       
                        if (wrapper == null) unloadedFiles.Add(nextFileName);
                        else if (!File.Exists(XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName))
                            File.Copy(nextFileName, XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName);

                        if (wrapper != null) 
                        {
                            string wrapperDictionaryKey = wrapper.xmlFile.GetFileNameWithoutExtension();
                            UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                            SearchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                            NewObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                        }
                    }
                }
                else 
                {
                    XmlObjectsListWrapper wrapper = LoadWrapperFromFile(openFileDialog.FileName);
                    if (wrapper == null) unloadedFiles.Add(openFileDialog.FileName);
                    else if (!File.Exists(XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName)) 
                    {
                        File.Copy(openFileDialog.FileName, XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName);
                        this.LoadedListWrappers.Add(wrapper.xmlFile.GetFileNameWithoutExtension(), wrapper);
                    }
                    if (wrapper != null)
                    {
                        string wrapperDictionaryKey = wrapper.xmlFile.GetFileNameWithoutExtension();
                        UpdateWrapperInDictionary(wrapperDictionaryKey, wrapper);
                        SearchTreeLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                        NewObjectViewLoadedFilesComboBox.AddUniqueValueTo(wrapperDictionaryKey);
                    }
                }
            }
            //There were files with problems
            if(unloadedFiles.Count > 0)
            {
                string allFilesString = "";
                foreach(string nextFile in unloadedFiles) 
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
        public void LoadCustomTagWrappers(string nextModTag, ComboBox loadedModsComboBox, ComboBox openDirectEditLoadedFilesComboBox) 
        {
            loadedModsComboBox.AddUniqueValueTo(nextModTag);
            string modOutputPath = XmlFileManager.Get_ModOutputPath(nextModTag);
            string[] modOutputFiles = Directory.GetFiles(modOutputPath);
            string modFileTagPath = String.IsNullOrEmpty(nextModTag) ? "" : nextModTag + "/";
            string newOutputLocation = XmlFileManager._LoadedFilesPath + modFileTagPath;
            foreach (string nextModFile in modOutputFiles)
            {
                XmlObjectsListWrapper wrapper = LoadWrapperFromFile(nextModFile);
                if (wrapper != null)
                {
                    string wrapperDictionKey = nextModTag + ":" + wrapper.xmlFile.GetFileNameWithoutExtension();
                    UpdateWrapperInDictionary(wrapperDictionKey, wrapper);
                    if (nextModTag.Equals(Properties.Settings.Default.CustomTagName)) openDirectEditLoadedFilesComboBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    if (!Directory.Exists(newOutputLocation)) Directory.CreateDirectory(newOutputLocation);
                    if (!File.Exists(newOutputLocation + wrapper.xmlFile.FileName)) File.Copy(nextModFile, XmlFileManager._LoadedFilesPath + modFileTagPath + wrapper.xmlFile.FileName);
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
