using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace SevenDaysToDieModCreator.Extensions
{
    static class MyExtensions
    {
        public static List<TreeViewItem> GetTreeViewChildren(this StackPanel parent)
        {
            List<TreeViewItem> children = new List<TreeViewItem>();

            if (parent != null)
            {
                foreach (var item in parent.Children)
                {
                    TreeViewItem child = item as TreeViewItem;
                    if (child != null) children.Add(child);
                }
            }

            return children;
        }
        public static List<TreeViewItem> GetTreeViewChildren(this ItemsControl parent)
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
        public static int GetValidChildrenCount(this XmlNode xmlNode) 
        {
            int count = 0;
            foreach (XmlNode child in xmlNode.ChildNodes) 
            {
                if (!child.Name.Contains("#")) count++;
            }
            return count;
        }
        public static ContextMenu AddContextMenu(this Control objectControl, RoutedEventHandler myOnClickFunction, string headerText, string onHoverMessageText = "",  string xpathAction = "")
        {

            ContextMenu newButtonRightClickMenu = objectControl.ContextMenu == null ? new ContextMenu() : objectControl.ContextMenu;
            MenuItem newContextMenuItem = new MenuItem();
            if (!String.IsNullOrEmpty(onHoverMessageText)) newContextMenuItem.AddToolTip(onHoverMessageText);
            newContextMenuItem.Name = xpathAction;
            newContextMenuItem.Header = headerText;
            newContextMenuItem.Click += myOnClickFunction;
            newContextMenuItem.Tag = objectControl.GetType() == typeof(TextBox) ? objectControl.Tag : objectControl;
            newButtonRightClickMenu.Items.Add(newContextMenuItem);
            objectControl.ContextMenu = newButtonRightClickMenu;
            return newButtonRightClickMenu;
        }
        public static List<string> GetAllCommonAttributes(this XmlNode nextObjectNode)
        {
            List<string> allCommonAttributes = new List<string>();
            foreach (XmlNode nextNode in nextObjectNode.ChildNodes)
            {
                XmlAttribute nextAvaliableAttribute = nextNode.GetAvailableAttribute();
                if (nextAvaliableAttribute != null) allCommonAttributes.Add(nextAvaliableAttribute.Value);
            }
            return allCommonAttributes;
        }
        public static XmlAttribute GetAvailableAttribute(this XmlNode nextObjectNode)
        {
            XmlAttribute valueToReturn = null;
            if (nextObjectNode.Attributes != null)
            {
                XmlAttribute attributeToUse = null;
                foreach (XmlAttribute nextAttribute in nextObjectNode.Attributes)
                {
                    if (nextAttribute.Name.ToLower().Equals("name")) attributeToUse = nextAttribute;
                    if (nextAttribute.Name.ToLower().Equals("id") && attributeToUse == null) attributeToUse = nextAttribute;
                }
                if (attributeToUse != null) valueToReturn = attributeToUse;
                else
                {
                    foreach (XmlAttribute nextAttribute in nextObjectNode.Attributes)
                    {
                        valueToReturn = nextAttribute;
                        if (valueToReturn != null) break;
                    }
                }
            }
            return valueToReturn;
        }
        public static Queue<TreeViewItem> getChildTreeViewQueue(this TreeViewItem nextTreeItem, string childName)
        {
            Queue<TreeViewItem> allTreeViews = new Queue<TreeViewItem>();
            foreach (Control nextControl in nextTreeItem.Items)
            {
                if (nextControl.GetType() == typeof(TreeViewItem))
                {
                    TreeViewItem treeViewToReturn = (TreeViewItem)nextControl;
                    Button nextTreeItemHeader = (Button)treeViewToReturn.Header;
                    if(nextTreeItemHeader != null && nextTreeItemHeader.Content + "" == childName) allTreeViews.Enqueue(treeViewToReturn);
                }
            }
            return allTreeViews;
        }
        //Adds the XMLAttribute value to the used dictionary, while ensuring uniquness of the key and value
        public static void AddUniqueAttributeValueToMap(this Dictionary<string, List<string>> objectAttributesMap, XmlAttribute nextAttribute)
        {
            if (objectAttributesMap.ContainsKey(nextAttribute.Name))
            {
                List<string> commonValues = objectAttributesMap.GetValueOrDefault(nextAttribute.Name);
                commonValues.AddUnique(nextAttribute.Value);
            }
            else
            {
                List<string> attributeValues = new List<string>();
                attributeValues.Add(nextAttribute.Value);
                objectAttributesMap.Add(nextAttribute.Name, attributeValues);
            }
        }
        //Adds the string value to the used dictionary, while ensuring uniquness of the key and value
        public static void AddUniqueValueToMap(this Dictionary<string, List<string>> objectAttributesMap, string key, string valueToAdd)
        {
            if (objectAttributesMap.ContainsKey(key))
            {
                List<string> commonValues = objectAttributesMap.GetValueOrDefault(key);
                commonValues.AddUnique(valueToAdd);
            }
            else
            {
                List<string> attributeValues = new List<string>();
                attributeValues.Add(valueToAdd);
                objectAttributesMap.Add(key, attributeValues);
            }
        }
        //Adds the XMLAttribute value to the used dictionary, while ensuring uniquness of the key and value
        public static void AddUniqueAttributeToMapOfMaps(this Dictionary<string, Dictionary<string, List<string>>> objectAttributesMap, string key, XmlAttribute attribute)
        {
            if (objectAttributesMap.ContainsKey(key))
            {
                Dictionary<string, List<string>> commonValuesMap = objectAttributesMap.GetValueOrDefault(key);
                commonValuesMap.AddUniqueAttributeValueToMap(attribute);
            }
            else
            {
                Dictionary<string, List<string>> attributeValuesMap = new Dictionary<string, List<string>>();
                attributeValuesMap.AddUniqueAttributeValueToMap(attribute);
                objectAttributesMap.Add(key, attributeValuesMap);
            }
        }
        //Gets the wrapper by checking the keys and if the value has the key within
        public static XmlObjectsListWrapper GetWrapperFromDictionary(this Dictionary<string, XmlObjectsListWrapper> dictionaryToCheck, string value)
        {
            XmlObjectsListWrapper wrapperToReturn = null;
            foreach (string key in dictionaryToCheck.Keys) 
            {
                if (value.Contains(key)) wrapperToReturn = dictionaryToCheck.GetValueOrDefault(key);
            }
            return wrapperToReturn;
        }
        //Adds the value to the list, while ensuring uniqueness.
        public static bool AddUnique(this List<string> listToCheck, String value)
        {
            bool didAdd = false;
            if (listToCheck != null && !listToCheck.Contains(value))
            {
                listToCheck.Add(value);
                didAdd = true;
            }
            return didAdd;
        }
        public static List<string> GetValueAndAddIfNotExist(this Dictionary<string, List<string>> dictionaryToCheck, string value)
        {
            List<string> nextList = dictionaryToCheck.GetValueOrDefault(value);
            if (nextList == null)
            {
                nextList = new List<string>();
                nextList.Add("");
                dictionaryToCheck.Add(value, nextList);
            }
            return nextList;
        }
        public static void AddValueIfNotInDictionary(this Dictionary<string, Control> dictionaryToCheck, string key, Control value)
        {
            Control nextControl = dictionaryToCheck.GetValueOrDefault(key);
            if (nextControl == null) 
            {
                dictionaryToCheck.Add(key, value);
            }
        }
        //Adds a hover message to a button
        public static bool AddToolTip(this Control controlObject, string onHoverMessage, int fontSize = 0, SolidColorBrush forgroundColor = null)
        {
            bool isHoverMessageAdded = true;
            if (onHoverMessage.Length < 1) isHoverMessageAdded = false;
            ToolTip newToolTip = new ToolTip();
            TextBlock myTip = new TextBlock { Text = onHoverMessage};
            if (forgroundColor != null) myTip.Foreground = forgroundColor;
            if (fontSize > 0) myTip.FontSize = fontSize;
            newToolTip.Content = myTip;
            controlObject.ToolTip = newToolTip;
            return isHoverMessageAdded;
        }
        public static void SetComboBox<T>(this ComboBox comboBox, IList<T> listToUse, string name = null)
        {
            if (name != null) comboBox.Name = name;
            ObservableCollection<string> allItems = new ObservableCollection<string>();
            foreach (T nextString in listToUse)
            {
                allItems.Add(nextString.ToString());
            }
            comboBox.ItemsSource = allItems;
        }
        //Creates a ComboBox using a list object
        public static ComboBox CreateComboBoxList<T>(this IList<T> listToUse, string name = null, SolidColorBrush forgroundColor = null)
        {
            ComboBox newBox = new ComboBox();
            if (forgroundColor != null) newBox.Foreground = forgroundColor;
            newBox.IsEditable = true;
            newBox.SetComboBox(listToUse, name);
            return newBox;
        }
        public static MyComboBox CreateMyComboBoxList<T>(this IList<T> listToUse, ObjectViewController objectViewController, bool doAddContextMenu = true, string name = null)
        {
            MyComboBox newBox = new MyComboBox(objectViewController, doAddContextMenu);
            newBox.IsEditable = true;
            if (name != null) newBox.Name = name;
            ObservableCollection<string> allItems = new ObservableCollection<string>();
            foreach (T nextString in listToUse)
            {
                allItems.Add(nextString.ToString());
            }
            newBox.ItemsSource = allItems;
            return newBox;
        }
        public static void AddUniqueValueTo(this ComboBox boxToAddTo, string valueToAdd)
        {
            if (boxToAddTo.ItemsSource == null) 
            {
                ObservableCollection<string> allItems = new ObservableCollection<string>();
                allItems.Add(valueToAdd);
                boxToAddTo.ItemsSource = allItems;
                return;
            }
            else if (boxToAddTo.ItemsSource.GetType() == typeof(ObservableCollection<string>))
            {
                ObservableCollection<string> allItems = (ObservableCollection<string>)boxToAddTo.ItemsSource;
                if (allItems == null)
                {
                    allItems = new ObservableCollection<string>();
                }
                if (!allItems.Contains(valueToAdd)) allItems.Add(valueToAdd);
                boxToAddTo.ItemsSource = allItems;
            }
        }
        public static Dictionary<string, Queue<string>> GetDictionaryAsListQueue(this Dictionary<string, List<string>> dictionaryToUse) 
        {
            Dictionary<string, Queue<string>> dictionaryToReturn = new Dictionary<string, Queue<string>>();
            foreach (string key in dictionaryToUse.Keys) 
            {
                Queue<string> returnQ = new Queue<string>(dictionaryToUse.GetValueOrDefault(key));
                dictionaryToReturn.Add(key, returnQ);
            }
            return dictionaryToReturn;
        }
    }
}
