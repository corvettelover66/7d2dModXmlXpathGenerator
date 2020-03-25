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
        //A dictionary for finding XmlListWrappers by filename
        //Key file name without .xml i.e. recipes, progressions, items
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        public ObjectViewController LeftNewObjectViewController { get; private set; }

        public MainWindowViewController(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox,  RoutedEventHandler removeChildContextMenu_Click) 
        {
            this.LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.LeftNewObjectViewController = new ObjectViewController(xmlOutputBox, removeChildContextMenu_Click);
        }

        public void LoadStartingDirectory(ComboBox SearchTreeLoadedFilesComboBox, ComboBox NewObjectViewLoadedFilesComboBox, ComboBox OpenDirectEditLoadedFilesComboBox)
        {
            if (!Directory.Exists(XmlFileManager._LoadedFilesPath)) Directory.CreateDirectory(XmlFileManager._LoadedFilesPath);
            string[] files = Directory.GetFiles(XmlFileManager._LoadedFilesPath);
            string[] directories = Directory.GetDirectories(XmlFileManager._LoadedFilesPath);
            foreach (string file in files)
            {
                LoadFile(SearchTreeLoadedFilesComboBox, NewObjectViewLoadedFilesComboBox, OpenDirectEditLoadedFilesComboBox, file);
            }
            foreach (string directory in directories)
            {
                LoadModDirectory(SearchTreeLoadedFilesComboBox, directory);
            }
        }

        public void LoadFilesViewControl(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox, ComboBox openDirectEditViewComboBox) 
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
                        bool didLoad = LoadFile(allLoadedFilesBox, allLoadedObjectsBox, openDirectEditViewComboBox, nextFileName);
                        if (!didLoad) unloadedFiles.Add(nextFileName);
                    }
                }
                else 
                {
                    bool didLoad = LoadFile(allLoadedFilesBox, allLoadedObjectsBox, openDirectEditViewComboBox, openFileDialog.FileName);
                    if (!didLoad) unloadedFiles.Add(openFileDialog.FileName);
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
        private bool LoadModDirectory(ComboBox searchTreeLoadedFilesComboBox, string directory)
        {
            bool didLoad = false;
            if (directory.EndsWith(".xml"))
            {
            }
            return didLoad;
        }
        private bool LoadFile(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox, ComboBox openDirectEditViewComboBox, string fileName) 
        {
            bool didLoad = false;
            if (fileName.EndsWith(".xml"))
            {
                XmlObjectsListWrapper wrapper = new XmlObjectsListWrapper(new XmlFileObject(fileName));
                try
                {
                    string wrapperDictionKey = wrapper.xmlFile.GetFileNameWithoutExtension();
                    //If the first tag is not similar to the file name it is most likely a mod.
                    bool isModFile = !wrapperDictionKey.Contains(wrapper.FirstChildTagName);
                    //Try to copy the file
                    if(!File.Exists(XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName) ) File.Copy(fileName, XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName);
                    this.LoadedListWrappers.Add(wrapperDictionKey, wrapper);
                    allLoadedFilesBox.AddUniqueValueTo(wrapperDictionKey);
                    allLoadedObjectsBox.AddUniqueValueTo(wrapperDictionKey);
                    openDirectEditViewComboBox.AddUniqueValueTo(wrapperDictionKey);
                    didLoad = true;
                }
                catch (ArgumentException argException) 
                {
                    XmlFileManager.WriteStringToLog("Failed to load file with exception:\n" + argException);
                }
                catch (IOException ioException)
                {
                    XmlFileManager.WriteStringToLog("Failed to load file with exception:\n" + ioException );
                }
            }
            return didLoad;
        }
    }
}
