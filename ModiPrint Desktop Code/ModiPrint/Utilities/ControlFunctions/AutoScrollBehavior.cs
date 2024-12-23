using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Utilities.ControlFunctions
{
    /// <summary>
    /// Automatically scrolls to the bottom of the ScrollViewer
    /// Usage: Attach u:AutoScrollBehavior.AutoScroll="True" in the XAML
    /// </summary>
    /// <remarks>
    /// Taken from: http://stackoverflow.com/questions/8370209/how-to-scroll-to-the-bottom-of-a-scrollviewer-automatically-with-xaml-and-bindin
    /// </remarks>
    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));

        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var scrollViewer = obj as ScrollViewer;
            if (scrollViewer != null && (bool)args.NewValue)
            {
                scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
                scrollViewer.ScrollToEnd();
            }
            else
            {
                scrollViewer.LayoutUpdated -= ScrollViewer_SizeChanged;
            }
        }

        private static void ScrollViewer_SizeChanged(object sender, EventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            scrollViewer?.ScrollToEnd();
        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }
    }
}
