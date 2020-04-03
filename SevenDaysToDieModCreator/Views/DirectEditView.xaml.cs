using ICSharpCode.AvalonEdit.Search;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
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

        public DirectEditView(XmlObjectsListWrapper wrapperToUse, string title = null, string contentsForXmlOutputBox = null)
        {
            InitializeComponent();
            this.Wrapper = wrapperToUse;
            this.SaveXmlButton.AddToolTip("Click to save all changes");
            this.ReloadFileXmlButton.AddToolTip("Click here to reload the file from disk");
            this.CloseButton.AddToolTip("Click here to close the window");
            this.ValidateXmlButton.AddToolTip("Click here to validate the xml");

            if (contentsForXmlOutputBox == null) 
            {
                string parentString = Wrapper.xmlFile.ParentPath == null
                    ? ""
                    : Wrapper.xmlFile.ParentPath;
                XmlOutputBox.Text = XmlFileManager.ReadExistingFile(Path.Combine(parentString, Wrapper.xmlFile.FileName));
            }
            else XmlOutputBox.Text = contentsForXmlOutputBox;

            this.StartingFileContents = XmlOutputBox.Text;
            SearchPanel.Install(XmlOutputBox);
            this.XmlOutputBox.ShowLineNumbers = true;
            this.XmlOutputBox.GotMouseCapture += XmlOutputBox_GotMouseCapture;
            this.XmlOutputBox.PreviewMouseWheel += XmlOutputBox_PreviewMouseWheel;
            this.XmlOutputBox.TextChanged += XmlOutputBox_TextChanged;

            string labelContents = "Mod: " + Properties.Settings.Default.ModTagSetting + "\n" +
                "File: " + wrapperToUse.xmlFile.FileName + "\n";
            this.Title = wrapperToUse.xmlFile.FileName;
            ModNameLabel.Content = String.IsNullOrEmpty(title) ? labelContents : title;
            Closing += new CancelEventHandler(DirectEditView_Closing);
        }
        private void XmlOutputBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            HandleExternalFileChanges();
        }
        private void XmlOutputBox_TextChanged(object sender, EventArgs e)
        {
            string currentContents = GetCurrentFileContents();
            this.Title =  XmlOutputBox.Text.Equals(currentContents) ? Wrapper.xmlFile.FileName : "*" + Wrapper.xmlFile.FileName; 
        }
        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = SaveFile();
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
                        if (!String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToFile(Path.Combine(XmlFileManager._ModOutputPath, parentString), Wrapper.xmlFile.FileName, xmlOut, true);
                        break;
                    case MessageBoxResult.Cancel:
                        DirectEditView directEditView = new DirectEditView(this.Wrapper, ModNameLabel.Content.ToString(), XmlOutputBox.Text);
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
            string fileContents = XmlFileManager.ReadExistingFile(Path.Combine(parentString, Wrapper.xmlFile.FileName));
            return fileContents;
        }
        private bool IsFileChanged() 
        {
            string fileContents = GetCurrentFileContents();
            //                      Are there file contents                 do the fileContents equal what is in the XmlOutput
            bool isFileChanged = !String.IsNullOrEmpty(fileContents) && !fileContents.Equals(XmlOutputBox.Text);
            return isFileChanged;
        }
        private MessageBoxResult SaveFile() 
        {
            MessageBoxResult result = MessageBox.Show(
                "This will overwrite the file in the output location with your changes. Are you sure?",
                "Save Direct Changes",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.OK:
                    string xmlOut = XmlOutputBox.Text;
                    string parentPath = Wrapper.xmlFile.ParentPath == null ? "" : Wrapper.xmlFile.ParentPath;
                    if (!String.IsNullOrEmpty(xmlOut))
                    {
                        XmlFileManager.WriteStringToFile(Path.Combine(XmlFileManager._ModOutputPath, parentPath), Wrapper.xmlFile.FileName, xmlOut);
                        StartingFileContents = xmlOut;
                        this.Title = Wrapper.xmlFile.FileName;
                    }
                    break;
            }
            return result;
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
        private void ValidateXmlButton_Click(object sender, RoutedEventArgs e)
        {
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
            }
            MessageBox.Show(fileValidationString, "Xml Validation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
