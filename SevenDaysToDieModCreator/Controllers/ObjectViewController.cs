using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace SevenDaysToDieModCreator.Controllers
{
    class ObjectViewController
    {
        private const int FONT_SIZE = 20;

        public StackPanel newObjectFormView { get; set; }
        private ICSharpCode.AvalonEdit.TextEditor xmlOutBlock { get; set; }
        private RoutedEventHandler RemoveChildContextMenu_Click { get; set; }


        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> loadedListWrappers { get; private set; }

        public ObjectViewController(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox, RoutedEventHandler RemoveChildContextMenu_Click)
        {
            this.loadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.xmlOutBlock = xmlOutputBox;
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
            newObjectFormTreeView.AddContextMenu(this.RemoveChildContextMenu_Click, "Remove Object From View");

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
            topTreeLabel.AddContextMenu(this.RemoveChildContextMenu_Click, "Remove Object From View");
            newObjectFormView.Children.Add(topTreeLabel);

            foreach (string topTag in xmlObjectListWrapper.allTopLevelTags)
            {
                TreeViewItem returnedTree = CreateNewObjectFormTree(xmlObjectListWrapper, topTag);
                returnedTree.Name = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension();
                if (xmlObjectListWrapper.TopTagName == StringConstants.PROGRESSION_TAG_NAME) returnedTree.Name = xmlObjectListWrapper.TopTagName;
                returnedTree.AddContextMenu(this.RemoveChildContextMenu_Click, "Remove Object From View");
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
                    newAttributesViewItem.Items.Add(newAttributesComboBox);
                }
            }
            return newAttributesViewItem;
        }
        public TreeViewItem GetObjectTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper)
        {
            XmlNodeList allObjects = xmlObjectListWrapper.xmlFile.xmlDocument.GetElementsByTagName(xmlObjectListWrapper.TopTagName);
            TreeViewItem topObjectsTreeView = new TreeViewItem()
            {
                Header = xmlObjectListWrapper.TopTagName,
                IsExpanded = true,
                FontSize = FONT_SIZE
            };
            topObjectsTreeView = SetObjectTreeView(topObjectsTreeView, allObjects, xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension(), xmlObjectListWrapper);
            return topObjectsTreeView;
        }
        private TreeViewItem SetObjectTreeView(TreeViewItem topObjectsTreeView, XmlNodeList allObjects, string wrapperName, XmlObjectsListWrapper xmlObjectListWrapper)
        {
            foreach (XmlNode nextObjectNode in allObjects)
            {
                TreeViewItem nextTreeView = SetNextObject(nextObjectNode, wrapperName, xmlObjectListWrapper);

                if (nextTreeView != null)
                {
                    AddContextMenuToTreeView(nextTreeView);
                    topObjectsTreeView.Items.Add(nextTreeView);
                }
            }
            return topObjectsTreeView;
        }

        private void AddContextMenuToTreeView(TreeViewItem nextTreeView)
        {
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Append to target");
        }

        private void AppendToContextMenu_ClickFunction(object sender, RoutedEventArgs e)
        {
            //The code below was what was previously there for the same function,
            // now the difference is that the sender will be the context menu button. 
            // Need to use that to find the correct information to generate an object tree
            throw new NotImplementedException("Need to finish the function");


            //Finish writing the 
            //Button senderAsButton = (Button)sender;
            //string[] contentSplit = senderAsButton.Content.ToString().Split(":");
            //XmlObjectsListWrapper wrapperToUse = this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(senderAsButton.Name);
            //if (!MainWindowViewController.LeftNewObjectViewController.loadedListWrappers.ContainsValue(wrapperToUse) && wrapperToUse != null)
            //{
            //    MainWindowViewController.LeftNewObjectViewController.loadedListWrappers.Add(wrapperToUse.xmlFile.GetFileNameWithoutExtension(), wrapperToUse);
            //}
            //TreeViewItem newObjectFormTree = this.MainWindowViewController.LeftNewObjectViewController.GenerateNewObjectFormTreeAddButton(wrapperToUse, contentSplit[0], true);
            ////Set the name to the wrapper so we can find the wrapper later
            //newObjectFormTree.Name = senderAsButton.Name.ToString();
            ////set the xmlNode that was included with the object into the top tree view
            //newObjectFormTree.Tag = senderAsButton.Tag;
            ////The button should be in the form "TagName:AttribiuteNameVaue"
            //if (contentSplit.Length > 1)
            //{
            //    newObjectFormTree.Header = senderAsButton.Content.ToString();
            //}
            ////There is the edge case where the object did not have a name value to use
            //else
            //{
            //    newObjectFormTree.Header = ((Button)newObjectFormTree.Header).Content;
            //}
            //newObjectFormTree.AddOnHoverMessage("Using this form you can add new objects into the " + newObjectFormTree.Header + " object\n" +
            //    "For Example: You want to add an ingredient into a certain, existing, recipe.");
            //newObjectFormTree.AddContextMenu(RemoveChildContextMenu_Click, "Remove Object From View");
            //NewObjectFormsPanel.Children.Add(newObjectFormTree);
        }
        private TreeViewItem SetNextObject(XmlNode nextObjectNode, string wrapperKey, XmlObjectsListWrapper xmlObjectListWrapper)
        {
            if (nextObjectNode.Name.Contains("#") || nextObjectNode == null) return null;
            XmlAttribute nextAvailableAttribute = nextObjectNode.GetAvailableAttribute();
            string attributeValue = nextAvailableAttribute == null ? "" : ":" + nextAvailableAttribute.Value + " (" + nextAvailableAttribute.Name + ")";
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem
            {
                Name = wrapperKey,
                FontSize = FONT_SIZE,
                Header = nextObjectNode.Name + attributeValue,
                Tag = nextObjectNode
            };
            if (nextObjectNode.Attributes != null)
            {
                SetNextObjectTreeViewAtrributes(nextObjectTreeViewItem, nextObjectNode.Attributes, nextObjectNode.Name);
            }
            if (nextObjectNode.HasChildNodes)
            {
                if (nextObjectNode.ChildNodes.Count > 50)
                {
                    MakeSearchTreeView(nextObjectTreeViewItem, nextObjectNode);
                }
                nextObjectTreeViewItem = SetObjectTreeView(nextObjectTreeViewItem, nextObjectNode.ChildNodes, wrapperKey, xmlObjectListWrapper);
            }
            return nextObjectTreeViewItem;
        }

        private void MakeSearchTreeView(TreeViewItem nextObjectTreeViewItem, XmlNode nextObjectNode)
        {
            //make a new treeview item with the box as the header add all children to that.
            ComboBox topTreeSearchBox = null;
            List<string> attributeCommon = nextObjectNode.GetAllCommonAttributes();
            topTreeSearchBox = topTreeSearchBox == null ? attributeCommon.CreateComboBoxList() : topTreeSearchBox.AddToComboBox(attributeCommon);
            XmlAttribute valueToUse = nextObjectNode.GetAvailableAttribute();
            string attributeValue = valueToUse != null ? ": " + valueToUse.Value + " (" + valueToUse.Name + ") " : "";
            topTreeSearchBox.Text = nextObjectNode.Name + attributeValue;
            topTreeSearchBox.Width = 325;
            topTreeSearchBox.FontSize = 18;
            topTreeSearchBox.DropDownClosed += TopTreeSearchBox_LostKeyboard_Focus;
            topTreeSearchBox.PreviewKeyDown += TopTreeSearchBox_KeyDown_Focus;
            nextObjectTreeViewItem.Header = topTreeSearchBox;
            topTreeSearchBox.AddOnHoverMessage(nextObjectNode.Name + attributeValue + " search box. ");
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
            //Add all removed trees to start
            List<TreeViewItem> removedTreeList;
            if (senderAsBox.Tag == null)
            {
                removedTreeList = new List<TreeViewItem>();
                senderAsBox.Tag = removedTreeList;
            }
            else removedTreeList = (List<TreeViewItem>)senderAsBox.Tag;
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
        private void SetNextObjectTreeViewAtrributes(TreeViewItem nextObjectTreeViewItem, XmlAttributeCollection attributes, string objectNodeName)
        {
            foreach (XmlAttribute nextAttribute in attributes)
            {
                TreeViewItem attributeBox = new TreeViewItem
                {
                    Header = nextAttribute.Name + " = " + nextAttribute.Value,
                    Tag = nextAttribute
                };
                if (!nextAttribute.Name.Contains("#whitespace"))
                {
                    attributeBox.AddOnHoverMessage("You can click me to copy the value");
                    attributeBox.PreviewMouseDown += NewObjectTreeAttributeCombo_MouseDown;
                    nextObjectTreeViewItem.Items.Add(attributeBox);
                }
            }
        }
        private void NewObjectTreeAttributeCombo_MouseDown(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((TreeViewItem)sender).Header.ToString().Split("=")[1].Trim());
        }
    }
}
