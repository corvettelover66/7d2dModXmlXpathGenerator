using ICSharpCode.AvalonEdit.Folding;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public int SearchTreeFontChange { get; private set; }
        public void IncreaseSearchTreeFontChange()
        {
            int newFontChange = SearchTreeFontChange + 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            this.SearchTreeFontChange = (SEARCH_VIEW_FONT_SIZE + newFontChange) > 0 ? newFontChange : this.SearchTreeFontChange;
        }
        public void DecreaseSearchTreeFontChange()
        {
            int newFontChange = SearchTreeFontChange - 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            this.SearchTreeFontChange = (SEARCH_VIEW_FONT_SIZE + newFontChange) > 0 ? newFontChange : this.SearchTreeFontChange;
        }
        public int ObjectTreeFontChange { get; private set; }
        public void IncreaseObjectTreeFontChange()
        {
            int newFontChange = ObjectTreeFontChange + 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            this.ObjectTreeFontChange = (OBJECT_VIEW_FONT_SIZE + newFontChange) > 0 ? newFontChange : this.ObjectTreeFontChange;
        }
        public void DereasecObjectTreeFontChange()
        {
            int newFontChange = ObjectTreeFontChange - 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            this.ObjectTreeFontChange = (OBJECT_VIEW_FONT_SIZE + newFontChange) > 0 ? newFontChange : this.ObjectTreeFontChange;
        }
        private ContextMenu XmlNodeContextMenu { get; set; }
        private ContextMenu ModXmlNodeContextMenu { get; set; }
        private ContextMenu XmlAttributeContextMenu { get; set; }
        public int SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD { get; private set; } = 15;
        public MyStackPanel NewObjectFormViewPanel { get; set; }
        public MyStackPanel SearchTreeFormViewPanel { get; set; }
        public ICSharpCode.AvalonEdit.TextEditor XmlOutBlock { get; private set; }
        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        public ObjectViewController(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox, Dictionary<string, XmlObjectsListWrapper> loadedListWrappers)
        {
            this.LoadedListWrappers = loadedListWrappers;
            this.XmlOutBlock = xmlOutputBox;
            SearchTreeFontChange = 0;
        }
        private void AddNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderAsButton = (Button)sender;
            TreeViewItem sendersParent = (TreeViewItem)senderAsButton.Parent;
            XmlObjectsListWrapper xmlObjectsListWrapper = this.NewObjectFormViewPanel.LoadedListWrappers.GetValueOrDefault(senderAsButton.Tag.ToString());
            string startingNodeName = senderAsButton.Content.ToString().Split(":")[0];
            TreeViewItem newObjectFormTreeView = this.GetNewObjectFormTreeAddButton(xmlObjectsListWrapper, senderAsButton.Tag.ToString(), startingNodeName);
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
                FontSize = OBJECT_VIEW_FONT_SIZE + 6 + ObjectTreeFontChange,
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
                FontSize = OBJECT_VIEW_FONT_SIZE + 6 + ObjectTreeFontChange,
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
                topTreeView.AddToolTip("Edit form for the " + currentTagName + " object");
                Button addNewObjectButton = new Button
                {
                    Content = currentTagName,
                    Tag = wrapperKey,
                    FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange,
                    Foreground = Brushes.Purple,
                    Background = Brushes.White
                };
                addNewObjectButton.AddToolTip("Click to add another " + currentTagName + " object");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                topTreeView.Header = addNewObjectButton;
            }
            SetNextNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, topTreeView, currentTagName, childrenDictionary);

            return topTreeView;
        }
        private void DuplicateTreeViewInParent_CentextMenuClick(object sender, RoutedEventArgs e)
        {
            //topTreeView.AddContextMenu(DuplicateTreeViewInParent_CentextMenuClick,
            //    "Duplicate",
            //    "Click here to duplicate the tree and add it into the object view");
            MenuItem senderAsMenuItem = sender as MenuItem;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem sendersAsTreeView = parentMenu.PlacementTarget as TreeViewItem;
            Button headerButton = sendersAsTreeView.Header as Button;
            XmlObjectsListWrapper xmlObjectsListWrapper = this.NewObjectFormViewPanel.LoadedListWrappers.GetValueOrDefault(headerButton.Tag.ToString());
            TreeViewItem newObjectFormTreeView = this.GetNewObjectFormTreeAddButton(xmlObjectsListWrapper, headerButton.Tag.ToString(), headerButton.Content.ToString().Split(":")[0]);
            sendersAsTreeView.CopyComboBoxesTreeView(newObjectFormTreeView);
            newObjectFormTreeView.AddContextMenu(this.RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
            newObjectFormTreeView.Uid = headerButton.Tag.ToString();
            if (sendersAsTreeView.Parent.GetType() == typeof(MyStackPanel))
            {
                int indexToInsert = ((MyStackPanel)sendersAsTreeView.Parent).Children.IndexOf(sendersAsTreeView) + 1;
                ((MyStackPanel)sendersAsTreeView.Parent).Children.Insert(indexToInsert, newObjectFormTreeView);
            }
            else if (sendersAsTreeView.Parent.GetType() == typeof(TreeViewItem))
            {
                int indexToInsert = ((TreeViewItem)sendersAsTreeView.Parent).Items.IndexOf(sendersAsTreeView) + 1;
                ((TreeViewItem)sendersAsTreeView.Parent).Items.Insert(indexToInsert, newObjectFormTreeView);
            }
        }
        public TreeViewItem GetNewObjectFormTreeAddButton(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, string startingXmlTagName, bool doSkipFirstAttributes = false)
        {
            if (startingXmlTagName.Length < 1) startingXmlTagName = xmlObjectListWrapper.FirstChildTagName;
            TreeViewItem newTopTree = new TreeViewItem
            {
                Header = startingXmlTagName,
                FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange,
            };
            return SetNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newTopTree, startingXmlTagName, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue(), doSkipFirstAttributes);
        }
        private void SetNextNewObjectFormChildren(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, string tagName, Dictionary<string, Queue<string>> allChildrenDictionary, HashSet<string> alreadyAddedChildNodes = null)
        {
            Queue<string> allChildren = allChildrenDictionary != null
                 ? allChildrenDictionary.GetValueOrDefault(tagName)
                 : null;

            if (allChildren != null && allChildren.Count > 0)
            {
                string nextChild = allChildren.Dequeue();
                while (!String.IsNullOrEmpty(nextChild))
                {
                    //If we have already added elements and the child was already added we want to skip it.
                    if (alreadyAddedChildNodes != null && alreadyAddedChildNodes.Contains(nextChild))
                    {
                        allChildren.TryDequeue(out nextChild);
                        continue;
                    }
                    TreeViewItem newChildTopTree = new TreeViewItem();
                    TreeViewItem childrenTreeView = SetNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newChildTopTree, nextChild, allChildrenDictionary);
                    topTreeView.Items.Add(childrenTreeView);
                    allChildren.TryDequeue(out nextChild);
                }
            }
        }
        private TreeViewItem SetNextObjectTreeViewAtrributes(List<string> attributes, XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, XmlNode currentNode = null)
        {
            TreeViewItem newAttributesViewItem = new TreeViewItem
            {
                FontSize = OBJECT_VIEW_FONT_SIZE + 6 + ObjectTreeFontChange,
            };
            foreach (string nextAttribute in attributes)
            {
                Label newLabel = new Label()
                {
                    Content = nextAttribute,
                    FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange,
                    Foreground = Brushes.Red
                };
                newAttributesViewItem.Items.Add(newLabel);
                ComboBox newAttributesComboBox = SetAttributesComboBox(xmlObjectListWrapper, currentTagName, nextAttribute);
                if (newAttributesComboBox == null) newAttributesViewItem.Items.Add(new ComboBox());
                else
                {
                    if (currentNode != null)
                    {
                        XmlAttribute currentNodeAttribute = currentNode.GetAttributeByName(nextAttribute);
                        if (currentNodeAttribute != null) newAttributesComboBox.Text = currentNodeAttribute.Value;
                    }
                    newAttributesViewItem.Items.Add(newAttributesComboBox);
                }
            }
            return newAttributesViewItem;
        }
        private ComboBox SetAttributesComboBox(XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string nextAttribute)
        {
            List<string> attributeCommon = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(currentTagName).GetValueOrDefault(nextAttribute);
            ComboBox newAttributesComboBox = attributeCommon != null ? attributeCommon.CreateComboBoxList(forgroundColor: Brushes.Blue) : null;
            if (newAttributesComboBox != null)
            {
                newAttributesComboBox.FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange;
                newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                newAttributesComboBox.Tag = nextAttribute;
                newAttributesComboBox.AddToolTip("Here you can set the value of the " + nextAttribute + " for the " + currentTagName);
            }
            return newAttributesComboBox;
        }
        private void NewAttributesComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.XmlOutBlock.Text = XmlXpathGenerator.GenerateXmlViewOutput(this.NewObjectFormViewPanel);
        }
        private void NewAttributesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            this.XmlOutBlock.Text = XmlXpathGenerator.GenerateXmlViewOutput(this.NewObjectFormViewPanel);
        }
        private void RemoveTreeNewObjectsContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = (ContextMenu)((MenuItem)sender).Parent;

            if (contextMenu.PlacementTarget is TreeViewItem senderTreeView)
            {
                if (senderTreeView.Parent is TreeViewItem parent) parent.Items.Remove(senderTreeView);
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
                            this.NewObjectFormViewPanel.Children.Remove(senderTreeView);
                            break;
                    }
                }
            }
        }
        private TreeViewItem GetNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, XmlNode startingNode, string wrapperKey)
        {
            TreeViewItem newTreeView = SetNewObjectFormTree(xmlObjectListWrapper, wrapperKey, startingNode, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue());
            newTreeView.FontSize = OBJECT_VIEW_FONT_SIZE + 6 + ObjectTreeFontChange;
            newTreeView.IsExpanded = true;
            newTreeView.AddToolTip("Here you can create new " + startingNode.Name + " tags");

            return newTreeView;
        }
        private TreeViewItem SetNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, XmlNode currentNode, Dictionary<string, Queue<string>> allChildrenDictionary, string nodeName = null)
        {
            string nodeNameToUse = String.IsNullOrEmpty(nodeName) ? currentNode.Name : nodeName;
            List<string> attributes = xmlObjectListWrapper.objectNameToAttributesMap.GetValueOrDefault(nodeNameToUse);
            TreeViewItem newTreeView = attributes != null
                ? SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, nodeNameToUse, currentNode)
                : null;
            if (newTreeView != null)
            {
                newTreeView.AddToolTip("Edit form for the " + nodeNameToUse + " object");
                string attributeValue = currentNode != null && currentNode.GetAvailableAttribute() != null ?
                    ":" + currentNode.GetAvailableAttribute().Value : "";
                Button addNewObjectButton = new Button
                {
                    Content = nodeNameToUse + attributeValue,
                    Tag = wrapperKey,
                    FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange,
                    Foreground = Brushes.Purple,
                    Background = Brushes.White
                };
                addNewObjectButton.AddToolTip("Click to add another " + nodeNameToUse + " object");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                newTreeView.Header = addNewObjectButton;
            }
            TreeViewItem finalTreeView = SetNextNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, newTreeView, currentNode, allChildrenDictionary, nodeNameToUse);

            return finalTreeView;
        }
        private TreeViewItem SetNextNewObjectFormChildren(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, XmlNode currentNode, Dictionary<string, Queue<string>> allChildrenDictionary, string nodeName = null)
        {
            HashSet<string> childNodeNames = new HashSet<string>();
            if (currentNode != null && currentNode.HasChildNodes)
            {
                //There are children but no attributes
                if (topTreeView == null) 
                { 
                    topTreeView = new TreeViewItem();
                    topTreeView.AddToolTip("Edit form for the " + nodeName + " object");
                    string attributeValue = currentNode != null && currentNode.GetAvailableAttribute() != null ?
                        ":" + currentNode.GetAvailableAttribute().Value : "";
                    Button addNewObjectButton = new Button
                    {
                        Content = nodeName + attributeValue,
                        Tag = wrapperKey,
                        FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange,
                        Foreground = Brushes.Purple,
                        Background = Brushes.White
                    };
                    addNewObjectButton.AddToolTip("Click to add another " + nodeName + " object");
                    addNewObjectButton.Click += AddNewObjectButton_Click;
                    topTreeView.Header = addNewObjectButton;
                }
                foreach (XmlNode nextChildNode in currentNode.ChildNodes)
                {
                    TreeViewItem childTree = SetNewObjectFormTree(xmlObjectListWrapper, wrapperKey, nextChildNode, allChildrenDictionary);
                    if (childTree != null)
                    {
                        topTreeView.Items.Add(childTree);
                        childNodeNames.Add(nextChildNode.Name);
                    }
                }
            }
            SetNextNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, topTreeView, currentNode.Name, allChildrenDictionary, childNodeNames);
            return topTreeView;
        }
        private void EditObject_ContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;
            if (senderTreeView == null)
            {
                if (parentMenu.PlacementTarget is TextBox searchTreeBox)
                {
                    senderTreeView = searchTreeBox.Tag as TreeViewItem;
                }
            }
            if (senderTreeView != null)
            {
                XmlNode xmlNode = senderTreeView.Tag as XmlNode;
                string[] wrapperKeySplit = senderTreeView.Uid.Split("_");
                string mainWrapperKey = wrapperKeySplit[wrapperKeySplit.Length - 1];
                XmlObjectsListWrapper wrapperToUse = this.LoadedListWrappers.GetValueOrDefault(mainWrapperKey);
                if (wrapperToUse == null)
                {
                    throw new Exception("Issue in Edit functionality, Edit was clicked and the wrapper was not loaded. This should not be possible. Try restarting the app.");
                }
                if (wrapperToUse.allTopLevelTags.Contains(xmlNode.Name))
                {
                    TreeViewItem newObjectFormTree = this.GetNewObjectFormTree(wrapperToUse, xmlNode, mainWrapperKey);
                    XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                    //Set the uid to the wrapper so we can find the wrapper later
                    newObjectFormTree.Uid = mainWrapperKey;
                    //Set the xmlNode that was included with the object into the top tree view
                    newObjectFormTree.Tag = xmlNode;
                    newObjectFormTree.Foreground = Brushes.Purple;
                    newObjectFormTree.AddToolTip("Object tree for the " + senderAsMenuItem.Name + " action");
                    newObjectFormTree.AddContextMenu(RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
                    NewObjectFormViewPanel.Children.Add(newObjectFormTree);
                }
                //This should also not be possible, edit should only exist for top lvel tags.
                else
                {
                    MessageBox.Show("This action can only be done on a top level object, for example if this object were in a recipes file you would need to right click a recipe object. ");
                }
            }
        }
        public TreeViewItem GetSearchTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, bool addContextMenu = true, bool includeChildrenInOnHover = false, bool includeComments = false)
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
            topObjectsTreeView = SetSearchTreeViewNextObject(topObjectsTreeView, allObjects, wrapperKey, xmlObjectListWrapper, addContextMenu, includeChildrenInOnHover, includeComments: includeComments);
            topObjectsTreeView.AddContextMenu(RemoveTreeSearchViewContextMenu_Click,
                "Remove Object",
                "Click here to remove this tree from the view");
            showEditLoadError = false;
            return topObjectsTreeView;
        }
        private void RemoveTreeSearchViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (sender as MenuItem);
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            if (parentMenu.PlacementTarget is TreeViewItem senderTreeView)
            {
                this.SearchTreeFormViewPanel.Children.Remove(senderTreeView);
            }
        }
        //Make sure to reset this after any calls ti SetSearchTreeViewObject
        private bool showEditLoadError { get; set; } = false;
        private TreeViewItem SetSearchTreeViewNextObject(TreeViewItem topObjectsTreeView, XmlNodeList allObjects, string wrapperName, XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true, bool includeChildrenInOnHover = false, bool includeComments = false)
        {
            foreach (XmlNode nextObjectNode in allObjects)
            {
                TreeViewItem nextNewTreeViewItem = null;
                if (nextObjectNode.Name.Contains("#text"))
                {
                    nextNewTreeViewItem = new TreeViewItem
                    {
                        FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange,
                        Header = "Inner Text = " + nextObjectNode.InnerText.Trim()
                    };
                }
                else if (nextObjectNode.Name.Contains("#comment"))
                {
                    if (includeComments)
                    {
                        nextNewTreeViewItem = new TreeViewItem
                        {
                            FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange,
                            Header = "#comment"
                        };
                        TreeViewItem innerTextTreeView = new TreeViewItem
                        {
                            FontSize = SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange,
                            Header = nextObjectNode.InnerText.Trim()
                        };
                        nextNewTreeViewItem.Items.Add(innerTextTreeView);
                    }
                }
                else if (!nextObjectNode.Name.Contains("#whitespace") && nextObjectNode != null)
                {
                    nextNewTreeViewItem = SetNextSearchTreeObject(nextObjectNode, wrapperName, xmlObjectListWrapper, addContextMenu: addContextMenu, includeChildrenInOnHover: includeChildrenInOnHover, includeComments: includeComments);
                }
                if (nextNewTreeViewItem != null)
                {
                    AddSearchTreeContextMenu(nextNewTreeViewItem, xmlObjectListWrapper, nextObjectNode.Name, addContextMenu);
                    topObjectsTreeView.Items.Add(nextNewTreeViewItem);
                }
            }
            return topObjectsTreeView;
        }
        private void AddSearchTreeContextMenu(TreeViewItem nextNewTreeViewItem, XmlObjectsListWrapper xmlObjectListWrapper, string nextObjectNodeName, bool addContextMenu)
        {
            if (addContextMenu)
            {
                if (XmlNodeContextMenu == null) XmlNodeContextMenu = AddTargetContextMenuToControl(nextNewTreeViewItem);
                else nextNewTreeViewItem.ContextMenu = XmlNodeContextMenu;
            }
            else
            {
                nextNewTreeViewItem.AddContextMenu(CollapseParentContextMenu_ClickFunction,
                    "Collapse Parent",
                    "Click here to collapse the parent tree");
                XmlObjectsListWrapper standardFileWrapper = this.LoadedListWrappers.GetValueOrDefault(xmlObjectListWrapper.GenerateDictionaryKey());
                if (standardFileWrapper != null && standardFileWrapper.allTopLevelTags.Contains(nextObjectNodeName))
                    ModXmlNodeContextMenu = nextNewTreeViewItem.AddContextMenu(EditObject_ContextMenuClick,
                        "Edit Object",
                        "Click here to add object to left panel to make edits.");
                else if (standardFileWrapper == null && !showEditLoadError)
                {
                    MessageBox.Show(
                        "Failed loading the edit functionality for the mod file. That means you will be unable to copy objects using the search tree.\n\n" +
                        "To fix this you need to load the game xml file " + xmlObjectListWrapper.xmlFile.FileName + ".",
                        "Failed Loading Edit Functionality",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    showEditLoadError = true;
                }
            }
        }
        public ContextMenu AddTargetContextMenuToControl(Control controlToAddMenu, bool isAttributeControl = false)
        {
            TreeViewItem controlAsTreeView = controlToAddMenu as TreeViewItem;
            TextBox controlAsTextBox = controlToAddMenu as TextBox;
            if ((controlAsTreeView != null) || (controlAsTextBox != null) || isAttributeControl)
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
            MenuItem senderAsMenuItem = (sender as MenuItem);
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;
            if (senderTreeView == null)
            {
                if (parentMenu.PlacementTarget is TextBox searchTreeBox)
                {
                    senderTreeView = searchTreeBox.Tag as TreeViewItem;
                }
            }
            if (senderTreeView != null && senderTreeView.Parent != null && senderTreeView.Parent.GetType() == typeof(TreeViewItem)) 
                (senderTreeView.Parent as TreeViewItem).IsExpanded = false;
        }
        private void AppendToContextMenu_ClickFunction(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;
            if (senderTreeView == null) 
            {
                if(parentMenu.PlacementTarget is TextBox searchTreeBox)
                {
                    senderTreeView = searchTreeBox.Tag as TreeViewItem;
                }
            }
            if(senderTreeView != null)
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
                    newObjectFormTree = new TreeViewItem { FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange };
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
                FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange,
                //          Node Name           Attribute targeted            Xpath action              
                Header = xmlNode.Name + ": " + xmlAttributeName + "=" + xmlAttributeValue + " (" + senderAsMenuItem.Name + ") "
            };
            if (senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE) || senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER)
                || senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE))
            {
                TextBox attributeNameBox = new TextBox { Text = "NewName", FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange };
                attributeNameBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeNameBox.AddToolTip("Type the new attribute name here.");
                TreeViewItem newAttributeTreeView = new TreeViewItem
                {
                    FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange,
                    Header = attributeNameBox,
                    Name = XmlXpathGenerator.ATTRIBUTE_NAME
                };
                newObjectFormTree.Items.Add(newAttributeTreeView);
                TextBox attributeValueBox = new TextBox { Text = "NewValue", FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange };
                attributeValueBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeValueBox.AddToolTip("Type the new attribute value here.");
                TreeViewItem newAttributeValueTreeView = new TreeViewItem
                {
                    FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange,
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
                    newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                    newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                    newAttributesComboBox.Tag = xmlAttributeName;
                    newAttributesComboBox.AddToolTip("Here you can set the value of the " + xmlAttributeName + " for the " + xmlNode.Name);
                    TreeViewItem headerTreeView = new TreeViewItem
                    {
                        FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange,
                        Header = newAttributesComboBox,
                        Name = XmlXpathGenerator.ATTRIBUTE_VALUE
                    };
                    newObjectFormTree.Items.Add(headerTreeView);
                }
            }
            return newObjectFormTree;
        }
        private TreeViewItem SetNextSearchTreeObject(XmlNode nextObjectNode, string wrapperKey, XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true, bool includeChildrenInOnHover = false, bool includeComments = false)
        {
            StringBuilder onHoverStringBuilder = new StringBuilder();
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
            nextObjectTreeViewItem.PreviewMouseDown += NewObjectTreeObjectCombo_MouseDown;
            if (nextObjectNode.Attributes != null)
            {
                string attributesString = SetNextObjectSearchTreeViewAtrributes(nextObjectTreeViewItem, nextObjectNode.Attributes, wrapperKey, nextObjectNode, addContextMenu);
                onHoverStringBuilder.Append(attributesString);
            }
            if (nextObjectNode.HasChildNodes)
            {
                if(includeChildrenInOnHover) onHoverStringBuilder.Append(GetChildrenNames(nextObjectNode.ChildNodes));
                if (nextObjectNode.GetValidChildrenCount() > SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD)
                {
                    MakeSearchTreeView(nextObjectTreeViewItem, nextObjectNode, addContextMenu);
                }
                nextObjectTreeViewItem = SetSearchTreeViewNextObject(nextObjectTreeViewItem, nextObjectNode.ChildNodes, wrapperKey, xmlObjectListWrapper, addContextMenu: addContextMenu, includeChildrenInOnHover: includeChildrenInOnHover, includeComments: includeComments);
            }
            if(!nextObjectNode.Name.Contains("#comment") && !String.IsNullOrEmpty(onHoverStringBuilder.ToString())) nextObjectTreeViewItem.AddToolTip(onHoverStringBuilder.ToString(), SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange, Brushes.Blue);

            return nextObjectTreeViewItem;
        }
        private string GetChildrenNames(XmlNodeList childNodes)
        {
            StringBuilder allChildNames = new StringBuilder();
            allChildNames.AppendLine("");
            allChildNames.AppendLine("Children:");

            foreach (XmlNode nextChild in childNodes) 
            {
                XmlAttribute availableAttribute = nextChild.GetAvailableAttribute();
                string childString = availableAttribute == null 
                    ? nextChild.Name 
                    : nextChild.Name + ": " + availableAttribute.Value + " (" + availableAttribute.Name + ")"; 
                allChildNames.AppendLine(childString);
            }
            if (allChildNames.Length > 0)
            {
                //Remove the last newline
                allChildNames.Remove(allChildNames.Length - 2, 2);
            }
            return allChildNames.ToString();
        }
        private void MakeSearchTreeView(TreeViewItem nextObjectTreeViewItem, XmlNode nextObjectNode, bool isGameFileSearchTree = true)
        {
            //make a new treeview item with the box as the header add all children to that.
            List<string> attributeCommon = nextObjectNode.GetAllCommonAttributes();
            MyComboBox topTreeSearchBox = attributeCommon.CreateMyComboBoxList(this, isGameFileSearchTree);
            XmlAttribute valueToUse = nextObjectNode.GetAvailableAttribute();
            string attributeValue = valueToUse != null ? ": " + valueToUse.Value + " (" + valueToUse.Name + ") " : "";
            //Wrapper key
            topTreeSearchBox.Uid = nextObjectTreeViewItem.Uid;
            topTreeSearchBox.Text = nextObjectNode.Name + attributeValue;
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
        private string SetNextObjectSearchTreeViewAtrributes(TreeViewItem nextObjectTreeViewItem, XmlAttributeCollection attributes, string wrapperKey, XmlNode currentNode, bool addContextMenu = true)
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
                    attributeValueTree.AddToolTip("You can click me to copy the value.\n You can also click the main object to copy the first attribute.");
                    attributeValueTree.PreviewMouseDown += NewObjectTreeAttributeCombo_MouseDown;
                    if (addContextMenu)
                    {
                        if (XmlAttributeContextMenu == null) XmlAttributeContextMenu = AddTargetContextMenuToControl(attributeNameTree, true);
                        else attributeNameTree.ContextMenu = XmlAttributeContextMenu;
                    }
                    attributeNameTree.Items.Add(attributeValueTree);
                    nextObjectTreeViewItem.Items.Add(attributeNameTree);
                }
            }
            if (stringBuilder.Length > 0)
            {
                //Remove the last newline
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
            }
            return stringBuilder.ToString();
        }
        private void NewObjectTreeAttributeCombo_MouseDown(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((TreeViewItem)sender).Header.ToString());
        }
        private void NewObjectTreeObjectCombo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem senderAsTreeViewItem = sender as TreeViewItem;
            XmlNode treeViewNode = senderAsTreeViewItem.Tag as XmlNode;
            XmlAttribute nextAvailableAttribute = treeViewNode.GetAvailableAttribute();
            string attributeValue = nextAvailableAttribute != null ? nextAvailableAttribute.Value : null;
            if(!String.IsNullOrEmpty(attributeValue))Clipboard.SetText(attributeValue);
        }
    }
}
