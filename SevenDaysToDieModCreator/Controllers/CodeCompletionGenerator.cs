using ICSharpCode.AvalonEdit.CodeCompletion;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace SevenDaysToDieModCreator.Controllers
{
    class CodeCompletionGenerator
    {
        internal static IList<ICompletionData> GenerateTagList(CompletionWindow completionWindow, XmlObjectsListWrapper wrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            data.Add(new MyCompletionData("!-- -->", "Xml Comment: "));
            data.Add(new MyCompletionData("!-- \n\n -->", "Xml Comment on multiple lines:"));
            foreach (string nextKey in wrapper.objectNameToAttributeValuesMap.Keys) 
            {
                string justTag = nextKey;
                data.Add(new MyCompletionData(justTag, "Xml Node: "));
                string tagAndClosingTag = nextKey + "></" + nextKey +">";
                data.Add(new MyCompletionData(tagAndClosingTag, "Xml Node Open and Closing tags: "));
            }
            return data;
        }
        internal static IList<ICompletionData> GenerateAttributeList(CompletionWindow completionWindow, XmlObjectsListWrapper gameFileWrapper, XmlObjectsListWrapper currentFileWrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            AddAttributesFromWrapper(gameFileWrapper, data);
            AddAttributesFromWrapper(currentFileWrapper, data);

            return data;
        }
        private static void AddAttributesFromWrapper(XmlObjectsListWrapper wrapper, IList<ICompletionData> data)
        {
            foreach (string nextKey in wrapper.objectNameToAttributeValuesMap.Keys)
            {
                //Get an attribute dictionary for a tag
                Dictionary<string, List<string>> attributeDictinaryForTag = wrapper.objectNameToAttributeValuesMap.GetValueOrDefault(nextKey);
                if (attributeDictinaryForTag != null)
                {
                    foreach (string attributeKey in attributeDictinaryForTag.Keys)
                    {
                        List<string> allAttributesForTag = attributeDictinaryForTag.GetValueOrDefault(attributeKey);
                        foreach (string nextAttribute in allAttributesForTag)
                        {
                            MyCompletionData attributeCompletionData = new MyCompletionData("\"" + nextAttribute + "\" "," Xml Node attribute value : ");
                            data.Add(attributeCompletionData);
                        }
                    }
                }
            }
        }
        internal static IList<ICompletionData> GenerateCommonAttributesList(CompletionWindow completionWindow, XmlObjectsListWrapper wrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            SortedSet<string> allKeysToAdd = new SortedSet<string>();
            foreach (string nextKey in wrapper.objectNameToAttributeValuesMap.Keys)
            {
                Dictionary<string, List<string>> attributeDictinaryForTag = wrapper.objectNameToAttributeValuesMap.GetValueOrDefault(nextKey);
                if (attributeDictinaryForTag != null)
                {
                    foreach (string attributeKey in attributeDictinaryForTag.Keys)
                    {
                        allKeysToAdd.Add(attributeKey);
                    }
                }
            }
            foreach (string nextKeyToAdd in allKeysToAdd) 
            {
                data.Add(new MyCompletionData(nextKeyToAdd, "Xml Node Attribute"));
            }

            return data;
        }

        internal static IList<ICompletionData> GenerateEndTagList(CompletionWindow completionWindow, XmlObjectsListWrapper wrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            foreach (string nextKey in wrapper.objectNameToAttributeValuesMap.Keys)
            {
                string justClosingTag = "</" + nextKey + ">";
                data.Add(new MyCompletionData(justClosingTag, "Xml Node Closing tag: "));
            }
            return data;
        }
    }
}
