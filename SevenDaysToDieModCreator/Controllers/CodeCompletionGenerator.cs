using ICSharpCode.AvalonEdit.CodeCompletion;
using SevenDaysToDieModCreator.Models;
using SevenDaysToDieModCreator.Views;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SevenDaysToDieModCreator.Controllers
{
    class CodeCompletionGenerator
    {
        //Regex for matching any digit not in a word
        //This is used to simply filter out numbers
        private static Regex IsStringNumberRegex = new Regex(@"\b(\d+)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static List<string> AllXpathComands = new List<string> {
                XmlXpathGenerator.XPATH_ACTION_APPEND,
                XmlXpathGenerator.XPATH_ACTION_INSERT_AFTER,
                XmlXpathGenerator.XPATH_ACTION_INSERT_BEFORE,
                XmlXpathGenerator.XPATH_ACTION_REMOVE,
                XmlXpathGenerator.XPATH_ACTION_REMOVE_ATTRIBUTE,
                XmlXpathGenerator.XPATH_ACTION_SET,
                XmlXpathGenerator.XPATH_ACTION_SET_ATTRIBUTE
            };
        //Type: <
        internal static IList<ICompletionData> GenerateTagList(CompletionWindow completionWindow, XmlObjectsListWrapper wrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            data.Add(new MyCompletionData("!-- -->", "Xml Comment: "));
            data.Add(new MyCompletionData("!-- \n\n -->", "Xml Comment on multiple lines: "));
            AddXpathCommands(data);
            foreach (string nextKey in wrapper.ObjectNameToAttributeValuesMap.Keys) 
            {
                string justTag = nextKey;
                data.Add(new MyCompletionData(justTag, "Xml Node: "));
                string justClosingTag = "/" + nextKey + ">";
                data.Add(new MyCompletionData(justClosingTag, "Xml Node Closing tag: "));
                string tagAndClosingTag = nextKey + "></" + nextKey +">";
                data.Add(new MyCompletionData(tagAndClosingTag, "Xml Node Open and Closing tags: "));
            }
            return data;
        }

        private static void AddXpathCommands(IList<ICompletionData> data)
        {
            foreach (string nextCommand in AllXpathComands)
            {
                string tagAndClosingTag = nextCommand + "></" + nextCommand + ">";
                data.Add(new MyCompletionData(tagAndClosingTag, "Xml Node Xpath Command Open and Closing tags: "));
                data.Add(new MyCompletionData(nextCommand, "Xml Node Xpath Command: "));
                string justClosingTag = "/" + nextCommand + ">";
                data.Add(new MyCompletionData(justClosingTag, "Xml Node Xpath Command Closing tag: "));
            }
        }
        // Type: " 
        internal static IList<ICompletionData> GenerateAttributeList(CompletionWindow completionWindow, XmlObjectsListWrapper gameFileWrapper, XmlObjectsListWrapper currentFileWrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            AddAttributesFromWrapper(gameFileWrapper, data);
            AddAttributesFromWrapper(currentFileWrapper, data);

            return data;
        }
        private static void AddAttributesFromWrapper(XmlObjectsListWrapper wrapper, IList<ICompletionData> data, bool excludeQuotes = false)
        {
            foreach (string nextKey in wrapper.ObjectNameToAttributeValuesMap.Keys)
            {
                //Get an attribute dictionary for a tag
                Dictionary<string, List<string>> attributeDictinaryForTag = wrapper.ObjectNameToAttributeValuesMap.GetValueOrDefault(nextKey);
                if (attributeDictinaryForTag != null)
                {
                    foreach (string attributeKey in attributeDictinaryForTag.Keys)
                    {
                        List<string> allAttributesForTag = attributeDictinaryForTag.GetValueOrDefault(attributeKey);
                        SortedSet<string> allKeysToAdd = new SortedSet<string> ();
                        allKeysToAdd.UnionWith(allAttributesForTag);

                        foreach (string nextAttribute in allKeysToAdd)
                        {
                            MatchCollection matches = IsStringNumberRegex.Matches(nextAttribute);
                            //If there are matches that means it is a number, and we want to filter those out
                            if (matches.Count < 1) 
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
        }
        // Type: ctrl-space
        internal static IList<ICompletionData> GenerateCommonAttributesList(CompletionWindow completionWindow, XmlObjectsListWrapper gameFileWrapper, XmlObjectsListWrapper currentFileWrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            SortedSet<string> allKeysToAdd = new SortedSet<string>();
            foreach (string nextKey in gameFileWrapper.ObjectNameToAttributeValuesMap.Keys)
            {
                Dictionary<string, List<string>> attributeDictinaryForTag = gameFileWrapper.ObjectNameToAttributeValuesMap.GetValueOrDefault(nextKey);
                if (attributeDictinaryForTag != null)
                {
                    foreach (string attributeKey in attributeDictinaryForTag.Keys)
                    {
                        MatchCollection matches = IsStringNumberRegex.Matches(attributeKey);
                        //If there are matches that means it is a number, and we want to filter those out
                        if (matches.Count <1) allKeysToAdd.Add(attributeKey);
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
        // Type: >
        internal static IList<ICompletionData> GenerateEndTagList(CompletionWindow completionWindow, XmlObjectsListWrapper wrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            foreach (string nextKey in wrapper.ObjectNameToAttributeValuesMap.Keys)
            {
                string justClosingTag = "</" + nextKey + ">";
                data.Add(new MyCompletionData(justClosingTag, "Xml Node Closing tag: "));
            }
            foreach (string nextCommand in AllXpathComands)
            {
                string justClosingTag = "</" + nextCommand + ">";
                data.Add(new MyCompletionData(justClosingTag, "Xml Node Xpath Command Closin tag: "));
            }
            return data;
        }
        //NEED TO MAKE THIS WORK FOR /
        internal static IList<ICompletionData> GenerateEndTagListAfterSlash(CompletionWindow completionWindow, XmlObjectsListWrapper wrapper)
        {
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            foreach (string nextKey in wrapper.ObjectNameToAttributeValuesMap.Keys)
            {
                string justClosingTag = nextKey + ">";
                data.Add(new MyCompletionData(justClosingTag, "Xml Node Closing tag: "));
            }
            foreach (string nextCommand in AllXpathComands)
            {
                string justClosingTag = nextCommand + ">";
                data.Add(new MyCompletionData(justClosingTag, "Xml Node Xpath Command Closin tag: "));
            }
            data.Add(new MyCompletionData(">", "Single Line Close"));

            return data;
        }
    }
}
