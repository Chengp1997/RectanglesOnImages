using RectanglesOnImageMVVM.common;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RectanglesOnImageMVVM.ViewModel
{
    class CanvasViewModel
    {
        private ObservableCollection<Rectangle> _rectangles = new ObservableCollection<Rectangle>();
        private BitmapImage _canvasImage;
        private bool _drawingMode;//only when button is clicked can start drawing
        private Rectangle? _rect; // current chosen rectangle
        private readonly ImageService _fileService;
        private double _spanLeft;
        private double _spanTop;
        private int _currentIndex;
        private bool _canDraw;
        private bool _canMove;
        private Point _startPosition;// use when drawing -- check current startPosition

        enum BeChosen { NOTCHOSEN = 2, CHOSEN = 5 }

        public CanvasViewModel()
        {
            //default image
            _fileService = new ImageService();
            _canvasImage = _fileService.DefaultImage;
            ResetCurrentStatus();
            _drawingMode = false;
            _canDraw = false;
            _canMove = false;
        }

        public ObservableCollection<Rectangle> Rectangles
        {
            get { return _rectangles; }
            set { _rectangles = value; }
        }

        public BitmapImage CanvasImage
        {
            get { return _canvasImage; }
        }

        public Rectangle? Rect
        {
            get { return _rect; }
            set { if (_rect != value) { _rect = value; } }
        }

        public bool DrawingMode { get { return _drawingMode; } set { _drawingMode = value; } }

        public int RemoveRectangle()
        {
            int toDelete = _currentIndex;
            if (_rect != null)
            {
                _rectangles.RemoveAt(_currentIndex);
                ResetCurrentStatus();
            }
            return toDelete;
        }

        public void DrawRectangle(Point start, Point end)
        {
            //calculate the rectangle attributes
            var top = Math.Min(start.Y, end.Y);
            var bottom = Math.Max(start.Y, end.Y);
            var left = Math.Min(start.X, end.X);
            var right = Math.Max(start.X, end.X);

            _rect.Width = right - left;
            _rect.Height = bottom - top;

            //have to set it align to the left top -- or it will go to the middle
            _rect.HorizontalAlignment = HorizontalAlignment.Left;
            _rect.VerticalAlignment = VerticalAlignment.Top;

            //put in my canvas
            _rect.Margin = new Thickness(left, top, 0, 0);
            //have to set left and top inorder to get the position of the rectangle
            Canvas.SetLeft(_rect, left);
            Canvas.SetTop(_rect, top);
            Canvas.SetRight(_rect, right);
            Canvas.SetBottom(_rect, bottom);
        }

        public void ChooseCurrentRectangle(Point cursorPosition)
        {
            var x = cursorPosition.X;
            var y = cursorPosition.Y;
            int index = -1;

            //search the rectangles
            for (int i = 0; i < _rectangles.Count; i++)//search for the desire rectangles
            {
                Rectangle rectangle = _rectangles[i];
                if (x <= Canvas.GetRight(rectangle) && x >= Canvas.GetLeft(rectangle))
                {
                    if (y >= Canvas.GetTop(rectangle) && y <= Canvas.GetBottom(rectangle))
                    {
                        index = i;//have to find the last one -- it's the top one
                    }
                }
            }

            _currentIndex = index;

            //refresh the rectangle styles
            RefreshRectangles();

            if (_currentIndex == -1)
            {
                ResetCurrentStatus();
                return;
            }// if not found ,just return

            //if found, set the status of current rectangles
            _rect = _rectangles[_currentIndex];

            _spanLeft = cursorPosition.X - Canvas.GetLeft(_rect);
            _spanTop = cursorPosition.Y - Canvas.GetTop(_rect);

        }

        public BitmapImage UploadCanvasImage()
        {
            this._canvasImage = _fileService.FindImage();
            return _canvasImage;
        }

        public void SaveToFile(FrameworkElement visual)
        {
            _fileService.SaveToFile(visual);
        }

        public void ResetCurrentStatus()
        {
            this._currentIndex = -1;
            this._rect = null;
            this._spanLeft = 0;
            this._spanTop = 0;
        }

        private void RefreshRectangles()
        {
            //refresh stroke
            for (int i = 0; i < _rectangles.Count; i++)
            {
                var layer = AdornerLayer.GetAdornerLayer(_rectangles[i]);
                var adorner = layer.GetAdorners(_rectangles[i]);
                if (adorner != null) { layer.Remove(adorner[0]); }
                if (i == _currentIndex)
                {
                    layer.Add(new DraggableRectangle(_rectangles[i]));
                    _rectangles[i].StrokeThickness = (double)BeChosen.CHOSEN;
                    continue;
                }
                _rectangles[i].StrokeThickness = (double)BeChosen.NOTCHOSEN;
            }

        }

        public void MouseDownOnCanvas(Point mouseDownPosition, SolidColorBrush currentColor)
        {
            if (_drawingMode)
            {

                _startPosition = mouseDownPosition;
                _canDraw = true;//drawing now
                _rect = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = (double)BeChosen.NOTCHOSEN,
                    Fill = currentColor,
                };

            }
            else //when not drawing mode - can choose rectangles
            {
                //choose rectangles on the canvas
                ChooseCurrentRectangle(mouseDownPosition);
                //refresh the rectangle styles
                RefreshRectangles();

                //if find the rectangle, capture the rectangle
                if (_rect != null)
                {
                    Mouse.Capture(_rect);
                    _canMove = true;// if chosen - can move now
                }
            }
        }

        public void MouseMoveOnCanvas(Point endPosition, FrameworkElement canvas)
        {
            if (_drawingMode && _canDraw)
            {
                DrawRectangle(_startPosition, endPosition);
            }
            else if (_canMove)//if is not drawing mode -- move will change the size of the shape
            {
                //need to calculate the position of the leftTop and right Bottom to draw the rectangle
                var left = endPosition.X - _spanLeft;
                var top = endPosition.Y - _spanTop;

                //regulation -- can not access the grid
                if (left < 0) { left = 0; }
                if (top < 0) { top = 0; }
                if (left + _rect.Width > canvas.ActualWidth) { left = canvas.ActualWidth - _rect.Width; }
                if (top + _rect.Height > canvas.ActualHeight) { top = canvas.ActualHeight - _rect.Height; }

                //start drawing
                Point leftTop = new(left, top);
                Point rightBottom = new(left + _rect.Width, top + _rect.Height);

                DrawRectangle(leftTop, rightBottom);
            }
        }

        public void MouseUpOnCanvas()
        {
            if (_rect != null)
            {
                if (_drawingMode && _canDraw)
                {
                    _canDraw = false;

                    //add current rectangle into the list
                    _rectangles.Add(_rect);
                    _rect = null;
                }
                else if (_canMove)//for choosing mode -- when mouseup, update the new rectangle
                {
                    _canMove = false;
                    _rect.ReleaseMouseCapture();
                    _spanLeft = 0;
                    _spanTop = 0;

                    //update rect in the list
                    _rectangles[_currentIndex] = _rect;
                }
            }
        }
    }
}
