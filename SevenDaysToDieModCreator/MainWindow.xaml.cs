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
using System.Windows.Media;

namespace SevenDaysToDieModCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowFileController MainWindowFileController { get; set; }
        private MainWindowViewController MainWindowViewController { get; set; }

        //A dictionary for finding XmlListWrappers by filename
        //Key file name without .xml i.e. recipes, progressions, items
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        private MyStackPanel NewObjectFormsPanel { get; set; }
        private MyStackPanel SearchTreeFormsPanel { get; set; }
        public ComboBox LoadedModFilesSearchViewComboBox { get; private set; }
        public Button LoadedModFilesButton { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            Loaded += MyWindow_Loaded;
            Closing += new CancelEventHandler(MainWindow_Closing);
            SetupExceptionHandling();
            this.XmlOutputBox.ShowLineNumbers = true;
            SearchPanel.Install(XmlOutputBox);
        }
        private void SetBackgroundFromSetting(bool removeExistingResources = false)
        {
            this.Background = BackgroundColorController.GetBackgroundColor();
            this.XmlOutputBox.Background = BackgroundColorController.GetBackgroundColor();
            if (removeExistingResources) RemoveExistingColorFromComboBoxResource();
            SetBackgroundForComboBoxes();
        }

        private void RemoveExistingColorFromComboBoxResource()
        {
            LoadedModFilesSearchViewComboBox.Resources.Clear();
            LoadedModsSearchViewComboBox.Resources.Clear();
            LoadedModsCenterViewComboBox.Resources.Clear();
            CurrentModFilesCenterViewComboBox.Resources.Clear();
            SearchTreeLoadedFilesComboBox.Resources.Clear();
            NewObjectViewLoadedFilesComboBox.Resources.Clear();
            CurrentGameFilesCenterViewComboBox.Resources.Clear();
        }

        private void SetBackgroundForComboBoxes()
        {
            LoadedModFilesSearchViewComboBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            LoadedModsSearchViewComboBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            LoadedModsCenterViewComboBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            CurrentModFilesCenterViewComboBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            SearchTreeLoadedFilesComboBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            NewObjectViewLoadedFilesComboBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            CurrentGameFilesCenterViewComboBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string openingText = "Thank you for downloading the 7 days to die Mod Creator! \n" +
                "If you have any issues please report them in the comments on the nexus page.\n\n" +
                "IMPORTANT: If you lose work check the log.txt in the Output folder.\n" +
                "Any time you close the app or clear the object view, the xml that could be generated is output in that log.\n\n" +
                "If you like the application don't forget to leave me a comment or better yet drop an endorsment!\n" +
                "Good luck with your mods!";
            this.XmlOutputBox.Text = openingText;
            this.LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.MainWindowFileController = new MainWindowFileController(this.LoadedListWrappers);
            this.MainWindowViewController = new MainWindowViewController();
            MainWindowViewController.XmlOutputBox = this.XmlOutputBox;

            MainWindowViewController.LoadedListWrappers = this.LoadedListWrappers;
            SetPanels();
            SetCustomModViewElements();
            SetEvents();
            MainWindowViewController.IncludeAllModsCheckBox = this.IncludeAllModsInBoxesCheckBox;
            this.IncludeCommentsCheckBox.IsChecked = Properties.Settings.Default.IncludeCommentsSearchTreeTooltip;
            this.IncludeChildrenInOnHoverCheckBox.IsChecked = Properties.Settings.Default.IncludeChildrenSearchTreeTooltip;
            this.IncludeAllModsInBoxesCheckBox.IsChecked = Properties.Settings.Default.IncludeAllModsObjectTreeAttributes;
            this.IgnoreAllAttributesCheckBox.IsChecked = Properties.Settings.Default.IgnoreAllAttributesCheckbox;
            this.IgnoreAllAttributesCheckBox.Click += IgnoreAllAttributesCheckBox_Click;

            MainWindowFileController.LoadStartingDirectory(SearchTreeLoadedFilesComboBox, NewObjectViewLoadedFilesComboBox, CurrentModFilesCenterViewComboBox, LoadedModsSearchViewComboBox, CurrentGameFilesCenterViewComboBox);
            this.LoadedModsCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFoldersInOutput());
            this.CurrentModFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(Properties.Settings.Default.ModTagSetting, Properties.Settings.Default.ModTagSetting + "_"));
            this.LoadedModsCenterViewComboBox.Text = Properties.Settings.Default.ModTagSetting;
            SetMainWindowToolTips();
            SetBackgroundFromSetting();
        }
        private void IgnoreAllAttributesCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IgnoreAllAttributesCheckbox = IgnoreAllAttributesCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormsPanel);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
            Properties.Settings.Default.IncludeChildrenSearchTreeTooltip = IncludeChildrenInOnHoverCheckBox.IsChecked.Value;
            Properties.Settings.Default.IncludeCommentsSearchTreeTooltip = IncludeCommentsCheckBox.IsChecked.Value;
            Properties.Settings.Default.IncludeAllModsObjectTreeAttributes = IncludeAllModsInBoxesCheckBox.IsChecked.Value;
            Properties.Settings.Default.IgnoreAllAttributesCheckbox = IgnoreAllAttributesCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
            //SaveExternalXaml();
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
            ChangeModGameDirectoryMenu.AddToolTip("Click here to change the directory for the Auto Move Function");
            MoveFileMenuItem.AddToolTip("Click here to move all mod files from the current selected mod directly to the User Set \"Auto Move\" Directory");
            SaveFileMenuItem.AddToolTip("Click here to save all generated XML into the appropriate files in the output location");
            LoadFileMenuItem.AddToolTip("Click to load an xml file or multiple xml files\nThis would typically be a game xml file such as recipes.xml");
            LoadModDirectoryMenuItem.AddToolTip("Click here to load a mod directory\nThis would typically be the main mod folder with the mod name not the Config directory within");
            ValidateXmlMenuItem.AddToolTip("Click here to validate all xml files for the selected tag/mod\nAny xml violations will be displayed");
            CreateEditModInfoMenuItem.AddToolTip("Click here to create a new mod or edit the Mod Info for an existing mod");
            ChangeLogTimeStampMenuItem.AddToolTip("Click here to change the Timestamp setting");
            OpenLocalizationMenuItem.AddToolTip("Click here to open the localization window to manage Localization for your mods");
            NormalThemeMenuItem.AddToolTip("Click here to change the background color to the normal theme");
            MediumThemeMenuItem.AddToolTip("Click here to change the background color to the medium theme");
            DarkThemeMenuItem.AddToolTip("Click here to change the background color to the dark theme");
            //LoadGameModDirectoryMenuItem.AddToolTip("Click here to load the 7 days to die \"Mods\" directory, to load all mods");
            //Buttons
            SaveXmlViewButton.AddToolTip("Click here to save all generated XML into the appropriate files in the output location");
            Stage_AllViewButton.AddToolTip("Click here to move all mod files from the current selected mod directly to the User Set \"Auto Move\" Directory");
            OpenModFileDirectEditViewButton.AddToolTip("Click to open a window to make direct edits to the selected mod file from the combo box to the left");
            AddObjectViewButton.AddToolTip("Click to add a new object creation view using the game file from above\nWARNING: This could take awhile");
            AddNewTreeViewButton.AddToolTip("Click to add a new searchable tree view using the game file from above" +
                "\nWith this tree you can perform any xpath command on any in game object" +
                " \nWARNING: This could take awhile");
            ClearAllObjectsViewButton.AddToolTip("Click to remove all objects from the view above");
            ClearTreesViewButton.AddToolTip("Click to remove all trees from the view above");
            LoadedModFilesButton.AddToolTip("Click to add a search tree for the mod file selected above");
            OpenGameFileDirectEditViewButton.AddToolTip("Click to open a window to make direct edits to the selected game file from the combo box to the left");
            DeleteModFileDirectEditViewButton.AddToolTip("Click to delete the file from the combo box to the left for the current mod");
            DeleteModButton.AddToolTip("Click to get more information on deleting the mod.\n" +
                "Does NOT delete the mod folder!");
            //Combo Boxes
            LoadedModFilesSearchViewComboBox.AddToolTip("Mod file used to generate a search tree when clicking the button below");
            LoadedModsSearchViewComboBox.AddToolTip("Select a mod here to generate search trees for its files");
            LoadedModsCenterViewComboBox.AddToolTip("Using this combo box you can switch between loaded/created mods");
            CurrentModFilesCenterViewComboBox.AddToolTip("Select a file here to make direct edits\nThese are the currently selected mod's files");
            SearchTreeLoadedFilesComboBox.AddToolTip("The selected file here is used to create a search tree below\nAdd files to the list by loading an xml file from the game folder");
            NewObjectViewLoadedFilesComboBox.AddToolTip("The selected file here is used to create the new object view below\nAdd files to the list by loading an xml file from the game folder");
            CurrentGameFilesCenterViewComboBox.AddToolTip("Select a file here to make direct edits\nThese are the game files");
            //Check Box
            IncludeChildrenInOnHoverCheckBox.AddToolTip("Keeping this checked will include the children\nin the on hover messages for new search trees");
            IncludeCommentsCheckBox.AddToolTip("Keeping this checked will include comments in newly generated search trees");
            IncludeAllModsInBoxesCheckBox.AddToolTip("Keeping this checked will use common attributes from all mods\nLeaving this unchecked will use the common attributes from only the selected mod file");
            IgnoreAllAttributesCheckBox.AddToolTip("Keeping this checked will flag the \"copy\" function to Hide Unused Attributes for all children automatically.");
        }
        private void SetPanels()
        {
            NewObjectFormsPanel = new MyStackPanel(this.LoadedListWrappers);
            MainWindowViewController.NewObjectFormViewPanel = NewObjectFormsPanel;

            NewObjectScrollView.Content = NewObjectFormsPanel;
            SearchTreeFormsPanel = new MyStackPanel(this.LoadedListWrappers);
            MainWindowViewController.SearchTreeFormViewPanel = SearchTreeFormsPanel;
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
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            this.LoadedModFilesButton = new Button
            {
                Name = "LoadedModFilesButton",
                FontSize = 18,
                Content = "Add Mod Search Tree",
                HorizontalContentAlignment = HorizontalAlignment.Center
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
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                IsFolderPicker = true
            };
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
        private void SearchObjectScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            MyStackPanel myStackPanel = sender as MyStackPanel;
            //User rotated forward
            if (e.Delta > 0)
            {
                MainWindowViewController.IncreaseSearchTreeFontChange();
                MainWindowViewController.ModifySearchViewFont(1, myStackPanel.Children);
            }
            //User rotated backwards
            else if (e.Delta < 0)
            {
                MainWindowViewController.DecreaseSearchTreeFontChange();
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
                MainWindowViewController.IncreaseObjectTreeFontChange();
                MainWindowViewController.ModifySearchViewFont(1, myStackPanel.Children);
            }
            //User rotated backwards
            else if (e.Delta < 0)
            {
                MainWindowViewController.DereasecObjectTreeFontChange();
                MainWindowViewController.ModifySearchViewFont(-1, myStackPanel.Children);
            }
        }
        private void CurrentModFilesCenterViewComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox senderAsBox = sender as ComboBox;
            string wrapperKey = senderAsBox.Text;
            if (!String.IsNullOrEmpty(wrapperKey))
            {
                XmlObjectsListWrapper xmlObjectsListWrapper = this.MainWindowFileController.LoadedListWrappers.GetValueOrDefault(wrapperKey);
                xmlObjectsListWrapper ??= this.MainWindowFileController.LoadedListWrappers.GetValueOrDefault(Properties.Settings.Default.ModTagSetting + "_" + wrapperKey);
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
                string parentPath = xmlObjectsListWrapper.XmlFile.ParentPath ?? "";

                this.XmlOutputBox.Text = XmlFileManager.ReadExistingFile(Path.Combine(parentPath, xmlObjectsListWrapper.XmlFile.FileName));
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
            this.MainWindowFileController.LoadCustomTagWrappers(Properties.Settings.Default.ModTagSetting, this.CurrentModFilesCenterViewComboBox);
            this.MainWindowViewController.AddSearchTree(SearchTreeFormsPanel, LoadedModFilesSearchViewComboBox, isGameFileTree: false, includeChildrenInOnHover: IncludeChildrenInOnHoverCheckBox.IsChecked.Value, includeComments: IncludeCommentsCheckBox.IsChecked.Value);
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
                this.MainWindowFileController.LoadCustomTagWrappers(Properties.Settings.Default.ModTagSetting, this.CurrentModFilesCenterViewComboBox);
            }
            else
            {
                this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesSearchViewComboBox);
                this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesButton);
            }
        }
        private void XmlOutputBoxGotKeyboardFocus_Handler(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(NewObjectFormsPanel);
        }
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            MainWindowFileController.LoadFilesViewControl(SearchTreeLoadedFilesComboBox, NewObjectViewLoadedFilesComboBox, CurrentGameFilesCenterViewComboBox);
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
                    XmlXpathGenerator.SaveAllGeneratedXmlToPath(NewObjectFormsPanel, XmlFileManager.ModConfigOutputPath, true);
                    if (Properties.Settings.Default.AutoMoveMod) XmlFileManager.CopyAllOutputFiles();
                    this.MainWindowFileController.LoadCustomTagWrappers(Properties.Settings.Default.ModTagSetting, this.CurrentModFilesCenterViewComboBox);
                    this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesSearchViewComboBox);
                    this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesButton);
                    break;
            }
        }
        private void AddObjectView_Click(object sender, RoutedEventArgs e)
        {
            this.MainWindowViewController.AddObjectTree(this.NewObjectViewLoadedFilesComboBox.Text);
        }
        private void AddNewSearchTreeView_Click(object sender, RoutedEventArgs e)
        {
            this.MainWindowViewController.AddSearchTree(SearchTreeFormsPanel, SearchTreeLoadedFilesComboBox, includeChildrenInOnHover: IncludeChildrenInOnHoverCheckBox.IsChecked.Value, includeComments: IncludeCommentsCheckBox.IsChecked.Value);
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
                    MainWindowViewController.ResetNewObjectView();
                    break;
            }
        }
        private void ClearAllTreesView_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove all trees?", "Clear Tree View", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    this.SearchTreeFormsPanel.Children.Clear();
                    break;
            }
        }
        private void ModInfoDialogPopUp(string dialogText, string windowTitle)
        {
            var dialog = new ModInfoDialogBox(dialogText, windowTitle);

            if (dialog.ShowDialog() == true)
            {
                MainWindowFileController.FinishModInfoSave(CurrentModFilesCenterViewComboBox, LoadedModsCenterViewComboBox, LoadedModsSearchViewComboBox);
            }
        }
        private void OpenDirectEditModXmlViewButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = CurrentModFilesCenterViewComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            string defaultWrapperKey = selectedObject.Replace(Properties.Settings.Default.ModTagSetting + "_", "");
            //Try to grab the default wrapper
            XmlObjectsListWrapper selectedWrapper = MainWindowFileController.LoadedListWrappers.GetValueOrDefault(defaultWrapperKey);

            if (selectedWrapper == null)
            {
                //Try to load the wrapper from the selected object.
                selectedWrapper = MainWindowFileController.LoadedListWrappers.GetValueOrDefault(selectedObject);
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
                else 
                {
                    MessageBox.Show("Missing game file "+ defaultWrapperKey  +".xml. In order to use all direct edit functions, you must load this file and reopen the file in a new direct edit window.",
                        "Missing Game File", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
            }
            DirectEditView directEdit = new DirectEditView(selectedWrapper, false, fileLocationPath: XmlFileManager.ModConfigOutputPath);
            directEdit.Closed += DirectEdit_Closed;
            directEdit.Show();
        }
        private void DirectEdit_Closed(object sender, EventArgs e)
        {
            string currentLoadedMod = Properties.Settings.Default.ModTagSetting;
            this.MainWindowFileController.LoadCustomTagWrappers(currentLoadedMod, this.CurrentModFilesCenterViewComboBox);
            this.CurrentModFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(currentLoadedMod, currentLoadedMod + "_"));
        }
        private void OpenDirectEditGameXmlViewButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedObject = CurrentGameFilesCenterViewComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            //Try to grab the default wrapper
            XmlObjectsListWrapper selectedWrapper = MainWindowFileController.LoadedListWrappers.GetValueOrDefault(selectedObject);
            //If it is still null there is an issue with the file and the file has not been loaded.
            if (selectedWrapper == null)
            {
                selectedWrapper = MainWindowFileController.LoadedListWrappers.GetValueOrDefault(Properties.Settings.Default.ModTagSetting + "_" + selectedObject);
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
            DirectEditView directEdit = new DirectEditView(selectedWrapper, true, fileLocationPath: XmlFileManager.LoadedFilesPath);
            directEdit.Closed += DirectEdit_Closed;
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
                    XmlFileManager.ModConfigOutputPath + "\n" +
                " and replace the files at \n" +
                Path.Combine(gameModDirectory, Properties.Settings.Default.ModTagSetting ) + "\n" +
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
        private void ChangeAutoMoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckAutoMoveProperty();
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
        private void CheckAllSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string currentStatus = Properties.Settings.Default.AutoMoveMod ? "Activated" : "Deactived";
            string autoMoveStatus = "Auto Move Status: " + currentStatus + "\n\n";
            string currentStatusTimestampLog = Properties.Settings.Default.DoLogTimestampOnSave ? "Activated" : "Deactived";
            string logTimestampStatus = "Write Time Stamp On Save: " + currentStatusTimestampLog + "\n\n";
            string autoMoveDirectory = String.IsNullOrEmpty(Properties.Settings.Default.GameFolderModDirectory) ? "Auto Move Directory: Not Set\n\n" :
                "Auto Move Directory: " + Properties.Settings.Default.GameFolderModDirectory + "\n\n";
            string customTag = String.IsNullOrEmpty(Properties.Settings.Default.ModTagSetting) ? "Mod Tag: Not Set\n\n" :
                "Mod Tag: " + Properties.Settings.Default.ModTagSetting + "\n\n";

            string messageString = autoMoveStatus + logTimestampStatus + autoMoveDirectory + customTag;
            MessageBox.Show(messageString, "All Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void LoadModDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindowFileController.LoadDirectoryViewControl(this.LoadedModsSearchViewComboBox, this.LoadedModsCenterViewComboBox, this.CurrentModFilesCenterViewComboBox);
            this.LoadedModFilesSearchViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(Properties.Settings.Default.ModTagSetting, Properties.Settings.Default.ModTagSetting + "_"));
        }
        private void ValidateXmlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string modOutputPath = XmlFileManager.Get_ModOutputPath(Properties.Settings.Default.ModTagSetting);
            string[] modOutputFiles = Directory.GetFiles(modOutputPath);
            StringBuilder builder = new StringBuilder();
            foreach (string modFile in modOutputFiles)
            {
                string isInvalid = XmlXpathGenerator.ValidateXml(XmlFileManager.ReadExistingFile(modFile));
                //The xml is valid
                if (isInvalid == null)
                {
                    builder.AppendLine("File: " + Path.GetFileName(modFile));
                    builder.AppendLine("Valid");
                }
                else 
                {
                    builder.Insert(0, isInvalid);
                    builder.Insert(0, "File: " + Path.GetFileName(modFile) + "\n");
                }
            }
            builder.Insert(0, "All files: \n");
            builder.Insert(0, "Xml Validation for mod " + Properties.Settings.Default.ModTagSetting + "\n\n");
            //Remove the trailing newline
            builder.Remove(builder.Length - 2, 2);
            MessageBox.Show(builder.ToString(), "Xml Validation", MessageBoxButton.OK, MessageBoxImage.Information);
            this.MainWindowFileController.LoadCustomTagWrappers(Properties.Settings.Default.ModTagSetting, this.CurrentModFilesCenterViewComboBox);
        }
        private void CreateEditModInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ModInfoDialogPopUp("Please input all of the necessary fields for the ModInfo.xml file here. " +
                "The ModInfo.xml file is required for every mod and should contain relevant information for the mod.\n\n" +
                "You can edit existing mods' ModInfo.xml or create new mods using this window.", 
                "Create/Edit Mod's ModInfo");
        }
        private void ChangeLogTimeStampMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckLogTimestampProperty();
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
        private void DeleteModFileXmlViewButton_Click(object sender, RoutedEventArgs e)
        {
            string currentModName = Properties.Settings.Default.ModTagSetting;
            bool didDeleteFile = MainWindowFileController.DeleteModFile(currentModName, this.CurrentModFilesCenterViewComboBox.Text);
            if (didDeleteFile) 
            {
                this.MainWindowFileController.LoadCustomTagWrappers(currentModName, this.CurrentModFilesCenterViewComboBox);
                this.CurrentModFilesCenterViewComboBox.SetComboBox(XmlFileManager.GetCustomModFilesInOutput(currentModName, currentModName + "_"));
                //Reset the search view mod UI components as the files have changed.
                this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesSearchViewComboBox);
                this.SearchViewModSelectionPanel.Children.Remove(this.LoadedModFilesButton);
            }
        }
        private void OpenLocalizationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.MainWindowFileController.HandleLocalizationFile();
        }

        private void NormalThemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsDarkModeActive = false;
            Properties.Settings.Default.SettingIsMediumModeActive = false;
            Properties.Settings.Default.Save();
            SetBackgroundFromSetting(true);
        }

        private void MediumThemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsDarkModeActive = false;
            Properties.Settings.Default.SettingIsMediumModeActive = true;
            Properties.Settings.Default.Save();
            SetBackgroundFromSetting(true);
        }

        private void DarkThemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsDarkModeActive = true;
            Properties.Settings.Default.SettingIsMediumModeActive = false;
            Properties.Settings.Default.Save();
            SetBackgroundFromSetting(true);
        }

        private void DeleteModButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "Mod folders cannot be deleted directly in the app. To delete this mod you must restart the app, and delete the folder:\n\n" 
                + XmlFileManager.Get_ModDirectoryOutputPath(LoadedModsCenterViewComboBox.Text);
            string title = "Delete Mod Help";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //private void LoadGameModDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindowViewController.LoadGameModDirectoryViewControl(this.LoadedModsSearchViewComboBox, this.LoadedModsCenterViewComboBox, this.CurrentModFilesCenterViewComboBox);
        //}
    }
}