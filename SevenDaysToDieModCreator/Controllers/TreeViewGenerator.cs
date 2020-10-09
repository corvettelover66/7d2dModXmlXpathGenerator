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
    class TreeViewGenerator
    {
        public static int SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD { get; private set; } = 15;

        private const string HIDE_TREEVIEW_ATTRIBUTE_BOXES = "Hide Attributes";
        private const string UNHIDE_TREEVIEW_ATTRIBUTE_BOXES = "Unhide Attributes";

        //A dictionary for getting a list of hidden comboboxes
        //Key The tree view with hidden boxes
        //Value Stack of hidden combo boxes
        private static Dictionary<TreeViewItem, Stack<Tuple<Label, ComboBox, ComboBox>>> HiddenSearchTreeComboBoxDictionary { get; } = new Dictionary<TreeViewItem, Stack<Tuple<Label, ComboBox, ComboBox>>>();
        private static ContextMenu RemoveContextMenu { get; set; }
        private static ContextMenu GameFileXpathContextMenuWithCopy { get; set; }
        private static ContextMenu ModFileXpathContextMenuWithCopy { get; set; }
        private static ContextMenu XmlAttributeContextMenu { get; set; }
        public static void CreateEmptyNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey)
        {
            if (xmlObjectListWrapper.allTopLevelTags.Count > 1) 
            {
                Label topTreeLabel = new Label
                {
                    Content = xmlObjectListWrapper.TopTagName,
                    FontSize = MainWindowViewController.ObjectTreeFontChange + 6,
                    Uid = wrapperKey,
                    Foreground = Brushes.Purple
                };
                topTreeLabel.AddContextMenu(RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
                MainWindowViewController.NewObjectFormViewPanel.Children.Add(topTreeLabel);
            }
            foreach (string topTag in xmlObjectListWrapper.allTopLevelTags)
            {
                TreeViewItem returnedTree = GetEmptyNewObjectFormTree(xmlObjectListWrapper, topTag, wrapperKey);                
                MainWindowViewController.NewObjectFormViewPanel.Children.Add(returnedTree);
            }
        }
        private static void AddIgnoreFlagToTreeContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;
            if (senderTreeView.Tag != null && senderTreeView.Tag is bool)
            {
                bool doIgnore = (bool)senderTreeView.Tag;
                senderTreeView.Tag = !doIgnore;
                if ((bool)senderTreeView.Tag)
                {
                    Button senderTreeViewHeaderAsButton = senderTreeView.Header as Button;
                    senderTreeViewHeaderAsButton.Content = XmlXpathGenerator.IGNORE_STRING + ":" + senderTreeViewHeaderAsButton.Content;
                }
                else
                {
                    Button senderTreeViewHeaderAsButton = senderTreeView.Header as Button;
                    senderTreeViewHeaderAsButton.Content = senderTreeViewHeaderAsButton.Content.ToString().Replace(XmlXpathGenerator.IGNORE_STRING + ":", "");
                }
            }
            else if (senderTreeView.Tag == null)
            {
                return;
            }
            else
            {
                string contentString = senderTreeView.Header.ToString();
                if (contentString.Contains(XmlXpathGenerator.IGNORE_STRING))
                {
                    senderTreeView.Header = contentString.Replace(XmlXpathGenerator.IGNORE_STRING + ":", "");
                }
                else
                {
                    senderTreeView.Header = XmlXpathGenerator.IGNORE_STRING + ":" + senderTreeView.Header.ToString();
                }
            }
        }
        public static TreeViewItem GetEmptyNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string tagName, string wrapperKey)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem
            {
                FontSize = MainWindowViewController.ObjectTreeFontChange + 6,
                Uid = wrapperKey,
                IsExpanded = true
            };
            newObjectFormTree.AddToolTip("Here you can create new " + tagName + " tags");

            newObjectFormTree = SetEmptyNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newObjectFormTree, tagName, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue());
            return newObjectFormTree;
        }
        public static TreeViewItem SetEmptyNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, string currentTagName, Dictionary<string, Queue<string>> childrenDictionary, bool doSkipFirstAttributeSet = false, HashSet<string> alreadyAddedChildNodes = null)
        {
            List<string> attributes = xmlObjectListWrapper.objectNameToAttributesMap.GetValueOrDefault(currentTagName);
            if (attributes != null && !doSkipFirstAttributeSet) topTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, currentTagName);

            if (topTreeView != null)
            {
                topTreeView.AddToolTip("Edit form for the " + currentTagName + " object");
                Button addNewObjectButton = new Button
                {
                    Content = currentTagName.Replace("_", "__"),
                    Tag = wrapperKey,
                    Uid = currentTagName,
                    FontSize = MainWindowViewController.ObjectTreeFontChange + 4,
                    Foreground = Brushes.Purple,
                    Background = Brushes.White
                };
                addNewObjectButton.AddToolTip("Click to add another " + currentTagName + " object\n(*) means there are hidden attributes");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                topTreeView.Header = addNewObjectButton;
                topTreeView.AddContextMenu(AddHideAllAttributesFlagContextMenu_Click, "Toggle Hide/Unhide All", "Click here to hide/unhide all unused attributes in the entire tree");
                topTreeView.AddContextMenu(AddHideAttributesFlagContextMenu_Click, HIDE_TREEVIEW_ATTRIBUTE_BOXES, "Click here to hide/unhide all unused attributes in the current tag");
                topTreeView.Tag = false;
                topTreeView.AddContextMenu(AddIgnoreFlagToTreeContextMenu_Click, "Toggle Ignore", "Click here to flag the tree for ignore when clicking Save All XML ");
                topTreeView.AddContextMenu(RemoveTreeNewObjectsContextMenu_Click, "Remove Object", "Click here to remove this tag from the tree");
            }
            bool hasChildren = SetNextEmptyNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, topTreeView, currentTagName, childrenDictionary, alreadyAddedChildNodes);
            //If there is a top tree with no children and no attributes that means it's an empty tag.
            if (!hasChildren && topTreeView != null && attributes == null)
            {
                bool isChecked = alreadyAddedChildNodes != null && alreadyAddedChildNodes.Contains(currentTagName);
                topTreeView.Items.Add(new CheckBox() { Tag = wrapperKey, Content = "Add empty tag", IsChecked = isChecked });
            }
            topTreeView.Uid = wrapperKey;
            return topTreeView;
        }
        private static void AddNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderAsButton = (Button)sender;
            TreeViewItem sendersParent = (TreeViewItem)senderAsButton.Parent;
            XmlObjectsListWrapper xmlObjectsListWrapper = MainWindowViewController.NewObjectFormViewPanel.StackPanelLoadedListWrappers.GetValueOrDefault(sendersParent.Uid);
            string startingNodeName = senderAsButton.Uid;
            TreeViewItem newObjectFormTreeView = TreeViewGenerator.GetNewObjectFormTreeAddButton(xmlObjectsListWrapper, sendersParent.Uid, startingNodeName);
            newObjectFormTreeView.Uid = sendersParent.Uid.ToString();

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
        private static void AddHideAllAttributesFlagContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;
            SetAllTreeViewAttributesToHidden(senderTreeView);
        }
        private static void AddHideAttributesFlagContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;
            SetTreeViewAttributeBoxesToHidden(senderTreeView, senderAsMenuItem);
        }
        private static void SetTreeViewAttributeBoxesToHidden(TreeViewItem senderTreeView, MenuItem senderAsMenuItem)
        {
            bool isHidden = senderAsMenuItem.Header.ToString().Equals(UNHIDE_TREEVIEW_ATTRIBUTE_BOXES);
            if (isHidden)
            {
                //We want to unhide the attributes
                if (senderTreeView.Header.GetType() == typeof(string))
                {
                    senderTreeView.Header = senderTreeView.Header.ToString().Replace(XmlXpathGenerator.HEADER_UNUSED_ATTRIBUTES_STRING, "");
                }
                else if (senderTreeView.Header is Button senderTreeViewHeaderButton)
                {
                    senderTreeViewHeaderButton.Content = senderTreeViewHeaderButton.Content.ToString().Replace(XmlXpathGenerator.HEADER_UNUSED_ATTRIBUTES_STRING, "");
                }
                senderAsMenuItem.Header = HIDE_TREEVIEW_ATTRIBUTE_BOXES;
                AddAllRemovedAttributeBoxes(senderTreeView);
            }
            else
            {
                //We want to hide the attributes
                if (senderTreeView.Header.GetType() == typeof(string))
                {
                    senderTreeView.Header = senderTreeView.Header.ToString() + XmlXpathGenerator.HEADER_UNUSED_ATTRIBUTES_STRING;
                }
                else if (senderTreeView.Header is Button senderTreeViewHeaderButton)
                {
                    senderTreeViewHeaderButton.Content = senderTreeViewHeaderButton.Content.ToString() + XmlXpathGenerator.HEADER_UNUSED_ATTRIBUTES_STRING;
                }
                senderAsMenuItem.Header = UNHIDE_TREEVIEW_ATTRIBUTE_BOXES;
                RemoveUnusedAttributeComboBoxes(senderTreeView);
            }
        }
        private static void AddAllRemovedAttributeBoxes(TreeViewItem senderTreeView)
        {
            Stack<Tuple<Label, ComboBox, ComboBox>> hiddenBoxes = HiddenSearchTreeComboBoxDictionary.GetValueOrDefault(senderTreeView);
            if (hiddenBoxes == null)
            {
                HiddenSearchTreeComboBoxDictionary.Add(senderTreeView, new Stack<Tuple<Label, ComboBox, ComboBox>>());
                hiddenBoxes = HiddenSearchTreeComboBoxDictionary.GetValueOrDefault(senderTreeView);
            }
            int indexOfFirstChildTree = 0;
            foreach (Control nextControl in senderTreeView.Items)
            {
                if (nextControl is TreeViewItem) break;
                indexOfFirstChildTree++;
            }
            while (hiddenBoxes.Count > 0)
            {
                int indexToInsert = indexOfFirstChildTree;
                Tuple<Label, ComboBox, ComboBox> boxTuple = hiddenBoxes.Pop();
                if(boxTuple.Item3 != null)senderTreeView.Items.Insert(indexToInsert, boxTuple.Item3);
                if (boxTuple.Item2 != null) senderTreeView.Items.Insert(indexToInsert, boxTuple.Item2);
                if (boxTuple.Item1 != null) senderTreeView.Items.Insert(indexToInsert, boxTuple.Item1);
            }
        }
        private static void RemoveUnusedAttributeComboBoxes(TreeViewItem senderTreeView)
        {
            List<Tuple<Label, ComboBox, ComboBox>> boxesToRemove = new List<Tuple<Label, ComboBox, ComboBox>>();
            Stack<Tuple<Label, ComboBox, ComboBox>> hiddenBoxes = HiddenSearchTreeComboBoxDictionary.GetValueOrDefault(senderTreeView);
            if (hiddenBoxes == null)
            {
                HiddenSearchTreeComboBoxDictionary.Add(senderTreeView, new Stack<Tuple<Label, ComboBox, ComboBox>>());
                hiddenBoxes = HiddenSearchTreeComboBoxDictionary.GetValueOrDefault(senderTreeView);
            }
            bool previousWasLabel = false;
            int count = 0;
            foreach (Control currentControl in senderTreeView.Items)
            {
                if (currentControl is ComboBox && previousWasLabel)
                {
                    ComboBox currentControlAsComboBox = currentControl as ComboBox;
                    //If the current box is not empty make it null
                    if (!String.IsNullOrEmpty(currentControlAsComboBox.Text.Trim()))
                    {
                        previousWasLabel = false;
                        count++;
                        continue;
                    }
                    ComboBox modControlComboBoxBox = null;

                    //if the next control is a combobox 
                    int nextControlIndex = count + 1;
                    if (nextControlIndex < senderTreeView.Items.Count)
                    {
                        modControlComboBoxBox = senderTreeView.Items[nextControlIndex] as ComboBox;
                        //and check to see if a mod box even exists
                        if (modControlComboBoxBox != null)
                        {
                            //If the mod box string is not empty we set it to null so it isn't removed.
                            if (!String.IsNullOrEmpty(modControlComboBoxBox.Text.Trim()))
                            {
                                previousWasLabel = false;
                                count++;
                                continue;
                            }
                        }
                    }
                    Label attributeLabel = null;
                    int prevControlIndex = count - 1;
                    attributeLabel = senderTreeView.Items[prevControlIndex] as Label;
                    boxesToRemove.Add(new Tuple<Label, ComboBox, ComboBox>(attributeLabel, currentControlAsComboBox, modControlComboBoxBox));
                }
                previousWasLabel = false;
                if (currentControl is Label)
                {
                    previousWasLabel = true;
                }
                count++;
            }
            if (boxesToRemove.Count > 0)
            {
                foreach (Tuple<Label, ComboBox, ComboBox> boxToRemove in boxesToRemove)
                {
                    hiddenBoxes.Push(boxToRemove);
                    if (boxToRemove.Item1 != null) senderTreeView.Items.Remove(boxToRemove.Item1);
                    if (boxToRemove.Item2 != null) senderTreeView.Items.Remove(boxToRemove.Item2);
                    if(boxToRemove.Item3 != null) senderTreeView.Items.Remove(boxToRemove.Item3);
                }
            }
        }
        public static TreeViewItem GetNewObjectFormTreeAddButton(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, string startingXmlTagName, bool doSkipFirstAttributes = false)
        {
            if (startingXmlTagName.Length < 1) startingXmlTagName = xmlObjectListWrapper.FirstChildTagName;
            TreeViewItem newTopTree = new TreeViewItem
            {
                Header = startingXmlTagName,
                Uid = wrapperKey,
                FontSize = MainWindowViewController.ObjectTreeFontChange + 4,
            };
            return SetEmptyNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newTopTree, startingXmlTagName, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue(), doSkipFirstAttributes);
        }
        private static bool SetNextEmptyNewObjectFormChildren(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, string tagName, Dictionary<string, Queue<string>> allChildrenDictionary, HashSet<string> alreadyAddedChildNodes = null)
        {
            bool hasChildren = false;
            Queue<string> allChildren = allChildrenDictionary != null
                 ? allChildrenDictionary.GetValueOrDefault(tagName)
                 : null;

            if (allChildren != null && allChildren.Count > 0)
            {
                hasChildren = true;
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
                    TreeViewItem childrenTreeView = SetEmptyNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newChildTopTree, nextChild, allChildrenDictionary, alreadyAddedChildNodes: alreadyAddedChildNodes);
                    topTreeView.Items.Add(childrenTreeView);
                    allChildren.TryDequeue(out nextChild);
                }
            }
            return hasChildren;
        }
        private static TreeViewItem SetNextObjectTreeViewAtrributes(List<string> attributes, XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, XmlNode currentNode = null)
        {
            TreeViewItem newAttributesViewItem = new TreeViewItem
            {
                FontSize = MainWindowViewController.ObjectTreeFontChange + 6,
            };
            foreach (string nextAttribute in attributes)
            {
                Label newLabel = new Label()
                {
                    Content = nextAttribute.Replace("_", "__"),
                    FontSize = MainWindowViewController.ObjectTreeFontChange,
                    Foreground = Brushes.Red
                };
                newAttributesViewItem.Items.Add(newLabel);
                ComboBox newAttributesComboBox = SetAttributesComboBox(xmlObjectListWrapper, currentTagName, nextAttribute);

                if (newAttributesComboBox != null)
                {
                    if (currentNode != null)
                    {
                        XmlAttribute currentNodeAttribute = currentNode.GetAttributeByName(nextAttribute);
                        if (currentNodeAttribute != null) newAttributesComboBox.Text = currentNodeAttribute.Value;
                    }
                    newAttributesViewItem.Items.Add(newAttributesComboBox);
                }
                ComboBox modFileAttributeBox;
                if (MainWindowViewController.IncludeAllModsCheckBox.IsChecked.Value)
                {
                    modFileAttributeBox = GetAllModsFileAttributeComboBox(xmlObjectListWrapper, currentTagName, nextAttribute);
                }
                else 
                {
                    modFileAttributeBox = GetModFileAttributeComboBox(xmlObjectListWrapper, currentTagName, nextAttribute);
                }
                if (modFileAttributeBox != null)
                {
                    modFileAttributeBox.AddToolTip("Common attributes from the mod(s) file");
                    newAttributesViewItem.Items.Add(modFileAttributeBox);
                }
            }
            return newAttributesViewItem;
        }
        private static ComboBox GetModFileAttributeComboBox(XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string nextAttribute)
        {
            XmlObjectsListWrapper modWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(Properties.Settings.Default.ModTagSetting + "_" + xmlObjectListWrapper.GenerateDictionaryKey());
            ComboBox newModAttributesComboBox = null;
            if (modWrapper != null)
            {
                newModAttributesComboBox = SetAttributesComboBox(modWrapper, currentTagName, nextAttribute);
            }
            return newModAttributesComboBox;
        }
        private static ComboBox GetAllModsFileAttributeComboBox(XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string nextAttribute)
        {
            List<string> allMods = XmlFileManager.GetCustomModFoldersInOutput();
            HashSet<string> allCommonAttributes = new HashSet<string>();
            foreach (string nextMod in allMods) 
            {
                XmlObjectsListWrapper modWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(nextMod + "_" + xmlObjectListWrapper.GenerateDictionaryKey());
                if (modWrapper != null) 
                {
                    Dictionary<string, List<string>> attributesDictionary = modWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(currentTagName);
                    List<string> attributeCommon = attributesDictionary != null ? attributesDictionary.GetValueOrDefault(nextAttribute) : null;
                    if(attributeCommon != null)allCommonAttributes.UnionWith(attributeCommon);                
                }
            }
            ComboBox boxToReturn = allCommonAttributes.Count == 0 ? null 
                : SetAttributesComboBox(xmlObjectListWrapper, currentTagName, nextAttribute, allCommonAttributes.ToList());
            return boxToReturn;
        }
        private static ComboBox SetAttributesComboBox(XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string nextAttribute, List<string> allCommonAttributes = null)
        {
            ComboBox newAttributesComboBox = null;
            if (allCommonAttributes == null)
            {
                Dictionary<string, List<string>> attributesDictionary = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(currentTagName);
                List<string> attributeCommon = attributesDictionary?.GetValueOrDefault(nextAttribute);
                newAttributesComboBox = attributeCommon?.CreateComboBoxFromList(forgroundColor: Brushes.Blue);
            }
            else 
            {
                newAttributesComboBox = allCommonAttributes.CreateComboBoxFromList(forgroundColor: Brushes.Blue);
            }
;
            if (newAttributesComboBox != null)
            {
                newAttributesComboBox.FontSize = MainWindowViewController.ObjectTreeFontChange;
                newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                newAttributesComboBox.Tag = nextAttribute;
                newAttributesComboBox.AddToolTip("Common attributes from the game file");
            }
            return newAttributesComboBox;
        }
        private static void NewAttributesComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            MainWindowViewController.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(MainWindowViewController.NewObjectFormViewPanel);
        }
        private static void NewAttributesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            MainWindowViewController.XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(MainWindowViewController.NewObjectFormViewPanel);
        }
        private static void RemoveTreeNewObjectsContextMenu_Click(object sender, RoutedEventArgs e)
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
                            MainWindowViewController.NewObjectFormViewPanel.Children.Remove(senderTreeView);
                            break;
                    }
                }
            } 
            else if (contextMenu.PlacementTarget is Control myControl) 
            {
                if (myControl.Parent is MyStackPanel parent) parent.Children.Remove(myControl);
            }
        }
        private static TreeViewItem GetNewObjectFormTreeWithExistingData(XmlObjectsListWrapper xmlObjectListWrapper, XmlNode startingNode, string wrapperKey)
        {
            TreeViewItem newTreeView = SetNewObjectFormTreeWithExistingData(xmlObjectListWrapper, wrapperKey, startingNode, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue());
            newTreeView.FontSize = MainWindowViewController.ObjectTreeFontChange + 6;
            newTreeView.IsExpanded = true;
            newTreeView.AddToolTip("Here you can create new " + startingNode.Name + " tags");

            return newTreeView;
        }
        private static TreeViewItem SetNewObjectFormTreeWithExistingData(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, XmlNode currentNode, Dictionary<string, Queue<string>> allChildrenDictionary, string nodeName = null)
        {
            string nodeNameToUse = String.IsNullOrEmpty(nodeName) ? currentNode.Name : nodeName;
            List<string> attributes = xmlObjectListWrapper.objectNameToAttributesMap.GetValueOrDefault(nodeNameToUse);
            TreeViewItem newTreeView = new TreeViewItem();
            if (attributes != null) newTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, nodeNameToUse, currentNode);
            newTreeView = SetNextNewObjectWithExistingDataFormChildren(xmlObjectListWrapper, wrapperKey, newTreeView, currentNode, allChildrenDictionary, nodeNameToUse);
            if (newTreeView != null) 
            {
                newTreeView.AddToolTip("Edit form for the " + nodeNameToUse + " object");
                string attributeValue = currentNode != null && currentNode.GetAvailableAttribute() != null ?
                    ":" + currentNode.GetAvailableAttribute().Value : "";
                Button addNewObjectButton = new Button
                {
                    Content = nodeNameToUse.Replace("_", "__") + attributeValue.Replace("_", "__"),
                    Uid = nodeNameToUse, 
                    FontSize = MainWindowViewController.ObjectTreeFontChange + 4,
                    Foreground = Brushes.Purple,
                    Background = Brushes.White
                };
                addNewObjectButton.AddToolTip("Click to add another " + nodeNameToUse + " object\n(*) means there are hidden attributes");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                newTreeView.Header = addNewObjectButton;
                newTreeView.AddContextMenu(AddHideAllAttributesFlagContextMenu_Click, "Toggle Hide/Unhide All", "Click here to hide/unhide all unused attributes in the entire tree");
                newTreeView.AddContextMenu(AddHideAttributesFlagContextMenu_Click, HIDE_TREEVIEW_ATTRIBUTE_BOXES, "Click here to hide all unused attributes in the tree");
                newTreeView.Tag = false;
                newTreeView.AddContextMenu(AddIgnoreFlagToTreeContextMenu_Click, "Toggle Ignore", "Click here to flag the tree for ignore when clicking Save All XML ");
                newTreeView.AddContextMenu(RemoveTreeNewObjectsContextMenu_Click, "Remove Object", "Click here to remove this tag from the tree");

                newTreeView.Uid = wrapperKey;
            }
            return newTreeView;
        }
        private static TreeViewItem SetNextNewObjectWithExistingDataFormChildren(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, XmlNode currentNode, Dictionary<string, Queue<string>> allChildrenDictionary, string nodeName = null)
        {
            HashSet<string> childNodeNames = new HashSet<string>();
            if (currentNode != null && currentNode.HasChildNodes)
            {
                foreach (XmlNode nextChildNode in currentNode.ChildNodes)
                {
                    TreeViewItem childTree = SetNewObjectFormTreeWithExistingData(xmlObjectListWrapper, wrapperKey, nextChildNode, allChildrenDictionary);
                    if (childTree != null)
                    {
                        topTreeView.Items.Add(childTree);
                        childNodeNames.Add(nextChildNode.Name);
                    }
                }
            }
            SetNextEmptyNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, topTreeView, currentNode.Name, allChildrenDictionary, childNodeNames);
            return topTreeView;
        }
        private static void CopyObject_ContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (MenuItem)sender;
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu != null ? parentMenu.PlacementTarget as TreeViewItem : null;
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
                XmlObjectsListWrapper modFileWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(senderTreeView.Uid);
                string mainWrapperKey = modFileWrapper.GenerateDictionaryKey();
                XmlObjectsListWrapper gameFileWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(mainWrapperKey);
                if (gameFileWrapper == null)
                {
                    MessageBox.Show("Missing the game xml for the object. Try loading the " + mainWrapperKey + ".xml file to have this functionality.", "Missing Game File", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                bool didSetTree = false;
                foreach (string nodeName in gameFileWrapper.allTopLevelTags)
                { 
                    if (nodeName.Contains(xmlNode.Name))
                    {
                        TreeViewItem newObjectFormTree = GetNewObjectFormTreeWithExistingData(gameFileWrapper, xmlNode, mainWrapperKey);
                        XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                        //Set the tag to be not ignored
                        newObjectFormTree.Tag = false;
                        newObjectFormTree.Foreground = Brushes.Purple;
                        newObjectFormTree.AddToolTip("Object tree for the " + senderAsMenuItem.Name + " action");

                        MainWindowViewController.NewObjectFormViewPanel.Children.Add(newObjectFormTree);
                        if (Properties.Settings.Default.IgnoreAllAttributesCheckbox) 
                        {
                            SetAllTreeViewAttributesToHidden(newObjectFormTree);
                        }
                        didSetTree = true;
                    }             
                }
                if(!didSetTree)
                {
                    MessageBox.Show("This action can only be done on a top level object, for example if this object were in a recipes file you would need to right click a recipe object. ");
                }
            }
        }
        public static TreeViewItem GetSearchTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, bool addContextMenu = true, bool includeChildrenInOnHover = false, bool includeComments = false)
        {
            XmlNodeList allObjects = xmlObjectListWrapper.xmlFile.xmlDocument.GetElementsByTagName(xmlObjectListWrapper.TopTagName);
            TreeViewItem topObjectsTreeView = new TreeViewItem()
            {
                Header = xmlObjectListWrapper.TopTagName,
                IsExpanded = true,
                FontSize = MainWindowViewController.SearchTreeFontChange + 3,
                Foreground = Brushes.Purple,
                Uid = wrapperKey
            };
            topObjectsTreeView = SetSearchTreeViewNextObject(topObjectsTreeView, allObjects, wrapperKey, xmlObjectListWrapper, addContextMenu, includeChildrenInOnHover, includeComments: includeComments);

            if (RemoveContextMenu == null)
            {
                RemoveContextMenu = topObjectsTreeView.AddContextMenu(RemoveTreeSearchViewContextMenu_Click,
                    "Remove Object",
                    "Click here to remove this tree from the view");
            }
            else 
            {
                topObjectsTreeView.ContextMenu = RemoveContextMenu;
            }
            ShowEditLoadError = false;
            return topObjectsTreeView;
        }
        private static void RemoveTreeSearchViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (sender as MenuItem);
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            if (parentMenu.PlacementTarget is TreeViewItem senderTreeView)
            {
                MainWindowViewController.SearchTreeFormViewPanel.Children.Remove(senderTreeView);
            }
        }
        //Make sure to reset this after any calls to SetSearchTreeViewObject
        private static bool ShowEditLoadError { get; set; } = false;
        private static TreeViewItem SetSearchTreeViewNextObject(TreeViewItem topObjectsTreeView, XmlNodeList allObjects, string wrapperName, XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true, bool includeChildrenInOnHover = false, bool includeComments = false)
        {
            foreach (XmlNode nextObjectNode in allObjects)
            {
                TreeViewItem nextNewTreeViewItem = null;
                if (nextObjectNode.Name.Contains("#text"))
                {
                    nextNewTreeViewItem = new TreeViewItem
                    {
                        FontSize = MainWindowViewController.SearchTreeFontChange,
                        Header = "Inner Text = " + nextObjectNode.InnerText.Trim()
                    };
                }
                else if (nextObjectNode.Name.Contains("#comment"))
                {
                    if (includeComments)
                    {
                        nextNewTreeViewItem = new TreeViewItem
                        {
                            FontSize = MainWindowViewController.SearchTreeFontChange,
                            Header = "#comment"
                        };
                        TreeViewItem innerTextTreeView = new TreeViewItem
                        {
                            FontSize = MainWindowViewController.SearchTreeFontChange,
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
        private static void AddSearchTreeContextMenu(TreeViewItem nextNewTreeViewItem, XmlObjectsListWrapper xmlObjectListWrapper, string nextObjectNodeName, bool isGameFileTree)
        {
            if (isGameFileTree)
            {
                if (GameFileXpathContextMenuWithCopy == null)
                {
                    AddTargetContextMenuToControl(nextNewTreeViewItem, xmlObjectListWrapper);
                    GameFileXpathContextMenuWithCopy = nextNewTreeViewItem.AddContextMenu(CopyObject_ContextMenuClick,
                        "Copy Object",
                        "Click here to copy the object to left panel to make edits.");
                }
                else nextNewTreeViewItem.ContextMenu = GameFileXpathContextMenuWithCopy;
            }
            else 
            {
                ContextMenu collapseParentMenu = nextNewTreeViewItem.AddContextMenu(CollapseParentContextMenu_ClickFunction,
                    "Collapse Parent",
                    "Click here to collapse the parent tree");
                if (ModFileXpathContextMenuWithCopy == null)
                {
                    ModFileXpathContextMenuWithCopy = nextNewTreeViewItem.AddContextMenu(CopyObject_ContextMenuClick,
                        "Copy Object",
                        "Click here to copy the object to left panel to make edits.");
                }
                else nextNewTreeViewItem.ContextMenu = ModFileXpathContextMenuWithCopy;
            }
        }
        public static ContextMenu AddTargetContextMenuToControl(Control controlToAddMenu, XmlObjectsListWrapper modListWrapper = null, bool isAttributeControl = false)
        {
            TreeViewItem controlAsTreeView = controlToAddMenu as TreeViewItem;
            TextBox controlAsTextBox = controlToAddMenu as TextBox;
            if ((controlAsTreeView != null) || (controlAsTextBox != null) || isAttributeControl) 
            {
                controlToAddMenu.AddContextMenu(GenerateXpathActionTreeContextMenu_ClickFunction,
                    "Append",
                    "The append command is used to add either more nodes or more attribute values",
                    XmlXpathGenerator.XPATH_ACTION_APPEND);
            }
            ContextMenu menuToReturn = controlToAddMenu.AddContextMenu(GenerateXpathActionTreeContextMenu_ClickFunction,
                "Remove",
                "The remove command is used to remove nodes or attributes",
                XmlXpathGenerator.XPATH_ACTION_REMOVE);
            controlToAddMenu.AddContextMenu(GenerateXpathActionTreeContextMenu_ClickFunction,
                "Insert After",
                "Much like append, insertAfter will add nodes and attributes after the selected xpath",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER);
            controlToAddMenu.AddContextMenu(GenerateXpathActionTreeContextMenu_ClickFunction,
                "Insert Before",
                "Much like insertAfter, insertBefore will add nodes and attributes before the selected xpath",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE);
            if (isAttributeControl) controlToAddMenu.AddContextMenu(GenerateXpathActionTreeContextMenu_ClickFunction,
                "Set",
                "The set command is used to change individual attributes",
                xpathAction: XmlXpathGenerator.XPATH_ACTION_SET);
            else controlToAddMenu.AddContextMenu(GenerateXpathActionTreeContextMenu_ClickFunction,
               "Set Attribute",
               "The setattribute command is used to add a new attribute to an XML node",
               xpathAction: XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE);
            controlToAddMenu.AddContextMenu(CollapseParentContextMenu_ClickFunction,
               "Collapse Parent",
               "Click here to collapse the parent tree");
            if (modListWrapper != null && controlAsTextBox != null)
            {
                XmlObjectsListWrapper standardFileWrapper = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(modListWrapper.GenerateDictionaryKey());
                string nodeName = controlAsTextBox.Uid;
                if (standardFileWrapper != null && standardFileWrapper.allTopLevelTags.Contains(nodeName))
                {
                    controlToAddMenu.AddContextMenu(CopyObject_ContextMenuClick,
                        "Copy Object",
                        "Click here to copy the object to left panel to make edits.");
                }
                else if (standardFileWrapper == null && !ShowEditLoadError)
                {
                    MessageBox.Show(
                        "Failed loading the copy functionality for the mod file. That means you will be unable to copy objects using the search tree.\n\n" +
                        "To fix this you need to load the game xml file " + modListWrapper.xmlFile.FileName + ".",
                        "Failed Loading Edit Functionality",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    ShowEditLoadError = true;
                }
            }
            return menuToReturn;
        }
        private static void CollapseParentContextMenu_ClickFunction(object sender, RoutedEventArgs e)
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
        private static void GenerateXpathActionTreeContextMenu_ClickFunction(object sender, RoutedEventArgs e)
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
                XmlObjectsListWrapper wrapperToUse = MainWindowViewController.SearchTreeFormViewPanel.StackPanelLoadedListWrappers.GetValueOrDefault(senderTreeView.Uid.ToString());
                XmlNode xmlNode = senderTreeView.Tag as XmlNode;
                string attributeNameForAction = "";
                TreeViewItem newObjectFormTree; 
                //If there is an attribute Create a Special Object View with just the box for the attribute or a holder for the xml to generate.
                
                //Target attrbute
                if (senderTreeView.Name.Equals(XmlXpathGenerator.ATTRIBUTE_NAME))
                {
                    string attributeName = senderTreeView.Header.ToString().Trim();
                    string attributeValue = (senderTreeView.Items.GetItemAt(0) as TreeViewItem).Header.ToString().Trim();

                    attributeNameForAction = "_" + attributeName;
                    newObjectFormTree = GenerateNewObjectAttributeTree(senderAsMenuItem, wrapperToUse, xmlNode, attributeName, attributeValue);
                }
                //Set attribute
                else if (xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE))
                {
                    XmlAttribute avaliableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeName = avaliableAttribute == null ? "" : avaliableAttribute.Name;
                    string attributeValue = avaliableAttribute == null ? "" : avaliableAttribute.Value;
                    newObjectFormTree = GenerateNewObjectAttributeTree(senderAsMenuItem, wrapperToUse, xmlNode, attributeName, attributeValue);
                }
                //Target a node with insertBefore/after, append
                else if (!xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_REMOVE))
                {
                    string nodeName = xmlNode.Name;
                    bool doSkipAttributes = true;
                    if (xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE) || xPathAction.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER))
                    {
                        if (!wrapperToUse.allTopLevelTags.Contains(xmlNode.Name)) nodeName = xmlNode.ParentNode.Name;
                        else doSkipAttributes = false;
                    }
                    newObjectFormTree = GetNewObjectFormTreeAddButton(wrapperToUse, senderTreeView.Uid.ToString(), nodeName, doSkipAttributes);
                    XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                    string attributeValue = avalailableAttribute == null ? "" : ": " + avalailableAttribute.Name + "=" + avalailableAttribute.Value;
                    newObjectFormTree.Header = xmlNode.Name + attributeValue + " (" + xPathAction + ") ";
                }
                //Target remove
                else
                {
                    newObjectFormTree = new TreeViewItem { FontSize = MainWindowViewController.ObjectTreeFontChange + 4 };
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
                SetAllTreeViewAttributesToHidden(newObjectFormTree);
                MainWindowViewController.NewObjectFormViewPanel.Children.Add(newObjectFormTree);
            }
        }
        private static void SetAllTreeViewAttributesToHidden(TreeViewItem newObjectFormTree)
        {
            ContextMenu newObjectFormTreeMenu = newObjectFormTree.ContextMenu;
            MenuItem newObjectFormTreeMenuItem = null;
            //Need menu item
            foreach (MenuItem nextMenuItem in newObjectFormTreeMenu.Items)
            {
                if (nextMenuItem.Header.ToString().Equals(HIDE_TREEVIEW_ATTRIBUTE_BOXES) || nextMenuItem.Header.ToString().Equals(UNHIDE_TREEVIEW_ATTRIBUTE_BOXES))
                {
                    newObjectFormTreeMenuItem = nextMenuItem;
                    break;
                }
            }
            SetTreeViewAttributeBoxesToHidden(newObjectFormTree, newObjectFormTreeMenuItem);
            if (newObjectFormTree.HasItems) 
            {
                foreach (TreeViewItem nextTreeView in newObjectFormTree.GetTreeViewChildren())
                {
                    SetAllTreeViewAttributesToHidden(nextTreeView);
                }
            }
        }
        private static TreeViewItem GenerateNewObjectAttributeTree(MenuItem senderAsMenuItem, XmlObjectsListWrapper xmlObjectListWrapper, XmlNode xmlNode, string xmlAttributeName, string xmlAttributeValue)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem
            {
                FontSize = MainWindowViewController.ObjectTreeFontChange + 4,
                //          Node Name           Attribute targeted            Xpath action              
                Header = xmlNode.Name + ": " + xmlAttributeName + "=" + xmlAttributeValue + " (" + senderAsMenuItem.Name + ") "
            };
            if (senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE) || senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER)
                || senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE))
            {
                TextBox attributeNameBox = new TextBox { Text = "NewName", FontSize = MainWindowViewController.ObjectTreeFontChange };
                attributeNameBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeNameBox.AddToolTip("Type the new attribute name here.");
                TreeViewItem newAttributeTreeView = new TreeViewItem
                {
                    FontSize = MainWindowViewController.ObjectTreeFontChange,
                    Header = attributeNameBox,
                    Name = XmlXpathGenerator.ATTRIBUTE_NAME
                };
                newObjectFormTree.Items.Add(newAttributeTreeView);
                TextBox attributeValueBox = new TextBox { Text = "NewValue", FontSize = MainWindowViewController.ObjectTreeFontChange };
                attributeValueBox.LostFocus += NewAttributesComboBox_LostFocus;
                attributeValueBox.AddToolTip("Type the new attribute value here.");
                TreeViewItem newAttributeValueTreeView = new TreeViewItem
                {
                    FontSize = MainWindowViewController.ObjectTreeFontChange,
                    Header = attributeValueBox,
                    Name = XmlXpathGenerator.ATTRIBUTE_VALUE

                };
                newObjectFormTree.Items.Add(newAttributeValueTreeView);
            }
            else if (!senderAsMenuItem.Name.Equals(XmlXpathGenerator.XPATH_ACTION_REMOVE))
            {
                List<string> attributeCommon = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(xmlNode.Name).GetValueOrDefault(xmlAttributeName);
                ComboBox newAttributesComboBox = attributeCommon != null ? attributeCommon.CreateComboBoxFromList(forgroundColor: Brushes.Blue) : null;
                if (newAttributesComboBox != null)
                {
                    newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                    newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                    newAttributesComboBox.Tag = xmlAttributeName;
                    newAttributesComboBox.AddToolTip("Here you can set the value of the " + xmlAttributeName + " for the " + xmlNode.Name);
                    TreeViewItem headerTreeView = new TreeViewItem
                    {
                        FontSize = MainWindowViewController.ObjectTreeFontChange,
                        Header = newAttributesComboBox,
                        Name = XmlXpathGenerator.ATTRIBUTE_VALUE
                    };
                    newObjectFormTree.Items.Add(headerTreeView);
                }
            }
            return newObjectFormTree;
        }
        private static TreeViewItem SetNextSearchTreeObject(XmlNode nextObjectNode, string wrapperKey, XmlObjectsListWrapper xmlObjectListWrapper, bool addContextMenu = true, bool includeChildrenInOnHover = false, bool includeComments = false)
        {
            StringBuilder onHoverStringBuilder = new StringBuilder();
            XmlAttribute nextAvailableAttribute = nextObjectNode.GetAvailableAttribute();
            string attributeValue = nextAvailableAttribute == null ? "" : ": " + nextAvailableAttribute.Value + " (" + nextAvailableAttribute.Name + ")";
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem
            {
                Uid = wrapperKey,
                FontSize = MainWindowViewController.SearchTreeFontChange,
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
                
                if (nextObjectNode.GetValidChildrenCount() > SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD)
                {
                    MakeSearchTreeView(nextObjectTreeViewItem, nextObjectNode, addContextMenu);
                }
                else if (includeChildrenInOnHover) onHoverStringBuilder.Append(GetChildrenNames(nextObjectNode.ChildNodes));

                nextObjectTreeViewItem = SetSearchTreeViewNextObject(nextObjectTreeViewItem, nextObjectNode.ChildNodes, wrapperKey, xmlObjectListWrapper, addContextMenu: addContextMenu, includeChildrenInOnHover: includeChildrenInOnHover, includeComments: includeComments);
            }
            if (!nextObjectNode.Name.Contains("#comment") && !String.IsNullOrEmpty(onHoverStringBuilder.ToString()))
                nextObjectTreeViewItem.AddToolTip(onHoverStringBuilder.ToString(), MainWindowViewController.SearchTreeFontChange, Brushes.Blue);
            return nextObjectTreeViewItem;
        }
        private static string GetChildrenNames(XmlNodeList childNodes)
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
        private static void MakeSearchTreeView(TreeViewItem nextObjectTreeViewItem, XmlNode nextObjectNode, bool isGameFileSearchTree = true)
        {
            //make a new treeview item with the box as the header add all children to that.
            List<string> attributeCommon = nextObjectNode.GetAllCommonAttributes();
            MyComboBox topTreeSearchBox = attributeCommon.CreateMyComboBoxList(nextObjectNode, isGameFileSearchTree: isGameFileSearchTree);
            XmlAttribute valueToUse = nextObjectNode.GetAvailableAttribute();
            string attributeValue = valueToUse != null ? ": " + valueToUse.Value + " (" + valueToUse.Name + ") " : "";
            //Wrapper key
            topTreeSearchBox.Uid = nextObjectTreeViewItem.Uid;
            topTreeSearchBox.Text = nextObjectNode.Name + attributeValue;
            topTreeSearchBox.Foreground = Brushes.Purple;
            topTreeSearchBox.FontSize = MainWindowViewController.SearchTreeFontChange;
            topTreeSearchBox.DropDownClosed += TopTreeSearchBox_DropDownClosed;
            topTreeSearchBox.PreviewKeyDown += TopTreeSearchBox_KeyEnterDown_Focus;
            topTreeSearchBox.AddToolTip(nextObjectNode.Name + attributeValue + " search box. ");
            nextObjectTreeViewItem.Header = topTreeSearchBox;
        }
        private static void TopTreeSearchBox_KeyEnterDown_Focus(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                SearchBoxUpdate(sender);
            }
        }
        private static void TopTreeSearchBox_DropDownClosed(object sender, EventArgs e)
        {
            SearchBoxUpdate(sender);
        }
        private static void SearchBoxUpdate(object sender)
        {
            MyComboBox senderAsBox = (MyComboBox)sender;
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
        private static string SetNextObjectSearchTreeViewAtrributes(TreeViewItem nextObjectTreeViewItem, XmlAttributeCollection attributes, string wrapperKey, XmlNode currentNode, bool addContextMenu = true)
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
                    FontSize = MainWindowViewController.SearchTreeFontChange
                };
                TreeViewItem attributeValueTree = new TreeViewItem
                {
                    Header = nextAttribute.Value,
                    Foreground = Brushes.Blue,
                    Uid = wrapperKey,
                    FontSize = MainWindowViewController.SearchTreeFontChange
                };
                if (!nextAttribute.Name.Contains("#whitespace"))
                {
                    stringBuilder.AppendLine(nextAttribute.Name + " : " + nextAttribute.Value);
                    attributeValueTree.AddToolTip("You can click me to copy the value.\n You can also click the main object to copy the first attribute.");
                    attributeValueTree.PreviewMouseDown += NewObjectTreeAttributeCombo_MouseDown;
                    if (addContextMenu)
                    {
                        if (XmlAttributeContextMenu == null) XmlAttributeContextMenu = AddTargetContextMenuToControl(attributeNameTree, isAttributeControl: true);
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
        private static void NewObjectTreeAttributeCombo_MouseDown(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(((TreeViewItem)sender).Header.ToString());
        }
        private static void NewObjectTreeObjectCombo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem senderAsTreeViewItem = sender as TreeViewItem;
            XmlNode treeViewNode = senderAsTreeViewItem.Tag as XmlNode;
            XmlAttribute nextAvailableAttribute = treeViewNode.GetAvailableAttribute();
            string attributeValue = nextAvailableAttribute != null ? nextAvailableAttribute.Value : null;
            if(!String.IsNullOrEmpty(attributeValue))Clipboard.SetText(attributeValue);
        }
    }
}
