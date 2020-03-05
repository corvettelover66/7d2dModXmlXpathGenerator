using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;

namespace SevenDaysToDieModCreator.Controllers
{
    class TreeViewController
    {
        private const int FONT_SIZE = 20;

        //A dictionary for keeping track of TreeViews when using the search bar filter
        //Key file name for a wrapper
        //Value a List of TreeViews that were removed in the last search
        private Dictionary<string, List<TreeViewItem>> RemovedTreeViews { get; set; }

        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        public TreeViewController()
        {
            RemovedTreeViews = new Dictionary<string, List<TreeViewItem>>();
            LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
        }

        public TreeViewItem GetObjectTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper, RoutedEventHandler MakeObjectATargetButton_Click)
        {
            XmlNodeList allObjects = xmlObjectListWrapper.xmlFile.xmlDocument.GetElementsByTagName(xmlObjectListWrapper.TopTagName);
            //ViewSp.Children.Add(treeViewSearchBox);
            TreeViewItem topObjectsTreeView = new TreeViewItem()
            {
                Header = xmlObjectListWrapper.TopTagName,
                IsExpanded = true,
                FontSize = FONT_SIZE
            };
            topObjectsTreeView = SetObjectTreeView(topObjectsTreeView, allObjects, xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension(), xmlObjectListWrapper, MakeObjectATargetButton_Click);
            return topObjectsTreeView;
        }
        private TreeViewItem SetObjectTreeView(TreeViewItem topObjectsTreeView, XmlNodeList allObjects, string wrapperName, XmlObjectsListWrapper xmlObjectListWrapper, RoutedEventHandler MakeObjectATargetButton_Click)
        {
            foreach (XmlNode nextObjectNode in allObjects)
            {
                TreeViewItem nextTreeView = SetNextObject(nextObjectNode, wrapperName, xmlObjectListWrapper, MakeObjectATargetButton_Click);
                if (nextTreeView != null)
                {
                    topObjectsTreeView.Items.Add(nextTreeView);
                }
            }
            return topObjectsTreeView;
        }
        private TreeViewItem SetNextObject(XmlNode nextObjectNode, string wrapperKey, XmlObjectsListWrapper xmlObjectListWrapper, RoutedEventHandler MakeObjectATargetButton_Click)
        {
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem { FontSize = FONT_SIZE };
            if (nextObjectNode.Attributes != null)
            {
                nextObjectTreeViewItem = SetNextObjectTreeViewAtrributes(nextObjectNode.Attributes, nextObjectNode.Name);
            }
            if (nextObjectNode.HasChildNodes)
            {
                //If it is a first level tree we only need to set the first treenode with the search box (EX: recipes)
                if (nextObjectNode.Name.Equals(xmlObjectListWrapper.TopTagName) && xmlObjectListWrapper.allTopLevelTags.Count == 1)
                {
                    SetTreeViewSearchBoxHeader(nextObjectTreeViewItem, xmlObjectListWrapper, xmlObjectListWrapper.FirstChildTagName);
                }
                //If it is an edge case tree with multiple top levels we need to set it on all of those (EX: progressions)
                else if (xmlObjectListWrapper.allTopLevelTags.Count > 1 && xmlObjectListWrapper.allTopLevelTags.Contains(nextObjectNode.Name))
                {
                    List<string> children = xmlObjectListWrapper.objectNameToChildrenMap.GetValueOrDefault(nextObjectNode.Name);
                    if (children != null && children.Count > 0) SetTreeViewSearchBoxHeader(nextObjectTreeViewItem, xmlObjectListWrapper, children[0]);
                }
                //It is an internal node make it a button with the target action
                else
                {
                    string tagAttributeName = "";
                    if (!nextObjectNode.Name.Equals(StringConstants.PROGRESSION_TAG_NAME))
                    {
                        XmlNode parent = nextObjectNode.ParentNode;
                        if (xmlObjectListWrapper.TopTagName.Equals(StringConstants.PROGRESSION_TAG_NAME))
                        {
                            while (parent.Attributes.Count < 1 && (nextObjectTreeViewItem.Tag != null || !parent.Name.Equals(xmlObjectListWrapper.TopTagName)))
                            {
                                parent = parent.ParentNode;
                            }
                        }
                        string parentAttributeValue = parent.Attributes != null && parent.Attributes.Count > 0 ? parent.Attributes[0].Value : "";
                        tagAttributeName = nextObjectTreeViewItem.Tag == null ? parentAttributeValue : nextObjectTreeViewItem.Tag.ToString();
                    }
                    Button makeObjectATargetButton = new Button
                    {
                        Content = nextObjectNode.Name + ":" + tagAttributeName,
                        Name = wrapperKey,
                        Tag = nextObjectNode
                    };
                    makeObjectATargetButton.AddOnHoverMessage("Click here to make this a target for object insertion");
                    makeObjectATargetButton.Width = 250;
                    makeObjectATargetButton.Click += MakeObjectATargetButton_Click;
                    nextObjectTreeViewItem.Header = makeObjectATargetButton;
                }
                nextObjectTreeViewItem = SetObjectTreeView(nextObjectTreeViewItem, nextObjectNode.ChildNodes, wrapperKey, xmlObjectListWrapper, MakeObjectATargetButton_Click);
            }
            if (nextObjectNode.Name.Contains("#") || nextObjectNode == null) nextObjectTreeViewItem = null;
            return nextObjectTreeViewItem;
        }

