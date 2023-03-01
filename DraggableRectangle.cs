using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RectanglesOnImages
{
    class DraggableRectangle : Adorner
    {
        //8thumb for different corner
        Thumb _leftThumb, _topThumb, _rightThumb, _bottomThumb;
        Thumb _leftTopThumb, _rightTopThumb, _leftBottomThumb, _rightBottomThumb;

        UIElement _adornedElement;
        Grid _grid;
        public DraggableRectangle(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = adornedElement;

            //initialize thumb
            _leftThumb = new Thumb();
            _leftThumb.HorizontalAlignment = HorizontalAlignment.Left;
            _leftThumb.VerticalAlignment = VerticalAlignment.Center;
            _leftThumb.Cursor = Cursors.SizeWE;

            _topThumb = new Thumb();
            _topThumb.HorizontalAlignment = HorizontalAlignment.Center;
            _topThumb.VerticalAlignment = VerticalAlignment.Top;
            _topThumb.Cursor = Cursors.SizeNS;

            _rightThumb = new Thumb();
            _rightThumb.HorizontalAlignment = HorizontalAlignment.Right;
            _rightThumb.VerticalAlignment = VerticalAlignment.Center;
            _rightThumb.Cursor = Cursors.SizeWE;

            _bottomThumb = new Thumb();
            _bottomThumb.HorizontalAlignment = HorizontalAlignment.Center;
            _bottomThumb.VerticalAlignment = VerticalAlignment.Bottom;
            _bottomThumb.Cursor = Cursors.SizeNS;

            _leftTopThumb = new Thumb();
            _leftTopThumb.HorizontalAlignment = HorizontalAlignment.Left;
            _leftTopThumb.VerticalAlignment = VerticalAlignment.Top;
            _leftTopThumb.Cursor = Cursors.SizeNWSE;

            _rightTopThumb = new Thumb();
            _rightTopThumb.HorizontalAlignment = HorizontalAlignment.Right;
            _rightTopThumb.VerticalAlignment = VerticalAlignment.Top;
            _rightTopThumb.Cursor = Cursors.SizeNESW;

            _rightBottomThumb = new Thumb();
            _rightBottomThumb.HorizontalAlignment = HorizontalAlignment.Right;
            _rightBottomThumb.VerticalAlignment = VerticalAlignment.Bottom;
            _rightBottomThumb.Cursor = Cursors.SizeNWSE;

            _leftBottomThumb = new Thumb();
            _leftBottomThumb.HorizontalAlignment = HorizontalAlignment.Left;
            _leftBottomThumb.VerticalAlignment = VerticalAlignment.Bottom;
            _leftBottomThumb.Cursor = Cursors.SizeNESW;

            //add thumb to the grid for this layer
            _grid = new Grid();
            _grid.Children.Add(_leftThumb);
            _grid.Children.Add(_topThumb);
            _grid.Children.Add(_rightThumb);
            _grid.Children.Add(_bottomThumb);
            _grid.Children.Add(_leftTopThumb);
            _grid.Children.Add(_rightTopThumb);
            _grid.Children.Add(_rightBottomThumb);
            _grid.Children.Add(_leftBottomThumb);
            AddVisualChild(_grid);

            foreach (Thumb thumb in _grid.Children)
            {
                thumb.Width = 10;
                thumb.Height = 10;
                thumb.Background = Brushes.Green;
                thumb.Template = new ControlTemplate(typeof(Thumb))
                {
                    VisualTree = GetFactory(new SolidColorBrush(Colors.White))
                };
                thumb.DragDelta += Thumb_DragDelta;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _grid;
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            //arrange the size of the grid according to the thumb position
            _grid.Arrange(new Rect(new Point(-_leftThumb.Width / 2, -_leftThumb.Height / 2), new Size(finalSize.Width + _leftThumb.Width, finalSize.Height + _leftThumb.Height)));
            return finalSize;
        }


        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var element = _adornedElement as FrameworkElement;
            var thumb = sender as FrameworkElement;
            double left, top, right, bottom, width, height;

            //change left side element 
            if (thumb.HorizontalAlignment == HorizontalAlignment.Left)
            {
                //right - not changed; left - change margin
                right = element.Margin.Right;
                left = element.Margin.Left + e.HorizontalChange;
                width = (double.IsNaN(element.Width) ? element.ActualWidth : element.Width) - e.HorizontalChange;
            }
            else
            {
                left = element.Margin.Left;
                right = element.Margin.Right - e.HorizontalChange;
                width = (double.IsNaN(element.Width) ? element.ActualWidth : element.Width) + e.HorizontalChange;
            }

            //change top side element
            if (thumb.VerticalAlignment == VerticalAlignment.Top)
            {
                bottom = element.Margin.Bottom;
                top = element.Margin.Top + e.VerticalChange;
                height = (double.IsNaN(element.Height) ? element.ActualHeight : element.Height) - e.VerticalChange;

            }
            else
            {
                top = element.Margin.Top;
                bottom = element.Margin.Bottom - e.VerticalChange;
                height = (double.IsNaN(element.Height) ? element.ActualHeight : element.Height) + e.VerticalChange;
            }

            //change middle element
            if (thumb.HorizontalAlignment != HorizontalAlignment.Center)
            {
                if (width >= 0)
                {
                    element.Margin = new Thickness(left, element.Margin.Top, right, element.Margin.Bottom);
                    element.Width = width;
                }
            }

            if (thumb.VerticalAlignment != VerticalAlignment.Center)
            {
                if (height >= 0)
                {
                    element.Margin = new Thickness(element.Margin.Left, top, element.Margin.Right, bottom);
                    element.Height = height;
                }
            }
        }
        //thumb style
        FrameworkElementFactory GetFactory(Brush back)
        {
            var fef = new FrameworkElementFactory(typeof(Ellipse));
            fef.SetValue(Ellipse.FillProperty, back);
            fef.SetValue(Ellipse.StrokeProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")));
            fef.SetValue(Ellipse.StrokeThicknessProperty, (double)2);
            return fef;
        }

    }
}
