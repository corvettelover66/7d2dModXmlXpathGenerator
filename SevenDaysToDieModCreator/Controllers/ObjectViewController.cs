using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace SevenDaysToDieModCreator.Controllers
{
    class ObjectViewController
    {
        private const int OBJECT_VIEW_FONT_SIZE = 20;
        public const int SEARCH_VIEW_FONT_SIZE = 17;

        public int SearchTreeFontChange { get; set; }
        public int SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD { get; private set; } = 25;
        public MyStackPanel NewObjectFormViewPanel { get; set; }
        public MyStackPanel SearchTreeFormViewPanel { get; set; }
        public ICSharpCode.AvalonEdit.TextEditor xmlOutBlock { get; private set; }
        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper>    loadedListWrappers { get; private set; }
        public ObjectViewController(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox)
        {
            this.loadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.xmlOutBlock = xmlOutputBox;
            SearchTreeFontChange = 0;
        }
        private void AddNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderAsButton = (Button)sender;
            TreeViewItem sendersParent = (TreeViewItem)senderAsButton.Parent;
            XmlObjectsListWrapper xmlObjectsListWrapper = this.NewObjectFormViewPanel.LoadedListWrappers.GetValueOrDefault(senderAsButton.Tag.ToString());
            TreeViewItem newObjectFormTreeView = this.GetNewObjectFormTreeAddButton(xmlObjectsListWrapper, senderAsButton.Tag.ToString(), senderAsButton.Content.ToString());
            newObjectFormTreeView.AddContextMenu(this.RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
            newObjectFormTreeView.Uid = senderAsButton.Tag.ToString();
            if (sendersParent.Parent.GetType() == typeof(MyStackPanel))
            {
                int indexToInsert = ((MyStackPanel)sendersParent.Parent).Children.IndexOf(sendersParent) + 1;
                ((MyStackPanel)sendersParent.Parent).Children.Insert(indexToInsert, newObjectFormTreeView);
            }
            else if (sendersParent.Parent.GetType() == typeof(TreeViewItem))
            {
                int indexToInsert = ((TreeViewItem)sendersParent.Parent).Items.IndexOf(sendersParent) + 1;
                ((TreeViewItem)sendersParent.Parent).Items.Insert(indexToInsert, newObjectFormTreeView);
            }
        }
        public void CreateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey)
        {
            Label topTreeLabel = new Label
            {
                Content = xmlObjectListWrapper.TopTagName,
                FontSize = OBJECT_VIEW_FONT_SIZE,
                Uid = wrapperKey,
                Foreground = Brushes.Purple
            };
            topTreeLabel.AddContextMenu(this.RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
            NewObjectFormViewPanel.Children.Add(topTreeLabel);

            foreach (string topTag in xmlObjectListWrapper.allTopLevelTags)
            {
                TreeViewItem returnedTree = GetNewObjectFormTree(xmlObjectListWrapper, topTag, wrapperKey);
                returnedTree.Uid = wrapperKey;
                returnedTree.AddContextMenu(this.RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
                NewObjectFormViewPanel.Children.Add(returnedTree);
            }
        }
        public TreeViewItem GetNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string tagName, string wrapperKey)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem
            {
                FontSize = OBJECT_VIEW_FONT_SIZE,
                IsExpanded = true
            };
            newObjectFormTree.AddToolTip("Here you can create new " + tagName + " tags");

            newObjectFormTree = SetNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newObjectFormTree, tagName, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue());
            return newObjectFormTree;
        }
        public TreeViewItem SetNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, string currentTagName, Dictionary<string, Queue<string>> childrenDictionary, bool doSkipFirstAttributeSet = false)
        {
            List<string> attributes = xmlObjectListWrapper.objectNameToAttributesMap.GetValueOrDefault(currentTagName);
            if (attributes != null && !doSkipFirstAttributeSet) topTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, currentTagName);

            if (topTreeView != null)
            {
                topTreeView.FontSize = OBJECT_VIEW_FONT_SIZE;
                topTreeView.AddToolTip("Edit form for the " + currentTagName + " object");
                Button addNewObjectButton = new Button
                {
                    Content = currentTagName,
                    Tag = wrapperKey,
                    Foreground = Brushes.Purple,
                    Background = Brushes.White
                };
                addNewObjectButton.AddToolTip("Click to add another " + currentTagName + " object");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                addNewObjectButton.Width = 250;
                topTreeView.Header = addNewObjectButton;
            }
            SetNextNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, topTreeView, currentTagName, childrenDictionary);

            return topTreeView;
        }
        public TreeViewItem GetNewObjectFormTreeAddButton(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, string startingXmlTagName, bool doSkipFirstAttributes = false)
        {
            if (startingXmlTagName.Length < 1) startingXmlTagName = xmlObjectListWrapper.FirstChildTagName;
            TreeViewItem newTopTree = new TreeViewItem
            {
                Header = startingXmlTagName
            };
            return SetNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newTopTree, startingXmlTagName, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue(), doSkipFirstAttributes);
        }
        private void SetNextNewObjectFormChildren(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, string tagName, Dictionary<string, Queue<string>> allChildrenDictionary)
        {
            Queue<string> allChildren = allChildrenDictionary.GetValueOrDefault(tagName);

            if (allChildren != null && allChildren.Count > 0)
            {
                string nextChild = allChildren.Dequeue();
                while (!String.IsNullOrEmpty(nextChild))
                {
                    TreeViewItem newChildTopTree = new TreeViewItem();
                    TreeViewItem childrenTreeView = SetNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newChildTopTree, nextChild, allChildrenDictionary);
                    topTreeView.Items.Add(childrenTreeView);
                    allChildren.TryDequeue(out nextChild);
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
                    Content = nextAttribute,
                    Foreground = Brushes.Red
                };
                newAttributesViewItem.Items.Add(newLabel);

                List<string> attributeCommon = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(currentTagName).GetValueOrDefault(nextAttribute);
                ComboBox newAttributesComboBox = attributeCommon != null ? attributeCommon.CreateComboBoxList(forgroundColor: Brushes.Blue) : null;
                newAttributesComboBox.Width = 300;
                newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                newAttributesComboBox.Tag = nextAttribute;
                newAttributesComboBox.AddToolTip("Here you can set the value of the " + nextAttribute + " for the " + currentTagName);
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
            this.xmlOutBlock.Text = XmlXpathGenerator.GenerateXmlViewOutput(this.NewObjectFormViewPanel);
        }
        private void NewAttributesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            this.xmlOutBlock.Text = XmlXpathGenerator.GenerateXmlViewOutput(this.NewObjectFormViewPanel);
        }
        private void RemoveTreeNewObjectsContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = (ContextMenu)((MenuItem)sender).Parent;
            if (contextMenu.Tag is TreeViewItem myObjectControl)
            {
                if (myObjectControl.Parent is TreeViewItem parent) parent.Items.Remove(myObjectControl);
                else 
                {
                    MessageBoxResult result = MessageBox.Show(
                        "This will remove the entire object. Are you sure?",
                        "Remove Top Level",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            this.NewObjectFormViewPanel.Children.Remove(myObjectControl);
                            break;
                    }
                }
            }
        }
        public TreeViewItem GetSearchTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, bool addContextMenu = true)
        {
            XmlNodeList allObjects = xmlObjectListWrapper.xmlFile.xmlDocument.GetElementsByTagName(xmlObjectListWrapper.TopTagName);
            TreeViewItem topObjectsTreeView = new TreeViewItem()
            {
                Header = xmlObjectListWrapper.TopTagName,
                IsExpanded = true,
                FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange + 3,
                Foreground = Brushes.Purple, 
                Uid = wrapperKey
            };
            topObjectsTreeView = SetSearchTreeViewNextObject(topObjectsTreeView, allObjects, wrapperKey, xmlObjectListWrapper, addContextMenu);
            topObjectsTreeView.AddContextMenu(RemoveTreeSearchViewContextMenu_Click,
                "Remove Object From View",
                "Click here to remove this tree from the view.");
            return topObjectsTreeView;
        }
        private void RemoveTreeSearchViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = ((MenuItem)sender).Parent as ContextMenu;
            if(contextMenu.Tag is TreeViewItem myObjectControl)
            {
                this.SearchTreeFormViewPanel.Children.Remove(myObjectControl);
            }
        }
        private ContextMenu xmlNodeContextMenu;
        private TreeViewItem SetSearchTreeViewNextObject(TreeViewItem topObjectsTreeView, XmlNodeList allObjects, string wrapperName, XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true)
        {
            foreach (XmlNode nextObjectNode in allObjects)
            {
                TreeViewItem nextTreeView = SetNextSearchTreeObject(nextObjectNode, wrapperName, xmlObjectListWrapper, addContextMenu);

                if (nextTreeView != null)
                {
                    if (addContextMenu)
                    {
                        if (xmlNodeContextMenu == null) xmlNodeContextMenu = AddTargetContextMenuToControl(nextTreeView);
                        else nextTreeView.ContextMenu = xmlNodeContextMenu;
                    }
                    topObjectsTreeView.Items.Add(nextTreeView);
                }
            }

            return topObjectsTreeView;
        }
        public ContextMenu AddTargetContextMenuToControl(Control controlToAddMenu, bool isAttributeControl = false)
        {
            TreeViewItem controlAsTreeView = controlToAddMenu as TreeViewItem;
            if ((controlAsTreeView != null && controlAsTreeView.HasItems) || isAttributeControl)
                controlToAddMenu.AddContextMenu(AppendToContextMenu_ClickFunction,
                    "Append",
                    "The append command is used to add either more nodes or more attribute values",
                    XmlXpathGenerator.XPATH_ACTION_APPEND);
            ContextMenu menuToReturn = controlToAddMenu.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Remove",
                "The remove command is used to remove nodes or attributes",
                XmlXpathGenerator.XPATH_ACTION_REMOVE);
            controlToAddMenu.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Insert After",
                "Much like append, insertAfter will add nodes and attributes after the selected xpath",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER);
            controlToAddMenu.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Insert Before",
                "Much like insertAfter, insertBefore will add nodes and attributes before the selected xpath",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE);
            if (isAttributeControl) controlToAddMenu.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Set",
                "The set command is used to change individual attributes",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_SET);
            else controlToAddMenu.AddContextMenu(AppendToContextMenu_ClickFunction,
               "Set Attribute",
               "The setattribute command is used to add a new attribute to an XML node",
               xpathAction: XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE);
            controlToAddMenu.AddContextMenu(CollapseParentContextMenu_ClickFunction,
                "Collapse Parent",
                "Click here to collapse the parent tree");
            return menuToReturn;
        }
        private void CollapseParentContextMenu_ClickFunction(object sender, RoutedEventArgs e)
        {
            MenuItem sendersAsMenuItem = (sender as MenuItem);
            if (sendersAsMenuItem.Parent is ContextMenu sendersParent && sendersParent.Tag != null)
            {
                TreeViewItem menuItemsTreeView = sendersParent.Tag as TreeViewItem;
                if (menuItemsTreeView.Parent != null && menuItemsTreeView.Parent.GetType() == typeof(TreeViewItem))
                {
                    (menuItemsTreeView.Parent as TreeViewItem).IsExpanded = false;
                }
            }
        }
        private void AppendToContextMenu_ClickFunction(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;

            if (senderTreeView != null)
            {
                string xPathAction = senderAsMenuItem.Name;
                XmlObjectsListWrapper wrapperToUse = this.SearchTreeFormViewPanel.LoadedListWrappers.GetValueOrDefault(senderTreeView.Uid.ToString());
                XmlNode xmlNode = senderTreeView.Tag as XmlNode;
                string attributeNameForAction = "";
                TreeViewItem newObjectFormTree;
                //If there is an attribute Create a Special Object View with just the box for the attribute or a holder for the xml to generate.
                if (senderTreeView.Name.Equals(XmlXpathGenerator.ATTRIBUTE_NAME))
                {
                    string attributeName = senderTreeView.Header.ToString().Trim();
                    string attributeValue = (senderTreeView.Items.GetItemAt(0) as TreeViewItem).Header.ToString().Trim();

                    attributeNameForAction = "_" + attributeName;
                    newObjectFormTree = GenerateNewObjectAttributeTree(senderAsMenuItem, wrapperToUse, xmlNode, attributeName, attributeValue);
                }
                else if (xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE))
                {
                    XmlAttribute avaliableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeName = avaliableAttribute == null ? "" : avaliableAttribute.Name;
                    string attributeValue = avaliableAttribute == null ? "" : avaliableAttribute.Value;
                    newObjectFormTree = GenerateNewObjectAttributeTree(senderAsMenuItem, wrapperToUse, xmlNode, attributeName, attributeValue);
                }
                else if (!xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_REMOVE))
                {
                    string nodeName = xmlNode.Name;
                    bool doSkipAttributes = true;
                    if (xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE) || xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER))
                    {
                        if (!wrapperToUse.allTopLevelTags.Contains(xmlNode.Name)) nodeName = xmlNode.ParentNode.Name;
                        else doSkipAttributes = false;
                    }
                    newObjectFormTree = this.GetNewObjectFormTreeAddButton(wrapperToUse, senderTreeView.Uid.ToString(), nodeName, doSkipAttributes);
                    XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeValue = avalailableAttribute == null ? "" : ": " + avalailableAttribute.Name + "=" + avalailableAttribute.Value;
                    newObjectFormTree.Header = xmlNode.Name + attributeValue + " (" + xPathAction + ") ";
                }
                else
                {
                    newObjectFormTree = new TreeViewItem { FontSize = OBJECT_VIEW_FONT_SIZE };
                    XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeValue = avalailableAttribute == null ? "" : ": " + avalailableAttribute.Name + "=" + avalailableAttribute.Value;
                    newObjectFormTree.Header = xmlNode.Name + attributeValue + " (" + xPathAction + ") ";
                }
                //Set the uid to the wrapper so we can find the wrapper later
                newObjectFormTree.Uid = senderTreeView.Uid.ToString();
                //Set the xmlNode that was included with the object into the top tree view
                newObjectFormTree.Tag = xmlNode;
                //Set the newObjectFormTree uuid to the XmlXpathAction_AttributeName that is set on the menu item name
                newObjectFormTree.Name = xPathAction + attributeNameForAction;
                newObjectFormTree.Foreground = Brushes.Purple;
                newObjectFormTree.AddToolTip("Object tree for the " + senderAsMenuItem.Name + " action");
                newObjectFormTree.AddContextMenu(RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
                NewObjectFormViewPanel.Children.Add(newObjectFormTree);
            }
        }
        private TreeViewItem GenerateNewObjectAttributeTree(MenuItem senderAsMenuItem, XmlObjectsListWrapper xmlObjectListWrapper, XmlNode xmlNode, string xmlAttributeName, string xmlAttributeValue)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem
            {
                FontSize = OBJECT_VIEW_FONT_SIZE,
                //          Node Name           Attribute targeted            Xpath action              
                Header = xmlNode.Name + ": " + xmlAttributeName + "=" + xmlAttributeValue + " (" + senderAsMenuItem.Name + ") "
            };
            if (senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE))
            {
                TextBox attributeNameBox = new TextBox { Text = "NewName", FontSize = OBJECT_VIEW_FONT_SIZE, Width = 250 };
                attributeNameBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeNameBox.AddToolTip("Type the new attribute name here.");
                TreeViewItem newAttributeTreeView = new TreeViewItem
                {
                    FontSize = OBJECT_VIEW_FONT_SIZE,
                    Header = attributeNameBox,
                    Name = XmlXpathGenerator.ATTRIBUTE_NAME
                };
                newObjectFormTree.Items.Add(newAttributeTreeView);
                TextBox attributeValueBox = new TextBox { Text = "NewValue", FontSize = OBJECT_VIEW_FONT_SIZE, Width = 250 };
                attributeValueBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeValueBox.AddToolTip("Type the new attribute value here.");
                TreeViewItem newAttributeValueTreeView = new TreeViewItem
                {
                    FontSize = OBJECT_VIEW_FONT_SIZE,
                    Header = attributeValueBox,
                    Name = XmlXpathGenerator.ATTRIBUTE_VALUE

                };
                newObjectFormTree.Items.Add(newAttributeValueTreeView);
            }
            else if (!senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_REMOVE))
            {
                List<string> attributeCommon = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(xmlNode.Name).GetValueOrDefault(xmlAttributeName);
                ComboBox newAttributesComboBox = attributeCommon != null ? attributeCommon.CreateComboBoxList(forgroundColor: Brushes.Blue) : null;
                if (newAttributesComboBox != null)
                {
                    newAttributesComboBox.Width = 300;
                    newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                    newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                    newAttributesComboBox.Tag = xmlAttributeName;
                    newAttributesComboBox.AddToolTip("Here you can set the value of the " + xmlAttributeName + " for the " + xmlNode.Name);
                    TreeViewItem headerTreeView = new TreeViewItem
                    {
                        FontSize = OBJECT_VIEW_FONT_SIZE,
                        Header = newAttributesComboBox,
                        Name = XmlXpathGenerator.ATTRIBUTE_VALUE
                    };
                    newObjectFormTree.Items.Add(headerTreeView);
                }
            }
            return newObjectFormTree;
        }
        private TreeViewItem SetNextSearchTreeObject(XmlNode nextObjectNode, string wrapperKey, XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true)
        {
            if (nextObjectNode.Name.Contains("#whitespace") || nextObjectNode == null) return null;
            if (nextObjectNode.Name.Contains("#text"))
            {
                TreeViewItem innerTextTreeView = new TreeViewItem
                {
                    FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange,
                    Header = "Inner Text = " + nextObjectNode.InnerText.Trim()
                };
                return innerTextTreeView;
            }
            if (nextObjectNode.Name.Contains("#comment"))
            {
                TreeViewItem outerTextTreeView = new TreeViewItem
                {
                    FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange,
                    Header = "#comment"
                };
                TreeViewItem innerTextTreeView = new TreeViewItem
                {
                    FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange - 2,
                    Header = nextObjectNode.InnerText.Trim()
                };
                outerTextTreeView.Items.Add(innerTextTreeView);
                return outerTextTreeView;
            }
            XmlAttribute nextAvailableAttribute = nextObjectNode.GetAvailableAttribute();
            string attributeValue = nextAvailableAttribute == null ? "" : ": " + nextAvailableAttribute.Value + " (" + nextAvailableAttribute.Name + ")";
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem
            {
                Uid = wrapperKey,
                FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange,
                Header = nextObjectNode.Name + attributeValue,
                Tag = nextObjectNode,
                Foreground = Brushes.Purple
            };
            if (nextObjectNode.Attributes != null)
            {
                SetNextObjectSearchTreeViewAtrributes(nextObjectTreeViewItem, nextObjectNode.Attributes, wrapperKey, nextObjectNode, addContextMenu);
            }
            if (nextObjectNode.HasChildNodes)
            {
                if (nextObjectNode.GetValidChildrenCount() > SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD)
                {
                    MakeSearchTreeView(nextObjectTreeViewItem, nextObjectNode, addContextMenu);
                }
                nextObjectTreeViewItem = SetSearchTreeViewNextObject(nextObjectTreeViewItem, nextObjectNode.ChildNodes, wrapperKey, xmlObjectListWrapper, addContextMenu);
            }
            return nextObjectTreeViewItem;
        }
        private void MakeSearchTreeView(TreeViewItem nextObjectTreeViewItem, XmlNode nextObjectNode, bool doAddContextMenu = true)
        {
            //make a new treeview item with the box as the header add all children to that.
            List<string> attributeCommon = nextObjectNode.GetAllCommonAttributes();
            MyComboBox topTreeSearchBox = attributeCommon.CreateMyComboBoxList(this, doAddContextMenu);
            XmlAttribute valueToUse = nextObjectNode.GetAvailableAttribute();
            string attributeValue = valueToUse != null ? ": " + valueToUse.Value + " (" + valueToUse.Name + ") " : "";
            topTreeSearchBox.Uid = nextObjectTreeViewItem.Uid;
            topTreeSearchBox.Text = nextObjectNode.Name + attributeValue;
            topTreeSearchBox.Width = 275;
            topTreeSearchBox.Foreground = Brushes.Purple;
            topTreeSearchBox.FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange;
            topTreeSearchBox.DropDownClosed += TopTreeSearchBox_DropDownClosed;
            topTreeSearchBox.PreviewKeyDown += TopTreeSearchBox_KeyEnterDown_Focus;
            topTreeSearchBox.AddToolTip(nextObjectNode.Name + attributeValue + " search box. ");
            nextObjectTreeViewItem.Header = topTreeSearchBox;
        }
        private void TopTreeSearchBox_KeyEnterDown_Focus(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                SearchBoxUpdate(sender);
            }
        }
        private void TopTreeSearchBox_DropDownClosed(object sender, EventArgs e)
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
            List<TreeViewItem> children = topTreeView.GetTreeViewChildren();
            List<TreeViewItem> treesToAdd = new List<TreeViewItem>();
            //Go through all children and filter 
            foreach (TreeViewItem nextTreeViewItem in children)
            {
                string treeIdentifier = nextTreeViewItem.Header.ToString().ToLower();
                if (nextTreeViewItem.Header.GetType() == typeof(MyComboBox))
                {
                    XmlNode myNode = (XmlNode)nextTreeViewItem.Tag;
                    treeIdentifier = myNode.GetAvailableAttribute().Value.ToLower();
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
        private ContextMenu attributeContextMenu;
        private void SetNextObjectSearchTreeViewAtrributes(TreeViewItem nextObjectTreeViewItem, XmlAttributeCollection attributes, string wrapperKey, XmlNode currentNode, bool addContextMenu = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (XmlAttribute nextAttribute in attributes)
            {
                TreeViewItem attributeNameTree = new TreeViewItem
                {
                    Header = nextAttribute.Name,
                    Foreground = Brushes.Red,
                    Tag = currentNode,
                    Name = XmlXpathGenerator.ATTRIBUTE_NAME,
                    Uid = wrapperKey,
                    IsExpanded = true,
                    FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange
                };
                TreeViewItem attributeValueTree = new TreeViewItem
                {
                    Header = nextAttribute.Value,
                    Foreground = Brushes.Blue,
                    Uid = wrapperKey,
                    FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange
                };
                if (!nextAttribute.Name.Contains("#whitespace"))
                {
                    stringBuilder.AppendLine(nextAttribute.Name + " : " + nextAttribute.Value);
                    attributeValueTree.AddToolTip("You can click me to copy the value");
                    attributeValueTree.PreviewMouseDown += NewObjectTreeAttributeCombo_MouseDown;
                    if (addContextMenu)
                    {
                        if (attributeContextMenu == null) attributeContextMenu = AddTargetContextMenuToControl(attributeNameTree, true);
                        else attributeNameTree.ContextMenu = attributeContextMenu;
                    }
                    attributeNameTree.Items.Add(attributeValueTree);
                    nextObjectTreeViewItem.Items.Add(attributeNameTree);
                }
            }
            if (stringBuilder.Length > 0)
            {
                //Remove the last newline
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                nextObjectTreeViewItem.AddToolTip(stringBuilder.ToString(), SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange - 3, Brushes.Blue);
            }
        }
        private void NewObjectTreeAttributeCombo_MouseDown(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((TreeViewItem)sender).Header.ToString());
        }
    }
}
