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
        public TreeViewController RightSearchTreeViewController { get; private set; }
        public ObjectViewController LeftNewObjectViewController { get; private set; }

        public MainWindowViewController(StackPanel NewObjectsFormsPanel, RichTextBox XmlOutBox, RoutedEventHandler removeChildContextMenu_Click) 
        {
            this.LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.RightSearchTreeViewController = new TreeViewController();
            this.LeftNewObjectViewController = new ObjectViewController(NewObjectsFormsPanel, XmlOutBox, removeChildContextMenu_Click);
        }

        public void LoadStartingDirectory(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox, ComboBox openDirectEditViewComboBox)
        {
            if (!Directory.Exists(XmlFileManager._LoadedFilesPath)) Directory.CreateDirectory(XmlFileManager._LoadedFilesPath);
            string[] files = Directory.GetFiles(XmlFileManager._LoadedFilesPath);
            foreach (string file in files) 
            {
                LoadFile(allLoadedFilesBox, allLoadedObjectsBox, openDirectEditViewComboBox, file);
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
        private bool LoadFile(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox, ComboBox openDirectEditViewComboBox, string fileName) 
        {
            bool didLoad = false;
            if (fileName.EndsWith(".xml"))
            {
                XmlObjectsListWrapper wrapper = new XmlObjectsListWrapper(new XmlFileObject(fileName));
                try
                {
                    if(!File.Exists(XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName))File.Copy(fileName, XmlFileManager._LoadedFilesPath + wrapper.xmlFile.FileName);
                    this.LoadedListWrappers.Add(wrapper.xmlFile.GetFileNameWithoutExtension(), wrapper);
                    allLoadedFilesBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    allLoadedObjectsBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    openDirectEditViewComboBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
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
