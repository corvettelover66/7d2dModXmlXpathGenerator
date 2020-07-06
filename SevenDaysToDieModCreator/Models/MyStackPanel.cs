using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class MyStackPanel : StackPanel
    {
        private MainWindowFileController MainWindowViewController { get; set; }
        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        private Dictionary<string, int[]> LoadedListWrappersCount { get; set; }

        public MyStackPanel(MainWindowFileController mainWindowViewController)
        {
            this.MainWindowViewController = mainWindowViewController;
            this.LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
            this.LoadedListWrappersCount = new Dictionary<string, int[]>();
        }
        public MyStackPanel()
        {
            LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
        }
        protected override void OnVisualChildrenChanged(System.Windows.DependencyObject visualAdded, System.Windows.DependencyObject visualRemoved)
        {
            if (visualAdded != null)
            {
                if (MainWindowViewController != null) HandleVisualChangedAdded(visualAdded);
            }
            if (visualRemoved != null)
            {
                if (MainWindowViewController != null)
                {
                    if (visualRemoved.GetType() == typeof(TreeViewItem))
                    {
                        TreeViewItem senderAsTreeView = (TreeViewItem)visualRemoved;
                        string wrapperKey = senderAsTreeView.Uid;
                        int[] count = this.LoadedListWrappersCount.GetValueOrDefault(wrapperKey);
                        if (this.LoadedListWrappers.ContainsKey(wrapperKey) && count[0] == 1)
                        {
                            this.LoadedListWrappersCount.Remove(wrapperKey);
                            this.LoadedListWrappers.Remove(wrapperKey);
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
                XmlObjectsListWrapper wrapperToUse = this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(senderAsTreeView.Uid);
                string wrapperKey = senderAsTreeView.Uid;
                if (!this.LoadedListWrappers.ContainsKey(wrapperKey) && wrapperToUse != null)
                {
                    this.LoadedListWrappers.Add(wrapperKey, wrapperToUse);
                    this.LoadedListWrappersCount.Add(wrapperKey, new int[1] { 1 });
                }
                else
                {
                    int[] count = this.LoadedListWrappersCount.GetValueOrDefault(wrapperKey);
                    if (count != null) count[0]++;
                }
            }
            this.MainWindowViewController.LeftNewObjectViewController.XmlOutBlock.Text =
                XmlXpathGenerator.GenerateXmlViewOutput(this.MainWindowViewController.LeftNewObjectViewController.NewObjectFormViewPanel);
        }
    }
}