        private void SetTreeViewSearchBoxHeader(TreeViewItem nextObjectTreeViewItem, XmlObjectsListWrapper xmlObjectListWrapper, string firstChildTagName)
        {
            Dictionary<string, List<string>> attributesDictionary = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(firstChildTagName);
            List<string> attributeCommon = null;
            if (attributesDictionary != null) attributeCommon = attributesDictionary.GetValueOrDefault("name");
            if (attributeCommon != null)
            {
                ComboBox topTreeSearchBox = attributeCommon.CreateComboBoxList();
                topTreeSearchBox.Width = 250;
                topTreeSearchBox.FontSize = 18;
                topTreeSearchBox.DropDownClosed += TopTreeSearchBox_LostKeyboard_Focus;
                topTreeSearchBox.PreviewKeyDown += TopTreeSearchBox_KeyDown_Focus;
                topTreeSearchBox.Tag = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension();
                nextObjectTreeViewItem.Header = topTreeSearchBox;
                nextObjectTreeViewItem.IsExpanded = true;
                topTreeSearchBox.AddOnHoverMessage("Here you can filter the list of " + firstChildTagName + " objects by typing in single keywords\n" +
                    "Examples:resource, resourceScrap");
            }
        }
        private void TopTreeSearchBox_KeyDown_Focus(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                SearchBoxUpdate(sender);
            }
        }
        private void TopTreeSearchBox_LostKeyboard_Focus(object sender, EventArgs e)
        {
            SearchBoxUpdate(sender);
        }
        private void SearchBoxUpdate(object sender)
        {
            ComboBox senderAsBox = (ComboBox)sender;
            XmlObjectsListWrapper currentWrapper = this.LoadedListWrappers.GetValueOrDefault(senderAsBox.Tag.ToString());
            List<TreeViewItem> removedTreeList = this.RemovedTreeViews.GetValueOrDefault(currentWrapper.xmlFile.GetFileNameWithoutExtension());
            if (removedTreeList == null)
            {
                removedTreeList = new List<TreeViewItem>();
                this.RemovedTreeViews.Add(currentWrapper.xmlFile.GetFileNameWithoutExtension(), removedTreeList);
                removedTreeList = this.RemovedTreeViews.GetValueOrDefault(currentWrapper.xmlFile.GetFileNameWithoutExtension());
            }
            TreeViewItem topTreeView = (TreeViewItem)senderAsBox.Parent;
            foreach (TreeViewItem removedTreeView in removedTreeList)
            {
                topTreeView.Items.Add(removedTreeView);
            }
            removedTreeList.Clear();
            string contents = senderAsBox.Text;
            List<TreeViewItem> children = GetChildren(topTreeView);
            List<TreeViewItem> treesToAdd = new List<TreeViewItem>();
            foreach (TreeViewItem nextTreeViewItem in children)
            {
                string treeIdentifier = nextTreeViewItem.Tag == null ?
                    nextTreeViewItem.Header.ToString().ToLower() :
                    nextTreeViewItem.Tag.ToString().ToLower();
                if (treeIdentifier.Contains(contents.ToLower()))
                {
                    treesToAdd.Add(nextTreeViewItem);
                }
                //if the object should be removed
                else
                {
                    removedTreeList.Add(nextTreeViewItem);
                }
            }
            topTreeView.Items.Clear();
            foreach (TreeViewItem treeViewItem in treesToAdd)
            {
                topTreeView.Items.Add(treeViewItem);
            }
        }
        private List<TreeViewItem> GetChildren(TreeViewItem parent)
        {
            List<TreeViewItem> children = new List<TreeViewItem>();

            if (parent != null)
            {
                foreach (var item in parent.Items)
                {
                    TreeViewItem child = item as TreeViewItem;

                    if (child == null)
                    {
                        child = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
                    }

                    if (child != null) children.Add(child);
                }
            }

            return children;
        }
        private TreeViewItem SetNextObjectTreeViewAtrributes(XmlAttributeCollection attributes, string objectNodeName)
        {
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem { FontSize = FONT_SIZE - 5 };

            foreach (XmlAttribute nextAttribute in attributes)
            {
                TextBox attributeBox = new TextBox
                {
                    Text = nextAttribute.Name + " = " + nextAttribute.Value,
                    IsReadOnly = true
                };
                if (nextAttribute.Name.Equals("name"))
                {
                    nextObjectTreeViewItem.Header = objectNodeName + " : " + nextAttribute.Value;
                    nextObjectTreeViewItem.Tag = nextAttribute.Value;
                }
                if (!nextAttribute.Name.Contains("#whitespace"))
                {
                    attributeBox.AddOnHoverMessage("You can click me to copy the value");
                    attributeBox.PreviewMouseDown += NewObjectTreeAttributeCombo_MouseDown;
                    nextObjectTreeViewItem.Items.Add(attributeBox);
                }
            }
            if (nextObjectTreeViewItem.Header == null) nextObjectTreeViewItem.Header = objectNodeName;
            return nextObjectTreeViewItem;
        }
        private void NewObjectTreeAttributeCombo_MouseDown(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((TextBox)sender).Text.Split("=")[1].Trim());
        }
    }
}
