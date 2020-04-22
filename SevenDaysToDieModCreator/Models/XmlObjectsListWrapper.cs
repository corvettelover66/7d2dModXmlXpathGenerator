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

        public XmlFileObject xmlFile { get; private set; }
        //Any Object's Common Attributes Note: Values are unique to the tag
        //Key = "Tag Name" E.G. "recipe or ingredient"
        //Value = "All attributes for that tag"
        public Dictionary<string, List<string>> objectNameToAttributesMap { get; private set; }
        //Any Object's Common Attributes Note: Values are unique to the tag
        //First level Dictionary
        //Key = "Tag Name" E.G. "recipe or ingredient"
        //Value = "Attributes Dictionary "
        //Attributes Dictionary
        //Key = "Attribute name" E.G. "name or count"
        //Value = "All common values for that attribute, exclusive to the Object Tag"
        public Dictionary<string, Dictionary<string, List<string>>> objectNameToAttributeValuesMap { get; private set; }

        //A dictionary of any Tag name to all children tag names
        //Key = "Tag Name" E.G. "recipe or ingredient"
        //Value = "List of all child tag names"
        public Dictionary<string, List<string>> objectNameToChildrenMap { get; private set; }
        //A List of string with all topLevelNodes in the XML file, for most that is only one for progressions that is four
        public List<string> allTopLevelTags { get; private set; }

        public XmlObjectsListWrapper(XmlFileObject xmlFileObject)
        {
            this.xmlFile = xmlFileObject;
            this.objectNameToAttributesMap = new Dictionary<string, List<string>>();
            this.objectNameToAttributeValuesMap = new Dictionary<string, Dictionary<string, List<string>>>();
            this.objectNameToChildrenMap = new Dictionary<string, List<string>>();
            this.allTopLevelTags = new List<string>();

            TraverseXml(false);
            SetTopLevelNodes();
        }
        public string GenerateDictionaryKey() 
        {
            return  this.xmlFile.ParentPath == null ? this.xmlFile.GetFileNameWithoutExtension() : this.xmlFile.ParentPath + "_" + this.xmlFile.GetFileNameWithoutExtension();

        }
        public void ClearAllLists()
        {
            this.objectNameToAttributesMap.Clear();
            this.objectNameToAttributeValuesMap.Clear();
            this.objectNameToChildrenMap.Clear();
            allTopLevelTags.Clear();
        }
        public void TraverseXml(bool clearList = true)
        {
            string firstChildName = this.xmlFile.xmlDocument.DocumentElement.FirstChild.Name;
            int count = 0;
            while (firstChildName.Contains("#") && count != this.xmlFile.xmlDocument.DocumentElement.ChildNodes.Count)
            {
                firstChildName = this.xmlFile.xmlDocument.DocumentElement.ChildNodes.Item(count).Name;
                count++;
            }
            this.FirstChildTagName = firstChildName;
            this.TopTagName = this.xmlFile.xmlDocument.DocumentElement.Name;
            if (clearList) ClearAllLists();
            XmlNodeList allObjects = this.xmlFile.xmlDocument.DocumentElement.ChildNodes;
            TraverseXmlNodeList(allObjects, "");
        }
        private void SetTopLevelNodes()
        {
            XmlNodeList allObjects = this.xmlFile.xmlDocument.DocumentElement.ChildNodes;
            foreach (XmlNode nextObjectNode in allObjects)
            {
                if (!nextObjectNode.Name.Contains("#")) allTopLevelTags.AddUnique(nextObjectNode.Name);
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
                List<string> childrenList = this.objectNameToChildrenMap.GetValueOrDefault(lastParentName);
                childrenList.AddUnique(nextObjectNode.Name.ToLower());
            }
            if (nextObjectNode.Attributes != null) TraverseAttributes(nextObjectNode);
            if (nextObjectNode.HasChildNodes)
            {
                List<string> currentMap = this.objectNameToChildrenMap.GetValueOrDefault(nextObjectNode.Name);
                if (currentMap == null)
                {
                    this.objectNameToChildrenMap.Add(nextObjectNode.Name, new List<string>());
                }
                TraverseXmlNodeList(nextObjectNode.ChildNodes, nextObjectNode.Name);
            }
        }
        private void TraverseAttributes(XmlNode nextObjectNode)
        {
            foreach (XmlAttribute nextAttribute in nextObjectNode.Attributes)
            {
                if (nextAttribute.Name.Contains("name"))
                {
                    this.objectNameToAttributesMap.AddUniqueValueToMap(nextObjectNode.Name, nextAttribute.Name);
                    this.objectNameToAttributeValuesMap.AddUniqueAttributeToMapOfMaps(nextObjectNode.Name, nextAttribute);
                }
                else if (!nextAttribute.Value.Contains("whitspace"))
                {
                    this.objectNameToAttributesMap.AddUniqueValueToMap(nextObjectNode.Name, nextAttribute.Name);
                    this.objectNameToAttributeValuesMap.AddUniqueAttributeToMapOfMaps(nextObjectNode.Name, nextAttribute);
                }
            }
        }
    }
}
