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
        private const int FONT_SIZE = 20;
        private MainWindowViewController mainWindowViewController { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.mainWindowViewController = new MainWindowViewController(NewObjectFormsPanel, XmlOutputBox, RemoveChildContextMenu_Click);
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
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel, mainWindowViewController.leftNewObjectViewController.loadedListWrappers);
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
                    XmlXpathGenerator.GenerateXmlForSave(NewObjectFormsPanel, mainWindowViewController.leftNewObjectViewController.loadedListWrappers, true);
                    break;
            }
        }
        private void AddObjectView_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = AllLoadedNewObjectViewsComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = mainWindowViewController.loadedListWrappers.GetWrapperFromDictionary(selectedObject);
            mainWindowViewController.leftNewObjectViewController.CreateNewObjectFormTree(selectedWrapper);
            if (!mainWindowViewController.leftNewObjectViewController.loadedListWrappers.ContainsValue(selectedWrapper) && selectedWrapper != null) 
            {
                mainWindowViewController.leftNewObjectViewController.loadedListWrappers.Add(selectedWrapper.xmlFile.GetFileNameWithoutExtension(), selectedWrapper);
            }
        }
        private void AddNewTreeView_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = AllLoadedFilesComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = mainWindowViewController.loadedListWrappers.GetWrapperFromDictionary(selectedObject);
            TreeViewItem nextTreeView = mainWindowViewController.rightSearchTreeViewController.GetObjectTreeViewRecursive(selectedWrapper, MakeObjectATargetButton_Click);
            ViewSp.Children.Add(nextTreeView);
            if (!mainWindowViewController.rightSearchTreeViewController.loadedListWrappers.ContainsValue(selectedWrapper) && selectedWrapper != null)
            {
                mainWindowViewController.rightSearchTreeViewController.loadedListWrappers.Add(selectedWrapper.xmlFile.GetFileNameWithoutExtension(), selectedWrapper);
            }
        }
        private void MakeObjectATargetButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderAsButton = (Button)sender;
            XmlObjectsListWrapper wrapperToUse = this.mainWindowViewController.loadedListWrappers.GetValueOrDefault(senderAsButton.Name);
            if (!mainWindowViewController.leftNewObjectViewController.loadedListWrappers.ContainsValue(wrapperToUse) && wrapperToUse != null)
            {
                mainWindowViewController.leftNewObjectViewController.loadedListWrappers.Add(wrapperToUse.xmlFile.GetFileNameWithoutExtension(), wrapperToUse);
            }
            string[] contentSplit = senderAsButton.Content.ToString().Split(":");

            TreeViewItem newObjectFormTree = this.mainWindowViewController.leftNewObjectViewController.GenerateNewObjectFormTreeAddButton(wrapperToUse, contentSplit[0], true);
            //Set the name to the wrapper so we can find the wrapper later
            newObjectFormTree.Name = senderAsButton.Name.ToString();
            //set the xmlNode that was included with the object into the top tree view
            newObjectFormTree.Tag = senderAsButton.Tag;
            //The button should be in the form "TagName:AttribiuteNameVaue"
            if (contentSplit.Length > 1)
            {
                newObjectFormTree.Header = senderAsButton.Content.ToString();
            }
            //There is the edge case where the object did not have a name value to use
            else
            {
                newObjectFormTree.Header = ((Button)newObjectFormTree.Header).Content;
            }
            newObjectFormTree.AddOnHoverMessage("Using this form you can add new objects into the " + newObjectFormTree.Header + " object\n" +
                "For Example: You want to add an ingredient into a certain, existing, recipe.");
            newObjectFormTree.AddContextMenu(RemoveChildContextMenu_Click);
            NewObjectFormsPanel.Children.Add(newObjectFormTree);
            //OpenTargetDialogWindow(newObjectFormTree, wrapperToUse, senderAsButton.Content.ToString().Split(":")[1]);
        }
        private void RemoveChildContextMenu_Click(object sender, RoutedEventArgs e)
        {
            Control myObjectControl = (Control)((MenuItem)sender).Tag;
            NewObjectFormsPanel.Children.Remove(myObjectControl);
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
                    this.ResetNewObjectView();
                    break;
            }
        }
        private void ResetNewObjectView() 
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel, mainWindowViewController.leftNewObjectViewController.loadedListWrappers);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteXmlToLog(xmltoWrite, true);
            NewObjectFormsPanel.Children.Clear();
            mainWindowViewController.leftNewObjectViewController.loadedListWrappers.Clear();
            XmlXpathGenerator.GenerateXmlViewOutput(NewObjectFormsPanel, mainWindowViewController.leftNewObjectViewController.loadedListWrappers, XmlOutputBox);
        }
        private void ClearAllTreesView_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove all trees?", "Clear Tree View", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    ViewSp.Children.Clear();
                    mainWindowViewController.rightSearchTreeViewController.loadedListWrappers.Clear();
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