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
        FrameworkElement target;
        double? initialLength;
        double availableSpace;


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

            if (GetTargetOrDefault() == default)
                throw new InvalidOperationException($"{nameof(DockPanelSplitter)} must be directly after a FrameworkElement");
        }

        void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (initialLength != null)
                SetTargetLength(initialLength.Value);
        }

        void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            isHorizontal = GetIsHorizontal(this);
            isBottomOrRight = GetIsBottomOrRight();
            target = GetTargetOrDefault();
            initialLength ??= GetTargetLength();
            availableSpace = GetAvailableSpace();
        }

        void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var change = isHorizontal ? e.VerticalChange : e.HorizontalChange;
            if (isBottomOrRight) change = -change;

            var targetLength = GetTargetLength();
            var newTargetLength = targetLength + change;
            newTargetLength = Clamp(newTargetLength, 0, availableSpace);
            newTargetLength = Math.Round(newTargetLength);

            SetTargetLength(newTargetLength);
        }

        FrameworkElement GetTargetOrDefault()
        {
            var children = Panel.Children.OfType<object>();
            var splitterIndex = Panel.Children.IndexOf(this);
            return children.ElementAtOrDefault(splitterIndex - 1) as FrameworkElement;
        }

        bool GetIsBottomOrRight()
        {
            var position = DockPanel.GetDock(this);
            return position == Dock.Bottom || position == Dock.Right;
        }

        double GetAvailableSpace()
        {
            var lastChild =
                Panel.LastChildFill ?
                Panel.Children.OfType<object>().Last() as FrameworkElement :
                null;

            var fixedChildren =
                from child in Panel.Children.OfType<FrameworkElement>()
                where GetIsHorizontal(child) == isHorizontal
                where child != target
                where child != lastChild
                select child;

            var panelLength = GetLength(Panel);
            var unavailableSpace = fixedChildren.Sum(c => GetLength(c));
            return panelLength - unavailableSpace;
        }

        void SetTargetLength(double length)
        {
            if (isHorizontal) target.Height = length;
            else target.Width = length;
        }

        double GetTargetLength() => GetLength(target);

        static bool GetIsHorizontal(FrameworkElement element)
        {
            var position = DockPanel.GetDock(element);
            return GetIsHorizontal(position);
        }

        static bool GetIsHorizontal(Dock position)
            => position == Dock.Top || position == Dock.Bottom;

        static double Clamp(double value, double min, double max)
            => value < min ? min :
               value > max ? max :
               value;

        double GetLength(FrameworkElement element)
            => isHorizontal ?
               element.ActualHeight :
               element.ActualWidth;


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
