using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Search;
using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for AddInsideTagetView.xaml
    /// </summary>
    public partial class DirectEditView : Window
    {
        private bool IsIgnoringExternalChanges { get; set; }
        private XmlObjectsListWrapper Wrapper { get; set; }
        private string StartingFileContents { get; set; }
        private string UnchangedStartingFileContents { get; set; }
        private string StartingTitle { get; set; }
        public string DictionaryKey { get; private set; }
        private string FileLocationPath { get; set; }
        private bool IsGameFile { get; set; }
        public bool IsFirstWindowOpen { get; private set; }

        private FoldingManager FoldingManager { get; }
        private XmlFoldingStrategy FoldingStrategy { get; }
        public DirectEditView(XmlObjectsListWrapper wrapperToUse, string dictionaryKey, bool isGameFile, bool isFirstWindowOpen = true, string title = null, string contentsForXmlOutputBox = null, string fileLocationPath = "", string unchangedStartingFileContents = null)
        {
            InitializeComponent();
            this.Wrapper = wrapperToUse;
            this.IsGameFile = isGameFile;
            this.FileLocationPath = fileLocationPath;
            this.DictionaryKey = dictionaryKey;
            this.IsFirstWindowOpen = isFirstWindowOpen;
            if (contentsForXmlOutputBox == null)
            {
                string parentString = Wrapper.XmlFile.ParentPath ?? "";
                XmlOutputBox.Text = XmlFileManager.GetFileContents(Path.Combine(this.FileLocationPath, parentString), Wrapper.XmlFile.FileName);
            }
            else XmlOutputBox.Text = contentsForXmlOutputBox;
            this.StartingFileContents = XmlOutputBox.Text;
            this.UnchangedStartingFileContents = unchangedStartingFileContents ?? XmlOutputBox.Text;
            
            string labelContents = isGameFile
                ? "Game File: " + wrapperToUse.XmlFile.FileName + "\n"
                : "Mod: " + Properties.Settings.Default.ModTagSetting + "\n" + "File: " + wrapperToUse.XmlFile.FileName + "\n";
            this.StartingTitle = isGameFile
                ? "Game File: " + wrapperToUse.XmlFile.FileName
                : wrapperToUse.XmlFile.GetFileNameWithoutExtension() + " : " + Properties.Settings.Default.ModTagSetting;

            this.Title = StartingTitle;

            this.SaveXmlButton.AddToolTip("Click to save all changes");
            this.ReloadFileXmlButton.AddToolTip("Click here to reload the file from disk");
            this.CloseButton.AddToolTip("Click here to close the window");
            this.ValidateXmlButton.AddToolTip("Click here to validate the xml");
            this.UndoAllChangesXmlButton.AddToolTip("Click here to undo any changes made since opening the window");
            this.CodeCompletionKeysHelpButton.AddToolTip("Click here to see the keys used for\nAuto Complete within this window");
            this.CombineTagsXmlButton.AddToolTip("Click here to combine all top level APPEND tags, into a single APPEND tag.\n" +
                                               "EX: In the file there are completly new RECIPES under seperate APPEND tags that can be combined.");

            SearchPanel.Install(XmlOutputBox);

            FoldingManager = FoldingManager.Install(this.XmlOutputBox.TextArea);
            FoldingStrategy = new XmlFoldingStrategy
            {
                ShowAttributesWhenFolded = true
            };
            FoldingStrategy.UpdateFoldings(FoldingManager, this.XmlOutputBox.Document);

            TextEditorOptions newOptions = new TextEditorOptions
            {
                EnableRectangularSelection = true,
                EnableTextDragDrop = true,
                HighlightCurrentLine = true,
                ShowTabs = true
            };
            this.XmlOutputBox.TextArea.Options = newOptions;
            
            this.XmlOutputBox.ShowLineNumbers = true;

            this.XmlOutputBox.AddContextMenu(CollapseAllContextMenu_Clicked, 
                "Collapse All", 
                "Click here to collapse all nodes in the document.");
            this.XmlOutputBox.AddContextMenu(ExpandAllContextMenu_Clicked,
                "Expand All",
                "Click here to expand all nodes in the document.");

            this.XmlOutputBox.GotMouseCapture += XmlOutputBox_GotMouseCapture;
            this.XmlOutputBox.PreviewMouseWheel += XmlOutputBox_PreviewMouseWheel;
            this.XmlOutputBox.TextChanged += XmlOutputBox_TextChanged;
            this.XmlOutputBox.TextArea.TextEntering += TextArea_TextEntering; 
            this.XmlOutputBox.TextArea.TextEntered += TextArea_TextEntered;
            this.XmlOutputBox.Focus();
            SetBackgroundColor();
            Closing += new CancelEventHandler(DirectEditView_Closing);
        }

        private void SetBackgroundColor()
        {
            this.XmlOutputBox.Background = BackgroundColorController.GetBackgroundColor();
        }

        CompletionWindow completionWindow;
        private void TextArea_TextEntered(object sender, TextCompositionEventArgs textCompositionEvent)
        {
            //
            if (textCompositionEvent.Text == "<")
            {
                completionWindow = new CompletionWindow(this.XmlOutputBox.TextArea);
                IList<ICompletionData> data = CodeCompletionGenerator.GenerateTagList(completionWindow, this.Wrapper);
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            else if (textCompositionEvent.Text == ">")
            {
                completionWindow = new CompletionWindow(this.XmlOutputBox.TextArea);
                IList<ICompletionData> data = CodeCompletionGenerator.GenerateEndTagList(completionWindow, this.Wrapper);
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            else if (textCompositionEvent.Text == "\"")
            {
                completionWindow = new CompletionWindow(this.XmlOutputBox.TextArea);
                string parentString = Wrapper.XmlFile.ParentPath ?? "";
                string fullFilePath = Path.Combine(this.FileLocationPath, parentString, this.Wrapper.XmlFile.FileName);
                IList<ICompletionData> data = CodeCompletionGenerator.GenerateAttributeList(completionWindow, this.Wrapper, new XmlObjectsListWrapper(new XmlFileObject(fullFilePath)));
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            else if (textCompositionEvent.Text == "/")
            {
                completionWindow = new CompletionWindow(this.XmlOutputBox.TextArea);
                IList<ICompletionData> data = CodeCompletionGenerator.GenerateEndTagListAfterSlash(completionWindow, this.Wrapper);
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            else if (textCompositionEvent.Text == "!")
            {
                completionWindow = new CompletionWindow(this.XmlOutputBox.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("-- -->", "Xml Comment: "));
                data.Add(new MyCompletionData("-- \n\n -->", "Xml Comment on multiple lines: "));
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && textCompositionEvent.Text == " ")
            {
                completionWindow = new CompletionWindow(this.XmlOutputBox.TextArea);
                string parentString = Wrapper.XmlFile.ParentPath ?? "";
                string fullFilePath = Path.Combine(this.FileLocationPath, parentString, this.Wrapper.XmlFile.FileName);
                IList<ICompletionData> data = CodeCompletionGenerator.GenerateCommonAttributesList(completionWindow, this.Wrapper, new XmlObjectsListWrapper(new XmlFileObject(fullFilePath)));
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void XmlOutputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            //if (e.Key == System.Windows.Input.Key.Up)
            //{

            //}
            //else if (e.Key == System.Windows.Input.Key.Down)
            //{

            //}
            //else if (e.Key == System.Windows.Input.Key.C) 
            //{
            //    string selection = this.XmlOutputBox.SelectedText;
            //    if (String.IsNullOrEmpty(selection))
            //    {
            //        int currentCaretPosition = this.XmlOutputBox.CaretOffset;
            //        this.XmlOutputBox.TextArea.Options = TextEditorOptions
            //        string allText = this.XmlOutputBox.Text;
            //        string allTextAtCarret = allText.
            //    }
            //}
        }

        private void CollapseAllContextMenu_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (FoldingSection nextSection in FoldingManager.AllFoldings)
            {
                nextSection.IsFolded = true;
            }
        }
        private void ExpandAllContextMenu_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (FoldingSection nextSection in FoldingManager.AllFoldings) 
            {
                nextSection.IsFolded = false;
            }
        }
        private void XmlOutputBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if(!IsIgnoringExternalChanges) HandleExternalFileChanges();
        }
        private void XmlOutputBox_TextChanged(object sender, EventArgs e)
        {
            string currentContents = GetCurrentFileContents();
            this.Title = XmlOutputBox.Text.Equals(currentContents)
                ? this.StartingTitle
                : "*" + this.StartingTitle;
            FoldingStrategy.UpdateFoldings(FoldingManager, this.XmlOutputBox.Document);
        }
        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            //Reset the boolean to look for external changes again.
            IsIgnoringExternalChanges = false;
        }
        private void ReloadFileXmlButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewComponents();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void XmlOutputBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            //User rotated forward
            if (e.Delta > 0)
            {
                if (XmlOutputBox.FontSize != 200) XmlOutputBox.FontSize += 1;
            }
            //User rotated backwards
            else if (e.Delta < 0)
            {
                if (XmlOutputBox.FontSize != 10) XmlOutputBox.FontSize -= 1;
            }
        }
        private void DirectEditView_Closing(object sender, CancelEventArgs e)
        {
            if (IsFileChanged())
            {
                MessageBoxResult result = MessageBox.Show(
                    "You have unsaved changes! Would you like to save them now?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string parentString = Wrapper.XmlFile.ParentPath ?? "";
                        string xmlOut = XmlOutputBox.Text;
                        if (!String.IsNullOrEmpty(xmlOut)) XmlFileManager.WriteStringToFile(Path.Combine(XmlFileManager.ModConfigOutputPath, parentString), Wrapper.XmlFile.FileName, xmlOut, true);
                        break;
                    case MessageBoxResult.Cancel:
                        DirectEditView directEditView = new DirectEditView(this.Wrapper, this.DictionaryKey, this.IsGameFile, this.IsFirstWindowOpen, this.StartingTitle, XmlOutputBox.Text, FileLocationPath, this.UnchangedStartingFileContents);
                        directEditView.Show();
                        break;
                }
            }
        }
        private void HandleExternalFileChanges()
        {
            if (HasFileChangedExternally())
            {
                MessageBoxResult result = MessageBox.Show(
                    "The file has changed externally! Would you like to load those changes?",
                    "Load External Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string fileContents = GetCurrentFileContents();
                        if (!String.IsNullOrEmpty(fileContents))
                        {
                            XmlOutputBox.Text = fileContents;
                            StartingFileContents = fileContents;
                            this.Title = this.StartingTitle;
                        }
                        break;
                    case MessageBoxResult.No:
                        IsIgnoringExternalChanges = true;
                        this.Title ="*"+ this.StartingTitle ;
                        break;
                }
            }
        }
        private bool HasFileChangedExternally()
        {
            string fileContents = GetCurrentFileContents();
            //                      Are there file contents                 do the fileContents equal what is in the XmlOutput
            bool isFileChanged = !String.IsNullOrEmpty(fileContents) && !fileContents.Equals(StartingFileContents);
            return isFileChanged;
        }
        private string GetCurrentFileContents()
        {
            string parentString = Wrapper.XmlFile.ParentPath ?? "";
            string fileContents = XmlFileManager.GetFileContents(Path.Combine(this.FileLocationPath, parentString), Wrapper.XmlFile.FileName);
            return fileContents;
        }
        private bool IsFileChanged()
        {
            string fileContents = GetCurrentFileContents();
            //                      Are there file contents                 do the fileContents equal what is in the XmlOutput
            bool isFileChanged = !String.IsNullOrEmpty(fileContents) && !fileContents.Equals(XmlOutputBox.Text);
            return isFileChanged;
        }
        private void SaveFile()
        {
            string xmlOut = XmlOutputBox.Text;
            string isInvalid = XmlXpathGenerator.ValidateXml(xmlOut);
            if (isInvalid != null)
            {
                MessageBoxResult saveInvalidXmlDecision = MessageBox.Show(
                    "The xml is not valid! Would you like to save anyway?\n\n" + isInvalid,
                    "Invalid XML!",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);
                switch (saveInvalidXmlDecision)
                {
                    case MessageBoxResult.No:
                        return;
                }
            }
            string parentPath = Wrapper.XmlFile.ParentPath ?? "";
            if (!String.IsNullOrEmpty(xmlOut))
            {
                XmlFileManager.WriteStringToFile(Path.Combine(this.FileLocationPath, parentPath), Wrapper.XmlFile.FileName, xmlOut);
                if (Properties.Settings.Default.AutoMoveMod) XmlFileManager.CopyAllOutputFiles();
                StartingFileContents = xmlOut;
                this.Title = this.StartingTitle;
            }
        }
        private void UpdateViewComponents()
        {
            if (IsFileChanged())
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to reload the file and overwrite any changes?",
                    "Reload File",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string fileContents = GetCurrentFileContents();
                        if (!String.IsNullOrEmpty(fileContents))
                        {
                            XmlOutputBox.Text = fileContents;
                            StartingFileContents = fileContents;
                            this.Title = Wrapper.XmlFile.FileName;
                        }
                        break;
                }
            }
        }
        private void ValidateXmlButton_Click(object sender, RoutedEventArgs e)
        {
            XmlXpathGenerator.ValidateXml(XmlOutputBox.Text, doShowValidationMessage: true);
        }
        private void CombineTagsXmlButton_Click(object sender, RoutedEventArgs e)
        {
            string allXml = this.XmlOutputBox.Text;
            if (XmlXpathGenerator.ValidateXml(allXml, errorPrependMessage: "Error: Could not execute combine function because xml is invalid."))
            {
                this.XmlOutputBox.Text = XmlXpathGenerator.CombineAppendTags(this.Wrapper, allXml);
            }
        }
        private void UndoAllChangesXmlButton_Click(object sender, RoutedEventArgs e)
        {
            this.XmlOutputBox.Text = this.UnchangedStartingFileContents;
        }
        private void CodeCompletionKeys_Click(object sender, RoutedEventArgs e)
        {
            string message = "This window has many keys that open an auto complete window.\n\n" +
                "To use this one simply has to type a key and select the desired value from the dropdown box that appears.\n\n" +
                "Possible keys:\n" +
                "\"<\" (Less than): For opening/closing new tags\n" +
                "\">\" (Greater than): For closing a tag\n" +
                "\"\\\" (Backslash): For closing a tag on a single line\n" +
                "\"\"\" (Double Quote): For common attribute values from the game file and current file \n" +
                "\"!\" (Exclamation Mark): For xml comments\n" +
                "CTRL+\" \" (Spacebar): For attributes and common attribute values from the game file \n";
            string title = "Auto Complete Help";

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
