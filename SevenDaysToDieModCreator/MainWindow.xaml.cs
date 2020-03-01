using Microsoft.Win32;
using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Xml;

namespace SevenDaysToDieModCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewController mainWindowViewController { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.mainWindowViewController = new MainWindowViewController(NewObjectFormsPanel, XmlOutputBox);
            Loaded += MyWindow_Loaded;
            Closing += new CancelEventHandler(MainWindow_Closing);
        }
        private void SetOnHoverMessages() 
        {
            SaveXmlViewButton.AddOnHoverMessage("This will save the XML into the appropriate files found at:\n" + XmlFileManager._ModPath+"\n");
            AddObjectViewButton.AddOnHoverMessage("Click to add a new object edit view using the object above\nWARNING: This could take awhile");
            AddNewTreeViewButton.AddOnHoverMessage("Click to add a new searchable tree view using the object above." +
                "\nWith this tree you can also insert items into the existing items." +
                " \nWARNING: This could take awhile");
            LoadFileViewButton.AddOnHoverMessage("Click to load an xml file or multiple xml files\nLoaded files will persist on application close");
            AllLoadedFilesComboBox.AddOnHoverMessage("The selected object here is used to create the tree view below\nAdd objects to the list by loading an xml file from the game folder");
            AllLoadedNewObjectViewsComboBox.AddOnHoverMessage("The selected object here is used to create the new object view below\nAdd objects to the list by loading an xml file from the game folder.");
            ClearTreesViewButton.AddOnHoverMessage("Click to remove all trees from the view above");
            ClearAllObjectsViewButton.AddOnHoverMessage("Click to remove all objects from the view above");
        }
        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Need to reload all events when loading state like this.
            //bool didLoad = LoadExternalXaml();
            bool didLoad = false;
            if (!didLoad) 
            {
                SetOnHoverMessages();
                mainWindowViewController.LoadStartingDirectory(AllLoadedFilesComboBox, AllLoadedNewObjectViewsComboBox);
                foreach (XmlObjectsListWrapper wrapper in mainWindowViewController.loadedListWrappers.Values)
                {
                    AllLoadedFilesComboBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    AllLoadedNewObjectViewsComboBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                }
            }
            if (XmlXpathGenerator.MyCustomTagName.Equals("ThisNeedsToBeSet"))
            {
                string fileContents = XmlFileManager.GetFileContents(XmlFileManager._filePath, XmlXpathGenerator.CustomTagFileName);
                if(fileContents == null) OutputTagDialogPopup();
                else XmlXpathGenerator.MyCustomTagName = fileContents;
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel, mainWindowViewController.listWrappersInObjectView);
            if(!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteXmlToLog(xmltoWrite, true);
            //SaveExternalXaml();
        }
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            mainWindowViewController.LoadFilesViewControl(AllLoadedFilesComboBox, AllLoadedNewObjectViewsComboBox);
        }
        private void SaveXmlFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "This will overwrite all files in the output location. Are you sure?", 
                "Save XML", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    mainWindowViewController.Save();
                    break;
            }
        }
        private void AddObjectView_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = AllLoadedNewObjectViewsComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = mainWindowViewController.loadedListWrappers.GetWrapperFromDictionary(selectedObject);
            mainWindowViewController.CreateNewObjectFormTree(NewObjectFormsPanel, selectedWrapper);
            if (!mainWindowViewController.listWrappersInObjectView.ContainsValue(selectedWrapper) && selectedWrapper != null) 
            {
                mainWindowViewController.listWrappersInObjectView.Add(selectedWrapper.xmlFile.GetFileNameWithoutExtension(), selectedWrapper);
            }
        }
        private void AddNewTreeView_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = AllLoadedFilesComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = mainWindowViewController.loadedListWrappers.GetWrapperFromDictionary(selectedObject);
            TreeViewItem nextTreeView = mainWindowViewController.GetObjectTreeViewRecursive(selectedWrapper);
            ViewSp.Children.Add(nextTreeView);
            if (!mainWindowViewController.listWrappersInTreeView.ContainsValue(selectedWrapper) && selectedWrapper != null)
            {
                mainWindowViewController.listWrappersInTreeView.Add(selectedWrapper.xmlFile.GetFileNameWithoutExtension(), selectedWrapper);
            }
        }
        private void ClearAllObjectView_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to clear the view?\nYou will lose any unsaved work!\nTo save you must click the \"Save All Xml\" button.", 
                "Clear Object View", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Exclamation);
            switch (result)
            {
                case MessageBoxResult.OK:
                    mainWindowViewController.ResetCreateView();
                    break;
            }
        }
        private void ClearAllTreesView_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove all trees?", "Clear Tree View", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    ViewSp.Children.Clear();
                    mainWindowViewController.listWrappersInTreeView.Clear();
                    break;
            }
        }
        public bool LoadExternalXaml()
        {
            bool didLoad = false;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Output\\state.xml");
            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(@path, FileMode.Open))
                {
                    this.Content = XamlReader.Load(stream);
                    didLoad = true;
                }
            }
            return didLoad;
        }

        public void SaveExternalXaml()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Output\\state.xml");
            using (FileStream stream = new FileStream(@path, FileMode.Create))
            {
                XamlWriter.Save(this.Content, stream);
            }
        }
        private void OutputTagDialogPopup()
        {
            //while (XmlXpathGenerator.MyCustomTagName == null)
            //{
                var dialog = new CustomDialogBox();
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        string name = XmlConvert.VerifyName(dialog.ResponseText);
                        XmlXpathGenerator.MyCustomTagName = name;
                        XmlFileManager.WriteStringToFile(XmlFileManager._filePath, XmlXpathGenerator.CustomTagFileName, name);
                    }
                    catch (XmlException)
                    {
                        MessageBox.Show("The format was incorrect, the name must follow xml naming rules!", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (ArgumentNullException)
                    {
                        MessageBox.Show("The format was incorrect, you must include something!", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            //}
        }
    }
}