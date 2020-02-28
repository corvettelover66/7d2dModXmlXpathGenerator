using Microsoft.Win32;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace SevenDaysToDieModCreator.Controllers
{
    class MainWindowViewController
    {
        private const int FONT_SIZE = 20;
        private const string COUNTER_DICTIONARY_SPLITTER = "1";
        private StackPanel newObjectFormsPanel { get; set; }
        private TextBox xmlOutBlock { get; set; }

        public List<XmlObjectsListWrapper> listWrappersInObjectView { get; private set; }
        public List<XmlObjectsListWrapper> listWrappersInTreeView { get; private set; }

        private List<ComboBox> recentComboBoxList { get; set; }

        //A dictionary for keeping track of TreeViews when using the search bar filter
        //Key file name for a wrapper
        //Value a List of TreeViews that were removed in the last search
        private Dictionary<string, List<TreeViewItem>> removedTreeViews { get; set; }
        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> loadedListWrappers { get; private set; }
        //A dictionary of objects pertaining to the edit view
        //Key a complex key using tag name + list size + attribute name
        //Value the Windows Control object for the proggramitacaly generated edit view
        public Dictionary<string, Control> editFormsDictionary { get; set; }
        //A dictionary counters for the objects above
        //Key a complex key using tag name + current count
        //Value a list of strings that will fill with empty strings as a count
        public Dictionary<string, List<string>> editFormsDictionaryCounter { get; set; }
        public MainWindowViewController(StackPanel createView, TextBox xmlOutBlock) 
        {
            this.loadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.editFormsDictionary = new Dictionary<string, Control>();
            this.editFormsDictionaryCounter = new Dictionary<string, List<string>>();
            this.listWrappersInObjectView = new List<XmlObjectsListWrapper>();
            this.listWrappersInTreeView = new List<XmlObjectsListWrapper>();
            this.recentComboBoxList = new List<ComboBox>();
            this.removedTreeViews = new Dictionary<string, List<TreeViewItem>>();
            this.newObjectFormsPanel = createView;
            this.xmlOutBlock = xmlOutBlock;
        }
        public void LoadStartingDirectory(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox)
        {
            if (!Directory.Exists(XmlFileManager.LOCAL_DIR)) Directory.CreateDirectory(XmlFileManager.LOCAL_DIR);
            string[] files = Directory.GetFiles(XmlFileManager.LOCAL_DIR);
            foreach (string file in files) 
            {
                LoadFile(allLoadedFilesBox, allLoadedObjectsBox, file, true);
            }
        }
        public void ResetCreateView() 
        {
            foreach (XmlObjectsListWrapper xmlObjectsListWrapper in this.listWrappersInObjectView)
            {
                string xmltoWrite = XmlXpathGenerator.GenerateXmlWithWrapper(this.newObjectFormsPanel, xmlObjectsListWrapper, true);
                XmlFileManager.WriteXmlToLog(xmltoWrite, xmlObjectsListWrapper.xmlFile.FileName);
            }
            this.editFormsDictionary.Clear();
            this.editFormsDictionaryCounter.Clear();
            this.newObjectFormsPanel.Children.Clear();
            this.listWrappersInObjectView.Clear();
        }
        public void Save()
        {
            foreach (XmlObjectsListWrapper xmlObjectsListWrapper in this.listWrappersInObjectView)
            {
                string xmltoWrite = XmlXpathGenerator.GenerateXmlWithWrapper(this.newObjectFormsPanel, xmlObjectsListWrapper, true);
                XmlFileManager.WriteStringToFile(XmlFileManager._ModPath, xmlObjectsListWrapper.xmlFile.FileName, xmltoWrite, true);
            }
        }

        public void LoadFilesViewControl(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox) 
        {
            List<string> unloadedFiles = new List<string>();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Length > 1) 
                {
                    foreach (string nextFileName in openFileDialog.FileNames) 
                    {
                        bool didLoad = LoadFile(allLoadedFilesBox, allLoadedObjectsBox, nextFileName, false);
                        if (!didLoad) unloadedFiles.Add(nextFileName);
                    }
                }
                else 
                {
                    bool didLoad = LoadFile(allLoadedFilesBox, allLoadedObjectsBox, openFileDialog.FileName, false);
                    if (!didLoad) unloadedFiles.Add(openFileDialog.FileName);
                }
            }
            //There were files with problems
            if(unloadedFiles.Count > 0)
            {
                string allFilesString = "";
                foreach(string nextFile in unloadedFiles) 
                {
                    allFilesString += nextFile + "\n";
                }
                string messageBoxText = "Some files did not load correctly! \nFiles:\n" + allFilesString + "Only xml files can be loaded.";
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }
        private bool LoadFile(ComboBox allLoadedFilesBox, ComboBox allLoadedObjectsBox, string fileName, bool isStartup) 
        {
            bool didLoad = false;
            if (fileName.EndsWith(".xml"))
            {
                XmlObjectsListWrapper wrapper = new XmlObjectsListWrapper(new XmlFileObject(fileName));
                try
                {
                    if (!isStartup) File.Copy(fileName, XmlFileManager.LOCAL_DIR + wrapper.xmlFile.FileName);
                    this.loadedListWrappers.Add(wrapper.xmlFile.GetFileNameWithoutExtension(), wrapper);
                    allLoadedFilesBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    allLoadedObjectsBox.AddUniqueValueTo(wrapper.xmlFile.GetFileNameWithoutExtension());
                    didLoad = true;
                }
                catch (IOException) 
                {
                    string messageBoxText = "You can't load another file with the same name!\nTry to change the name of the file.";
                    string caption = "Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
            }
            return didLoad;
        }
        public void CreateNewObjectFormTree(StackPanel newObjectFormView, XmlObjectsListWrapper xmlObjectListWrapper) 
        {
            Label topTreeLabel = new Label { Content = xmlObjectListWrapper.TopTagName, FontSize = FONT_SIZE};
            newObjectFormView.Children.Add(topTreeLabel);

            foreach (string topTag in xmlObjectListWrapper.allTopLevelTags) 
            {
                TreeViewItem returnedTree = CreateNewObjectFormTree(xmlObjectListWrapper, topTag, xmlObjectListWrapper.TopTagName);
                returnedTree.Tag = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension();
                if (xmlObjectListWrapper.TopTagName == StringConstants.PROGRESSION_TAG_NAME) returnedTree.Name = xmlObjectListWrapper.TopTagName;
                newObjectFormView.Children.Add(returnedTree);
            }
        }
        public TreeViewItem CreateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, string tagName, string tagLevelString) 
        {
            TreeViewItem newObjectFormTree = new TreeViewItem();
            newObjectFormTree.Header = tagLevelString;
            newObjectFormTree.FontSize = FONT_SIZE;
            newObjectFormTree.IsExpanded = true;
            newObjectFormTree.AddOnHoverMessage("Here you can create new " + tagName + " tags");

            List<string> countList = editFormsDictionaryCounter.GetValueAndAddIfNotExist(tagLevelString + COUNTER_DICTIONARY_SPLITTER);
            string formDictionaryKey = tagName + countList.Count;
            editFormsDictionary.AddValueIfNotInDictionary(formDictionaryKey, newObjectFormTree);
            newObjectFormTree = GenerateNewObjectFormTree(xmlObjectListWrapper, newObjectFormTree, tagName, tagLevelString+COUNTER_DICTIONARY_SPLITTER, tagLevelString);
            return newObjectFormTree;
        }
        public TreeViewItem GenerateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, TreeViewItem topTreeView, string currentTagName, string counterDictionaryKey, string formDictionaryKey, bool doSkipFirstAttributeSet = false)
        {
            counterDictionaryKey += currentTagName + COUNTER_DICTIONARY_SPLITTER;
            List<string> countList = editFormsDictionaryCounter.GetValueAndAddIfNotExist(counterDictionaryKey + COUNTER_DICTIONARY_SPLITTER);
            formDictionaryKey += currentTagName + countList.Count;

            List<string> attributes = xmlObjectListWrapper.objectNameToAttributesMap.GetValueOrDefault(currentTagName);
            if (attributes != null && !doSkipFirstAttributeSet) topTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, currentTagName, counterDictionaryKey, formDictionaryKey);

            if (topTreeView != null)
            {
                topTreeView.FontSize = FONT_SIZE;
                topTreeView.AddOnHoverMessage("Edit form for the " + currentTagName + " object");
                Button addNewObjectButton = new Button { Content = currentTagName, Name = formDictionaryKey, Tag = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension() };
                if(!currentTagName.Equals(StringConstants.RECIPE_TAG_NAME)) addNewObjectButton.AddOnHoverMessage("Click to add another " + currentTagName + " object");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                addNewObjectButton.Width = 250;
                topTreeView.Header = addNewObjectButton;
                if (currentTagName.Equals(StringConstants.RECIPE_TAG_NAME))
                {
                    addNewObjectButton.AddOnHoverMessage("Click to add another " + currentTagName + " object, right click to add learning perks." );
                }
                editFormsDictionary.AddValueIfNotInDictionary(formDictionaryKey, topTreeView);
            }

            SetNextObjectCreateViewChildren(attributes, xmlObjectListWrapper, topTreeView, currentTagName, counterDictionaryKey, formDictionaryKey);
 
            return topTreeView;
        }

        public TreeViewItem GenerateNewObjectFormTreeAddButton(XmlObjectsListWrapper xmlObjectListWrapper, string startingXmlTagName, string formDictionaryKey, bool doSkipFirstAttributes = false)
        {
            this.recentComboBoxList.Clear();
            if (startingXmlTagName.Length < 1) startingXmlTagName = xmlObjectListWrapper.FirstChildTagName;
            string startingFormDictionaryCounterString = "";
            if (startingXmlTagName != xmlObjectListWrapper.FirstChildTagName) startingFormDictionaryCounterString = formDictionaryKey;
            TreeViewItem newTopTree = new TreeViewItem();
            newTopTree.Header = startingXmlTagName;
            return GenerateNewObjectFormTree(xmlObjectListWrapper, newTopTree, startingXmlTagName, startingFormDictionaryCounterString, formDictionaryKey, doSkipFirstAttributes);
        }
        private void AddNewObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderAsButton = (Button)sender;
            XmlObjectsListWrapper xmlObjectsListWrapper = this.loadedListWrappers.GetWrapperFromDictionary(senderAsButton.Tag.ToString());
            TreeViewItem sendersTree = (TreeViewItem)editFormsDictionary.GetValueOrDefault(senderAsButton.Name);
            TreeViewItem newObjectFormTreeView = GenerateNewObjectFormTreeAddButton(xmlObjectsListWrapper, senderAsButton.Content.ToString(), senderAsButton.Name);
            if (sendersTree.Parent.GetType() == typeof(StackPanel))
            {
                Label topTreeLabel = new Label { Content = xmlObjectsListWrapper.TopTagName, FontSize = FONT_SIZE };
                ((StackPanel)sendersTree.Parent).Children.Add(topTreeLabel);
                ((StackPanel)sendersTree.Parent).Children.Add(newObjectFormTreeView);
            }
            else ((TreeViewItem)sendersTree.Parent).Items.Add(newObjectFormTreeView);
        }
        private void SetNextObjectCreateViewChildren(List<string> attributes, XmlObjectsListWrapper xmlObjectListWrapper, TreeViewItem topTreeView, string tagName, string counterDictionaryKey, string formDictionaryKey )
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
                        editFormsDictionary.Add(formDictionaryKey+"propertyinner", innerPropertyTreeView);
                        if (attributes != null) innerPropertyTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, tagName, counterDictionaryKey, formDictionaryKey);
                        innerPropertyTreeView.AddOnHoverMessage("Edit form for the " + tagName + " object");
                        Button addNewObjectButton = new Button { Content = tagName, Name = formDictionaryKey, Tag = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension() };
                        addNewObjectButton.AddOnHoverMessage("Click to add another " + tagName);
                        addNewObjectButton.Width = 250;
                        innerPropertyTreeView.Header = addNewObjectButton;
                        topTreeView.Items.Add(innerPropertyTreeView);
                    }
                    else
                    {
                        TreeViewItem newChildTopTree = new TreeViewItem();
                        TreeViewItem childrenTreeView = GenerateNewObjectFormTree(xmlObjectListWrapper, newChildTopTree, childName, counterDictionaryKey, formDictionaryKey);
                        topTreeView.Items.Add(childrenTreeView);
                    }
                }
            }
        }

        private TreeViewItem SetNextObjectTreeViewAtrributes(List<string> attributes, XmlObjectsListWrapper xmlObjectListWrapper, string currentTagName, string counterDictionaryKey, string formDictionaryKey)
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
                newAttributesComboBox.AddOnHoverMessage("Here you can set the value of the "+ nextAttribute + " for the " + currentTagName);
                if (newAttributesComboBox == null) newAttributesViewItem.Items.Add(new ComboBox());
                else
                {
                    editFormsDictionaryCounter.GetValueAndAddIfNotExist(counterDictionaryKey);
                    recentComboBoxList.Add(newAttributesComboBox);
                    newAttributesViewItem.Items.Add(newAttributesComboBox);
                }
            }
            return newAttributesViewItem;
        }
        //All the same  abstract away
        private void NewAttributesComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormsPanel, this.listWrappersInObjectView, this.xmlOutBlock);
        }

        private void NewAttributesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormsPanel, this.listWrappersInObjectView, this.xmlOutBlock);
        }

        public TreeViewItem GetObjectTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper) 
        {
            XmlNodeList allObjects = xmlObjectListWrapper.xmlFile.xmlDocument.GetElementsByTagName(xmlObjectListWrapper.TopTagName);
            //ViewSp.Children.Add(treeViewSearchBox);
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
                    topObjectsTreeView.Items.Add(nextTreeView);
                }
            }
            return topObjectsTreeView;
        }
        private TreeViewItem SetNextObject(XmlNode nextObjectNode, string wrapperKey, XmlObjectsListWrapper xmlObjectListWrapper) 
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
                    if (children != null && children.Count > 0)  SetTreeViewSearchBoxHeader(nextObjectTreeViewItem, xmlObjectListWrapper, children[0]);
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
                        tagAttributeName = nextObjectTreeViewItem.Tag == null ? parent.Attributes[0].Value : nextObjectTreeViewItem.Tag.ToString();
                    }
                    Button makeObjectATargetButton = new Button { 
                        Content = nextObjectNode.Name + ":" + tagAttributeName, 
                        Name = wrapperKey, 
                        Tag = nextObjectNode };
                    makeObjectATargetButton.AddOnHoverMessage("Click to make this an Xpath target for a new object ");
                    makeObjectATargetButton.Width = 250;
                    makeObjectATargetButton.Click += MakeObjectATargetButton_Click;
                    nextObjectTreeViewItem.Header = makeObjectATargetButton;
                }
                nextObjectTreeViewItem = SetObjectTreeView(nextObjectTreeViewItem, nextObjectNode.ChildNodes, wrapperKey, xmlObjectListWrapper);
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
        private void MakeObjectATargetButton_Click(object sender, RoutedEventArgs e)
        {
            Button senderAsButton = (Button)sender;
            XmlObjectsListWrapper wrapperToUse = this.loadedListWrappers.GetValueOrDefault(senderAsButton.Name.ToString());
            if(!this.listWrappersInObjectView.Contains(wrapperToUse))this.listWrappersInObjectView.Add(wrapperToUse);
            string[] contentSplit = senderAsButton.Content.ToString().Split(":");

            TreeViewItem newObjectFormTree = GenerateNewObjectFormTreeAddButton(wrapperToUse, contentSplit[0], contentSplit[0], true);
            //Set the name to the wrapper so we can find the wrapper later
            newObjectFormTree.Name = senderAsButton.Name.ToString();
            //set the xmlNode that was included with the object into the top tree view
            newObjectFormTree.Tag = senderAsButton.Tag;
            //The button should be in the form "TagName:AttribiuteNameVaue"
            if (contentSplit.Length > 1)
            {
                newObjectFormTree.Header = senderAsButton.Content.ToString();
            }
            //There is the edge case where the object did not have a name value to use
            else
            {
                newObjectFormTree.Header = ((Button)newObjectFormTree.Header).Content;
            }
            newObjectFormTree.AddOnHoverMessage("Using this form you can add new objects into the " + newObjectFormTree.Header + " object\n" +
                "For Example: You want to add an ingredient into a certain, existing, recipe.");
            this.newObjectFormsPanel.Children.Add(newObjectFormTree);
            ResetComboBoxLostFocus(this.recentComboBoxList);
            //OpenTargetDialogWindow(newObjectFormTree, wrapperToUse, senderAsButton.Content.ToString().Split(":")[1]);
        }
        private void ResetComboBoxLostFocus(List<ComboBox> recentComboBoxList)
        {
            foreach (ComboBox nextBox in recentComboBoxList)
            {
                nextBox.LostFocus -= NewAttributesComboBox_LostFocus;
                nextBox.LostFocus += NextBox_LostFocus;
                nextBox.DropDownClosed -= NewAttributesComboBox_DropDownClosed;
                nextBox.DropDownClosed += TargetObjectViewComboBox_LostFocus;
            }
        }

        private void NextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormsPanel, this.listWrappersInObjectView, this.xmlOutBlock);
        }

        private void TargetObjectViewComboBox_LostFocus(object sender, EventArgs e)
        {
            XmlXpathGenerator.GenerateXmlViewOutput(this.newObjectFormsPanel, this.listWrappersInObjectView, this.xmlOutBlock);
        }
        private void TopTreeSearchBox_KeyDown_Focus(object sender, KeyEventArgs e)
        {
            ComboBox senderAsBox = (ComboBox)sender;
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return)
            {
                XmlObjectsListWrapper currentWrapper = this.loadedListWrappers.GetValueOrDefault(senderAsBox.Tag.ToString());
                SearchBoxUpdate(sender, this.removedTreeViews.GetValueOrDefault(currentWrapper.xmlFile.GetFileNameWithoutExtension()));
            }
        }
        private void TopTreeSearchBox_LostKeyboard_Focus(object sender, EventArgs e)
        {
            ComboBox senderAsComboBox = (ComboBox)sender;
            XmlObjectsListWrapper currentWrapper = this.loadedListWrappers.GetValueOrDefault(senderAsComboBox.Tag.ToString());
            List<TreeViewItem> removedTreeList = this.removedTreeViews.GetValueOrDefault(currentWrapper.xmlFile.GetFileNameWithoutExtension());
            if (removedTreeList == null)
            {
                removedTreeList = new List<TreeViewItem>();
                this.removedTreeViews.Add(currentWrapper.xmlFile.GetFileNameWithoutExtension(), removedTreeList);
            }
            SearchBoxUpdate(sender, removedTreeList);
        }
        private void SearchBoxUpdate(object sender, List<TreeViewItem> removedTreeViews) 
        {
            ComboBox senderAsBox = (ComboBox)sender;
            TreeViewItem topTreeView = (TreeViewItem)senderAsBox.Parent;
            foreach (TreeViewItem removedTreeView in removedTreeViews)
            {
                topTreeView.Items.Add(removedTreeView);
            }
            removedTreeViews.Clear();
            string contents = senderAsBox.Text;
            List<TreeViewItem> children = GetChildren(topTreeView);
            List<TreeViewItem> treesToAdd = new List<TreeViewItem>();
            foreach (TreeViewItem nextTreeViewItem in children)
            {
                if (nextTreeViewItem.Tag.ToString().ToLower().Contains(contents.ToLower()))
                {
                    treesToAdd.Add(nextTreeViewItem);
                }
                //if the object should be removed
                else
                {
                    removedTreeViews.Add(nextTreeViewItem);
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
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem { FontSize = FONT_SIZE - 5  };

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
            Clipboard.SetText(((TextBox)sender).Text.Split("=")[1]);
        }

        private void OpenTargetDialogWindow(TreeViewItem newObjectFormTree, XmlObjectsListWrapper wrapperToUse, string tagAttributeNameValue)
        {
            var dialog = new TargetObjectView(newObjectFormTree, this.recentComboBoxList, wrapperToUse, tagAttributeNameValue);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    //string name = XmlConvert.VerifyName(dialog.ResponseText);
                    //XmlXpathGenerator.MyCustomTagName = name;
                    //XmlFileManager.WriteStringToFile(XmlFileManager._filePath, XmlXpathGenerator.CustomTagFileName, name);
                }
                catch (XmlException)
                {
                    MessageBox.Show("The format was incorrect, the name must follow xml naming rules!", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (ArgumentNullException)
                {
                    MessageBox.Show("The format was incorrect, you must include something!", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AddContextMenu(Control objectControl)
        {
            ContextMenu newButtonRightClickMenu = new ContextMenu();
            MenuItem addUnlockingContextMenu = new MenuItem();
            addUnlockingContextMenu.Header = "Make Target";
            addUnlockingContextMenu.Click += AddUnlockingContextMenu_Click; ;
            newButtonRightClickMenu.Items.Add(addUnlockingContextMenu);
            objectControl.ContextMenu = newButtonRightClickMenu;
        }
        private void AddUnlockingContextMenu_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
