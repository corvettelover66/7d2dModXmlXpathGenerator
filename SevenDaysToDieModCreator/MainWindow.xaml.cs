using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Search;
using System.Collections.Generic;
using System.Linq;

namespace SevenDaysToDieModCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewController MainWindowViewController { get; set; }
        private MyStackPanel NewObjectFormsPanel { get; set; }
        private MyStackPanel SearchTreeFormsPanel { get; set; }
        private SearchViewCache SearchTreeFormsPanelCache { get; set; }
        static BackgroundWorker myBackgroundWorker { get; } = new BackgroundWorker();
        public ComboBox LoadedModFilesComboBox { get; private set; }
        public Button LoadedModFilesButton { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            Loaded += MyWindow_Loaded;
            Closing += new CancelEventHandler(MainWindow_Closing);
            SetupExceptionHandling();
        }
        private void SetupExceptionHandling()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        //Global Error Processing happens in the APP view 
        //but here I want to catch it as well to save any possible generated xml to the log
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs exception)
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel, NewObjectFormsPanel.LoadedListWrappers);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
        }
        private void SetOnHoverMessages() 
        {
            //Menu Item
            HelpMenuItem.AddOnHoverMessage("Click to see more information about the app");
            CheckAllSettingsMenuItem.AddOnHoverMessage("Click here to see all the User Settings above");
            AutoMoveMenuItem.AddOnHoverMessage("Click here to change the Auto Move setting");
            ChangeCustomTagMenuItem.AddOnHoverMessage("Click here to change the custom tag");
            ChangeModGameDirectoryMenu.AddOnHoverMessage("Click here to change the output directory for the Auto Move Function");
            MoveFileMenuItem.AddOnHoverMessage("Click here to move all generated mod files directly to the User Set Game Directory");
            SaveFileMenuItem.AddOnHoverMessage("Click here to save all generated XML into the appropriate files in the output location");
            LoadFileMenuItem.AddOnHoverMessage("Click to load an xml file or multiple xml files\nThis would typically be a game xml file such as recipes.xml");
            //Buttons
            SaveXmlViewButton.AddOnHoverMessage("Click here to save all generated XML into the appropriate files in the output location");
            OpenDirectEditViewButton.AddOnHoverMessage("Click to open a window to make direct edits to the selected file from the combo box above");
            AddObjectViewButton.AddOnHoverMessage("Click to add a new object edit view using the object above\nWARNING: This could take awhile");
            AddNewTreeViewButton.AddOnHoverMessage("Click to add a new searchable tree view using the object above." +
                "\nWith this tree you can also insert items into the existing items." +
                " \nWARNING: This could take awhile");
            ClearAllObjectsViewButton.AddOnHoverMessage("Click to remove all objects from the view above");
            ClearTreesViewButton.AddOnHoverMessage("Click to remove all trees from the view above");
            LoadedModFilesButton.AddOnHoverMessage("");
            //Combo Boxes
            LoadedModFilesComboBox.AddOnHoverMessage("");
            LoadedModsComboBox.AddOnHoverMessage("");
            OpenDirectEditLoadedFilesComboBox.AddOnHoverMessage("The combo box to select a file for direct edits");
            SearchTreeLoadedFilesComboBox.AddOnHoverMessage("The selected object here is used to create the tree view below\nAdd objects to the list by loading an xml file from the game folder");
            NewObjectViewLoadedFilesComboBox.AddOnHoverMessage("The selected object here is used to create the new object view below\nAdd objects to the list by loading an xml file from the game folder.");
        }
        private void SetPanels() 
        {
            SearchTreeFormsPanelCache = new SearchViewCache(this.MainWindowViewController);
            NewObjectFormsPanel = new MyStackPanel(this.MainWindowViewController, SearchTreeFormsPanelCache);
            this.MainWindowViewController.LeftNewObjectViewController.NewObjectFormViewPanel = NewObjectFormsPanel;

            CreateLabelScroller.Content = NewObjectFormsPanel;
            SearchTreeFormsPanel = new MyStackPanel(this.MainWindowViewController, SearchTreeFormsPanelCache);
            this.MainWindowViewController.LeftNewObjectViewController.SearchTreeFormViewPanel = SearchTreeFormsPanel;
            SearchObjectScrollViewer.Content = SearchTreeFormsPanel;
        }
        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.MainWindowViewController = new MainWindowViewController(XmlOutputBox, RemoveChildContextMenu_Click);
            SetPanels();
            SetSearchViewCustomModViewElements();

            MainWindowViewController.LoadStartingDirectory(SearchTreeLoadedFilesComboBox, NewObjectViewLoadedFilesComboBox, OpenDirectEditLoadedFilesComboBox, LoadedModsComboBox);
            
            if (Properties.Settings.Default.CustomTagName.Equals("ThisNeedsToBeSet")) CustomTagDialogPopUp("");
            
            XmlOutputBox.GotKeyboardFocus += GotKeyboardFocus_Handler;
            XmlOutputBox.PreviewMouseWheel += XmlOutputBox_PreviewMouseWheel;
            SearchPanel.Install(XmlOutputBox);
            SetOnHoverMessages();
        }

        private void XmlOutputBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            //User rotated forward
            if (e.Delta > 0)
            {
                if (XmlOutputBox.FontSize != 200) XmlOutputBox.FontSize += 1;
            }
            //User rotated backwards
            else if (e.Delta < 0)
            {
                if(XmlOutputBox.FontSize != 10) XmlOutputBox.FontSize -= 1;
            }
        }

        private void SetSearchViewCustomModViewElements()
        {
            this.LoadedModsComboBox.DropDownClosed += LoadedModsComboBox_DropDownClosed;
            this.LoadedModFilesComboBox = new ComboBox
            {
                Name = "LoadedModFilesComboBox",
                FontSize = 20
            };
            this.LoadedModFilesButton = new Button
            {
                Name = "LoadedModFilesComboBox",
                FontSize = 18,
                Content = "Add Mod Search Tree"
            };
            this.LoadedModFilesButton.Click += LoadedModFilesButton_Click;
        }
        private void LoadedModFilesButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.AddSearchTree(SearchTreeFormsPanel,LoadedModFilesComboBox, false);
        }
        private void LoadedModsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            HandleCustomModSelected(sender);
        }
        private void HandleCustomModSelected(object sender)
        {
            ComboBox senderAsBox = sender as ComboBox;
            string modToLoad = senderAsBox.Text;
            if (!String.IsNullOrEmpty(modToLoad))
            {
                if (!this.SearchTreeFormsPanel.Children.Contains(this.LoadedModFilesComboBox)
                    && !this.SearchTreeFormsPanel.Children.Contains(this.LoadedModFilesButton)) 
                {
                    this.SearchTreeFormsPanel.Children.Insert(0, this.LoadedModFilesComboBox);
                    this.SearchTreeFormsPanel.Children.Insert(1, this.LoadedModFilesButton);
                }
                this.LoadedModFilesComboBox.SetComboBox(XmlFileManager.GetCustomModFiles(modToLoad));
            }
            else
            {
                this.SearchTreeFormsPanel.Children.Remove(this.LoadedModFilesComboBox);
                this.SearchTreeFormsPanel.Children.Remove(this.LoadedModFilesButton);
            }
        }
        private void MyBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                SearchTreeFormsPanelCache.LoadCache();
            });
        }
        private void GotKeyboardFocus_Handler(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(NewObjectFormsPanel, NewObjectFormsPanel.LoadedListWrappers);
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel, NewObjectFormsPanel.LoadedListWrappers);
            if(!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
            //SaveExternalXaml();
        }
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.LoadFilesViewControl(SearchTreeLoadedFilesComboBox, NewObjectViewLoadedFilesComboBox);
        }
        private void SaveXmlFile_Click(object sender, RoutedEventArgs e)
        {
            string autoMoveString = "";
            if (!Properties.Settings.Default.AutoMoveDecisionMade) CheckAutoMoveProperty("You can change this setting later using the Settings menu.");
            if (Properties.Settings.Default.AutoMoveMod) autoMoveString = "Auto move is active! This will also automatically move the files to \n"+
                                                                                    Properties.Settings.Default.GameFolderModDirectory;
            MessageBoxResult result = MessageBox.Show(
                "This will write all current generated xml to the appropriate files in the output location.\n\n" +
                "Are you sure?\n" +
                autoMoveString, 
                "Save Generated XML", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    XmlXpathGenerator.SaveAllGeneratedXmlToPath(NewObjectFormsPanel, NewObjectFormsPanel.LoadedListWrappers, XmlFileManager._ModOutputPath, true);
                    if(Properties.Settings.Default.AutoMoveMod) XmlFileManager.CopyAllOutputFiles();
                    break;
            }
        }
        private void CheckAutoMoveProperty(string appendMessage = "") 
        {
            string currentStatus = Properties.Settings.Default.AutoMoveMod ? "Activated" : "Deactived";
            MessageBoxResult innerResult = MessageBox.Show("Would you like to change the status of the Auto Move feature?\n\n" +
                "Current status " + currentStatus +"\n\n" +
                "When activated, on saving, the application automatically moves all files to the Games Mod Folder chosen.\n" +
                appendMessage,
                "Auto Move Game Files",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) ;
            switch (innerResult)
            {
                case MessageBoxResult.Yes:
                    Properties.Settings.Default.AutoMoveMod = !Properties.Settings.Default.AutoMoveMod;
                    break;
            }
            //If the AutoMoveMod was turned on and the mod directory is not set
            if (String.IsNullOrEmpty(Properties.Settings.Default.GameFolderModDirectory)
                && Properties.Settings.Default.AutoMoveMod)
            {
                HandleMissingGameModDirectory();
                if (String.IsNullOrEmpty(Properties.Settings.Default.GameFolderModDirectory))
                {
                    MessageBox.Show("You need to set the mod directory for this feature to work!");
                    HandleMissingGameModDirectory();
                }
            }
            if (!Properties.Settings.Default.AutoMoveDecisionMade) Properties.Settings.Default.AutoMoveDecisionMade = true;
            Properties.Settings.Default.Save();
        }
        private void HandleMissingGameModDirectory()
        {
            MessageBoxResult result = MessageBox.Show(
             "For the Auto Move function to work you must set the Game Folder Directory.\n\n" +
             "HELP: This is usually a \"Mods\" folder located directly in your 7 Days to Die game folder installation.\n\n" +
             "Example: PathToGame \"7 Days To Die\\Mods\\\" \n\n" +
             "If that folder does not exist please create it first. ",
             "Set Game Mod Folder Location",
             MessageBoxButton.OK,
             MessageBoxImage.Warning);
            switch (result) 
            {
                case MessageBoxResult.OK:
                    OpenGameFolderSelectDialog();
                    break;
            }

        }
        private void OpenGameFolderSelectDialog() 
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (!String.IsNullOrEmpty(dialog.FileName))
                {
                    MessageBox.Show("Success!");
                    Properties.Settings.Default.GameFolderModDirectory = dialog.FileName +"/";
                    Properties.Settings.Default.AutoMoveDecisionMade = true;
                    Properties.Settings.Default.Save();
                }
            }
        }
        private void AddObjectView_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = NewObjectViewLoadedFilesComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = MainWindowViewController.LoadedListWrappers.GetWrapperFromDictionary(selectedObject);
            MainWindowViewController.LeftNewObjectViewController.CreateNewObjectFormTree(selectedWrapper);
        }
        private void AddNewSearchTreeView_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.AddSearchTree(SearchTreeFormsPanel, SearchTreeLoadedFilesComboBox);
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
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel, MainWindowViewController.LeftNewObjectViewController.loadedListWrappers);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
            NewObjectFormsPanel.Children.Clear();
            MainWindowViewController.LeftNewObjectViewController.loadedListWrappers.Clear();
            this.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(NewObjectFormsPanel, MainWindowViewController.LeftNewObjectViewController.loadedListWrappers);
        }
        private void ClearAllTreesView_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove all trees?", "Clear Tree View", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    SearchTreeFormsPanel.Children.Clear();
                    break;
            }
        }
        private void CustomTagDialogPopUp(string dialogText)
        {
            var dialog = new CustomDialogBox(true, dialogText);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string name = XmlConvert.VerifyName(dialog.ResponseText);
                    Properties.Settings.Default.CustomTagName = name;
                    Properties.Settings.Default.Save();
                    string modOutputPath = XmlFileManager.Get_ModOutputPath(name);
                    string[] modOutputFiles = Directory.GetFiles(modOutputPath);
                    List<string> allFilesToAdd = modOutputFiles.Select(c => { c = Path.GetFileName(c); return c; }).ToList();
                    this.OpenDirectEditLoadedFilesComboBox.SetComboBox(allFilesToAdd);
                }
                catch (XmlException)
                {
                    MessageBox.Show("The format was incorrect, the name must follow xml naming rules!\n" +
                        "Typical errors are spaces in the name, or unusable special characters.\n" +
                        "If you do not care about the custom tag and are having problems switching to a mod read below:\n\n" +
                        "The tag names in the combo box use the folders in the application Output/Mods/ directory.\n" +
                        "Changing the folder name to a valid CustomTag name in that location will fix this issue.",
                        "Format Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                catch (ArgumentNullException)
                {
                    MessageBox.Show("The format was incorrect, the tag cannot be empty! Please open the settings menu and set your tag.",
                        "Format Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        private void OpenDirectEditViewButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = OpenDirectEditLoadedFilesComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = MainWindowViewController.LoadedListWrappers.GetWrapperFromDictionary(selectedObject);
            DirectEditView directEdit = new DirectEditView(selectedWrapper);
            directEdit.Show();
        }
        private void HelpMenu_Click(object sender, RoutedEventArgs e)
        {
            string readmeFileContents = XmlFileManager.GetFileContents(Directory.GetCurrentDirectory()+"/", "README.txt");
            if (String.IsNullOrEmpty(readmeFileContents))
            {
                readmeFileContents = "For more information please read the README.txt. \n" +
                    "It can be found in the archive on downloading the app or on the Nexus." +
                    "If you are still having trouble send me a message on the nexus with as much information as possible." +
                    "Without enough information I cannot help you.";
                MessageBox.Show(readmeFileContents, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else 
            {
                XmlOutputBox.Text = readmeFileContents;
            }
        }
        private void ChangeModGameDirectoryMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenGameFolderSelectDialog();
        }
        private void MoveFileMenuHeader_Click(object sender, RoutedEventArgs e)
        {
            string gameModDirectory = Properties.Settings.Default.GameFolderModDirectory;
            MessageBoxResult result = MessageBox.Show(
                "This will copy all local generated xmls files at " +
                    XmlFileManager._ModOutputPath + "\n"+
                " and replace the files at \n" +
                gameModDirectory + XmlFileManager._ModPath +"\n"+
                "Are you sure?",
                "Stage Generated XMLS",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    //User clicked move before setting the Game Mod folder
                    if (String.IsNullOrEmpty(gameModDirectory))HandleMissingGameModDirectory();
                    //Make sure they set the Game mod directory
                    if(!String.IsNullOrEmpty(Properties.Settings.Default.GameFolderModDirectory))XmlFileManager.CopyAllOutputFiles();
                    break;
            }
        }
        private void ChangeCustomTagMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomTagDialogPopUp("Please input/select your custom tag.\n\n" +
                "This will also be used in the File Generation Folder and the Game Output folder with the Auto Move feature.\n\n" +
                "It is worth noting that the current tag will generate a tag specific folder in the output location.\n" +
                "This can be used to effectively \"Switch\" between mods you are working on." +
                " If you want to start a new mod create a new tag, or select an existing tag to continue work on those mod files.\n\n"+
                "Your current tag is: " + Properties.Settings.Default.CustomTagName);
        }
        private void ChangeAutoMoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckAutoMoveProperty();
        }
        private void CheckAllSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string currentStatus = Properties.Settings.Default.AutoMoveMod ? "Activated" : "Deactived";
            string autoMoveStatus = "Auto Move Status: " + currentStatus +"\n\n";
            string autoMoveDirectory = String.IsNullOrEmpty(Properties.Settings.Default.GameFolderModDirectory) ? "Auto Move Directory: Not Set\n\n" :
                "Auto Move Directory: " + Properties.Settings.Default.GameFolderModDirectory + "\n\n";
            string customTag = String.IsNullOrEmpty(Properties.Settings.Default.CustomTagName) ? "Custom Tag: Not Set\n\n"  :
                "Custom Tag: " + Properties.Settings.Default.CustomTagName + "\n\n";

            string messageString = autoMoveStatus + autoMoveDirectory + customTag;
            MessageBox.Show(messageString, "All Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}