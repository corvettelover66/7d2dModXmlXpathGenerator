using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows.Media;

namespace SevenDaysToDieModCreator.Views
{
    internal class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text, string descriptionAddition = "")
        {
            this.Text = text;
            this.DescriptionAddition = descriptionAddition;
        }
        public string DescriptionAddition { get; private set; }

        public ImageSource Image { get { return null; } }

        public string Text { get; private set; }

        public object Content
        {
            get { return this.Text; }
        }
        public object Description
        {
            get { return this.DescriptionAddition + this.Text; }
        }

        public double Priority
        {
            get { return 1; }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}