using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Controllers
{
    class ObjectViewController
    {
        private const int FONT_SIZE = 20;
        private List<ComboBox> recentComboBoxList { get; set; }

        private StackPanel newObjectFormView { get; set; }
        private RichTextBox xmlOutBlock { get; set; }
        private RoutedEventHandler RemoveChildContextMenu_Click { get; set; }


        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> loadedListWrappers { get; private set; }

        public ObjectViewController(StackPanel newObjectsFormPanel, RichTextBox xmlOutBlock, RoutedEventHandler RemoveChildContextMenu_Click)
        {
            this.loadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.recentComboBoxList = new List<ComboBox>();
            this.newObjectFormView = newObjectsFormPanel;
            this.xmlOutBlock = xmlOutBlock;
            this.RemoveChildContextMenu_Click = RemoveChildContextMenu_Click;
        }
        private void NewAttributesComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormView, this.loadedListWrappers, this.xmlOutBlock);
        }
        private void NewAttributesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormView, this.loadedListWrappers, this.xmlOutBlock);
        }
        private void AddNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderAsButton = (Button)sender;
            TreeViewItem sendersParent = (TreeViewItem)senderAsButton.Parent;
            XmlObjectsListWrapper xmlObjectsListWrapper = this.loadedListWrappers.GetWrapperFromDictionary(senderAsButton.Tag.ToString());
            TreeViewItem newObjectFormTreeView = this.GenerateNewObjectFormTreeAddButton(xmlObjectsListWrapper, senderAsButton.Content.ToString());
            newObjectFormTreeView.AddContextMenu(this.RemoveChildContextMenu_Click);

            newObjectFormTreeView.Name = senderAsButton.Tag.ToString();
            if (sendersParent.Parent.GetType() == typeof(StackPanel))
            {
                Label topTreeLabel = new Label { Content = xmlObjectsListWrapper.TopTagName, FontSize = FONT_SIZE };
                ((StackPanel)sendersParent.Parent).Children.Add(topTreeLabel);
                ((StackPanel)sendersParent.Parent).Children.Add(newObjectFormTreeView);
            }
            else ((TreeViewItem)sendersParent.Parent).Items.Add(newObjectFormTreeView);
        }
        public void CreateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper)
        {
            Label topTreeLabel = new Label { Content = xmlObjectListWrapper.TopTagName, FontSize = FONT_SIZE };
            topTreeLabel.AddContextMenu(this.RemoveChildContextMenu_Click);
            newObjectFormView.Children.Add(topTreeLabel);

            foreach (string topTag in xmlObjectListWrapper.allTopLevelTags)
            {
                TreeViewItem returnedTree = CreateNewObjectFormTree(xmlObjectListWrapper, topTag);
                returnedTree.Name = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension();
                if (xmlObjectListWrapper.TopTagName == StringConstants.PROGRESSION_TAG_NAME) returnedTree.Name = xmlObjectListWrapper.TopTagName;
                returnedTree.AddContextMenu(this.RemoveChildContextMenu_Click);
                newObjectFormView.Children.Add(returnedTree);
            }
        }
        public TreeViewItem CreateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string tagName)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem();
            newObjectFormTree.FontSize = FONT_SIZE;
            newObjectFormTree.IsExpanded = true;
            newObjectFormTree.AddOnHoverMessage("Here you can create new " + tagName + " tags");

            newObjectFormTree = GenerateNewObjectFormTree(xmlObjectListWrapper, newObjectFormTree, tagName);
            return newObjectFormTree;
        }
        public TreeViewItem GenerateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, TreeViewItem topTreeView, string currentTagName, bool doSkipFirstAttributeSet = false)
        {
            List<string> attributes = xmlObjectListWrapper.objectNameToAttributesMap.GetValueOrDefault(currentTagName);
            if (attributes != null && !doSkipFirstAttributeSet) topTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, currentTagName);

            if (topTreeView != null)
            {
                topTreeView.FontSize = FONT_SIZE;
                topTreeView.AddOnHoverMessage("Edit form for the " + currentTagName + " object");
                Button addNewObjectButton = new Button { Content = currentTagName, Tag = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension() };
                if (!currentTagName.Equals(StringConstants.RECIPE_TAG_NAME)) addNewObjectButton.AddOnHoverMessage("Click to add another " + currentTagName + " object");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                addNewObjectButton.Width = 250;
                topTreeView.Header = addNewObjectButton;
            }

            SetNextObjectCreateViewChildren(attributes, xmlObjectListWrapper, topTreeView, currentTagName);

            return topTreeView;
        }

        public TreeViewItem GenerateNewObjectFormTreeAddButton(XmlObjectsListWrapper xmlObjectListWrapper, string startingXmlTagName, bool doSkipFirstAttributes = false)
        {
            this.recentComboBoxList.Clear();
            if (startingXmlTagName.Length < 1) startingXmlTagName = xmlObjectListWrapper.FirstChildTagName;
            TreeViewItem newTopTree = new TreeViewItem();
            newTopTree.Header = startingXmlTagName;
            return GenerateNewObjectFormTree(xmlObjectListWrapper, newTopTree, startingXmlTagName, doSkipFirstAttributes);
        }
        private void SetNextObjectCreateViewChildren(List<string> attributes, XmlObjectsListWrapper xmlObjectListWrapper, TreeViewItem topTreeView, string tagName)
        {
            List<string> allChildren = xmlObjectListWrapper.objectNameToChildrenMap.GetValueOrDefault(tagName);
            if (allChildren != null)
            {
                foreach (string childName in allChildren)
                {
                    //Edge case for the property tag which can have a property tag
                    if (childName == tagName)
                    {
                        TreeViewItem innerPropertyTreeView = new TreeViewItem { Header = tagName, FontSize = FONT_SIZE };
                        if (attributes != null) innerPropertyTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, tagName);
                        innerPropertyTreeView.AddOnHoverMessage("Edit form for the " + tagName + " object");
                        Button addNewObjectButton = new Button { Content = tagName, Tag = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension() };
                        addNewObjectButton.AddOnHoverMessage("Click to add another " + tagName);
                        addNewObjectButton.Width = 250;
                        innerPropertyTreeView.Header = addNewObjectButton;
                        topTreeView.Items.Add(innerPropertyTreeView);
                    }
                    else
                    {
                        TreeViewItem newChildTopTree = new TreeViewItem();
                        TreeViewItem childrenTreeView = GenerateNewObjectFormTree(xmlObjectListWrapper, newChildTopTree, childName);
                        topTreeView.Items.Add(childrenTreeView);
                    }
                }
            }
        }
        private TreeViewItem SetNextObjectTreeViewAtrributes(List<string> attributes, XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName)
        {
            TreeViewItem newAttributesViewItem = new TreeViewItem();
            foreach (string nextAttribute in attributes)
            {
                Label newLabel = new Label()
                {
                    Content = nextAttribute
                };
                newAttributesViewItem.Items.Add(newLabel);

                List<string> attributeCommon = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(currentTagName).GetValueOrDefault(nextAttribute);
                ComboBox newAttributesComboBox = attributeCommon != null ? attributeCommon.CreateComboBoxList() : null;
                newAttributesComboBox.Width = 300;
                newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                newAttributesComboBox.Tag = nextAttribute;
                newAttributesComboBox.AddOnHoverMessage("Here you can set the value of the " + nextAttribute + " for the " + currentTagName);
                if (newAttributesComboBox == null) newAttributesViewItem.Items.Add(new ComboBox());
                else
                {
                    recentComboBoxList.Add(newAttributesComboBox);
                    newAttributesViewItem.Items.Add(newAttributesComboBox);
                }
            }
            return newAttributesViewItem;
        }
    }
}
