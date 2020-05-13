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
        private ContextMenu XmlAttributeContextMenu { get; set; }
        public int SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD { get; private set; } = 15;
        public MyStackPanel NewObjectFormViewPanel { get; set; }
        public MyStackPanel SearchTreeFormViewPanel { get; set; }

        private CheckBox IncludeAllModsCheckBox { get; set; }
        public ICSharpCode.AvalonEdit.TextEditor XmlOutBlock { get; private set; }
        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //Value The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        //A dictionary for getting a list of hidden comboboxes
        //Key The tree view with hidden boxes
        //Value Stack of hidden combo boxes
        private Dictionary<TreeViewItem, Stack<Tuple<Label, ComboBox, ComboBox>>> HiddenSearchTreeComboBoxDictionary { get; set; }

        private ContextMenu RemoveContextMenu { get; set; }
        private ContextMenu GameFileXpathContextMenuWithCopy { get; set; }
        private ContextMenu GameFileXpathContextMenuNoCopy { get; set; }
        private ContextMenu ModFileXpathContextMenuWithCopy { get; set; }
        private ContextMenu ModFileXpathContextMenuNoCopy { get; set; }
        public ObjectViewController(ICSharpCode.AvalonEdit.TextEditor xmlOutputBox, Dictionary<string, XmlObjectsListWrapper> loadedListWrappers, CheckBox includeAllModsCheckBox)
        {
            this.LoadedListWrappers = loadedListWrappers;
            this.XmlOutBlock = xmlOutputBox;
            SearchTreeFontChange = 0;
            this.IncludeAllModsCheckBox = includeAllModsCheckBox;
            this.HiddenSearchTreeComboBoxDictionary = new Dictionary<TreeViewItem, Stack<Tuple<Label, ComboBox, ComboBox>>>();
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
        public void CreateEmptyNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey)
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
                TreeViewItem returnedTree = GetEmptyNewObjectFormTree(xmlObjectListWrapper, topTag, wrapperKey);
                returnedTree.Uid = wrapperKey;
                returnedTree.AddContextMenu(this.RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");
                NewObjectFormViewPanel.Children.Add(returnedTree);
            }
        }
        private void AddIgnoreFlagToTreeContextMenu_Click(object sender, RoutedEventArgs e)
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
        public TreeViewItem GetEmptyNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string tagName, string wrapperKey)
        {
            TreeViewItem newObjectFormTree = new TreeViewItem
            {
                FontSize = OBJECT_VIEW_FONT_SIZE + 6 + ObjectTreeFontChange,
                IsExpanded = true
            };
            newObjectFormTree.AddToolTip("Here you can create new " + tagName + " tags");

            newObjectFormTree = SetEmptyNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newObjectFormTree, tagName, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue());
            return newObjectFormTree;
        }
        public TreeViewItem SetEmptyNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, string currentTagName, Dictionary<string, Queue<string>> childrenDictionary, bool doSkipFirstAttributeSet = false, HashSet<string> alreadyAddedChildNodes = null)
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
                    FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange,
                    Foreground = Brushes.Purple,
                    Background = Brushes.White
                };
                addNewObjectButton.AddToolTip("Click to add another " + currentTagName + " object\n(*) means there are hidden attributes");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                topTreeView.Header = addNewObjectButton;
                topTreeView.AddContextMenu(this.AddHideAttributesFlagContextMenu_Click, "Hide Unused Attributes", "Click here to hide all unused attributes in the tree");
                if (xmlObjectListWrapper.allTopLevelTags.Contains(currentTagName))
                {
                    topTreeView.Tag = false;
                    topTreeView.AddContextMenu(this.AddIgnoreFlagToTreeContextMenu_Click, "Toggle Ignore", "Click here to flag the tree for ignore when clicking Save All XML ");
                }
            }
            bool hasChildren = SetNextEmptyNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, topTreeView, currentTagName, childrenDictionary, alreadyAddedChildNodes);
            //If there is a top tree with no children and no attributes that means it's an empty tag.
            if (!hasChildren && topTreeView != null && attributes == null)
            {
                bool isChecked = alreadyAddedChildNodes != null && alreadyAddedChildNodes.Contains(currentTagName);
                topTreeView.Items.Add(new CheckBox() { Tag = wrapperKey, Content = "Add empty tag", IsChecked = isChecked });
            }
            return topTreeView;
        }
        private void AddHideAttributesFlagContextMenu_Click(object sender, RoutedEventArgs e)
        {
            const string hideString = "Hide Unused Attributes";
            const string unhideString = "Unhide Unused Attributes";
            const string headerAppendString = "(*)";

            MenuItem senderAsMenuItem = (MenuItem)sender;
            bool isHidden = senderAsMenuItem.Header.ToString().Equals(unhideString);
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            TreeViewItem senderTreeView = parentMenu.PlacementTarget as TreeViewItem;
            if (isHidden)
            {
                //We want to unhide the attributes
                if (senderTreeView.Header.GetType() == typeof(string)) 
                {
                    senderTreeView.Header = senderTreeView.Header.ToString().Replace(headerAppendString, "");
                }
                else if (senderTreeView.Header is Button senderTreeViewHeaderButton) 
                {
                    senderTreeViewHeaderButton.Content = senderTreeViewHeaderButton.Content.ToString().Replace(headerAppendString, "");
                }
                senderAsMenuItem.Header = hideString;
                AddAllRemovedAttributeBoxes(senderTreeView);
            }
            else
            {
                //We want to hide the attributes
                if (senderTreeView.Header.GetType() == typeof(string))
                {
                    senderTreeView.Header = senderTreeView.Header.ToString() + headerAppendString;
                }
                else if (senderTreeView.Header is Button senderTreeViewHeaderButton)
                {
                    senderTreeViewHeaderButton.Content = senderTreeViewHeaderButton.Content.ToString() + headerAppendString;
                }
                senderAsMenuItem.Header = unhideString;
                RemoveUnusedAttributeComboBoxes(senderTreeView);
            }
        }
        private void AddAllRemovedAttributeBoxes(TreeViewItem senderTreeView)
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
                senderTreeView.Items.Insert(indexToInsert, boxTuple.Item2);
                senderTreeView.Items.Insert(indexToInsert, boxTuple.Item1);
            }
        }
        private void RemoveUnusedAttributeComboBoxes(TreeViewItem senderTreeView)
        {
            List<Tuple<Label, ComboBox, ComboBox>> boxesToRemove = new List<Tuple<Label, ComboBox, ComboBox>>();
            Stack<Tuple<Label, ComboBox, ComboBox>> hiddenBoxes = HiddenSearchTreeComboBoxDictionary.GetValueOrDefault(senderTreeView);
            if (hiddenBoxes == null)
            {
                HiddenSearchTreeComboBoxDictionary.Add(senderTreeView, new Stack<Tuple<Label, ComboBox, ComboBox>>());
                hiddenBoxes = HiddenSearchTreeComboBoxDictionary.GetValueOrDefault(senderTreeView);
            }
            bool previousWasLabel = false;
            foreach (Control nextControl in senderTreeView.Items)
            {
                if (nextControl is Label) 
                {
                    previousWasLabel = true;
                }
                if (nextControl is ComboBox controlAsComboBox && previousWasLabel)
                {
                    if (String.IsNullOrEmpty(controlAsComboBox.Text.Trim()))
                    {
                        int labelIndex = senderTreeView.Items.IndexOf(controlAsComboBox) - 1;
                        int modBoxIndex = senderTreeView.Items.IndexOf(controlAsComboBox) + 1;
                        ComboBox modBox = modBoxIndex < senderTreeView.Items.Count ? senderTreeView.Items[modBoxIndex] as ComboBox : null;
                        boxesToRemove.Add(new Tuple<Label, ComboBox, ComboBox>(senderTreeView.Items[labelIndex] as Label, controlAsComboBox, modBox));
                    }
                    previousWasLabel = false;
                }
            }
            if (boxesToRemove.Count > 0)
            {
                foreach (Tuple<Label, ComboBox, ComboBox> boxToRemove in boxesToRemove)
                {
                    hiddenBoxes.Push(boxToRemove);
                    senderTreeView.Items.Remove(boxToRemove.Item1);
                    senderTreeView.Items.Remove(boxToRemove.Item2);
                    if(boxToRemove.Item3 != null) senderTreeView.Items.Remove(boxToRemove.Item3);
                }
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
            return SetEmptyNewObjectFormTree(xmlObjectListWrapper, wrapperKey, newTopTree, startingXmlTagName, xmlObjectListWrapper.objectNameToChildrenMap.GetDictionaryAsListQueue(), doSkipFirstAttributes);
        }
        private bool SetNextEmptyNewObjectFormChildren(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, string tagName, Dictionary<string, Queue<string>> allChildrenDictionary, HashSet<string> alreadyAddedChildNodes = null)
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
                    Content = nextAttribute.Replace("_", "__"),
                    FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange,
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
                if (this.IncludeAllModsCheckBox.IsChecked.Value)
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
        private ComboBox GetModFileAttributeComboBox(XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string nextAttribute)
        {
            XmlObjectsListWrapper modWrapper = this.LoadedListWrappers.GetValueOrDefault(Properties.Settings.Default.ModTagSetting + "_" + xmlObjectListWrapper.GenerateDictionaryKey());
            ComboBox newModAttributesComboBox = null;
            if (modWrapper != null)
            {
                newModAttributesComboBox = SetAttributesComboBox(modWrapper, currentTagName, nextAttribute);
            }
            return newModAttributesComboBox;
        }
        private ComboBox GetAllModsFileAttributeComboBox(XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string nextAttribute)
        {
            List<string> allMods = XmlFileManager.GetCustomModFoldersInOutput();
            HashSet<string> allCommonAttributes = new HashSet<string>();
            foreach (string nextMod in allMods) 
            {
                XmlObjectsListWrapper modWrapper = this.LoadedListWrappers.GetValueOrDefault(nextMod + "_" + xmlObjectListWrapper.GenerateDictionaryKey());
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
        private ComboBox SetAttributesComboBox(XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string nextAttribute, List<string> allCommonAttributes = null)
        {
            ComboBox newAttributesComboBox = null;
            if (allCommonAttributes == null)
            {
                Dictionary<string, List<string>> attributesDictionary = xmlObjectListWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(currentTagName);
                List<string> attributeCommon = attributesDictionary?.GetValueOrDefault(nextAttribute);
                newAttributesComboBox = attributeCommon?.CreateComboBoxList(forgroundColor: Brushes.Blue);
            }
            else 
            {
                newAttributesComboBox = allCommonAttributes.CreateComboBoxList(forgroundColor: Brushes.Blue);
            }
;
            if (newAttributesComboBox != null)
            {
                newAttributesComboBox.FontSize = OBJECT_VIEW_FONT_SIZE + ObjectTreeFontChange;
                newAttributesComboBox.DropDownClosed += NewAttributesComboBox_DropDownClosed;
                newAttributesComboBox.LostFocus += NewAttributesComboBox_LostFocus;
                newAttributesComboBox.Tag = nextAttribute;
                newAttributesComboBox.AddToolTip("Common attributes from the game file");
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
            TreeViewItem newTreeView = new TreeViewItem();
             newTreeView = attributes != null
                ? SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, nodeNameToUse, currentNode)
                : null;
            newTreeView = SetNextNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, newTreeView, currentNode, allChildrenDictionary, nodeNameToUse);
            if (newTreeView != null) 
            {
                newTreeView.AddToolTip("Edit form for the " + nodeNameToUse + " object");
                string attributeValue = currentNode != null && currentNode.GetAvailableAttribute() != null ?
                    ":" + currentNode.GetAvailableAttribute().Value : "";
                Button addNewObjectButton = new Button
                {
                    Content = nodeNameToUse.Replace("_", "__") + attributeValue.Replace("_", "__"),
                    Tag = wrapperKey,
                    FontSize = OBJECT_VIEW_FONT_SIZE + 4 + ObjectTreeFontChange,
                    Foreground = Brushes.Purple,
                    Background = Brushes.White
                };
                addNewObjectButton.AddToolTip("Click to add another " + nodeNameToUse + " object\n(*) means there are hidden attributes");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                newTreeView.Header = addNewObjectButton;
                newTreeView.AddContextMenu(this.AddHideAttributesFlagContextMenu_Click, "Hide Unused Attributes", "Click here to hide all unused attributes in the tree");
                if (xmlObjectListWrapper.allTopLevelTags.Contains(nodeNameToUse))
                {
                    newTreeView.AddContextMenu(this.AddIgnoreFlagToTreeContextMenu_Click, "Toggle Ignore", "Click here to flag the tree for ignore when clicking Save All XML ");
                }
            }
            return newTreeView;
        }
        private TreeViewItem SetNextNewObjectFormChildren(XmlObjectsListWrapper xmlObjectListWrapper, string wrapperKey, TreeViewItem topTreeView, XmlNode currentNode, Dictionary<string, Queue<string>> allChildrenDictionary, string nodeName = null)
        {
            HashSet<string> childNodeNames = new HashSet<string>();
            if (currentNode != null && currentNode.HasChildNodes)
            {
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
            SetNextEmptyNewObjectFormChildren(xmlObjectListWrapper, wrapperKey, topTreeView, currentNode.Name, allChildrenDictionary, childNodeNames);
            return topTreeView;
        }
        private void CopyObject_ContextMenuClick(object sender, RoutedEventArgs e)
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
                TreeViewItem newObjectFormTree = this.GetNewObjectFormTree(wrapperToUse, xmlNode, mainWrapperKey);
                XmlAttribute avalailableAttribute = xmlNode.GetAvailableAttribute();
                //Set the uid to the wrapper so we can find the wrapper later
                newObjectFormTree.Uid = mainWrapperKey;
                //Set the tag to be not ignored
                newObjectFormTree.Tag = false;
                newObjectFormTree.Foreground = Brushes.Purple;
                newObjectFormTree.AddToolTip("Object tree for the " + senderAsMenuItem.Name + " action");
                newObjectFormTree.AddContextMenu(RemoveTreeNewObjectsContextMenu_Click, "Remove Object From View");

                NewObjectFormViewPanel.Children.Add(newObjectFormTree);
                if (wrapperToUse.allTopLevelTags.Contains(xmlNode.Name))
                {
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
        private void RemoveTreeSearchViewContextMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem senderAsMenuItem = (sender as MenuItem);
            ContextMenu parentMenu = senderAsMenuItem.Parent as ContextMenu;
            if (parentMenu.PlacementTarget is TreeViewItem senderTreeView)
            {
                this.SearchTreeFormViewPanel.Children.Remove(senderTreeView);
            }
        }
        //Make sure to reset this after any calls to SetSearchTreeViewObject
        private bool ShowEditLoadError { get; set; } = false;
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
        private void AddSearchTreeContextMenu(TreeViewItem nextNewTreeViewItem, XmlObjectsListWrapper xmlObjectListWrapper, string nextObjectNodeName, bool isGameFileTree)
        {
            bool doAddCopyContextMenu = false;
            XmlObjectsListWrapper standardFileWrapper = this.LoadedListWrappers.GetValueOrDefault(xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension());
            if (standardFileWrapper != null && standardFileWrapper.allTopLevelTags.Contains(nextObjectNodeName))
            {
                doAddCopyContextMenu = true;
            }
            else if (standardFileWrapper == null && !ShowEditLoadError)
            {
                MessageBox.Show(
                    "Failed loading the copy object functionality for the mod file. That means you will be unable to copy objects using the search tree.\n\n" +
                    "To fix this you need to load the game xml file " + xmlObjectListWrapper.xmlFile.FileName + ".",
                    "Failed Loading Edit Functionality",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ShowEditLoadError = true;
            }

            doAddCopyContextMenu = true;
            if (isGameFileTree)
            {

                if (doAddCopyContextMenu)
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
                    if (GameFileXpathContextMenuNoCopy == null) GameFileXpathContextMenuNoCopy = AddTargetContextMenuToControl(nextNewTreeViewItem, xmlObjectListWrapper);
                    else nextNewTreeViewItem.ContextMenu = GameFileXpathContextMenuNoCopy;
                }
            }
            else 
            {
                ContextMenu collapseParentMenu = nextNewTreeViewItem.AddContextMenu(CollapseParentContextMenu_ClickFunction,
                    "Collapse Parent",
                    "Click here to collapse the parent tree");
                if (doAddCopyContextMenu)
                {
                    if (ModFileXpathContextMenuWithCopy == null)
                    {
                        ModFileXpathContextMenuWithCopy = nextNewTreeViewItem.AddContextMenu(CopyObject_ContextMenuClick,
                            "Copy Object",
                            "Click here to copy the object to left panel to make edits.");
                    }
                    else nextNewTreeViewItem.ContextMenu = ModFileXpathContextMenuWithCopy;
                }
                else 
                {
                    if (ModFileXpathContextMenuNoCopy == null)
                    {
                        ModFileXpathContextMenuNoCopy = collapseParentMenu;
                    }
                    else nextNewTreeViewItem.ContextMenu = ModFileXpathContextMenuNoCopy;
                }
            }
        }
        public ContextMenu AddTargetContextMenuToControl(Control controlToAddMenu, XmlObjectsListWrapper modListWrapper = null, bool isAttributeControl = false)
        {
            TreeViewItem controlAsTreeView = controlToAddMenu as TreeViewItem;
            TextBox controlAsTextBox = controlToAddMenu as TextBox;
            if ((controlAsTreeView != null) || (controlAsTextBox != null) || isAttributeControl) 
            {
                controlToAddMenu.AddContextMenu(AppendToContextMenu_ClickFunction,
                    "Append",
                    "The append command is used to add either more nodes or more attribute values",
                    XmlXpathGenerator.XPATH_ACTION_APPEND);
            }
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
            if (modListWrapper != null && controlAsTextBox != null)
            {
                XmlObjectsListWrapper standardFileWrapper = this.LoadedListWrappers.GetValueOrDefault(modListWrapper.xmlFile.GetFileNameWithoutExtension());
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
                        "Failed loading the edit functionality for the mod file. That means you will be unable to copy objects using the search tree.\n\n" +
                        "To fix this you need to load the game xml file " + modListWrapper.xmlFile.FileName + ".",
                        "Failed Loading Edit Functionality",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    ShowEditLoadError = true;
                }
            }
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
                
                if (nextObjectNode.GetValidChildrenCount() > SEARCH_VIEW_SEARCH_BOX_CREATION_THRESHOLD)
                {
                    MakeSearchTreeView(nextObjectTreeViewItem, nextObjectNode, addContextMenu);
                }
                else if (includeChildrenInOnHover) onHoverStringBuilder.Append(GetChildrenNames(nextObjectNode.ChildNodes));

                nextObjectTreeViewItem = SetSearchTreeViewNextObject(nextObjectTreeViewItem, nextObjectNode.ChildNodes, wrapperKey, xmlObjectListWrapper, addContextMenu: addContextMenu, includeChildrenInOnHover: includeChildrenInOnHover, includeComments: includeComments);
            }
            if (!nextObjectNode.Name.Contains("#comment") && !String.IsNullOrEmpty(onHoverStringBuilder.ToString()))
                nextObjectTreeViewItem.AddToolTip(onHoverStringBuilder.ToString(), SEARCH_VIEW_FONT_SIZE + SearchTreeFontChange, Brushes.Blue);
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
            MyComboBox topTreeSearchBox = attributeCommon.CreateMyComboBoxList(this, nextObjectNode, isGameFileSearchTree: isGameFileSearchTree);
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
