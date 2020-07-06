using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class MyStackPanel : StackPanel
    {
        //A dictionary for finding XmlListWrappers by filename. This will keep track of the wrappers that are in the stack panel.
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> StackPanelLoadedListWrappers { get; private set; }
        //A dictionary for finding XmlListWrappers by filename. This is a reference to all the wrapers in the main window.
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> MainWindowLoadedListWrappers { get; private set; }
        private Dictionary<string, int[]> LoadedListWrappersCount { get; set; }

        public MyStackPanel(Dictionary<string, XmlObjectsListWrapper> MainWindowLoadedListWrappers)
        {
            this.MainWindowLoadedListWrappers = MainWindowLoadedListWrappers;
            this.StackPanelLoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.LoadedListWrappersCount = new Dictionary<string, int[]>();
        }
        public MyStackPanel()
        {
            StackPanelLoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
        }
        protected override void OnVisualChildrenChanged(System.Windows.DependencyObject visualAdded, System.Windows.DependencyObject visualRemoved)
        {
            if (visualAdded != null)
            {
                if (MainWindowLoadedListWrappers != null) HandleVisualChangedAdded(visualAdded);
            }
            if (visualRemoved != null)
            {
                if (MainWindowLoadedListWrappers != null)
                {
                    if (visualRemoved.GetType() == typeof(TreeViewItem))
                    {
                        TreeViewItem senderAsTreeView = (TreeViewItem)visualRemoved;
                        string wrapperKey = senderAsTreeView.Uid;
                        int[] count = this.LoadedListWrappersCount.GetValueOrDefault(wrapperKey);
                        if (this.StackPanelLoadedListWrappers.ContainsKey(wrapperKey) && count[0] == 1)
                        {
                            this.LoadedListWrappersCount.Remove(wrapperKey);
                            this.StackPanelLoadedListWrappers.Remove(wrapperKey);
                        }
                        else
                        {
                            count[0]--;
                        }
                    }
                }
            }
        }
        private void HandleVisualChangedAdded(System.Windows.DependencyObject visualAdded)
        {
            if (visualAdded.GetType() == typeof(TreeViewItem))
            {
                TreeViewItem senderAsTreeView = (TreeViewItem)visualAdded;
                XmlObjectsListWrapper wrapperToUse = this.MainWindowLoadedListWrappers.GetValueOrDefault(senderAsTreeView.Uid);
                string wrapperKey = senderAsTreeView.Uid;
                if (!this.StackPanelLoadedListWrappers.ContainsKey(wrapperKey) && wrapperToUse != null)
                {
                    this.StackPanelLoadedListWrappers.Add(wrapperKey, wrapperToUse);
                    this.LoadedListWrappersCount.Add(wrapperKey, new int[1] { 1 });
                }
                else
                {
                    int[] count = this.LoadedListWrappersCount.GetValueOrDefault(wrapperKey);
                    if (count != null) count[0]++;
                }
            }
        }
    }
}
