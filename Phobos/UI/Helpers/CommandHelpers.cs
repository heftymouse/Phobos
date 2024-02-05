using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.UI.Helpers
{
    internal class CommandHelpers : DependencyObject
    {
        public static List<XamlUICommand> GetCommands(DependencyObject obj)
        {
            return (List<XamlUICommand>)obj.GetValue(CommandsProperty);
        }

        public static void SetCommands(DependencyObject obj, List<XamlUICommand> value)
        {
            if(obj is MenuFlyout flyout)
            {
                obj.SetValue(CommandsProperty, value);
                value.ForEach((e) => flyout.Items.Add(new MenuFlyoutItem()
                {
                    Command = e,
                }));
            }
        }

        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.RegisterAttached("Commands", typeof(List<XamlUICommand>), typeof(CommandHelpers), null);
    }
}
