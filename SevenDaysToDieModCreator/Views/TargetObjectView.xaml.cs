using SevenDaysToDieModCreator.Controller;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for AddInsideTagetView.xaml
    /// </summary>
    public partial class TargetObjectView : Window
    {
        private TreeViewItem newObjectFormTree;
        private XmlObjectsListWrapper currentWrapper { get; set; }

        public RecipePerkTagetController myController { get; set; }

        public TargetObjectView(TreeViewItem newObjectFormTree, List<ComboBox> recentComboBoxList, XmlObjectsListWrapper wrapperToUse, string tagAttributeNameValue)
        {
            InitializeComponent();
            this.newObjectFormTree = newObjectFormTree;
            this.currentWrapper = wrapperToUse;
            GeneratedViewPanel.Tag = XmlXpathGenerator.MyCustomTagName;
            Button formTreeButton = (Button)this.newObjectFormTree.Header;
            Label generatedViewLabel = new Label { 
                Content = "Create new "+ formTreeButton.Content + " in  " + tagAttributeNameValue,
                FontSize = 25
            };
            GeneratedViewPanel.Children.Add(generatedViewLabel);
            GeneratedViewPanel.Children.Add(this.newObjectFormTree);
            ResetComboBoxLostFocus(recentComboBoxList);
            //GenerateXML
            //Display the contents into the XmlOutputBox
            //Create the Save File action
            GenerateViewComponents();
        }

        private void ResetComboBoxLostFocus(List<ComboBox> recentComboBoxList)
        {
            foreach (ComboBox nextBox in recentComboBoxList) 
            {
                nextBox.LostFocus += TargetObjectViewComboBox_LostFocus;
            }
        }

        private void TargetObjectViewComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Button formTreeButton = (Button)this.newObjectFormTree.Header;
           // this.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlByTarget(this.GeneratedViewPanel, this.currentWrapper, formTreeButton.Content+"") ;
        }

        private void GenerateViewComponents()
        {
        }

        //public string ResponseText
        //{
            //get { return InputTextBlock.Text; }
            //set { InputTextBlock.Text = value; }
        //}

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
