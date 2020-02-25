using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace SevenDaysToDieModCreator.Extensions
{
    static class MyExtensions
    {
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
        public static bool AddOnHoverMessage(this Control controlObject, string onHoverMessage)
        {
            bool isHoverMessageAdded = true;
            if (onHoverMessage.Length < 1) isHoverMessageAdded = false;
            ToolTip newToolTip = new ToolTip();
            TextBlock myTip = new TextBlock { Text = onHoverMessage };
            newToolTip.Content = myTip;
            controlObject.ToolTip = newToolTip;
            return isHoverMessageAdded;
        }
        //Creates a ComboBox using a list object
        public static ComboBox CreateComboBoxList<T>(this IList<T> listToUse, string name = null)
        {
            ComboBox newBox = new ComboBox();
            newBox.IsEditable = true;
            if (name != null) newBox.Name = name;
            List<ComboBoxItem> allItems = new List<ComboBoxItem>();
            allItems.Add(new ComboBoxItem());
            foreach (T nextString in listToUse)
            {
                ComboBoxItem newItem = new ComboBoxItem
                {
                    Content = nextString.ToString()
                };
                allItems.Add(newItem);
            }
            newBox.ItemsSource = allItems;
            return newBox;
        }
        public static bool HasString(this ComboBox listToUse, string value)
        {
            bool hasString = false;
            List<ComboBoxItem> allItems = (List<ComboBoxItem>)listToUse.ItemsSource;
            if (allItems != null)
            {
                foreach (ComboBoxItem nextBox in allItems)
                {
                    if (nextBox.Content.ToString().Contains(value)) hasString = true;
                }
            }
            return hasString;
        }
        public static void AddUniqueValueTo(this ComboBox boxToAddTo, string valueToAdd)
        {
            ObservableCollection<string> allItems = (ObservableCollection<string>)boxToAddTo.ItemsSource;
            if (allItems == null)
            {
                allItems = new ObservableCollection<string>();
            }
            if(!allItems.Contains(valueToAdd))allItems.Add(valueToAdd);

            boxToAddTo.ItemsSource = allItems;
        }
    }
}
