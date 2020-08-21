using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace SevenDaysToDieModCreator.Views
{
    public partial class ModInfoDialogBox : Window
    {
        private ComboBox AllTagsComboBox { get; set; }
        private string StartingModTagSetting { get; set; }
        public string ResponseText
        {
            get {
                string newModTagSetting = this.AllTagsComboBox.Text;
                ModInfo newModIfo = new ModInfo(ModInfoNameBox.Text, ModInfoDescriptionBox.Text, ModInfoAuthorBox.Text, ModInfoVersionBox.Text);
                if (!this.StartingModTagSetting.Equals(newModTagSetting) && ChangeModTagCheckBox.IsChecked.Value) 
                {
                    XmlFileManager.RenameModDirectory(StartingModTagSetting, newModTagSetting);
                    XmlFileManager.ReplaceTagsInModFiles(StartingModTagSetting, newModTagSetting);
                }
                Properties.Settings.Default.ModTagSetting = newModTagSetting;
                Properties.Settings.Default.Save();
                return newModIfo.ToString();
            }
            set { ResponseText = value; }
        }
        public ModInfoDialogBox(string textBoxBody = "", string windowTitle = "")
        {
            InitializeComponent();
            this.StartingModTagSetting = Properties.Settings.Default.ModTagSetting;
            this.CurrentSelectedModLabel.Content = this.StartingModTagSetting;
            this.Title = windowTitle ?? "";
            string defaultText = "Thank you for downloading the 7 days to die Mod Creator! " +
                "Please input the mod info for your new mod or select a mod using the drop down box above.\n\n" +
                "It is worth noting that the current mod tag will be used as the name of the mod folder in the output location and the top tag for every file.\n" +
                "You can change this folder directly or use the \"Edit ModInfo\" menu item and change the current tag value.\n" +
                "IMPORTANT: If you lose work check the log.txt in the Output folder. " +
                "Any time you close the app or reset the object view, the xml that could be generated is output in that log. " +
                "If you like the mod don't forget to drop an endorsment or just leave me a comment!\n";
            if (!String.IsNullOrEmpty(textBoxBody)) defaultText = textBoxBody + "\n";
            LabelTextBlock.Text = "\n\n" +  defaultText;

            List<string> allCustomModsInPath = XmlFileManager.GetCustomModFoldersInOutput();
            this.AllTagsComboBox = allCustomModsInPath.CreateComboBoxFromList();
            this.AllTagsComboBox.IsEditable = true;
            this.AllTagsComboBox.LostFocus += AllTagsComboBox_LostFocus;
            this.AllTagsComboBox.DropDownClosed += AllTagsComboBox_DropDownClosed;
            this.AllTagsComboBox.Text = Properties.Settings.Default.ModTagSetting;
            this.AllTagsComboBox.FontSize = 22;
            FirstRowStackPanel.Children.Add(this.AllTagsComboBox);

            SetTooltips();
            SetTextBoxsWithExistingModInfo();
            Closing += new CancelEventHandler(ModInfoDialogBox_Closing);
        }

        private void ModInfoDialogBox_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = true;
        }

        private void AllTagsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox senderAsComboBox = sender as ComboBox;
            if (senderAsComboBox != null)
            {
                if (!String.IsNullOrEmpty(senderAsComboBox.Text.Trim()))
                {
                    this.CurrentSelectedModLabel.Content = senderAsComboBox.Text;
                    Properties.Settings.Default.ModTagSetting = senderAsComboBox.Text;
                    Properties.Settings.Default.Save();
                    this.StartingModTagSetting = senderAsComboBox.Text;
                    SetTextBoxsWithExistingModInfo();
                }
                else 
                {
                    ModInfoNameBox.Text = "";
                    ModInfoDescriptionBox.Text = "";
                    ModInfoAuthorBox.Text = "";
                    ModInfoVersionBox.Text = "";
                }
            }
        }
        private void AllTagsComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ComboBox senderAsComboBox = sender as ComboBox;
            if (senderAsComboBox != null) 
            {
                VerifyTagNameCorrectness(senderAsComboBox);
            }
        }
        private void SetTextBoxsWithExistingModInfo()
        {
            ModInfo currentModInfo = new ModInfo();
            if (currentModInfo.ModInfoExists)
            {
                ModInfoNameBox.Text = currentModInfo.Name;
                ModInfoDescriptionBox.Text = currentModInfo.Description;
                ModInfoAuthorBox.Text = currentModInfo.Author;
                ModInfoVersionBox.Text = currentModInfo.Version;
            }
        }
        private bool VerifyTagNameCorrectness(ComboBox allTagsComboBox, bool showMessageBox = true) 
        {
            bool isTagCorrect = true;
            try
            {
                XmlConvert.VerifyName(allTagsComboBox.Text);
            }
            catch (XmlException)
            {
                if(showMessageBox)MessageBox.Show("The Mod Tag format was incorrect, the value must follow xml tag naming rules!\n" +
                    "Typical errors are spaces in the name, or unusable special characters.\n\n" +
                    "You must correct this mistake before closing the window.",
                    "Format Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isTagCorrect = false;
            }
            catch (ArgumentNullException)
            {
                if (showMessageBox)MessageBox.Show("The Mod Tag format was incorrect, the Mod Tag cannot be empty! Please select or add a Mod Tag now.\n\n" +
                    "You must correct this mistake before closing the window.",
                    "Format Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isTagCorrect = false;
            }
            return isTagCorrect;
        }
        private void SetTooltips()
        {
            AllTagsComboBox.AddToolTip("Mod Tag\nThis is essentially the selected mod. This is also used as the directory and tag for the mod.\nThe mod selected here is what will be updated on save.");
            ModInfoNameBox.AddToolTip("Name\nThis is the name used in the ModInfo.xml file and the name of the mod seen in game.");
            ModInfoDescriptionBox.AddToolTip("Description\nThis is the description used in the ModInfo.xml file.");
            ModInfoAuthorBox.AddToolTip("Author\nThis is the Author for the mod, placed in the ModInfo.xml file.");
            ModInfoVersionBox.AddToolTip("Version\nThis is the current mod's version used in the ModInfo.xml file and the version seen in game.");
            ChangeModTagCheckBox.AddToolTip("By checking this box, you can MODIFY the value of the existing Mod Tag.\nIf left unchecked, any NEW values put in the input box above will be used to create a new mod.");
        }
        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (VerifyTagNameCorrectness(this.AllTagsComboBox)) 
            {
                string modInfoXmlOut = this.ResponseText;
                XmlFileManager.WriteStringToFile(XmlFileManager._ModDirectoryOutputPath, ModInfo.MOD_INFO_FILE_NAME, modInfoXmlOut);
                MessageBox.Show("Saved mod info for " + this.AllTagsComboBox.Text + ".", "Saving Mod info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
