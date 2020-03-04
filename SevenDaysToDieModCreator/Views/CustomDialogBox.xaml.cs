using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for CustomDialogBox.xaml
    /// </summary>
    public partial class CustomDialogBox : Window
    {
        private ComboBox AllTagsComboBox = new ComboBox();
        public CustomDialogBox(string textBoxBody = "")
        {
            InitializeComponent();
            string defaultText = "Thank you for downloading the 7 days to die Mod Creator! " +
                "Please input your custom tag now! This will be the top tag used in the xml output files. " +
                "It is also the name used in the Output folder when saving and mobing the mod. " +
                "IMPORTANT: If you lose work check the log.txt in the Output folder. " +
                "Any time you close the app or reset the object view, the xml that could be generated is output in that log. " +
                "If you like the mod don't forget to drop a like and tell your friends!";
            if (!String.IsNullOrEmpty(textBoxBody)) defaultText = textBoxBody;
            string[] allDirs = Directory.GetDirectories(XmlFileManager._filePath + "/Mods/", "*");
            List<string> justChildrenPathNames = new List<string>();
            foreach (string nextDir in allDirs) 
            {
                justChildrenPathNames.Add( Path.GetFileName(nextDir));
            }
            AllTagsComboBox = justChildrenPathNames.CreateComboBoxList();
            AllTagsComboBox.AddOnHoverMessage("Input a new tag or select a tag from the list of existing tags");
            AllTagsComboBox.IsEditable = true;
            CustomDialogPanel.Children.Add(AllTagsComboBox);
            LabelTextBlock.Text = defaultText;
        }

        public string ResponseText
        {
            get { return AllTagsComboBox.Text; }
            set { AllTagsComboBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            DialogResult = true;
        }
    }
}
