using SevenDaysToDieModCreator.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class SearchViewCache
    {
        private Dictionary<string, TreeViewItem> ViewCacheMap { get; set; }
        public MainWindowViewController MainWindowViewController { get; }

        public SearchViewCache(MainWindowViewController mainWindowViewController)
        {
            ViewCacheMap = new Dictionary<string, TreeViewItem>();
            this.MainWindowViewController = mainWindowViewController;
        }

        public void LoadCache(string addedWrapperKey = "")
        {
            //foreach (string wrapperKey in this.MainWindowViewController.LoadedListWrappers.Keys)
            //{
            //    XmlObjectsListWrapper nextXmlObjectsListWrapper = this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(wrapperKey);
            //    if (!this.HasTreeView(wrapperKey) || wrapperKey.Equals(addedWrapperKey)) 
            //    {
            //        TreeViewItem nextTreeView = MainWindowViewController.LeftNewObjectViewController.GetSearchTreeViewRecursive(nextXmlObjectsListWrapper);
            //        if(this.HasTreeView(wrapperKey)) this.ViewCacheMap.Remove(wrapperKey);
            //        this.AddToCache(wrapperKey, nextTreeView);
            //    }
            //}
        }

        public void AddToCache(string treeID, TreeViewItem topTree) 
        {
            this.ViewCacheMap.Add(treeID, topTree);
        }
        public TreeViewItem GetTreeViewByKey(string treeID) 
        {
            return this.ViewCacheMap.GetValueOrDefault(treeID);
        }
        public bool HasTreeView(string treeID) 
        {
            return this.ViewCacheMap.GetValueOrDefault(treeID) != null;
        }
    }
}
