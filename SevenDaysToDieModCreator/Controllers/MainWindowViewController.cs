using Microsoft.Win32;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
                string xmltoWrite = XmlXpathGenerator.GenerateXmlByWrapper(this.newObjectFormsPanel, xmlObjectsListWrapper);
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
                string xmltoWrite = XmlXpathGenerator.GenerateXmlByWrapper(this.newObjectFormsPanel, xmlObjectsListWrapper, true);
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
        public TreeViewItem GenerateNewObjectFormTree(XmlObjectsListWrapper xmlObjectListWrapper, TreeViewItem topTreeView, string currentTagName, string counterDictionaryKey, string formDictionaryKey)
        {
            TreeViewItem newObjectTreeView = new TreeViewItem();
            newObjectTreeView.Header = currentTagName;

            counterDictionaryKey += currentTagName + COUNTER_DICTIONARY_SPLITTER;
            List<string> countList = editFormsDictionaryCounter.GetValueAndAddIfNotExist(counterDictionaryKey + COUNTER_DICTIONARY_SPLITTER);
            formDictionaryKey += currentTagName + countList.Count;

            List<string> attributes = xmlObjectListWrapper.objectNameToAttributesMap.GetValueOrDefault(currentTagName);
            if (attributes != null) newObjectTreeView = SetNextObjectTreeViewAtrributes(attributes, xmlObjectListWrapper, currentTagName, counterDictionaryKey, formDictionaryKey);

            if (newObjectTreeView != null)
            {
                newObjectTreeView.FontSize = FONT_SIZE;
                newObjectTreeView.AddOnHoverMessage("Edit form for the " + currentTagName + " object");
                Button addNewObjectButton = new Button { Content = currentTagName, Name = formDictionaryKey, Tag = xmlObjectListWrapper.xmlFile.GetFileNameWithoutExtension() };
                addNewObjectButton.AddOnHoverMessage("Click to add another " + currentTagName + " object");
                addNewObjectButton.Click += AddNewObjectButton_Click;
                addNewObjectButton.Width = 250;
                newObjectTreeView.Header = addNewObjectButton;
                editFormsDictionary.AddValueIfNotInDictionary(formDictionaryKey, newObjectTreeView);
            }

            SetNextObjectCreateViewChildren(attributes, xmlObjectListWrapper, newObjectTreeView, currentTagName, counterDictionaryKey, formDictionaryKey);
 
            return newObjectTreeView;
        }
        public TreeViewItem GenerateNewObjectFormTreeAddButton(XmlObjectsListWrapper xmlObjectListWrapper, string tagName, string formDictionaryKey)
        {
            if (tagName.Length < 1) tagName = xmlObjectListWrapper.FirstChildTagName;
            string startingFormDictionaryCounterString = "";
            if (tagName != xmlObjectListWrapper.FirstChildTagName) startingFormDictionaryCounterString = formDictionaryKey;
            TreeViewItem newTopTree = new TreeViewItem();
            newTopTree.Header = tagName;
            return GenerateNewObjectFormTree(xmlObjectListWrapper, newTopTree, tagName, startingFormDictionaryCounterString, formDictionaryKey); ;
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
                        TreeViewItem childrenTreeView = GenerateNewObjectFormTree(xmlObjectListWrapper, null, childName, counterDictionaryKey, formDictionaryKey);
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
                newAttributesComboBox.LostFocus += NewAttributesComboBox_IsKeyboardFocusedChanged;
                newAttributesComboBox.Tag = nextAttribute;
                newAttributesComboBox.AddOnHoverMessage("Here you can set the value of the "+ nextAttribute + " for the " + currentTagName);
                if (newAttributesComboBox == null) newAttributesViewItem.Items.Add(new ComboBox());
                else
                {
                    editFormsDictionaryCounter.GetValueAndAddIfNotExist(counterDictionaryKey);
                    newAttributesViewItem.Items.Add(newAttributesComboBox);
                }
            }
            return newAttributesViewItem;
        }

        private void NewAttributesComboBox_IsKeyboardFocusedChanged(object sender, RoutedEventArgs e)
        {
            string addedViewTextStart = "WARNING: Direct text edits made here will NOT be saved.\n\n" +
            "<!-- -------------------------------------- Current Unsaved XML ----------------------------------- -->\n\n";
            string unsavedGeneratedXmlEnd = "\n\n<!-- --------------------------------------------------------------------------------------------------------- -->\n\n";
            string allWrappersText = "<!-- SAVED DATA  -->\n\n";
            foreach (XmlObjectsListWrapper nextWrapper in this.listWrappersInObjectView) 
            {
                allWrappersText += XmlFileManager.GetFileContents(XmlFileManager._ModPath, (nextWrapper.xmlFile.FileName));
            }

            this.xmlOutBlock.Text = addedViewTextStart + XmlXpathGenerator.GenerateFullXml(this.newObjectFormsPanel, this.loadedListWrappers, true)+ unsavedGeneratedXmlEnd + allWrappersText;
        }

        public TreeViewItem GetObjectTreeViewRecursive(XmlObjectsListWrapper xmlObjectListWrapper) 
        {
            XmlNodeList allObjects = xmlObjectListWrapper.xmlFile.xmlDocument.GetElementsByTagName(xmlObjectListWrapper.TopTagName);
            TextBox treeViewSearchBox = new TextBox()
            {
                Tag = "TreeViewSearchBox",
                Text = xmlObjectListWrapper.TopTagName,
                IsReadOnly = true
            };
            //ViewSp.Children.Add(treeViewSearchBox);
            TreeViewItem topObjectsTreeView = new TreeViewItem()
            {
                Header = xmlObjectListWrapper.TopTagName,
                IsExpanded = true,
                FontSize = FONT_SIZE
            };

            topObjectsTreeView = SetObjectTreeView(topObjectsTreeView, allObjects);
            return topObjectsTreeView;
        }
        private TreeViewItem SetObjectTreeView(TreeViewItem topObjectsTreeView, XmlNodeList allObjects)
        {
            foreach (XmlNode nextObjectNode in allObjects) 
            {
                TreeViewItem nextObject = SetNextObject(nextObjectNode);
                if(nextObject != null) topObjectsTreeView.Items.Add(nextObject);
            }
            return topObjectsTreeView;
        }
        private TreeViewItem SetNextObject(XmlNode nextObjectNode) 
        {
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem { FontSize = FONT_SIZE };
            if (nextObjectNode.Attributes != null) nextObjectTreeViewItem = SetNextObjectTreeViewAtrributes(nextObjectNode.Attributes, nextObjectNode.Name);
            if (nextObjectNode.HasChildNodes) nextObjectTreeViewItem = SetObjectTreeView(nextObjectTreeViewItem, nextObjectNode.ChildNodes);
            if (nextObjectNode.Name.Contains("#") || nextObjectNode == null) nextObjectTreeViewItem = null;
            return nextObjectTreeViewItem;
        }
        private TreeViewItem SetNextObjectTreeViewAtrributes(XmlAttributeCollection attributes, string objectNodeName)
        {
            TreeViewItem nextObjectTreeViewItem = new TreeViewItem { FontSize = FONT_SIZE - 5 };

            foreach (XmlAttribute nextAttribute in attributes)
            {
                if (nextAttribute.Name.Contains("name"))
                {
                    nextObjectTreeViewItem.Header = objectNodeName +" : "+ nextAttribute.Value;
                }
                if (!nextAttribute.Name.Contains("#whitespace"))
                {
                    TextBox attributeBox = new TextBox
                    {
                        Text = nextAttribute.Name + " = " + nextAttribute.Value,
                        IsReadOnly = true
                    };
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
    }
}
