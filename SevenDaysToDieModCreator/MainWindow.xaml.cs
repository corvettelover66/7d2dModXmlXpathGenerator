using ICSharpCode.AvalonEdit.Search;
using Microsoft.WindowsAPICodePack.Dialogs;
using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

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
        public ComboBox LoadedModFilesSearchViewComboBox { get; private set; }
        public Button LoadedModFilesButton { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            Loaded += MyWindow_Loaded;
            Closing += new CancelEventHandler(MainWindow_Closing);
            SetupExceptionHandling();
        }
        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.MainWindowViewController = new MainWindowViewController(XmlOutputBox);
            this.XmlOutputBox.ShowLineNumbers = true;
            SearchPanel.Install(XmlOutputBox);
            SetPanels();
            SetCustomModViewElements();
            SetEvents();
            MainWindowViewController.LoadStartingDirectory(SearchTreeLoadedFilesComboBox, NewObjectViewLoadedFilesComboBox, CurrentModFilesCenterViewComboBox, LoadedModsSearchViewComboBox, CurrentGameFilesCenterViewComboBox);
            if (Properties.Settings.Default.ModTagSetting.Equals("ThisNeedsToBeSet")) CustomTagDialogPopUp("", "Input a new tag or select a tag from the list of existing tags", "Set Custom Tag!");
            this.LoadedModsCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFoldersInOutput());
            this.CurrentModFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(Properties.Settings.Default.ModTagSetting, Properties.Settings.Default.ModTagSetting + "_"));
            this.LoadedModsCenterViewComboBox.Text = Properties.Settings.Default.ModTagSetting;
            SetMainWindowToolTips();
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
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
        }
        private void SetMainWindowToolTips()
        {
            //Menu Item
            HelpMenuItem.AddToolTip("Click to see more information about the app");
            CheckAllSettingsMenuItem.AddToolTip("Click here to see all the User Settings");
            AutoMoveMenuItem.AddToolTip("Click here to change the Auto Move setting");
            ChangeCustomTagMenuItem.AddToolTip("Click here to add a new custom tag/mod");
            ChangeModGameDirectoryMenu.AddToolTip("Click here to change the directory for the Auto Move Function");
            MoveFileMenuItem.AddToolTip("Click here to move all mod files from the current selected mod directly to the User Set \"Auto Move\" Directory");
            SaveFileMenuItem.AddToolTip("Click here to save all generated XML into the appropriate files in the output location");
            LoadFileMenuItem.AddToolTip("Click to load an xml file or multiple xml files\nThis would typically be a game xml file such as recipes.xml");
            LoadModDirectoryMenuItem.AddToolTip("Click here to load a mod directory\nThis would typically be the main mod folder with the mod name not the Config directory within");
            ValidateXmlMenuItem.AddToolTip("Click here to validate all xml files for the selected tag/mod\nAny xml violations will be displayed");
            EditTagNameMenuItem.AddToolTip("Click here to change the name of the current tag/mod\nThis will also change the folder name in the Output/Mods/ dir");
            ChangeLogTimeStampMenuItem.AddToolTip("Click here to change the Timestamp setting");
            //LoadGameModDirectoryMenuItem.AddToolTip("Click here to load the 7 days to die \"Mods\" directory, to load all mods");
            //Buttons
            SaveXmlViewButton.AddToolTip("Click here to save all generated XML into the appropriate files in the output location");
            OpenModFileDirectEditViewButton.AddToolTip("Click to open a window to make direct edits to the selected mod file from the combo box above");
            AddObjectViewButton.AddToolTip("Click to add a new object creation view using the game file from above\nWARNING: This could take awhile");
            AddNewTreeViewButton.AddToolTip("Click to add a new searchable tree view using the game file from above" +
                "\nWith this tree you can perform any xpath command on any in game object" +
                " \nWARNING: This could take awhile");
            ClearAllObjectsViewButton.AddToolTip("Click to remove all objects from the view above\nThis action will also free up the used memory instantly.");
            ClearTreesViewButton.AddToolTip("Click to remove all trees from the view above\nThis action will also free up the used memory instantly.");
            LoadedModFilesButton.AddToolTip("Click to add a search tree for the mod file selected above");
            OpenGameFileDirectEditViewButton.AddToolTip("Click to open a window to make direct edits to the selected game file from the combo box above");
            //Combo Boxes
            LoadedModFilesSearchViewComboBox.AddToolTip("Mod file used to generate a search tree when clicking the button below");
            LoadedModsSearchViewComboBox.AddToolTip("Select a mod here to generate search trees for its files");
            LoadedModsCenterViewComboBox.AddToolTip("Using this combo box you can switch between loaded/created mods");
            CurrentModFilesCenterViewComboBox.AddToolTip("Select a file here to make direct edits\nThese are the currently selected mod's files");
            SearchTreeLoadedFilesComboBox.AddToolTip("The selected file here is used to create a search tree below\nAdd files to the list by loading an xml file from the game folder");
            NewObjectViewLoadedFilesComboBox.AddToolTip("The selected file here is used to create the new object view below\nAdd files to the list by loading an xml file from the game folder");
            CurrentGameFilesCenterViewComboBox.AddToolTip("The selected file here is the game file opened when you click the direct edit button just below");
        }
        private void SetPanels()
        {
            SearchTreeFormsPanelCache = new SearchViewCache(this.MainWindowViewController);
            NewObjectFormsPanel = new MyStackPanel(this.MainWindowViewController, SearchTreeFormsPanelCache);
            this.MainWindowViewController.LeftNewObjectViewController.NewObjectFormViewPanel = NewObjectFormsPanel;

            NewObjectScrollView.Content = NewObjectFormsPanel;
            SearchTreeFormsPanel = new MyStackPanel(this.MainWindowViewController, SearchTreeFormsPanelCache);
            this.MainWindowViewController.LeftNewObjectViewController.SearchTreeFormViewPanel = SearchTreeFormsPanel;
            SearchObjectScrollViewer.Content = SearchTreeFormsPanel;
        }
        private void SetEvents()
        {
            XmlOutputBox.GotKeyboardFocus += XmlOutputBoxGotKeyboardFocus_Handler;
            XmlOutputBox.PreviewMouseWheel += XmlOutputBox_PreviewMouseWheel;
            SearchTreeFormsPanel.PreviewMouseWheel += SearchObjectScrollViewer_PreviewMouseWheel;
            NewObjectFormsPanel.PreviewMouseWheel += NewObjectScrollViewer_PreviewMouseWheel;

            LoadedModsSearchViewComboBox.DropDownClosed += LoadedModsComboBox_DropDownClosed;
            LoadedModFilesButton.Click += LoadedModFilesButton_Click;
            CurrentModFilesCenterViewComboBox.DropDownClosed += CurrentModFilesCenterViewComboBox_DropDownClosed;
            LoadedModsCenterViewComboBox.DropDownClosed += LoadedModsCenterViewComboBox_DropDownClosed;
        }
        private void SetCustomModViewElements()
        {
            this.LoadedModFilesSearchViewComboBox = new ComboBox
            {
                Name = "LoadedModFilesComboBox",
                FontSize = 20,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };
            this.LoadedModFilesButton = new Button
            {
                Name = "LoadedModFilesButton",
                FontSize = 18,
                Content = "Add Mod Search Tree",
                HorizontalContentAlignment = HorizontalAlignment.Left
            };
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
                    Properties.Settings.Default.GameFolderModDirectory = dialog.FileName + "/";
                    Properties.Settings.Default.AutoMoveDecisionMade = true;
                    Properties.Settings.Default.Save();
                }
            }
        }
        private void ResetNewObjectView()
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
            NewObjectFormsPanel.Children.Clear();
            NewObjectFormsPanel.LoadedListWrappers.Clear();
            this.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(NewObjectFormsPanel);
        }
        private void SearchObjectScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            MyStackPanel myStackPanel = sender as MyStackPanel;
            //User rotated forward
            if (e.Delta > 0)
            {
                MainWindowViewController.LeftNewObjectViewController.IncreaseSearchTreeFontChange();
                MainWindowViewController.ModifySearchViewFont(1, myStackPanel.Children);
            }
            //User rotated backwards
            else if (e.Delta < 0)
            {
                MainWindowViewController.LeftNewObjectViewController.DecreaseSearchTreeFontChange() ;
                MainWindowViewController.ModifySearchViewFont(-1, myStackPanel.Children);
            }
        }
        private void NewObjectScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            MyStackPanel myStackPanel = sender as MyStackPanel;
            //User rotated forward
            if (e.Delta > 0)
            {
                MainWindowViewController.LeftNewObjectViewController.IncreaseObjectTreeFontChange();
                MainWindowViewController.ModifySearchViewFont(1, myStackPanel.Children);
            }
            //User rotated backwards
            else if (e.Delta < 0)
            {
                MainWindowViewController.LeftNewObjectViewController.DereasecObjectTreeFontChange();
                MainWindowViewController.ModifySearchViewFont(-1, myStackPanel.Children);
            }
        }
        private void CurrentModFilesCenterViewComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox senderAsBox = sender as ComboBox;
            string wrapperKey = senderAsBox.Text;
            if (!String.IsNullOrEmpty(wrapperKey))
            {
                XmlObjectsListWrapper xmlObjectsListWrapper = this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(wrapperKey);
                xmlObjectsListWrapper = xmlObjectsListWrapper == null
                    ? this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(Properties.Settings.Default.ModTagSetting + "_" + wrapperKey)
                    : xmlObjectsListWrapper;
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
                string parentPath = xmlObjectsListWrapper.xmlFile.ParentPath == null ? "" : xmlObjectsListWrapper.xmlFile.ParentPath;

                this.XmlOutputBox.Text = XmlFileManager.ReadExistingFile(Path.Combine(parentPath, xmlObjectsListWrapper.xmlFile.FileName), Properties.Settings.Default.ModTagSetting);
            }
        }
        private void LoadedModsCenterViewComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox senderAsBox = sender as ComboBox;
            string customModFile = senderAsBox.Text;
            if (!String.IsNullOrEmpty(customModFile))
            {
                Properties.Settings.Default.ModTagSetting = customModFile;
                Properties.Settings.Default.Save();

                this.CurrentModFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(customModFile, Properties.Settings.Default.ModTagSetting + "_"));
            }
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
                if (XmlOutputBox.FontSize != 10) XmlOutputBox.FontSize -= 1;
            }
        }
        private void LoadedModFilesButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.AddSearchTree(SearchTreeFormsPanel, LoadedModFilesSearchViewComboBox, false);
        }
        private void LoadedModsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox senderAsBox = sender as ComboBox;
            string modToLoad = senderAsBox.Text;
            if (!String.IsNullOrEmpty(modToLoad))
            {
                if (!this.SearchViewModSelectionPanel.Children.Contains(this.LoadedModFilesSearchViewComboBox)
                    && !this.SearchViewModSelectionPanel.Children.Contains(this.LoadedModFilesButton))
                {
                    this.SearchViewModSelectionPanel.Children.Add(this.LoadedModFilesSearchViewComboBox);
                    this.SearchViewModSelectionPanel.Children.Add(this.LoadedModFilesButton);
                }
                this.LoadedModFilesSearchViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(modToLoad, modToLoad + "_"));
                this.MainWindowViewController.LoadCustomTagWrappers(Properties.Settings.Default.ModTagSetting, this.CurrentModFilesCenterViewComboBox);
            }
            else
            {
                this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesSearchViewComboBox);
                this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesButton);
            }
        }
        private void MyBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                SearchTreeFormsPanelCache.LoadCache();
            });
        }
        private void XmlOutputBoxGotKeyboardFocus_Handler(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(NewObjectFormsPanel);
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
            //SaveExternalXaml();
        }
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.LoadFilesViewControl(SearchTreeLoadedFilesComboBox, NewObjectViewLoadedFilesComboBox, CurrentGameFilesCenterViewComboBox);
        }
        private void SaveXmlFile_Click(object sender, RoutedEventArgs e)
        {
            string autoMoveString = "";
            if (!Properties.Settings.Default.AutoMoveDecisionMade) CheckAutoMoveProperty("You can change this setting later using the Settings menu.");
            if (Properties.Settings.Default.AutoMoveMod) autoMoveString = "Auto move is active! This will also automatically move the files to \n" +
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
                    XmlXpathGenerator.SaveAllGeneratedXmlToPath(NewObjectFormsPanel, XmlFileManager._ModOutputPath, true);
                    if (Properties.Settings.Default.AutoMoveMod) XmlFileManager.CopyAllOutputFiles();
                    string currentLoadedMod = Properties.Settings.Default.ModTagSetting;
                    this.MainWindowViewController.LoadCustomTagWrappers(Properties.Settings.Default.ModTagSetting, this.CurrentModFilesCenterViewComboBox);
                    this.CurrentModFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(currentLoadedMod, Properties.Settings.Default.ModTagSetting + "_"));
                    break;
            }
        }
        private void CheckAutoMoveProperty(string appendMessage = "")
        {
            string currentStatus = Properties.Settings.Default.AutoMoveMod ? "Activated" : "Deactived";
            MessageBoxResult innerResult = MessageBox.Show("Would you like to change the status of the Auto Move feature?\n\n" +
                "Current status " + currentStatus + "\n\n" +
                "When activated, on saving, the application automatically moves all files to the Games Mod Folder chosen as well.\n" +
                appendMessage,
                "Auto Move Game Files",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
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
        private void CheckLogTimestampProperty() 
        {
            string currentStatus = Properties.Settings.Default.DoLogTimestampOnSave ? "Activated" : "Deactived";
            MessageBoxResult innerResult = MessageBox.Show("Would you like to change the status writing a timestamp on saving?\n\n" +
                "Current status " + currentStatus + "\n\n" +
                "When activated, on saving, the application will write a timestamp above the generated xml.\n",
                "Timestamp Setting",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (innerResult)
            {
                case MessageBoxResult.Yes:
                    Properties.Settings.Default.DoLogTimestampOnSave = !Properties.Settings.Default.DoLogTimestampOnSave;
                    Properties.Settings.Default.Save();
                    break;
            }
        }
        private void AddObjectView_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = NewObjectViewLoadedFilesComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(selectedObject);
            MainWindowViewController.LeftNewObjectViewController.CreateNewObjectFormTree(selectedWrapper, selectedObject);
        }
        private void AddNewSearchTreeView_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.AddSearchTree(SearchTreeFormsPanel, SearchTreeLoadedFilesComboBox);
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
        private void CustomTagDialogPopUp(string dialogText, string toolTipMessage, string windowTitle, bool isModNameEdit = false)
        {
            var dialog = new CustomDialogBox(true, dialogText, toolTipMessage, windowTitle);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (isModNameEdit) MainWindowViewController.ChangeCustomTagName(dialog, CurrentModFilesCenterViewComboBox, LoadedModsCenterViewComboBox, LoadedModsSearchViewComboBox);
                    else MainWindowViewController.SetNewCustomTag(dialog, CurrentModFilesCenterViewComboBox, LoadedModsCenterViewComboBox);
                }
                catch (XmlException)
                {
                    MessageBox.Show("The format was incorrect, the name must follow xml tag naming rules!\n" +
                        "Typical errors are spaces in the name, or unusable special characters.",
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
        private void OpenDirectEditModXmlViewButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = CurrentModFilesCenterViewComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            string[] objectSplit = selectedObject.Split("_");
            string standardWrapperKey = "";
            if(objectSplit.Length > 0)for (int i = 1; i < objectSplit.Length; i++) standardWrapperKey += objectSplit[i] + "_";
            standardWrapperKey = standardWrapperKey.Substring(0, standardWrapperKey.Length - 1);
            //Try to grab the default wrapper
            XmlObjectsListWrapper selectedWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(standardWrapperKey);
            //If it is null there is an issue with the game file 
            if (selectedWrapper == null)
            {
                //Try to load the mod wrapper
                selectedWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(selectedObject);
                //If it is still null there is an xml issue
                if (selectedWrapper == null) 
                {
                    MessageBox.Show(
                        "The was an error opening the selected file.\n\n" +
                        "There are a couple of possible issues:\n" +
                        "One issue can be invalid xml for the file you are trying to open. You can validate the xml using the \"File Menu Option and fix any errors in an external editor. " +
                        "Note, after fixing any errors in the xml be sure to run the xml validation in the file menu to refresh the loaded objects.\n\n" +
                        "Another way to fix this issue is load the game xml file for the file you are trying to load. " +
                        "For example, if you are trying to open the recipes xml file for a mod, load the game recipes xml file and this will work, even with invalid xml.",
                        "File Loading Error!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            DirectEditView directEdit = new DirectEditView(selectedWrapper, false, fileLocationPath: XmlFileManager._ModOutputPath);
            directEdit.Show();
        }
        private void OpenDirectEditGameXmlViewButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = CurrentGameFilesCenterViewComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            //Try to grab the default wrapper
            XmlObjectsListWrapper selectedWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(selectedObject);
            //If it is still null there is an issue with the file and the file has not been loaded.
            if (selectedWrapper == null)
            {
                selectedWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(Properties.Settings.Default.ModTagSetting + "_" + selectedObject);
                if (selectedWrapper == null)
                {
                    MessageBox.Show(
                        "The was an error opening the selected file.\n\n" +
                        "There are a couple of possible issues:\n" +
                        "One issue can be invalid xml for the file you are trying to open. You can validate the xml using the \"File Menu Option and fix any errors in an external editor. " +
                        "Note, after fixing any errors in the xml be sure to run the xml validation in the file menu to refresh the loaded objects.\n\n" +
                        "Another way to fix this issue is load the game xml file for the file you are trying to load. " +
                        "For example, if you are trying to open the recipes xml file for a mod, load the game recipes xml file and this will work, even with invalid xml.",
                        "File Loading Error!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            DirectEditView directEdit = new DirectEditView(selectedWrapper, true, fileLocationPath: XmlFileManager._LoadedFilesPath);
            directEdit.Show();
        }
        private void HelpMenu_Click(object sender, RoutedEventArgs e)
        {
            string readmeFileContents = XmlFileManager.GetFileContents(Directory.GetCurrentDirectory(), "README.txt");
            if (String.IsNullOrEmpty(readmeFileContents))
            {
                readmeFileContents = "For more information please read the README.txt.\n" +
                    "It can be found in the archive on downloading the app or on the Nexus. " +
                    "If you are still having trouble send me a message on the nexus with as much information as possible. " +
                    "Without enough information I cannot help you. ";
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
                    XmlFileManager._ModOutputPath + "\n" +
                " and replace the files at \n" +
                gameModDirectory + XmlFileManager._ModPath + "\n" +
                "Are you sure?",
                "Stage Generated XMLS",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    //User clicked move before setting the Game Mod folder
                    if (String.IsNullOrEmpty(gameModDirectory)) HandleMissingGameModDirectory();
                    //Make sure they set the Game mod directory
                    if (!String.IsNullOrEmpty(Properties.Settings.Default.GameFolderModDirectory)) XmlFileManager.CopyAllOutputFiles();
                    break;
            }
        }
        private void ChangeCustomTagMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomTagDialogPopUp("Please input/select your custom tag.\n\n" +
                "This will also be used in the File Generation Folder and the Game Output folder with the Auto Move feature.\n\n" +
                "It is worth noting that the current tag will generate a tag specific folder in the output location.\n" +
                "You can change this folder directly or use the \"Edit Tag/Mod Name\" menu item to change the name.\n" +
                " If you want to start a new mod create a new tag, or select an existing tag to continue work on those mod files.\n\n" +
                "Your current tag is: " + Properties.Settings.Default.ModTagSetting,
                "Input a new tag or select a tag from the list of existing tags", 
                "Create/Switch Custom Tag");
        }
        private void ChangeAutoMoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckAutoMoveProperty();
        }
        private void CheckAllSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string currentStatus = Properties.Settings.Default.AutoMoveMod ? "Activated" : "Deactived";
            string autoMoveStatus = "Auto Move Status: " + currentStatus + "\n\n";
            string currentStatusTimestampLog = Properties.Settings.Default.DoLogTimestampOnSave ? "Activated" : "Deactived";
            string logTimestampStatus = "Write Time Stamp On Save: " + currentStatusTimestampLog + "\n\n";
            string autoMoveDirectory = String.IsNullOrEmpty(Properties.Settings.Default.GameFolderModDirectory) ? "Auto Move Directory: Not Set\n\n" :
                "Auto Move Directory: " + Properties.Settings.Default.GameFolderModDirectory + "\n\n";
            string customTag = String.IsNullOrEmpty(Properties.Settings.Default.ModTagSetting) ? "Custom Tag: Not Set\n\n" :
                "Custom Tag: " + Properties.Settings.Default.ModTagSetting + "\n\n";

            string messageString = autoMoveStatus + logTimestampStatus + autoMoveDirectory + customTag;
            MessageBox.Show(messageString, "All Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void LoadModDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.LoadDirectoryViewControl(this.LoadedModsSearchViewComboBox, this.LoadedModsCenterViewComboBox, this.CurrentModFilesCenterViewComboBox);
        }
        private void ValidateXmlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string modOutputPath = XmlFileManager.Get_ModOutputPath(Properties.Settings.Default.ModTagSetting);
            string[] modOutputFiles = Directory.GetFiles(modOutputPath);
            StringBuilder builder = new StringBuilder();
            foreach (string modFile in modOutputFiles)
            {
                try
                {
                    new XmlFileObject(modFile);
                    builder.AppendLine("File: " + Path.GetFileName(modFile));
                    builder.AppendLine("Valid");
                    builder.AppendLine("");
                }
                catch (Exception exception)
                {
                    builder.Insert(0, "Invalid: " + exception.Message + "\n\n");
                    builder.Insert(0, "File: " + Path.GetFileName(modFile) + "\n");
                }
            }
            builder.Insert(0, "All files: \n\n");
            builder.Insert(0, "Xml Validation for mod " + Properties.Settings.Default.ModTagSetting + "\n\n");
            MessageBox.Show(builder.ToString(), "Xml Validation", MessageBoxButton.OK, MessageBoxImage.Information);
            this.MainWindowViewController.LoadCustomTagWrappers(Properties.Settings.Default.ModTagSetting, this.CurrentModFilesCenterViewComboBox);
        }
        private void EditTagNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomTagDialogPopUp("Please input a new name for the tag.\n\n" +
                "Your current tag to change is: " + Properties.Settings.Default.ModTagSetting, 
                "Add your new tag name here", 
                "Create New Tag",true);
        }

        private void ChangeLogTimeStampMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckLogTimestampProperty();
        }
        //private void LoadGameModDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindowViewController.LoadGameModDirectoryViewControl(this.LoadedModsSearchViewComboBox, this.LoadedModsCenterViewComboBox, this.CurrentModFilesCenterViewComboBox);
        //}
    }
}