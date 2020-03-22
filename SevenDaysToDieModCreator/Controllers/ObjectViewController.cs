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

        public MyStackPanel newObjectFormView { get; set; }
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
            else if (sendersParent.Parent.GetType() == typeof(TreeViewItem)) ((TreeViewItem)sendersParent.Parent).Items.Add(newObjectFormTreeView);
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
        private void NewAttributesComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormView, this.loadedListWrappers, this.xmlOutBlock);
        }
        private void NewAttributesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormView, this.loadedListWrappers, this.xmlOutBlock);
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
                    if (nextTreeView.Header.GetType() != typeof(MyComboBox)) AddContextMenuToControl(nextTreeView);
                    topObjectsTreeView.Items.Add(nextTreeView);
                }
            }
            return topObjectsTreeView;
        }
        public void AddContextMenuToControl(Control nextTreeView)
        {
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Append to target", XmlXpathGenerator.XPATH_ACTION_APPEND).
                AddOnHoverMessage("The append command is used to add either more nodes or more attribute values");
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Remove target", XmlXpathGenerator.XPATH_ACTION_REMOVE).
                AddOnHoverMessage("The remove command is used to remove nodes or attributes");
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Insert After target", XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER).
                AddOnHoverMessage("Much like append, insertAfter will add nodes and attributes after the selected xpath");
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Insert Before target", XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE).
                AddOnHoverMessage("Much like insertAfter, insertBefore will add nodes and attributes before the selected xpath");
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Set", XmlXpathGenerator.XPATH_ACTION_SET).
                AddOnHoverMessage("The set command is used to change individual attributes");
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Remove Attribute", XmlXpathGenerator.XPATH_ACTION_REMOVE_ATTRIBUTE).
                AddOnHoverMessage("The removeattributes command is used to remove an existing attribute from an XML node");
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, "Set Attribute", XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE).
                AddOnHoverMessage("The setattribute command is used to add a new attribute to an XML node");
        }
        private void AppendToContextMenu_ClickFunction(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            TreeViewItem senderTreeView = senderAsMenuItem.Tag as TreeViewItem;

            if (senderTreeView != null)
            {
                XmlObjectsListWrapper wrapperToUse = this.loadedListWrappers.GetValueOrDefault(senderTreeView.Name.ToString());
                //Literally a hack to force the stackpanel to add a loaded list wrapper if it is empty
                // This relies on the StackPanel OnVisualChanged
                if (wrapperToUse == null)
                {
                    Button tempLabel = new Button() { Name = senderTreeView.Name };
                    newObjectFormView.Children.Add(tempLabel);
                    wrapperToUse = this.loadedListWrappers.GetValueOrDefault(senderTreeView.Name.ToString());
                    newObjectFormView.Children.Remove(tempLabel);
                }
                XmlNode xmlNode = senderTreeView.Tag as XmlNode;
                string isAttributeAction = "";
                TreeViewItem newObjectFormTree;
                //If there is an attribute Create a Special Object View with just the box for the attribute or a holder for the xml to generate.
                if (senderTreeView.Uid.Equals(XmlXpathGenerator.ATTRIBUTE_NAME))
                {
                    XmlAttribute treeAttribute = senderTreeView.Tag as XmlAttribute;
                    isAttributeAction = ":" + treeAttribute.Name;
                    newObjectFormTree = GenerateAttributeTree(senderAsMenuItem, xmlNode, treeAttribute, wrapperToUse);
                }
                else
                {
                    newObjectFormTree = this.GenerateNewObjectFormTreeAddButton(wrapperToUse, xmlNode.Name.ToString(), true);
                    XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeValue = avalailableAttribute == null ? "" : ": " + avalailableAttribute.Value + " (" + avalailableAttribute.Name + ")";
                    newObjectFormTree.Header = xmlNode.Name + attributeValue;
                }
                //Set the name to the wrapper so we can find the wrapper later
                newObjectFormTree.Name = senderTreeView.Name.ToString();
                //Set the xmlNode that was included with the object into the top tree view
                newObjectFormTree.Tag = xmlNode;
                //Set the newObjectFormTree uuid to the XmlXpath action that is set on the menu item name
                newObjectFormTree.Uid = senderAsMenuItem.Name + isAttributeAction;

                newObjectFormTree.AddOnHoverMessage("Using this form you can add new objects into the " + newObjectFormTree.Header.ToString() + " object\n" +
                    "For Example: You want to add an ingredient into a certain, existing, recipe.");
                newObjectFormTree.AddContextMenu(RemoveChildContextMenu_Click, "Remove Object From View");
                newObjectFormView.Children.Add(newObjectFormTree);
            }
        }

        private TreeViewItem GenerateAttributeTree(MenuItem senderAsMenuItem, XmlNode xmlNode, XmlAttribute xmlAttribute, XmlObjectsListWrapper xmlObjectListWrapper)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem
            {
                FontSize = FONT_SIZE,
                //                          FileName                                   Xpath action                   Node Name                      Attribute targeted
                Header = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension() + " action (" + senderAsMenuItem.Name + ") Node:" + xmlNode.Name + " target attribute:" + xmlAttribute.Name
            };
            if (senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE))
            {
                Label attributeNameLabel = new Label { Content = "New Attribute Name" };
                newObjectFormTree.Items.Add(attributeNameLabel);
                TreeViewItem newAttributeTreeView = new TreeViewItem
                {
                    FontSize = FONT_SIZE,
                    Header = new TextBox { FontSize = FONT_SIZE }.AddOnHoverMessage("Add new attribute name here."),
                    Name = XmlXpathGenerator.ATTRIBUTE_NAME
                };
                newObjectFormTree.Items.Add(newAttributeTreeView);
                Label attributeValueLabel = new Label { Content = "New Attribute Value" };
                newObjectFormTree.Items.Add(attributeValueLabel);
                TreeViewItem newAttributeValueTreeView = new TreeViewItem
                {
                    FontSize = FONT_SIZE,
                    Header = new TextBox { FontSize = FONT_SIZE }.AddOnHoverMessage("Add new attribute value here."), 
                    Name = XmlXpathGenerator.ATTRIBUTE_VALUE

                };
                newObjectFormTree.Items.Add(newAttributeValueTreeView);
            }
            else if (!senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_REMOVE) 
                || !senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_REMOVE_ATTRIBUTE)) 
            {
                List<string> attributeCommon = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(xmlNode.Name).GetValueOrDefault(xmlAttribute.Name);
                ComboBox newAttributesComboBox = attributeCommon != null ? attributeCommon.CreateComboBoxList() : null;
                if(newAttributesComboBox != null)
                {
                    newAttributesComboBox.Width = 300;
                    newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                    newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                    newAttributesComboBox.Tag = xmlAttribute.Name;
                    newAttributesComboBox.AddOnHoverMessage("Here you can set the value of the " + xmlAttribute.Name + " for the " + xmlNode.Name);
                    TreeViewItem headerTreeView = new TreeViewItem
                    {
                        FontSize = FONT_SIZE,
                        Header = newAttributesComboBox,
                        Name = XmlXpathGenerator.ATTRIBUTE_VALUE
                    };
                    newObjectFormTree.Items.Add(headerTreeView);
                }
            }
            return newObjectFormTree;
        }

        private TreeViewItem SetNextObject(XmlNode nextObjectNode, string wrapperKey, XmlObjectsListWrapper xmlObjectListWrapper)
        {
            if (nextObjectNode.Name.Contains("#") || nextObjectNode == null) return null;
            XmlAttribute nextAvailableAttribute = nextObjectNode.GetAvailableAttribute();
            string attributeValue = nextAvailableAttribute == null ? "" : ": " + nextAvailableAttribute.Value + " (" + nextAvailableAttribute.Name + ")";
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem
            {
                Name = wrapperKey,
                FontSize = FONT_SIZE,
                Header = nextObjectNode.Name + attributeValue,
                Tag = nextObjectNode
            };
            if (nextObjectNode.Attributes != null)
            {
                SetNextObjectTreeViewAtrributes(nextObjectTreeViewItem, nextObjectNode.Attributes, wrapperKey);
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
            List<string> attributeCommon = nextObjectNode.GetAllCommonAttributes();
            MyComboBox topTreeSearchBox = attributeCommon.CreateMyComboBoxList(this);
            XmlAttribute valueToUse = nextObjectNode.GetAvailableAttribute();
            string attributeValue = valueToUse != null ? ": " + valueToUse.Value + " (" + valueToUse.Name + ") " : "";
            topTreeSearchBox.Name = nextObjectTreeViewItem.Name;
            topTreeSearchBox.Text = nextObjectNode.Name + attributeValue;
            topTreeSearchBox.Width = 325;
            topTreeSearchBox.FontSize = 18;
            topTreeSearchBox.DropDownClosed += TopTreeSearchBox_LostKeyboard_Focus;
            topTreeSearchBox.PreviewKeyDown += TopTreeSearchBox_KeyDown_Focus;
            topTreeSearchBox.AddOnHoverMessage(nextObjectNode.Name + attributeValue + " search box. ");
            nextObjectTreeViewItem.Header = topTreeSearchBox;
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

            string searchText = senderAsBox.Text;
            List<TreeViewItem> children = GetChildren(topTreeView);
            List<TreeViewItem> treesToAdd = new List<TreeViewItem>();
            foreach (TreeViewItem nextTreeViewItem in children)
            {
                string treeIdentifier = nextTreeViewItem.Header.ToString().ToLower();
                if (nextTreeViewItem.Header.GetType() == typeof(ComboBox))
                {
                    XmlNode myNode = (XmlNode)nextTreeViewItem.Tag;
                    treeIdentifier =  myNode.GetAvailableAttribute().Value.ToLower();
                }
                if (treeIdentifier.Contains(searchText.ToLower()))
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
        private void SetNextObjectTreeViewAtrributes(TreeViewItem nextObjectTreeViewItem, XmlAttributeCollection attributes, string wrapperKey)
        {
            foreach (XmlAttribute nextAttribute in attributes)
            {
                TreeViewItem attributeBox = new TreeViewItem
                {
                    Header = nextAttribute.Name + " = " + nextAttribute.Value,
                    Tag = nextAttribute,
                    Uid = XmlXpathGenerator.ATTRIBUTE_NAME, 
                    Name = wrapperKey
                };
                if (!nextAttribute.Name.Contains("#whitespace"))
                {
                    attributeBox.AddOnHoverMessage("You can click me to copy the value");
                    AddContextMenuToControl(attributeBox); 
                    attributeBox.PreviewMouseDown += NewObjectTreeAttributeCombo_MouseDown;
                    nextObjectTreeViewItem.Items.Add(attributeBox);
                }
            }
        }

        private void HandleAttributeActionMenu_ClickFunction(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void NewObjectTreeAttributeCombo_MouseDown(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((TreeViewItem)sender).Header.ToString().Split("=")[1].Trim());
        }
    }
}
