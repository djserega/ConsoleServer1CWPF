using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using notification = Hardcodet.Wpf.TaskbarNotification;

namespace ConsoleServer1C.TaskbarIcon
{
    public static class Icon
    {
        private static readonly notification.TaskbarIcon _notification = new notification.TaskbarIcon();

        public static void SetContextMenu(ContextMenu value)
            => _notification.ContextMenu = value;

        public static void SetIconSource(System.Windows.Media.ImageSource value)
            => _notification.IconSource = value;

        public static void SetToolTipText(string value)
            => _notification.ToolTipText = value;


        public static void ShowBalloonTip(string title, string message)
            => _notification.ShowBalloonTip(title, message, BalloonIcon.Info);

        public static void ShowBalloonTip(string title, string message, System.Drawing.Icon customIcon)
            => _notification.ShowBalloonTip(title, message, customIcon);
    }
}
