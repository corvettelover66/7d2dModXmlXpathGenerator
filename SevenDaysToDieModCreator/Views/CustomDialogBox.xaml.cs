using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for CustomDialogBox.xaml
    /// </summary>
    public partial class CustomDialogBox : Window
    {
        private ComboBox AllTagsComboBox = new ComboBox();
        private Xceed.Wpf.Toolkit.ColorPicker picker { get; set; }
        private Color responseColor { get; set; }
        public CustomDialogBox(string textBoxBody = "")
        {
            InitializeComponent();
            string defaultText = "Thank you for downloading the 7 days to die Mod Creator! " +
                "Please input your custom tag now! You can think of this as the Mod Name. " +
                "This will be the top tag used in the xml output files. " +
                "It is also the name used in the Output folder when saving and moving the mod files.\n\n" +
                "IMPORTANT: If you lose work check the log.txt in the Output folder. " +
                "Any time you close the app or reset the object view, the xml that could be generated is output in that log. " +
                "If you like the mod don't forget to drop an endorsment and tell your friends!\n\n\n\n\n\n";
            if (!String.IsNullOrEmpty(textBoxBody)) defaultText = textBoxBody + "\n\n\n\n\n\n";
            if (!Directory.Exists(XmlFileManager._filePath + "/Mods/")) Directory.CreateDirectory(XmlFileManager._filePath + "/Mods/");
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
        public CustomDialogBox(Color xmlColorProperty)
        {
            InitializeComponent();
            CustomDialogPanel.VerticalAlignment = VerticalAlignment.Top;
            Xceed.Wpf.Toolkit.ColorPicker picker = new Xceed.Wpf.Toolkit.ColorPicker();
            picker.FontSize = 25;
            OkButton.FontSize = 25;
            LabelTextBlock.FontSize = 25;
            picker.SelectedColor = xmlColorProperty;
            this.responseColor = xmlColorProperty;
            SolidColorBrush xmlColorBrush = new SolidColorBrush(this.responseColor);
            LabelTextBlock.Foreground = xmlColorBrush;
            picker.DisplayColorAndName = true;
            picker.SelectedColorChanged += Picker_SelectedColorChanged;
            picker.AvailableColorsSortingMode = Xceed.Wpf.Toolkit.ColorSortingMode.HueSaturationBrightness;
            this.picker = picker;
            CustomDialogPanel.Children.Add(picker);
            LabelTextBlock.Text = "Please select a color. (Preview)";
        }

        private void Picker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (picker.SelectedColor.HasValue)
            {
                this.responseColor = picker.SelectedColor.Value;
                SolidColorBrush xmlColorBrush = new SolidColorBrush(this.responseColor);
                LabelTextBlock.Foreground = xmlColorBrush;
            } 
        }
        public Color ResponseColor
        {
            get { return this.responseColor; }
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
