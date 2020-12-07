using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace SevenDaysToDieModCreator.Controllers
{
    static class BackgroundColorController
    {
        public static SolidColorBrush NORMAL_MODE = Brushes.White;
        public static SolidColorBrush MEDUM_MODE = Brushes.Bisque;
        public static SolidColorBrush DARK_MODE = Brushes.DarkGray;

        public static bool IsModeActivated() 
        {
            return Properties.Settings.Default.IsDarkModeActive || Properties.Settings.Default.SettingIsMediumModeActive;
        }
        public static SolidColorBrush GetBackgroundColor() 
        {
            SolidColorBrush colorToReturn;
            if (Properties.Settings.Default.IsDarkModeActive) colorToReturn = DARK_MODE;
            else if (Properties.Settings.Default.SettingIsMediumModeActive) colorToReturn = MEDUM_MODE;
            else colorToReturn = NORMAL_MODE;
            return colorToReturn;
        }
    }
}
