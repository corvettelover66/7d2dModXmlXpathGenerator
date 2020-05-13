using SevenDaysToDieModCreator.Controllers;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    class MyComboBox : ComboBox
    {
        public MyComboBox(ObjectViewController objectViewController, XmlNode ObjectNode,  bool isGameFileSearchTRee = true)
        {
            ObjectViewController = objectViewController;
            this.IsGameFileSearchTree = isGameFileSearchTRee;
            this.ObjectNode = ObjectNode;
        }

        public TextBox MyTextBox { get; private set; }
        public ObjectViewController ObjectViewController { get; }
        public bool IsGameFileSearchTree { get; }

        public XmlNode ObjectNode { get; private set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            MyTextBox = textBox;
            MyTextBox.Tag = this.Parent;
            MyTextBox.Uid = ObjectNode == null ? "" : ObjectNode.Name;
            XmlObjectsListWrapper wrapperToUse = this.ObjectViewController.LoadedListWrappers.GetValueOrDefault(this.Uid);
            if (IsGameFileSearchTree) this.ObjectViewController.AddTargetContextMenuToControl(MyTextBox, wrapperToUse);
        }
    }
}
