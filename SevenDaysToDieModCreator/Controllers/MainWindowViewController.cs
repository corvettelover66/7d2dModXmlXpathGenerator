using Microsoft.Win32;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Controllers
{
    class MainWindowViewController
    {
        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> loadedListWrappers { get; private set; }
        public TreeViewController rightSearchTreeViewController { get; private set; }
        public ObjectViewController leftNewObjectViewController { get; private set; }

        public MainWindowViewController(StackPanel NewObjectsFormsPanel, TextBox XmlOutBox, RoutedEventHandler removeChildContextMenu_Click) 
        {
            this.loadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.rightSearchTreeViewController = new TreeViewController();
            this.leftNewObjectViewController = new ObjectViewController(NewObjectsFormsPanel, XmlOutBox, removeChildContextMenu_Click);
        }

        public void LoadStartingDirectory(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox)
        {
            if (!Directory.Exists(XmlFileManager.LOCAL_DIR)) Directory.CreateDirectory(XmlFileManager.LOCAL_DIR);
            string[] files = Directory.GetFiles(XmlFileManager.LOCAL_DIR);
            foreach (string file in files) 
            {
                LoadFile(allLoadedFilesBox, allLoadedObjectsBox, file, true);
            }
        }

        public void LoadFilesViewControl(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox) 
        {
            List<string> unloadedFiles = new List<string>();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Length > 1) 
                {
                    foreach (string nextFileName in openFileDialog.FileNames) 
                    {
                        bool didLoad = LoadFile(allLoadedFilesBox, allLoadedObjectsBox, nextFileName, false);
                        if (!didLoad) unloadedFiles.Add(nextFileName);
                    }
                }
                else 
                {
                    bool didLoad = LoadFile(allLoadedFilesBox, allLoadedObjectsBox, openFileDialog.FileName, false);
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
                string messageBoxText = "Some files did not load correctly! \nFiles:\n" + allFilesString + "Only xml files can be loaded.";
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }
        private bool LoadFile(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox, string fileName, bool isStartup) 
        {
            bool didLoad = false;
            if (fileName.EndsWith(".xml"))
            {
                XmlObjectsListWrapper wrapper = new XmlObjectsListWrapper(new XmlFileObject(fileName));
                try
                {
                    if (!isStartup) File.Copy(fileName, XmlFileManager.LOCAL_DIR + wrapper.xmlFile.FileName);
                    this.loadedListWrappers.Add(wrapper.xmlFile.GetFileNameWithoutExtension(), wrapper);
                    allLoadedFilesBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    allLoadedObjectsBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    didLoad = true;
                }
                catch (IOException) 
                {
                    string messageBoxText = "You can't load another file with the same name!\nTry to change the name of the file.";
                    string caption = "Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
            }
            return didLoad;
        }
    }
}
