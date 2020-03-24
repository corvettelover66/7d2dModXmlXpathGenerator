﻿using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class MyStackPanel : StackPanel
    {
        private MainWindowViewController MainWindowViewController { get; set; }
        public SearchViewCache SearchViewCache { get; }

        //A dictionary for finding XmlListWrappers by filename
        //Key top tag name i.e. recipe, progression, item
        //The corressponding list wrapper
        public Dictionary<string, XmlObjectsListWrapper> LoadedListWrappers { get; private set; }
        public MyStackPanel(MainWindowViewController mainWindowViewController, SearchViewCache searchViewCache)
        {
            this.MainWindowViewController = mainWindowViewController;
            this.SearchViewCache = searchViewCache;
            this.LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
        }
        public MyStackPanel()
        {
            LoadedListWrappers = new Dictionary<string, XmlObjectsListWrapper>();
        }
        protected override void OnVisualChildrenChanged(System.Windows.DependencyObject visualAdded, System.Windows.DependencyObject visualRemoved)
        {
            if (visualAdded != null)
            {
                if (MainWindowViewController != null) HandleVisualChangedForController(visualAdded);
            }
        }
        private void HandleVisualChangedForController(System.Windows.DependencyObject visualAdded) 
        {
            if (visualAdded.GetType() == typeof(Button))
            {
                Button senderAsButton = (Button)visualAdded;
                XmlObjectsListWrapper wrapperToUse = this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(senderAsButton.Name);
                if (!this.LoadedListWrappers.ContainsValue(wrapperToUse) && wrapperToUse != null)
                {
                    this.LoadedListWrappers.Add(wrapperToUse.xmlFile.GetFileNameWithoutExtension(), wrapperToUse);
                }
            }
            else if (visualAdded.GetType() == typeof(TreeViewItem))
            {
                TreeViewItem senderAsButton = (TreeViewItem)visualAdded;
                XmlObjectsListWrapper wrapperToUse = this.MainWindowViewController.LoadedListWrappers.GetWrapperFromDictionary(senderAsButton.Name);
                if(wrapperToUse == null) wrapperToUse = this.MainWindowViewController.LoadedListWrappers.GetWrapperFromDictionary(senderAsButton.Header.ToString());

                if (!this.LoadedListWrappers.ContainsValue(wrapperToUse) && wrapperToUse != null)
                {
                    this.LoadedListWrappers.Add(wrapperToUse.xmlFile.GetFileNameWithoutExtension(), wrapperToUse);
                }
            }
            this.MainWindowViewController.LeftNewObjectViewController.xmlOutBlock.Text = XmlXpathGenerator.GenerateXmlViewOutput(this.MainWindowViewController.LeftNewObjectViewController.NewObjectFormViewPanel, this.MainWindowViewController.LeftNewObjectViewController.NewObjectFormViewPanel.LoadedListWrappers);
        }
    }
}
