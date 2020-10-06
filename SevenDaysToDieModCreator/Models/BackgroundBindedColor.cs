using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace SevenDaysToDieModCreator.Models
{
    class BackgroundBindedColor
    {
        private Brush backgroundColor;
        public Brush BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
                OnPropertyChanged("FooColor");
            }
        }

        private void OnPropertyChanged(string v)
        {
            throw new NotImplementedException();
        }
    }
}
