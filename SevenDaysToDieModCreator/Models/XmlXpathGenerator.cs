using SevenDaysToDieModCreator.Extensions;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    static class XmlXpathGenerator
    {
        public const string XPATH_ACTION_SET = "set";
        public const string XPATH_ACTION_REMOVE_ATTRIBUTE = "removeattribute";
        public const string XPATH_ACTION_SET_ATTRIBUTE = "setattribute";
        public const string XPATH_ACTION_APPEND = "append";
        public const string XPATH_ACTION_REMOVE = "remove";
        public const string XPATH_ACTION_INSERT_BEFORE = "insertBefore";
        public const string XPATH_ACTION_INSERT_AFTER = "insertAfter";
        public const string ATTRIBUTE_NAME = "Attribute";
        public const string ATTRIBUTE_VALUE = "AttributeValue";

        public static string GenerateXmlForObjectView(StackPanel newObjectFormsPanel, Dictionary<string, XmlObjectsListWrapper> listWrappersInView)
        {
            string topTag = "\n<" + Properties.Settings.Default.CustomTagName + ">\n";
            string topTagEnd = "</" + Properties.Settings.Default.CustomTagName + ">\n";
            string xmlOut = "";
            foreach (Control nextChild in newObjectFormsPanel.Children)
            {
                //It is a top object in the view
                if (nextChild.GetType() == typeof(TreeViewItem))
                {
                    TreeViewItem nextChildAsTree = (TreeViewItem)nextChild;
                    XmlObjectsListWrapper xmlObjectsListWrapper = listWrappersInView.GetValueOrDefault(nextChildAsTree.Name);
                    xmlOut += GenerateXmlWithWrapper(nextChildAsTree, xmlObjectsListWrapper);
                }
            }
            return topTag + xmlOut + topTagEnd;
        }
        public static void SaveAllGeneratedXmlToPath(StackPanel newObjectFormsPanel, Dictionary<string, XmlObjectsListWrapper> listWrappersInView, string path, bool writeToLog = false)
        {
            string topTag = "\n<" + Properties.Settings.Default.CustomTagName + ">\n";
            string topTagEnd = "</" + Properties.Settings.Default.CustomTagName + ">\n";

            foreach (Control nextChild in newObjectFormsPanel.Children)
            {
                //It is a top object in the view
                if (nextChild.GetType() == typeof(TreeViewItem))
                {
                    TreeViewItem nextChildAsTree = (TreeViewItem)nextChild;
                    XmlObjectsListWrapper xmlObjectsListWrapper = listWrappersInView.GetValueOrDefault(nextChildAsTree.Name);
                    string xmlOut = GenerateXmlWithWrapper(nextChildAsTree, xmlObjectsListWrapper, true);
                    if (!String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToFile(path, xmlObjectsListWrapper.xmlFile.FileName, topTag+ xmlOut + topTagEnd, true);
                    if (writeToLog && !String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToLog(xmlOut, true);
                }
            }
        }
        private static string GenerateXmlWithWrapper(Control parentControl, XmlObjectsListWrapper xmlObjectsListWrapper, bool includeExistingData = false)
        {
            string xmlOut = "";
            string existingWrapperFileData = "";
            TreeViewItem nextChildAsTree = (TreeViewItem)parentControl;
            //We have a target type tree view
            if (nextChildAsTree.Header.GetType() == typeof(string))
            {
                //The header is in the form nodename:targetattributename
                string[] treeTagSplit = nextChildAsTree.Header.ToString().Split(":");
                xmlOut += GenerateAppendXmlForTargetObject(xmlObjectsListWrapper, nextChildAsTree, (XmlNode)nextChildAsTree.Tag, treeTagSplit[0]);
            }
            //We have a normal object creation tree view
            else
            {
                Button nextChildTreeButton = (Button)nextChildAsTree.Header;
                foreach (string nodeName in xmlObjectsListWrapper.allTopLevelTags)
                {
                    if (nextChildTreeButton.Content.ToString().Equals(nodeName))
                    {
                        xmlOut += GenerateAppendXmlForObject(xmlObjectsListWrapper, nextChildAsTree, nodeName);
                    }
                }
            }
            if (includeExistingData) existingWrapperFileData = XmlFileManager.ReadExistingFile(xmlObjectsListWrapper.xmlFile.FileName, Properties.Settings.Default.CustomTagName);
            if (!String.IsNullOrEmpty(xmlOut) && !String.IsNullOrEmpty(existingWrapperFileData)) xmlOut += existingWrapperFileData;
            return xmlOut;
        }
        private static string GenerateAppendXmlForTargetObject(XmlObjectsListWrapper xmlObjectsListWrapper, TreeViewItem topTree, XmlNode currentXmlNode, string nodeName)
        {

            string generatedXml = GenerateXmlForObject(xmlObjectsListWrapper, topTree, "", nodeName, nodeName);

            string[] actionSplit = topTree.Uid.Split(":");
            string xPathAction = actionSplit[0];
            string attributeInAction = actionSplit.Length > 1 ? actionSplit[1] : "";
            string attributeName = "";
            if (actionSplit[0].Equals(XPATH_ACTION_SET_ATTRIBUTE)) attributeName = ((topTree.FindName(ATTRIBUTE_NAME) as TreeViewItem).Header as TextBox).Text;

            string xmlOut = GenerateXpathTagetPath(xmlObjectsListWrapper.TopTagName, generatedXml, xPathAction, attributeInAction, attributeName, currentXmlNode);
            return xmlOut;
        }

        private static string GenerateXpathTagetPath(string topTagName, string generatedXml, string xpathAction, string attributeInAction, string attributeName, XmlNode currentXmlNode)
        {
            if (String.IsNullOrEmpty(generatedXml)) return "";

            string startingXml = "<"+ xpathAction + " xpath=\"";

            XmlNode nextParentNode = currentXmlNode;
            //
            string pathToParent = "";
            do
            {
                XmlAttribute attributeToUse = nextParentNode.GetAvailableAttribute();
                if (attributeToUse != null)
                {
                    string targetString = attributeToUse != null ? "[@" + attributeToUse.Name + "='" + attributeToUse.Value + "']" : "";
                    pathToParent = "/" + nextParentNode.Name + targetString + pathToParent;
                }
                else pathToParent = "/" + nextParentNode.Name + pathToParent;
                if (nextParentNode.ParentNode != null) nextParentNode = nextParentNode.ParentNode;
                else break;
            } while (!nextParentNode.Name.Equals(topTagName));
            if (String.IsNullOrEmpty(attributeInAction))attributeInAction =  "/@" + attributeInAction;
            else if (!String.IsNullOrEmpty(attributeName)) attributeInAction =  " " + attributeInAction + "=\""+ attributeName + "\" ";
            pathToParent = "/" + topTagName + pathToParent + attributeInAction;
            string endingXml = "\">\n" + generatedXml + "</"+ xpathAction + ">\n";

            return startingXml + pathToParent + endingXml;
        }

        private static string GenerateAppendXmlForObject(XmlObjectsListWrapper xmlObjectsListWrapper, TreeViewItem nextChildAsTree, string nodeName)
        {
            string generatedXml = GenerateXmlForObject(xmlObjectsListWrapper, nextChildAsTree, "", "");
            string xmlOut = "";
            if (generatedXml.Length > 0)
            {
                xmlOut = "<append xpath=\"/" + xmlObjectsListWrapper.TopTagName + "\">\n" + generatedXml + "</append>\n";
            }
            if (xmlObjectsListWrapper.TopTagName == StringConstants.PROGRESSION_TAG_NAME)
            {
                generatedXml = GenerateXmlForObject(xmlObjectsListWrapper, nextChildAsTree, "", nodeName);
                if (generatedXml.Length > 0) xmlOut = "<append xpath=\"/" + xmlObjectsListWrapper.TopTagName + "/" + nodeName + "\">\n" + generatedXml + "</append>\n";
            }
            return xmlOut;
        }
        //This has to return an empty string if the xml is invalid
        private static string GenerateXmlForObject(XmlObjectsListWrapper xmlObjectsListWrapper, TreeViewItem nextTreeItem, string xmlOut, string nodeToSkip, string targetNode = null, int level = 0)
        {
            if (nextTreeItem == null) return "";
            level++;
            string tabs = "";
            for (int i = 0; i < level; i++) tabs += "\t";
            if (nextTreeItem.Name.Equals(ATTRIBUTE_VALUE)) 
            {
                if(nextTreeItem.Header.GetType() == typeof(TextBox)) xmlOut = (nextTreeItem.Header as TextBox).Text;
                else if (nextTreeItem.Header.GetType() == typeof(ComboBox)) xmlOut = (nextTreeItem.Header as ComboBox).Text;
                return tabs + xmlOut;
            }
            //If the target node is null use the treeitem header else just the target node
            string headerContent = targetNode ?? ((Button)nextTreeItem.Header).Content.ToString();

            string xmlAttributeTag = AddTagWithAttributes(nextTreeItem, "", headerContent);
            if (xmlAttributeTag.Length > 0) xmlOut += tabs + xmlAttributeTag;

            List<string> children = xmlObjectsListWrapper.objectNameToChildrenMap.GetValueOrDefault(headerContent + "");
            if (children != null)
            {
                string childXml = "";
                foreach (string childName in children)
                {
                    //Check all possible children
                    Queue<TreeViewItem> allTreeViews = nextTreeItem.getChildTreeViewQueue(childName);
                    if (allTreeViews != null)
                    {
                        while (allTreeViews.Count > 0)
                        {
                            TreeViewItem nextChild = allTreeViews.Dequeue();
                            //Recurse on children
                            childXml += GenerateXmlForObject(xmlObjectsListWrapper, nextChild, "", nodeToSkip, null, level);
                        }
                    }
                }
                //There is child xml
                if (childXml.Length > 0)
                {
                    //if there aren't attributes print top tag
                    if (xmlAttributeTag.Length < 1 && headerContent + "" != nodeToSkip) xmlOut += tabs + "<" + headerContent + ">\n";
                    if (headerContent + "" != nodeToSkip) xmlOut += childXml + tabs + "</" + headerContent + ">\n";
                    else xmlOut += childXml;
                }
                //Children wasn't null but there was no children and there are attributes
                else if (xmlAttributeTag.Length > 0 && headerContent + "" != nodeToSkip)
                {
                    xmlOut += tabs + "</" + headerContent + ">\n";
                }
            }
            //There were attributes but no children, add closing tag.
            else if (xmlAttributeTag.Length > 0 && headerContent + "" != nodeToSkip)
            {
                xmlOut += tabs + "</" + headerContent + ">\n";
            }
            return xmlOut;
        }

        private static string AddTagWithAttributes(TreeViewItem nextTreeItem, string xmlOut, string headerContent)
        {
            bool hasFoundItem = false, didWriteStart = false;
            foreach (Control nextControl in nextTreeItem.Items)
            {
                if (nextControl.GetType() == typeof(ComboBox))
                {
                    ComboBox nextControlAsBox = (ComboBox)nextControl;
                    if (nextControlAsBox.Text.Length > 0)
                    {
                        hasFoundItem = true;
                        if (!didWriteStart) xmlOut += "<" + headerContent;
                        xmlOut += " " + nextControlAsBox.Tag + "=\"" + nextControlAsBox.Text + "\" ";
                        didWriteStart = true;
                    }
                }
            }
            if (hasFoundItem) xmlOut += ">\n";
            return xmlOut;
        }
        public static void GenerateXmlViewOutput(StackPanel newObjectFormsPanel, Dictionary<string, XmlObjectsListWrapper> listWrappersInObjectView, ICSharpCode.AvalonEdit.TextEditor xmlOutBlock)
        {
            string addedViewTextStart = "WARNING: Direct text edits made here will NOT be saved.\n\n" +
             "To make direct file edits you can select a file below and open the direct editor window for the file.\n\n" +
             "You can also make direct changes to the file(s) at the current output location: \n" + XmlFileManager._ModOutputPath + "\n";
            string unsavedGeneratedXmlStart = "<!-- -------------------------------------- Current Unsaved XML ----------------------------------- -->\n\n";

            string unsavedGeneratedXmlEnd = "\n\n<!-- --------------------------------------------------------------------------------------------------------- -->\n\n";
            string existingWrapperFileData = "<!-- SAVED DATA  -->\n\n";

            foreach (XmlObjectsListWrapper xmlObjectsListWrapper in listWrappersInObjectView.Values)
            {
                existingWrapperFileData += XmlFileManager.ReadExistingFile(xmlObjectsListWrapper.xmlFile.FileName, Properties.Settings.Default.CustomTagName);
            }

            string allGeneratedXml = GenerateXmlForObjectView(newObjectFormsPanel, listWrappersInObjectView);
            xmlOutBlock.Text = addedViewTextStart+ unsavedGeneratedXmlStart + allGeneratedXml + unsavedGeneratedXmlEnd + existingWrapperFileData;
        }
    }
}
