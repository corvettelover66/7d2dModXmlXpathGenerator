using ICSharpCode.AvalonEdit.CodeCompletion;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System.Collections.Generic;

namespace SevenDaysToDieModCreator.Controllers
{
    class CodeCompletionGenerator
    {
        internal static IList<ICompletionData> GenerateTagList(CompletionWindow completionWindow, XmlObjectsListWrapper wrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            data.Add(new MyCompletionData("!-- -->", "Xml Comment: "));
            data.Add(new MyCompletionData("!-- \n\n -->", "Xml Comment on multiple lines: "));
            AddXpathCommands(data);
            foreach (string nextKey in wrapper.objectNameToAttributeValuesMap.Keys) 
            {
                string justTag = nextKey;
                data.Add(new MyCompletionData(justTag, "Xml Node: "));
                string tagAndClosingTag = nextKey + "></" + nextKey +">";
                data.Add(new MyCompletionData(tagAndClosingTag, "Xml Node Open and Closing tags: "));
            }
            return data;
        }

        private static void AddXpathCommands(IList<ICompletionData> data)
        {
            List<string> allXpathComands = new List<string> {
                XmlXpathGenerator.XPATH_ACTION_APPEND,
                XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER,
                XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE,
                XmlXpathGenerator.XPATH_ACTION_REMOVE,
                XmlXpathGenerator.XPATH_ACTION_REMOVE_ATTRIBUTE,
                XmlXpathGenerator.XPATH_ACTION_SET,
                XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE
            };
            foreach (string nextCommand in allXpathComands)
            {
                data.Add(new MyCompletionData(nextCommand, "Xml Node Xpath Command: "));
                string tagAndClosingTag = nextCommand + "></" + nextCommand + ">";
                data.Add(new MyCompletionData(tagAndClosingTag, "Xml Node Xpath Command Open and Closing tags: "));
            }

        }

        internal static IList<ICompletionData> GenerateAttributeList(CompletionWindow completionWindow, XmlObjectsListWrapper gameFileWrapper, XmlObjectsListWrapper currentFileWrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            AddAttributesFromWrapper(gameFileWrapper, data);
            AddAttributesFromWrapper(currentFileWrapper, data);

            return data;
        }
        private static void AddAttributesFromWrapper(XmlObjectsListWrapper wrapper, IList<ICompletionData> data, bool excludeQuotes = false)
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
                        SortedSet<string> allKeysToAdd = new SortedSet<string> ();
                        allKeysToAdd.UnionWith(allAttributesForTag);
                        foreach (string nextAttribute in allKeysToAdd)
                        {
                            MyCompletionData attributeCompletionDataNoQuotes = new MyCompletionData(nextAttribute, "Xml Node Attribute value: ");
                            data.Add(attributeCompletionDataNoQuotes);
                            if (!excludeQuotes)
                            {
                                MyCompletionData attributeCompletionDataJustEndQuote = new MyCompletionData(nextAttribute + "\" ", "Xml Node Attribute value: ");
                                data.Add(attributeCompletionDataJustEndQuote);
                            }
                        }
                    }
                }
            }
        }
        internal static IList<ICompletionData> GenerateCommonAttributesList(CompletionWindow completionWindow, XmlObjectsListWrapper gameFileWrapper, XmlObjectsListWrapper currentFileWrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            SortedSet<string> allKeysToAdd = new SortedSet<string>();
            foreach (string nextKey in gameFileWrapper.objectNameToAttributeValuesMap.Keys)
            {
                Dictionary<string, List<string>> attributeDictinaryForTag = gameFileWrapper.objectNameToAttributeValuesMap.GetValueOrDefault(nextKey);
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
                data.Add(new MyCompletionData(nextKeyToAdd, "Xml Node Attribute: "));
            }
            AddAttributesFromWrapper(gameFileWrapper, data, true);
            AddAttributesFromWrapper(currentFileWrapper, data, true);
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
