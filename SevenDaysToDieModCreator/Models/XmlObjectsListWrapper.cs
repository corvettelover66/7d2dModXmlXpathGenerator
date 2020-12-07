using SevenDaysToDieModCreator.Extensions;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    public class XmlObjectsListWrapper
    {
        public string TopTagName { get; private set; }
        public string FirstChildTagName { get; private set; }

        public XmlFileObject XmlFile { get; private set; }
        //Any Object's Common Attributes Note: Values are unique to the tag
        //Key = "Tag Name" E.G. "recipe or ingredient"
        //Value = "All commmon attributes for that tag"
        public Dictionary<string, List<string>> ObjectNameToAttributesMap { get; private set; }
        //Any Object's Common Attributes Note: Values are unique to the tag
        //First level Dictionary
        //Key = "Tag Name" E.G. "recipe or ingredient"
        //Value = "Attributes Dictionary "
        //Attributes Dictionary
        //Key = "Attribute name" E.G. "name or count"
        //Value = "All common values for that attribute, exclusive to the Object Tag"
        public Dictionary<string, Dictionary<string, List<string>>> ObjectNameToAttributeValuesMap { get; private set; }
        //Any Object's Common Attributes Note: Values are unique to the tag, the map above does not exclude xpath commands making other uses in the application more difficult. This map solves that problem. 
        //First level Dictionary
        //Key = "Tag Name" E.G. "recipe or ingredient"
        //Value = "Attributes Dictionary "
        //Attributes Dictionary
        //Key = "Attribute name" E.G. "name or count"
        //Value = "All common values for that attribute, exclusive to the Object Tag"
        public Dictionary<string, Dictionary<string, List<string>>> ObjectNameToAttributeValuesMapNoXpath { get; private set; }


        //A dictionary of any Tag name to all children tag names
        //Key = "Tag Name" E.G. "recipe or ingredient"
        //Value = "List of all child tag names"
        public Dictionary<string, List<string>> ObjectNameToChildrenMap { get; private set; }
        //A List of string with all topLevelNodes in the XML file, for most that is only one for progressions that is four
        public List<string> AllTopLevelTags { get; private set; }

        public XmlObjectsListWrapper(XmlFileObject xmlFileObject)
        {
            this.XmlFile = xmlFileObject;
            this.ObjectNameToAttributesMap = new Dictionary<string, List<string>>();
            this.ObjectNameToAttributeValuesMap = new Dictionary<string, Dictionary<string, List<string>>>();
            this.ObjectNameToAttributeValuesMapNoXpath = new Dictionary<string, Dictionary<string, List<string>>>();
            this.ObjectNameToChildrenMap = new Dictionary<string, List<string>>();
            this.AllTopLevelTags = new List<string>();
            TraverseXml(false);
            SetTopLevelNodes();
        }
        public string GenerateDictionaryKey() 
        {
            return  this.XmlFile.ParentPath == null ? this.XmlFile.GetFileNameWithoutExtension() : this.XmlFile.ParentPath + "_" + this.XmlFile.GetFileNameWithoutExtension();

        }
        public void ClearAllLists()
        {
            this.ObjectNameToAttributesMap.Clear();
            this.ObjectNameToAttributeValuesMap.Clear();
            this.ObjectNameToChildrenMap.Clear();
            this.AllTopLevelTags.Clear();
        }
        public void TraverseXml(bool clearList = true)
        {
            string firstChildName = this.XmlFile.xmlDocument.DocumentElement.FirstChild.Name;
            int count = 0;
            while (firstChildName.Contains("#") && count != this.XmlFile.xmlDocument.DocumentElement.ChildNodes.Count)
            {
                firstChildName = this.XmlFile.xmlDocument.DocumentElement.ChildNodes.Item(count).Name;
                count++;
            }
            this.FirstChildTagName = firstChildName;
            this.TopTagName = this.XmlFile.xmlDocument.DocumentElement.Name;
            if (clearList) ClearAllLists();
            XmlNodeList allObjects = this.XmlFile.xmlDocument.DocumentElement.ChildNodes;
            TraverseXmlNodeList(allObjects, "");
        }
        private void SetTopLevelNodes()
        {
            XmlNodeList allObjects = this.XmlFile.xmlDocument.DocumentElement.ChildNodes;
            foreach (XmlNode nextObjectNode in allObjects)
            {
                if (!nextObjectNode.Name.Contains("#"))
                {
                    AllTopLevelTags.AddUnique(nextObjectNode.Name);
                }
            }
        }
        public void TraverseXmlNodeList(XmlNodeList allObjects, String lastParentName)
        {
            foreach (XmlNode nextObjectNode in allObjects)
            {
                TraverseXmlNode(nextObjectNode, lastParentName);
            }
        }
        public void TraverseXmlNode(XmlNode nextObjectNode, string lastParentName)
        {
            if (nextObjectNode == null || nextObjectNode.Name.Contains("#")) return;
            else if (lastParentName.Length > 0)
            {
                List<string> childrenList = this.ObjectNameToChildrenMap.GetValueOrDefault(lastParentName);
                if(childrenList!= null)childrenList.AddUnique(nextObjectNode.Name);
            }
            if (nextObjectNode.Attributes != null) TraverseAttributes(nextObjectNode);
            if (nextObjectNode.HasChildNodes)
            {
                List<string> currentMap = this.ObjectNameToChildrenMap.GetValueOrDefault(nextObjectNode.Name);
                if (currentMap == null)
                {
                    this.ObjectNameToChildrenMap.Add(nextObjectNode.Name, new List<string>());
                }
                TraverseXmlNodeList(nextObjectNode.ChildNodes, nextObjectNode.Name);
            }
        }
        private void TraverseAttributes(XmlNode nextObjectNode)
        {
            foreach (XmlAttribute nextAttribute in nextObjectNode.Attributes)
            {

                if (!nextAttribute.Value.Contains("whitspace"))
                {
                    this.ObjectNameToAttributesMap.AddUniqueValueToMap(nextObjectNode.Name, nextAttribute.Name);
                    this.ObjectNameToAttributeValuesMap.AddUniqueAttributeToMapOfMaps(nextObjectNode.Name, nextAttribute);
                }
                if (XmlAttributeIsNotWhitspaceOrXpath(nextAttribute)) 
                {
                    this.ObjectNameToAttributeValuesMapNoXpath.AddUniqueAttributeToMapOfMaps(nextObjectNode.Name, nextAttribute);
                }
            }
        }
        private bool XmlAttributeIsNotWhitspaceOrXpath(XmlAttribute nextAttribute) 
        {
            return !nextAttribute.Name.Contains("xpath") && !nextAttribute.Value.Contains("whitspace");
        }
    }
}
