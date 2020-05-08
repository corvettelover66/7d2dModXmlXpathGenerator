using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Search;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for AddInsideTagetView.xaml
    /// </summary>
    public partial class DirectEditView : Window
    {
        private XmlObjectsListWrapper Wrapper { get; set; }
        private string StartingFileContents { get; set; }
        private string StartingTitle { get; set; }
        private string FileLocationPath { get; set; }
        private bool IsGameFile { get; set; }
        private FoldingManager FoldingManager { get; }
        private XmlFoldingStrategy FoldingStrategy { get; }

        public DirectEditView(XmlObjectsListWrapper wrapperToUse, bool isGameFile, string title = null, string contentsForXmlOutputBox = null, string fileLocationPath = "")
        {
            InitializeComponent();
            this.Wrapper = wrapperToUse;
            this.IsGameFile = isGameFile;
            this.SaveXmlButton.AddToolTip("Click to save all changes");
            this.ReloadFileXmlButton.AddToolTip("Click here to reload the file from disk");
            this.CloseButton.AddToolTip("Click here to close the window");
            this.ValidateXmlButton.AddToolTip("Click here to validate the xml");
            this.FileLocationPath = fileLocationPath;
            if (contentsForXmlOutputBox == null)
            {
                string parentString = Wrapper.xmlFile.ParentPath == null
                    ? ""
                    : Wrapper.xmlFile.ParentPath;
                XmlOutputBox.Text = XmlFileManager.GetFileContents(Path.Combine(this.FileLocationPath, parentString), Wrapper.xmlFile.FileName);
            }
            else XmlOutputBox.Text = contentsForXmlOutputBox;

            this.StartingFileContents = XmlOutputBox.Text;
            SearchPanel.Install(XmlOutputBox);
            this.XmlOutputBox.ShowLineNumbers = true;
            this.XmlOutputBox.GotMouseCapture += XmlOutputBox_GotMouseCapture;
            this.XmlOutputBox.PreviewMouseWheel += XmlOutputBox_PreviewMouseWheel;
            this.XmlOutputBox.TextChanged += XmlOutputBox_TextChanged;
            this.XmlOutputBox.AddContextMenu(CollapseAllContextMenu_Clicked, 
                "Collapse All", 
                "Click here to collapse all nodes in the document.");
            this.XmlOutputBox.AddContextMenu(ExpandAllContextMenu_Clicked,
                "Expand All",
                "Click here to expand  all nodes in the document.");
            string labelContents = isGameFile
                ? "Game File: " + wrapperToUse.xmlFile.FileName + "\n"
                : "Mod: " + Properties.Settings.Default.ModTagSetting + "\n" + "File: " + wrapperToUse.xmlFile.FileName + "\n";
            this.StartingTitle = isGameFile 
                ? "Game File: " + wrapperToUse.xmlFile.FileName 
                : wrapperToUse.xmlFile.GetFileNameWithoutExtension() + " : " + Properties.Settings.Default.ModTagSetting; 
            this.Title = StartingTitle;

            ModNameLabel.Content = String.IsNullOrEmpty(title) ? labelContents : title;
            FoldingManager = FoldingManager.Install(this.XmlOutputBox.TextArea);
            FoldingStrategy = new XmlFoldingStrategy();
            FoldingStrategy.ShowAttributesWhenFolded = true;
            FoldingStrategy.UpdateFoldings(FoldingManager, this.XmlOutputBox.Document);
            Closing += new CancelEventHandler(DirectEditView_Closing);
        }

        private void CollapseAllContextMenu_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (FoldingSection nextSection in FoldingManager.AllFoldings)
            {
                nextSection.IsFolded = true;
            }
        }

        private void ExpandAllContextMenu_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (FoldingSection nextSection in FoldingManager.AllFoldings) 
            {
                nextSection.IsFolded = false;
            }
        }

        private void XmlOutputBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            HandleExternalFileChanges();
        }
        private void XmlOutputBox_TextChanged(object sender, EventArgs e)
        {
            string currentContents = GetCurrentFileContents();
            this.Title = XmlOutputBox.Text.Equals(currentContents)
                ? this.StartingTitle
                : "*" + this.StartingTitle;
            FoldingStrategy.UpdateFoldings(FoldingManager, this.XmlOutputBox.Document);
        }
        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        private void ReloadFileXmlButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewComponents();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
        private void DirectEditView_Closing(object sender, CancelEventArgs e)
        {
            if (IsFileChanged())
            {
                MessageBoxResult result = MessageBox.Show(
                    "You have unsaved changes! Would you like to save them now?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string parentString = Wrapper.xmlFile.ParentPath == null
                            ? ""
                            : Wrapper.xmlFile.ParentPath;
                        string xmlOut = XmlOutputBox.Text;
                        if (!String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToFile(Path.Combine(XmlFileManager._ModConfigOutputPath, parentString), Wrapper.xmlFile.FileName, xmlOut, true);
                        break;
                    case MessageBoxResult.Cancel:
                        DirectEditView directEditView = new DirectEditView(this.Wrapper, this.IsGameFile, this.StartingTitle, XmlOutputBox.Text, FileLocationPath);
                        directEditView.Show();
                        break;
                }
            }
        }
        private void HandleExternalFileChanges()
        {
            if (HasFileChangedExternally())
            {
                MessageBoxResult result = MessageBox.Show(
                    "The file has changed externally! Would you like to load those changes?",
                    "Load External Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string fileContents = GetCurrentFileContents();
                        if (!String.IsNullOrEmpty(fileContents))
                        {
                            XmlOutputBox.Text = fileContents;
                            StartingFileContents = fileContents;
                            this.Title = Wrapper.xmlFile.FileName;
                        }
                        break;
                }
            }
        }
        private bool HasFileChangedExternally()
        {
            string fileContents = GetCurrentFileContents();
            //                      Are there file contents                 do the fileContents equal what is in the XmlOutput
            bool isFileChanged = !String.IsNullOrEmpty(fileContents) && !fileContents.Equals(StartingFileContents);
            return isFileChanged;
        }
        private string GetCurrentFileContents()
        {
            string parentString = Wrapper.xmlFile.ParentPath == null
                ? ""
                : Wrapper.xmlFile.ParentPath;
            string fileContents = XmlFileManager.GetFileContents(Path.Combine(this.FileLocationPath, parentString), Wrapper.xmlFile.FileName);
            return fileContents;
        }
        private bool IsFileChanged()
        {
            string fileContents = GetCurrentFileContents();
            //                      Are there file contents                 do the fileContents equal what is in the XmlOutput
            bool isFileChanged = !String.IsNullOrEmpty(fileContents) && !fileContents.Equals(XmlOutputBox.Text);
            return isFileChanged;
        }
        private void SaveFile()
        {
            string xmlOut = XmlOutputBox.Text;
            bool isXmlValid = ValidateXml(xmlOut);
            if (!isXmlValid)
            {
                MessageBoxResult saveInvalidXmlDecision = MessageBox.Show(
                    "The xml is not valid! Would you like to save anyway?\n\n" +
                    " To see the error, run the xml validate function.",
                    "Invalid XML!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);
                switch (saveInvalidXmlDecision)
                {
                    case MessageBoxResult.No:
                        return;
                }
            }
            string parentPath = Wrapper.xmlFile.ParentPath == null ? "" : Wrapper.xmlFile.ParentPath;
            if (!String.IsNullOrEmpty(xmlOut))
            {
                XmlFileManager.WriteStringToFile(Path.Combine(this.FileLocationPath, parentPath), Wrapper.xmlFile.FileName, xmlOut);
                StartingFileContents = xmlOut;
                this.Title = this.StartingTitle;
            }
        }
        private void UpdateViewComponents()
        {
            if (IsFileChanged())
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to reload the file and overwrite any changes?",
                    "Reload File",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string fileContents = GetCurrentFileContents();
                        if (!String.IsNullOrEmpty(fileContents))
                        {
                            XmlOutputBox.Text = fileContents;
                            StartingFileContents = fileContents;
                            this.Title = Wrapper.xmlFile.FileName;
                        }
                        break;
                }
            }
        }
        private bool ValidateXml(string xmlToValidate, bool doShowValidationMessage = false) 
        {
            bool isValid = true;
            string fileValidationString;
            try
            {

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(XmlOutputBox.Text);
                fileValidationString = "File is valid Xml.";
            }
            catch (XmlException exception)
            {
                fileValidationString = "File is invalid Xml:\n\n" + exception.Message;
                isValid = false;
            }
            if(!String.IsNullOrEmpty(fileValidationString) && doShowValidationMessage)MessageBox.Show(fileValidationString, "Xml Validation", MessageBoxButton.OK, MessageBoxImage.Information);
            return isValid;
        }
        private void ValidateXmlButton_Click(object sender, RoutedEventArgs e)
        {
            ValidateXml(XmlOutputBox.Text, true);
        }
    }
}
