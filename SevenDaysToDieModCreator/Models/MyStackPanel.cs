using SevenDaysToDieModCreator.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class MyStackPanel : StackPanel
    {
        private MainWindowViewController MainWindowViewController { get; set; }

        public MyStackPanel(MainWindowViewController mainWindowViewController)
        {
            this.MainWindowViewController = mainWindowViewController;
        }

        protected override void OnVisualChildrenChanged(System.Windows.DependencyObject visualAdded, System.Windows.DependencyObject visualRemoved)
        {
            if (visualAdded.GetType() == typeof(Button))
            {
                Button senderAsButton = (Button)visualAdded;
                XmlObjectsListWrapper wrapperToUse = this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(senderAsButton.Name);
                if (!MainWindowViewController.LeftNewObjectViewController.loadedListWrappers.ContainsValue(wrapperToUse) && wrapperToUse != null)
                {
                    MainWindowViewController.LeftNewObjectViewController.loadedListWrappers.Add(wrapperToUse.xmlFile.GetFileNameWithoutExtension(), wrapperToUse);
                }
            }
            else if (visualAdded.GetType() == typeof(TreeViewItem))
            {
                TreeViewItem senderAsButton = (TreeViewItem)visualAdded;
                XmlObjectsListWrapper wrapperToUse = this.MainWindowViewController.LoadedListWrappers.GetValueOrDefault(senderAsButton.Name);
                if (!MainWindowViewController.LeftNewObjectViewController.loadedListWrappers.ContainsValue(wrapperToUse) && wrapperToUse != null)
                {
                    MainWindowViewController.LeftNewObjectViewController.loadedListWrappers.Add(wrapperToUse.xmlFile.GetFileNameWithoutExtension(), wrapperToUse);
                }
            }
        }
    }
}
