using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace SevenDaysToDieModCreator.Controllers
{
    class ObjectViewController
    {
        private const int OBJECT_VIEW_FONT_SIZE = 20;
        private const int SEARCH_VIEW_FONT_SIZE = 17;
        public int SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD { get; private set; } = 25;


        public MyStackPanel NewObjectFormViewPanel { get; set; }
        public MyStackPanel SearchTreeFormViewPanel { get; set; }

        public ICSharpCode.AvalonEdit.TextEditor xmlOutBlock { get; private set; }
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
            XmlObjectsListWrapper xmlObjectsListWrapper = this.NewObjectFormViewPanel.LoadedListWrappers.GetWrapperFromDictionary(senderAsButton.Tag.ToString());
            TreeViewItem newObjectFormTreeView = this.GenerateNewObjectFormTreeAddButton(xmlObjectsListWrapper, senderAsButton.Content.ToString());
            newObjectFormTreeView.AddContextMenu(this.RemoveChildContextMenu_Click, "Remove Object From View");

            newObjectFormTreeView.Name = senderAsButton.Tag.ToString();
            if (sendersParent.Parent.GetType() == typeof(MyStackPanel))
            {
                Label topTreeLabel = new Label { Content = xmlObjectsListWrapper.TopTagName, FontSize = OBJECT_VIEW_FONT_SIZE };
                ((MyStackPanel)sendersParent.Parent).Children.Add(topTreeLabel);
                ((MyStackPanel)sendersParent.Parent).Children.Add(newObjectFormTreeView);
            }
            else if (sendersParent.Parent.GetType() == typeof(TreeViewItem)) ((TreeViewItem)sendersParent.Parent).Items.Add(newObjectFormTreeView);
        }
        public void CreateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper)
        {
            Label topTreeLabel = new Label { Content = xmlObjectListWrapper.TopTagName, FontSize = OBJECT_VIEW_FONT_SIZE, Name = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension() };
            topTreeLabel.AddContextMenu(this.RemoveChildContextMenu_Click, "Remove Object From View");
            NewObjectFormViewPanel.Children.Add(topTreeLabel);

            foreach (string topTag in xmlObjectListWrapper.allTopLevelTags)
            {
                TreeViewItem returnedTree = CreateNewObjectFormTree(xmlObjectListWrapper, topTag);
                returnedTree.Name = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension();
                if (xmlObjectListWrapper.TopTagName == StringConstants.PROGRESSION_TAG_NAME) returnedTree.Name = xmlObjectListWrapper.TopTagName;
                returnedTree.AddContextMenu(this.RemoveChildContextMenu_Click, "Remove Object From View");
                NewObjectFormViewPanel.Children.Add(returnedTree);
            }
        }
        public TreeViewItem CreateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string tagName)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem();
            newObjectFormTree.FontSize = OBJECT_VIEW_FONT_SIZE;
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
                topTreeView.FontSize = OBJECT_VIEW_FONT_SIZE;
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
                        TreeViewItem innerPropertyTreeView = new TreeViewItem { Header = tagName, FontSize = OBJECT_VIEW_FONT_SIZE };
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
            this.xmlOutBlock.Text = XmlXpathGenerator.GenerateXmlViewOutput(this.NewObjectFormViewPanel, this.NewObjectFormViewPanel.LoadedListWrappers);
        }
        private void NewAttributesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            this.xmlOutBlock.Text = XmlXpathGenerator.GenerateXmlViewOutput(this.NewObjectFormViewPanel, this.NewObjectFormViewPanel.LoadedListWrappers);
        }
        public TreeViewItem GetSearchTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true)
        {
            XmlNodeList allObjects = xmlObjectListWrapper.xmlFile.xmlDocument.GetElementsByTagName(xmlObjectListWrapper.TopTagName);
            TreeViewItem topObjectsTreeView = new TreeViewItem()
            {
                Header = xmlObjectListWrapper.TopTagName,
                IsExpanded = true,
                FontSize = SEARCH_VIEW_FONT_SIZE + 3
            };
            topObjectsTreeView = SetSearchTreeViewNextObject(topObjectsTreeView, allObjects, xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension(), xmlObjectListWrapper, addContextMenu);
            return topObjectsTreeView;
        }
        private TreeViewItem SetSearchTreeViewNextObject(TreeViewItem topObjectsTreeView, XmlNodeList allObjects, string wrapperName, XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true)
        {
            foreach (XmlNode nextObjectNode in allObjects)
            {
                TreeViewItem nextTreeView = SetNextSearchTreeObject(nextObjectNode, wrapperName, xmlObjectListWrapper, addContextMenu);

                if (nextTreeView != null)
                {
                    if (nextTreeView.Header.GetType() != typeof(MyComboBox) && addContextMenu) AddTargetContextMenuToControl(nextTreeView);
                    topObjectsTreeView.Items.Add(nextTreeView);
                }
            }
            return topObjectsTreeView;
        }
        public void AddTargetContextMenuToControl(Control nextTreeView, bool isAttributeControl = false)
        {
            XmlNode xmlNode = nextTreeView.Tag as XmlNode;
            TreeViewItem comboParent = nextTreeView.Tag as TreeViewItem;
            if (comboParent != null) xmlNode = comboParent.Tag as XmlNode;
            if (xmlNode.HasChildNodes || isAttributeControl) 
                nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction, 
                "Append", 
                "The append command is used to add either more nodes or more attribute values", 
                XmlXpathGenerator.XPATH_ACTION_APPEND);
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Remove",
                "The remove command is used to remove nodes or attributes",
                XmlXpathGenerator.XPATH_ACTION_REMOVE);
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Insert After",
                "Much like append, insertAfter will add nodes and attributes after the selected xpath",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER);
            nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Insert Before",
                "Much like insertAfter, insertBefore will add nodes and attributes before the selected xpath",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE);
            if (isAttributeControl) nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Set",
                "The set command is used to change individual attributes",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_SET);
            else  nextTreeView.AddContextMenu(AppendToContextMenu_ClickFunction,
                "Set Attribute",
                "The setattribute command is used to add a new attribute to an XML node",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE);
        }
        private void AppendToContextMenu_ClickFunction(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            TreeViewItem senderTreeView = senderAsMenuItem.Tag as TreeViewItem;
            if (senderTreeView != null)
            {
                XmlObjectsListWrapper wrapperToUse = this.SearchTreeFormViewPanel.LoadedListWrappers.GetValueOrDefault(senderTreeView.Name.ToString());
                //Literally a hack to force the stackpanel to add a loaded list wrapper if it is empty
                // This relies on the StackPanel OnVisualChanged
                if (wrapperToUse == null)
                {
                    Button tempLabel = new Button() { Name = senderTreeView.Name };
                    NewObjectFormViewPanel.Children.Add(tempLabel);
                    wrapperToUse = this.SearchTreeFormViewPanel.LoadedListWrappers.GetValueOrDefault(senderTreeView.Name.ToString());
                    NewObjectFormViewPanel.Children.Remove(tempLabel);
                }
                XmlNode xmlNode = senderTreeView.Tag as XmlNode;
                string isAttributeAction = "";
                TreeViewItem newObjectFormTree;
                //If there is an attribute Create a Special Object View with just the box for the attribute or a holder for the xml to generate.
                if (senderTreeView.Uid.Equals(XmlXpathGenerator.ATTRIBUTE_NAME))
                {
                    string[] attributeSplit = senderTreeView.Header.ToString().Split("=");

                    isAttributeAction = ":" + attributeSplit[0];
                    newObjectFormTree = GenerateNewObjectAttributeTree(senderAsMenuItem, xmlNode, attributeSplit[0].Trim(), attributeSplit[1].Trim(), wrapperToUse);
                }
                else if (senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE)) 
                {
                    XmlAttribute avaliableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeName = avaliableAttribute == null ? "" : avaliableAttribute.Name;
                    string attributeValue = avaliableAttribute == null ? "" : avaliableAttribute.Value;
                    newObjectFormTree = GenerateNewObjectAttributeTree(senderAsMenuItem, xmlNode, attributeName, attributeValue, wrapperToUse);
                }
                //Add remove here to block the tree generation
                else if (!senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_REMOVE))
                {
                    string nodeName = xmlNode.Name;
                    bool doSkipAttributes = true;
                    if (senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE) || senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER)) 
                    {
                        if (!wrapperToUse.allTopLevelTags.Contains(xmlNode.Name))nodeName = xmlNode.ParentNode.Name;
                        else doSkipAttributes = false;
                    }
                    newObjectFormTree = this.GenerateNewObjectFormTreeAddButton(wrapperToUse, nodeName, doSkipAttributes);
                    XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeValue = avalailableAttribute == null ? "" : ": " + avalailableAttribute.Name + "=" + avalailableAttribute.Value;
                    newObjectFormTree.Header = xmlNode.Name + attributeValue + " (" + senderAsMenuItem.Name + ") ";
                }
                else
                {
                    newObjectFormTree = new TreeViewItem { FontSize = OBJECT_VIEW_FONT_SIZE };
                    XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeValue = avalailableAttribute == null ? "" : ": " + avalailableAttribute.Name + "=" + avalailableAttribute.Value;
                    newObjectFormTree.Header = xmlNode.Name + attributeValue + " (" + senderAsMenuItem.Name + ") ";
                }
                //Set the name to the wrapper so we can find the wrapper later
                newObjectFormTree.Name = senderTreeView.Name.ToString();
                //Set the xmlNode that was included with the object into the top tree view
                newObjectFormTree.Tag = xmlNode;
                //Set the newObjectFormTree uuid to the XmlXpath action that is set on the menu item name
                newObjectFormTree.Uid = senderAsMenuItem.Name + isAttributeAction;

                newObjectFormTree.AddOnHoverMessage("Object tree for the " + senderAsMenuItem.Name + " action");
                newObjectFormTree.AddContextMenu(RemoveChildContextMenu_Click, "Remove Object From View");
                NewObjectFormViewPanel.Children.Add(newObjectFormTree);
            }
        }

        private TreeViewItem GenerateNewObjectAttributeTree(MenuItem senderAsMenuItem, XmlNode xmlNode, string xmlAttributeName, string xmlAttributeValue, XmlObjectsListWrapper xmlObjectListWrapper)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem
            {
                FontSize = OBJECT_VIEW_FONT_SIZE,
                //          Node Name           Attribute targeted            Xpath action              
                Header =  xmlNode.Name + ": " + xmlAttributeName + "=" + xmlAttributeValue + " (" + senderAsMenuItem.Name + ") "
            };
            if (senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE))
            {
                TextBox attributeNameBox = new TextBox { Text = "NewName", FontSize = OBJECT_VIEW_FONT_SIZE, Width = 250 };
                attributeNameBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeNameBox.AddOnHoverMessage("Type the new attribute name here.");
                TreeViewItem newAttributeTreeView = new TreeViewItem
                {
                    FontSize = OBJECT_VIEW_FONT_SIZE,
                    Header = attributeNameBox,
                    Name = XmlXpathGenerator.ATTRIBUTE_NAME
                };
                newObjectFormTree.Items.Add(newAttributeTreeView);
                TextBox attributeValueBox = new TextBox { Text = "NewValue", FontSize = OBJECT_VIEW_FONT_SIZE, Width = 250 };
                attributeValueBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeValueBox.AddOnHoverMessage("Type the new attribute value here.");
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
                ComboBox newAttributesComboBox = attributeCommon != null ? attributeCommon.CreateComboBoxList() : null;
                if(newAttributesComboBox != null)
                {
                    newAttributesComboBox.Width = 300;
                    newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                    newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                    newAttributesComboBox.Tag = xmlAttributeName;
                    newAttributesComboBox.AddOnHoverMessage("Here you can set the value of the " + xmlAttributeName + " for the " + xmlNode.Name);
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
            if (nextObjectNode.Name.Contains("#") || nextObjectNode == null) return null;
            XmlAttribute nextAvailableAttribute = nextObjectNode.GetAvailableAttribute();
            string attributeValue = nextAvailableAttribute == null ? "" : ": " + nextAvailableAttribute.Value + " (" + nextAvailableAttribute.Name + ")";
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem
            {
                Name = wrapperKey,
                FontSize = SEARCH_VIEW_FONT_SIZE,
                Header = nextObjectNode.Name + attributeValue,
                Tag = nextObjectNode
            };
            if (nextObjectNode.Attributes != null)
            {
                SetNextObjectTreeViewAtrributes(nextObjectTreeViewItem, nextObjectNode.Attributes, wrapperKey, nextObjectNode, addContextMenu);
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
            topTreeSearchBox.Name = nextObjectTreeViewItem.Name;
            topTreeSearchBox.Text = nextObjectNode.Name + attributeValue;
            topTreeSearchBox.Width = 325;
            topTreeSearchBox.FontSize = SEARCH_VIEW_FONT_SIZE;
            topTreeSearchBox.DropDownClosed += TopTreeSearchBox_DropDownClosed;
            topTreeSearchBox.PreviewKeyDown += TopTreeSearchBox_KeyEnterDown_Focus;
            topTreeSearchBox.AddOnHoverMessage(nextObjectNode.Name + attributeValue + " search box. ");
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
            List<TreeViewItem> children = topTreeView.GetChildren();
            List<TreeViewItem> treesToAdd = new List<TreeViewItem>();
            foreach (TreeViewItem nextTreeViewItem in children)
            {
                string treeIdentifier = nextTreeViewItem.Header.ToString().ToLower();
                if (nextTreeViewItem.Header.GetType() == typeof(MyComboBox))
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
        private void SetNextObjectTreeViewAtrributes(TreeViewItem nextObjectTreeViewItem, XmlAttributeCollection attributes, string wrapperKey, XmlNode currentNode, bool addContextMenu = true)
        {
            foreach (XmlAttribute nextAttribute in attributes)
            {
                TreeViewItem attributeBox = new TreeViewItem
                {
                    Header = nextAttribute.Name + " = " + nextAttribute.Value,
                    Tag = currentNode,
                    Uid = XmlXpathGenerator.ATTRIBUTE_NAME, 
                    Name = wrapperKey, 
                    FontSize = SEARCH_VIEW_FONT_SIZE
                };
                if (!nextAttribute.Name.Contains("#whitespace"))
                {
                    attributeBox.AddOnHoverMessage("You can click me to copy the value");
                    attributeBox.PreviewMouseDown += NewObjectTreeAttributeCombo_MouseDown;
                    if(addContextMenu)AddTargetContextMenuToControl(attributeBox, true); 
                    nextObjectTreeViewItem.Items.Add(attributeBox);
                }
            }
        }

        private void NewObjectTreeAttributeCombo_MouseDown(object sender, RoutedEventArgs e)
        {
            string textToSet = null;
            foreach (string nextString in ((TreeViewItem)sender).Header.ToString().Split("=")) 
            {
                if (textToSet == null) textToSet = "";
                else textToSet += nextString;
            }
            Clipboard.SetText(textToSet);
        }
    }
}
