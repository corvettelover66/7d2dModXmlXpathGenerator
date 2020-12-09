using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace SevenDaysToDieModCreator.Extensions
{
    static class MyExtensions
    {
        public static string GridAsCSV(this Grid gridToTraverse) 
        {
            string csvToReturn = "";
            int startingRow = 0;
            foreach (UIElement element in gridToTraverse.Children)
            {
                if (element is TextBox textbox && textbox.Tag != null)
                {
                    int currentRow = int.Parse(textbox.Tag.ToString());
                    if (startingRow != currentRow)
                    {
                        //Remove the last comma
                        csvToReturn = csvToReturn.Trim(',');
                        //Add a newline
                        csvToReturn += "\n";
                    }
                    if (textbox.Text.Contains(",")) csvToReturn += "\"" + textbox.Text + "\"" + ",";
                    else csvToReturn += textbox.Text + ",";
                    //If the row has changed set the tracker
                    if (startingRow != currentRow) startingRow = currentRow;
                }
                if (element is ComboBox comboBox && comboBox.Tag != null) 
                {
                    int currentRow = int.Parse(comboBox.Tag.ToString());
                    if (startingRow != currentRow)
                    {
                        //Remove the last comma
                        csvToReturn = csvToReturn.Trim(',');
                        //Add a newline
                        csvToReturn += "\n";
                    }
                    if (comboBox.Text.Contains(",")) csvToReturn += "\"" + comboBox.Text + "\"" + ",";
                    else csvToReturn += comboBox.Text + ",";
                    //If the row has changed set the tracker
                    if (startingRow != currentRow) startingRow = currentRow;
                }
            }
            //Trim the last comma
            csvToReturn = csvToReturn.Trim(',');
            return csvToReturn;
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
        public static void CopyComboBoxesTreeView(this TreeViewItem treeViewToCopy, TreeViewItem treeViewToCopyInto) 
        {
            if (treeViewToCopyInto == null) return;
            int count = 0;
            foreach (Control control in treeViewToCopy.Items) 
            {
                if (control is ComboBox treeViewToCopyBox) 
                {
                    if (treeViewToCopyInto.Items.Count  > 0 && treeViewToCopyInto.Items[count] is ComboBox treeViewToCopyIntoBox) 
                        treeViewToCopyIntoBox.Text = treeViewToCopyBox.Text;
                }
                if (control is TreeViewItem innerView) 
                {
                    if (treeViewToCopyInto.Items.Count > 0) innerView.CopyComboBoxesTreeView(treeViewToCopyInto.Items[count] as TreeViewItem);
                }
                count++;
            }
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
        public static XmlAttribute GetAttributeByName(this XmlNode xmlNode, string attributeName)
        {
            XmlAttribute attributeToReturn = null;
            foreach (XmlAttribute nextAttribute in xmlNode.Attributes)
            {
                if (nextAttribute.Name.Equals(attributeName)) 
                {
                    attributeToReturn = nextAttribute;
                    break;
                }
            }
            return attributeToReturn;
        }
        public static ContextMenu AddContextMenu(this Control objectControl, RoutedEventHandler myOnClickFunction, string headerText, string onHoverMessageText = "", string xpathAction = "")
        {
            ContextMenu newButtonRightClickMenu = objectControl.ContextMenu ?? new ContextMenu();
            if (newButtonRightClickMenu.Tag == null) newButtonRightClickMenu.Tag = objectControl.GetType() == typeof(TextBox) ? objectControl.Tag : objectControl;
            MenuItem newContextMenuItem = new MenuItem
            {
                Name = xpathAction,
                Header = headerText
            };
            if (!String.IsNullOrEmpty(onHoverMessageText)) newContextMenuItem.AddToolTip(onHoverMessageText);
            newContextMenuItem.Click += myOnClickFunction;
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
        public static XmlNode GetChildNodeByName(this XmlNode nextObjectNode, string name)
        {
            XmlNode nodeToReturn = null;
            foreach (XmlNode nextNode in nextObjectNode.ChildNodes)
            {
                if (nextNode.Name.Equals(name)) 
                {
                    nodeToReturn = nextNode;
                    break;
                }
            }
            return nodeToReturn;
        }
        //GetChildNodeByName
        public static XmlAttribute GetAvailableAttribute(this XmlNode nextObjectNode)
        {
            XmlAttribute valueToReturn = null;
            if (nextObjectNode != null && nextObjectNode.Attributes != null)
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
                List<string> attributeValues = new List<string>
                {
                    nextAttribute.Value
                };
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
                List<string> attributeValues = new List<string>
                {
                    valueToAdd
                };
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
        //Dictionary used to store any tooltips that have been created to reuse objects to save used memory.
        //Key the tooltip message
        //Value the existing tooltip for the message.
        private static readonly Dictionary<string, ToolTip> addedToolTips = new Dictionary<string, ToolTip>();
        //Adds a hover message to an object control
        public static bool AddToolTip(this Control controlObject, string onHoverMessage, int fontSize = 0, SolidColorBrush forgroundColor = null)
        {
            bool isHoverMessageAdded = true;
            if (String.IsNullOrEmpty(onHoverMessage)) isHoverMessageAdded = false;

            ToolTip newToolTip = addedToolTips.GetValueOrDefault(onHoverMessage);
            if (newToolTip == null)
            {
                newToolTip = new ToolTip();
                TextBlock myTip = new TextBlock { Text = onHoverMessage };
                if (forgroundColor != null) myTip.Foreground = forgroundColor;
                if (fontSize > 0) myTip.FontSize = fontSize;
                newToolTip.Content = myTip;
                addedToolTips.Add(onHoverMessage, newToolTip);
            }
            controlObject.ToolTip = newToolTip;
            return isHoverMessageAdded;
        }
        public static void SetComboBox<T>(this ComboBox comboBox, IList<T> listToUse, bool includeEmptyItem = false,  string name = null)
        {
            comboBox.Background = BackgroundColorController.GetBackgroundColor();
            if (name != null) comboBox.Name = name;
            ObservableCollection<string> allItems = new ObservableCollection<string>();
            if(includeEmptyItem)allItems.Add("              ");
            foreach (var nextString in listToUse.OrderBy(i => i)) 
            {
                allItems.Add(nextString.ToString());
            }
            comboBox.ItemsSource = allItems;
        }
        //Creates a ComboBox using a list object
        public static ComboBox CreateComboBoxFromList<T>(this IList<T> listToUse, string name = null, SolidColorBrush forgroundColor = null, bool isEditable = true)
        {
            ComboBox newBox = new ComboBox();
            if (forgroundColor != null) newBox.Foreground = forgroundColor;
            newBox.Background = BackgroundColorController.GetBackgroundColor();
            newBox.IsEditable = isEditable;
            newBox.SetComboBox(listToUse, true, name);
            newBox.SelectedIndex = 0;
            return newBox;
        }
        public static MyComboBox CreateMyComboBoxList<T>(this IList<T> listToUse, XmlNode objectNode, bool isGameFileSearchTree = true, string name = null)
        {
            MyComboBox newBox = new MyComboBox(objectNode, isGameFileSearchTree)
            {
                IsEditable = true,
                Background = BackgroundColorController.GetBackgroundColor()
            };
            if (name != null) newBox.Name = name;
            ObservableCollection<string> allItems = new ObservableCollection<string>();
            foreach (var nextString in listToUse.OrderBy(i => i))
            {
                allItems.Add(nextString.ToString());
            }
            newBox.ItemsSource = allItems;
            return newBox;
        }
        public static void AddUniqueValueTo(this MyComboBox boxToAddTo, string valueToAdd)
        {
            AddUniqueValueTo(boxToAddTo as ComboBox, valueToAdd);
        }
        public static void AddUniqueValueTo(this ComboBox boxToAddTo, List<string> valuesToAdd)
        {
            if (boxToAddTo.ItemsSource == null)
            {
                ObservableCollection<string> allItems = new ObservableCollection<string>(valuesToAdd);
                boxToAddTo.ItemsSource = allItems;
                return;
            }
            else if (boxToAddTo.ItemsSource.GetType() == typeof(ObservableCollection<string>))
            {
                foreach (string value in valuesToAdd) 
                {
                    boxToAddTo.AddUniqueValueTo(value);
                }
            }
        }
        public static void AddUniqueValueTo(this ComboBox boxToAddTo, string valueToAdd)
        {
            if (boxToAddTo.ItemsSource == null)
            {
                ObservableCollection<string> allItems = new ObservableCollection<string>
                {
                    valueToAdd
                };
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
        public static bool ChildIsCheckBox(this TreeViewItem controlObject)
        {
            bool isCheckBox = false;
            if (controlObject.HasItems) 
            {
               isCheckBox = controlObject.Items[0] as CheckBox != null ;
            }
            return isCheckBox;
        }
    }
}
