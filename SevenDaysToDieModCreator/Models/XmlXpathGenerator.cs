using SevenDaysToDieModCreator.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    static class XmlXpathGenerator
    {
        public static string MyCustomTagName { get; set; } = "ThisNeedsToBeSet";
        public static string CustomTagFileName { get; private set; } = "customtag.txt";
        public static string GenerateXmlWithWrapper(StackPanel newObjectFormsPanel, XmlObjectsListWrapper xmlObjectsListWrapper, bool insertExistingFileContents = false)
        {
            string xmlOut = "<" + MyCustomTagName + ">\n";
            string existingWrapperFileData = "";
            foreach (Control nextChild in newObjectFormsPanel.Children)
            {
                if (nextChild.GetType() == typeof(TreeViewItem))
                {
                    TreeViewItem nextChildAsTree = (TreeViewItem)nextChild;
                    //We have a target type tree view
                    if (nextChildAsTree.Header.GetType() == typeof(string))
                    {
                        if (xmlObjectsListWrapper != null)
                        {
                            if (xmlObjectsListWrapper.TopTagName.Equals(StringConstants.ITEMS_TAG_NAME)) Console.Out.Write("");
                            //The header is in the form nodename:targetattributename
                            string[] treeTagSplit = nextChildAsTree.Header.ToString().Split(":");
                            xmlOut += GenerateAppendXmlForTargetObject(xmlObjectsListWrapper, nextChildAsTree, (XmlNode)nextChildAsTree.Tag, treeTagSplit[0], treeTagSplit[1]);
                        }
                    }
                    //We have a normal object creation tree view
                    else
                    {
                        Button nextChildTreeButton = (Button)nextChildAsTree.Header;
                        //Check to see if the above function found a wrapper
                        if (xmlObjectsListWrapper != null)
                        {
                            foreach (string nodeName in xmlObjectsListWrapper.allTopLevelTags)
                            {
                                if (nextChildTreeButton.Content.ToString().Equals(nodeName))
                                {
                                    xmlOut += GenerateAppendXmlForObject(xmlObjectsListWrapper, nextChildAsTree, nodeName);
                                }
                            }
                        }
                    }
                }
            }
            if (insertExistingFileContents) existingWrapperFileData += ReadExistingFileWithoutTopTag(xmlObjectsListWrapper.xmlFile.FileName);

            if (existingWrapperFileData.Length > 0) xmlOut += existingWrapperFileData;
            return xmlOut + "</" + MyCustomTagName + ">\n";
        }
        private static string ReadExistingFileWithoutTopTag(string fileName) 
        {
            string fileContents = XmlFileManager.GetFileContents(XmlFileManager._ModPath, fileName); 
            if (fileContents != null)
            {
                fileContents = fileContents.Replace("<" + MyCustomTagName + ">\n", "");
                fileContents = fileContents.Replace("</" + MyCustomTagName + ">", "");
            }
            else fileContents = "";

            return fileContents;
        }
        private static string GenerateAppendXmlForTargetObject(XmlObjectsListWrapper xmlObjectsListWrapper, TreeViewItem topTree, XmlNode currentXmlNode, string nodeName, string targetAttributeName)
        {
            //
            string generatedXml = GenerateXmlForObject(xmlObjectsListWrapper, topTree, "", nodeName, nodeName);
            string xmlOut = GenerateXpathTagetPath(xmlObjectsListWrapper.TopTagName, generatedXml, currentXmlNode, targetAttributeName);
            return xmlOut;
        }

        private static string GenerateXpathTagetPath(string topTagName, string generatedXml, XmlNode currentXmlNode, string targetAttributeName)
        {
            if (String.IsNullOrEmpty(generatedXml)) return "";

            string startingXml = "<append xpath=\"";
            string targetString = "[@name='" + targetAttributeName + "']";

            XmlNode nextParentNode = currentXmlNode;
            //
            string pathToParent ="";
            bool targetIsSet = false;
            do
            {
                if (nextParentNode.Attributes != null && nextParentNode.Attributes[0].Value.Equals(targetAttributeName) && !targetIsSet)
                {
                    pathToParent = "/" + nextParentNode.Name + targetString + pathToParent;
                    targetIsSet = true;
                }
                else pathToParent = "/" + nextParentNode.Name + pathToParent;
                nextParentNode = nextParentNode.ParentNode;
            } while (!nextParentNode.Name.Equals(topTagName));
            pathToParent = "/" + topTagName + pathToParent;
            string endingXml = "\">\n" + generatedXml + "</append>\n";

            return startingXml + pathToParent + endingXml;
        }

        private static string GenerateAppendXmlForObject(XmlObjectsListWrapper xmlObjectsListWrapper, TreeViewItem nextChildAsTree, string nodeName) 
        {
            string generatedXml  = GenerateXmlForObject(xmlObjectsListWrapper, nextChildAsTree, "", "");
            string xmlOut = "";
            if (generatedXml.Length > 0)
            {
                xmlOut = "<append xpath=\"/" + xmlObjectsListWrapper.TopTagName + "\">\n" + generatedXml + "</append>\n";
            }
            if (xmlObjectsListWrapper.TopTagName == StringConstants.PROGRESSION_TAG_NAME)
            {
                generatedXml = GenerateXmlForObject(xmlObjectsListWrapper, nextChildAsTree, "", nodeName);
                if(generatedXml.Length > 0) xmlOut = "<append xpath=\"/" + xmlObjectsListWrapper.TopTagName + "/" + nodeName + "\">\n" + generatedXml + "</append>\n";
            }
            return xmlOut;
        }
        //This has to return an empty string if the xml is invalid
        private static string GenerateXmlForObject(XmlObjectsListWrapper xmlObjectsListWrapper, TreeViewItem nextTreeItem, string xmlOut, string nodeToSkip, string targetNode = null, int level = 0)
        {
            level++;
            string tabs = "";
            for (int i = 0; i < level; i++) tabs += "\t";
            if (nextTreeItem == null) return "";
            string headerContent = targetNode == null ? ((Button)nextTreeItem.Header).Content.ToString(): targetNode; 

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
                    else xmlOut += childXml ;
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

        public static void GenerateXmlViewOutput(StackPanel newObjectFormsPanel, List<XmlObjectsListWrapper> listWrappersInObjectView, TextBox xmlOutBlock)
        {
            string addedViewTextStart = "WARNING: Direct text edits made here will NOT be saved.\n" +
             "However, you may access the files at " + XmlFileManager._ModPath + " and make direct changes there\n\n" +
             "<!-- -------------------------------------- Current Unsaved XML ----------------------------------- -->\n\n";
            string unsavedGeneratedXmlEnd = "\n\n<!-- --------------------------------------------------------------------------------------------------------- -->\n\n";
            string allWrappersText = "<!-- SAVED DATA  -->\n\n";
            string allGeneratedXml = "";
            foreach (XmlObjectsListWrapper nextWrapper in listWrappersInObjectView)
            {
                allWrappersText += XmlFileManager.GetFileContents(XmlFileManager._ModPath, (nextWrapper.xmlFile.FileName));
                allGeneratedXml += XmlXpathGenerator.GenerateXmlWithWrapper(newObjectFormsPanel, nextWrapper);
            }
            xmlOutBlock.Text = addedViewTextStart + allGeneratedXml + unsavedGeneratedXmlEnd + allWrappersText;
        }
    }
}
