using ICSharpCode.AvalonEdit.Search;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for AddInsideTagetView.xaml
    /// </summary>
    public partial class DirectEditView : Window
    {
        private XmlObjectsListWrapper Wrapper { get; set; }
        public DirectEditView(XmlObjectsListWrapper wrapperToUse, string contentsForXmlOutputBox = null)
        {
            InitializeComponent();
            this.Wrapper = wrapperToUse;
            SaveXmlButton.AddOnHoverMessage("Click to save all changes and close the window");
            if (contentsForXmlOutputBox == null) UpdateViewComponents();
            else XmlOutputBox.Text = contentsForXmlOutputBox;
            SearchPanel.Install(XmlOutputBox);
            XmlOutputBox.PreviewMouseWheel += XmlOutputBox_PreviewMouseWheel;
            Closing += new CancelEventHandler(DirectEditView_Closing);
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
            string fileContents = XmlFileManager.ReadExistingFile(Wrapper.xmlFile.FileName);
            if (!String.IsNullOrEmpty(fileContents) && !fileContents.Equals(XmlOutputBox.Text)) 
            {
                MessageBoxResult result = MessageBox.Show(
                    "You have unsaved changes! Would you like to save them now?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        string xmlOut = XmlOutputBox.Text;
                        if (!String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToFile(XmlFileManager._ModOutputPath, Wrapper.xmlFile.FileName, xmlOut, true);
                        break;
                    case MessageBoxResult.Cancel:
                        DirectEditView directEditView =  new DirectEditView(this.Wrapper, XmlOutputBox.Text);
                        directEditView.Show();
                        break;
                }
            }
        }

        private void UpdateViewComponents()
        {
            XmlOutputBox.Text = XmlFileManager.ReadExistingFile(Wrapper.xmlFile.FileName);
        }

        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
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
                    if (!String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToFile(XmlFileManager._ModOutputPath, Wrapper.xmlFile.FileName, xmlOut);
                    this.Close();
                    break;
            }
        }
    }
}
