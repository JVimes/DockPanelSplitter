using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace System.Windows.Controls
{
    /// <summary>
    ///   Like <see cref="GridSplitter"/>, but for <see cref="DockPanel"/>
    ///   instead of <see cref="Grid"/>.
    /// </summary>
    public partial class DockPanelSplitter : Thumb
    {
        bool isHorizontal;
        bool isBottomOrRight;
        FrameworkElement previousSibling;
        double? initialLength;


        /// <summary> </summary>
        public DockPanelSplitter()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            MouseDoubleClick += OnMouseDoubleClick;
            DragStarted += OnDragStarted;
            DragDelta += OnDragDelta;
        }


        DockPanel Panel => Parent as DockPanel;


        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(Parent is DockPanel))
                throw new InvalidOperationException($"{nameof(DockPanelSplitter)} must be directly in a DockPanel.");

            if (GetPreviousSiblingOrDefault() == default)
                throw new InvalidOperationException($"{nameof(DockPanelSplitter)} must be directly after a FrameworkElement");
        }

        void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
            => SetSiblingLength(initialLength.Value);

        void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            var position = DockPanel.GetDock(this);
            isHorizontal = GetIsHorizontal(position);
            isBottomOrRight = position == Dock.Bottom || position == Dock.Right;
            previousSibling = GetPreviousSiblingOrDefault();
            initialLength ??= GetSiblingLength();
        }

        void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var change = isHorizontal ? e.VerticalChange : e.HorizontalChange;
            if (isBottomOrRight) change = -change;

            var siblingLength = GetSiblingLength();
            var newSiblingLength = siblingLength + change;
            if (newSiblingLength < 0) newSiblingLength = 0;
            newSiblingLength = Math.Round(newSiblingLength);

            SetSiblingLength(newSiblingLength);
        }

        FrameworkElement GetPreviousSiblingOrDefault()
        {
            var siblings = Panel.Children.OfType<object>();
            var splitterIndex = Panel.Children.IndexOf(this);
            return siblings.ElementAtOrDefault(splitterIndex - 1) as FrameworkElement;
        }

        void SetSiblingLength(double length)
        {
            if (isHorizontal) previousSibling.Height = length;
            else previousSibling.Width = length;
        }

        double GetSiblingLength() => isHorizontal ?
                                     previousSibling.ActualHeight :
                                     previousSibling.ActualWidth;

        static bool GetIsHorizontal(Dock position)
            => position == Dock.Top || position == Dock.Bottom;


        internal class CursorConverter : IValueConverter
        {
            public static CursorConverter Instance { get; } = new CursorConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var position = (Dock)value;
                var isHorizontal = GetIsHorizontal(position);
                return isHorizontal ? Cursors.SizeNS : Cursors.SizeWE;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }
    }
}
