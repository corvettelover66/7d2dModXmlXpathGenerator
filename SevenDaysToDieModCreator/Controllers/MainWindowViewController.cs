using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Controllers
{
    class MainWindowViewController
    {
        public static MyStackPanel NewObjectFormViewPanel { get; set; }
        public static MyStackPanel SearchTreeFormViewPanel { get; set; }
        public static MyStackPanel SearchTreeFormViewPanelTwo { get; set; }
        public static CheckBox IncludeAllModsCheckBox { get; set; }
        public static ICSharpCode.AvalonEdit.TextEditor XmlOutputBox { get; set; }

        private readonly long FILE_SIZE_THRESHOLD = 1000000;
        //A dictionary for finding XmlListWrappers by filename
        //Key file name without .xml i.e. recipes, progressions, items
        //The corressponding list wrapper
        public static Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; set; }

        public void AddSearchTree(MyStackPanel searchTreeFormsPanel, ComboBox searchTreeLoadedFilesComboBox, bool isGameFileTree = true, bool includeChildrenInOnHover = false, bool includeComments = false)
        {
            string selectedObject = searchTreeLoadedFilesComboBox.Text;
            if (String.IsNullOrEmpty(selectedObject)) return;
            XmlObjectsListWrapper selectedWrapper = LoadedListWrappers.GetWrapperFromDictionary(selectedObject);
            if (selectedObject.Split("_").Length > 1)
            {
                selectedWrapper = LoadedListWrappers.GetValueOrDefault(selectedObject);
                if (selectedWrapper == null)
                {
                    MessageBox.Show(
                        "The was an error in the file for " + selectedObject + ".\n\n" +
                        "It is probably malformed xml, to check this, switch to the mod, open the \"File\" menu and click \"Validate Mod files\".",
                        "File Loading Error!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            XmlObjectsListWrapper leftObjectWrapper = searchTreeFormsPanel.StackPanelLoadedListWrappers.GetValueOrDefault(selectedObject);
            string gameWrapperKey = selectedWrapper.GenerateDictionaryKey();
            if (leftObjectWrapper == null || leftObjectWrapper.XmlFile.FileSize < this.FILE_SIZE_THRESHOLD)
            {
                TreeViewItem nextTreeView = TreeViewGenerator.GetSearchTreeViewRecursive(selectedWrapper, gameWrapperKey, isGameTree: isGameFileTree, includeChildrenInOnHover: includeChildrenInOnHover, includeComments: includeComments);
                nextTreeView.Header = selectedObject;
                searchTreeFormsPanel.Children.Add(nextTreeView);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "That is a large file and consumes a considerable amount of resources, you already have one of those objects in the view. Are you sure you want another? ",
                    "Add Another Large Search Tree",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        TreeViewItem nextTreeView = TreeViewGenerator.GetSearchTreeViewRecursive(selectedWrapper, gameWrapperKey, isGameTree: isGameFileTree, includeChildrenInOnHover: includeChildrenInOnHover, includeComments: includeComments);
                        nextTreeView.Header = selectedObject;
                        searchTreeFormsPanel.Children.Add(nextTreeView);
                        break;
                }
            }
        }

        public void AddObjectTree(string newObjectViewLoadedFilesComboBoxText)
        {
            if (String.IsNullOrEmpty(newObjectViewLoadedFilesComboBoxText)) return;
            XmlObjectsListWrapper selectedWrapper = LoadedListWrappers.GetValueOrDefault(newObjectViewLoadedFilesComboBoxText);
            TreeViewGenerator.CreateEmptyNewObjectFormTree(selectedWrapper, newObjectViewLoadedFilesComboBoxText);
        }
        public void ResetNewObjectView()
        {
            string xmltoWrite = XmlXpathGenerator.GenerateXmlForObjectView(NewObjectFormViewPanel);
            if (!String.IsNullOrEmpty(xmltoWrite)) XmlFileManager.WriteStringToLog(xmltoWrite, true);
            NewObjectFormViewPanel.Children.Clear();
            NewObjectFormViewPanel.StackPanelLoadedListWrappers.Clear();
            XmlOutputBox.Text = XmlXpathGenerator.GenerateXmlViewOutput(NewObjectFormViewPanel);
        }
        internal void ModifySearchViewFont(int fontChange, UIElementCollection allChildren)
        {
            foreach (Control nextControl in allChildren)
            {
                if (fontChange < 0 && nextControl.FontSize < 6) continue;
                if (nextControl is TreeViewItem nextTreeViewItem)
                {
                    if (nextTreeViewItem.Header is ComboBox comboBox)
                    {
                        if (comboBox.FontSize > 4 || fontChange > 0) comboBox.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header is MyComboBox myComboBox)
                    {
                        if (myComboBox.FontSize > 4 || fontChange > 0) myComboBox.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header is Button button)
                    {
                        if (button.FontSize > 4 || fontChange > 0) button.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header.GetType() == typeof(string))
                    {
                        if (nextTreeViewItem.FontSize > 4 || fontChange > 0) nextTreeViewItem.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.HasItems) ModifySearchViewFontTreeView(fontChange, nextTreeViewItem.Items);
                }
                else nextControl.FontSize += fontChange;
            }
        }
        private void ModifySearchViewFontTreeView(int fontChange, ItemCollection allChildren)
        {
            foreach (Control nextControl in allChildren)
            {
                if (fontChange < 0 && nextControl.FontSize < 6) continue;
                if (nextControl is TreeViewItem nextTreeViewItem)
                {
                    if (nextTreeViewItem.Header is MyComboBox myComboBox)
                    {
                        if (myComboBox.FontSize > 4 || fontChange > 0) myComboBox.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header is Button button)
                    {
                        if (button.FontSize > 4 || fontChange > 0) button.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.Header is Grid grid)
                    {
                        if (grid.Children.Count > 0) 
                        {
                            foreach (Control nextGrdControl in grid.Children) 
                            {
                                if (nextGrdControl is TextBox nextGridBox) 
                                {
                                    if (nextGridBox.FontSize > 4 || fontChange > 0) nextGridBox.FontSize += fontChange;
                                }
                            }
                        }
                    }
                    if (nextTreeViewItem.Header.GetType() == typeof(string))
                    {
                        if (nextTreeViewItem.FontSize > 4 || fontChange > 0) nextTreeViewItem.FontSize += fontChange;
                    }
                    if (nextTreeViewItem.HasItems) ModifySearchViewFontTreeView(fontChange, nextTreeViewItem.Items);
                }
                else nextControl.FontSize += fontChange;
            }
        }
        public static int SearchTreeFontChange { get; private set; } = 17;
        public void IncreaseSearchTreeFontChange()
        {
            int newFontChange = SearchTreeFontChange + 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            SearchTreeFontChange = (SearchTreeFontChange + newFontChange) > 0 ? newFontChange : SearchTreeFontChange;
        }
        public void DecreaseSearchTreeFontChange()
        {
            int newFontChange = SearchTreeFontChange - 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            SearchTreeFontChange = (SearchTreeFontChange + newFontChange) > 0 ? newFontChange : SearchTreeFontChange;
        }
        public static int ObjectTreeFontChange { get; private set; } = 20;
        public void IncreaseObjectTreeFontChange()
        {
            int newFontChange = ObjectTreeFontChange + 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            ObjectTreeFontChange = (ObjectTreeFontChange + newFontChange) > 0 ? newFontChange : ObjectTreeFontChange;
        }
        public void DereasecObjectTreeFontChange()
        {
            int newFontChange = ObjectTreeFontChange - 1;
            //If the change keeps the font above 0 it's fine otherwise just keep it the same.
            ObjectTreeFontChange = (ObjectTreeFontChange + newFontChange) > 0 ? newFontChange : ObjectTreeFontChange;
        }
    }
}
