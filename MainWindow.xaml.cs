using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RectanglesOnImages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool drawingMode;//only when button is clicked can start drawing
        private bool moveMode;
        private bool isDrawing;
        private bool isCapture;
        private Point startPosition;// mouse down position
        Rectangle rect;
        private List<Rectangle> rectangles;
        double spanLeft;
        double spanTop;
        int currentIndex;

        public MainWindow()
        {
            InitializeComponent();
            drawingMode = false;
            moveMode = false;
            isDrawing = false;
            isCapture = false;
            rectangles = new List<Rectangle>(); 
            spanLeft = 0;
            spanTop = 0;
            currentIndex = -1;
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {   
            //select file from computer
            OpenFileDialog  openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri uri = new Uri(openFileDialog.FileName);
                //using image brush to set the background
                canvasImage.Source = new BitmapImage(uri);
            }
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            //when draw button is clicked -- can only draw rectangles
            //if drawingMode is false -- can select/drag and drop/resize/delete
            drawingMode = !drawingMode;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(drawingMode)
            {
                isDrawing = true;//drawing now
                startPosition = e.GetPosition(appCanvas); //left top position

                rect = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Fill = Brushes.Yellow,
                };

                appCanvas.Children.Add(rect);
            }
            else if(moveMode) //when drawingMode == false -> when mouse down - the item is selected
            {
                //position of my cursor 
                var cursorPosition = e.GetPosition(appCanvas);

                int index = SelectRectangle(cursorPosition);
                Rectangle? selectedRectangle = index == -1? null : rectangles[index];

                //if find the rectangle, capture the rectangle
                if (selectedRectangle != null)
                {
                    rect = selectedRectangle;
                    Mouse.Capture(rect);
                    isCapture = true;
                    currentIndex = index;

                    spanLeft = cursorPosition.X - Canvas.GetLeft(rect);
                    spanTop = cursorPosition.Y - Canvas.GetTop(rect);
                }

            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawingMode && isDrawing)
            {
                Point endPosition = e.GetPosition(appCanvas);

                DrawRectangle(startPosition, endPosition);

            }else if (moveMode && isCapture)//if is not drawing mode -- move will change the size of the shape
            {
                Point cursorPosition = e.GetPosition(appCanvas);

                //need to calculate the position of the leftTop and right Bottom to draw the rectangle

                Point leftTop = new(cursorPosition.X-spanLeft,cursorPosition.Y-spanTop);
                Point rightBottom = new(leftTop.X+rect.Width, leftTop.Y+rect.Height);

                DrawRectangle(leftTop, rightBottom);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (drawingMode && isDrawing)
            {
                isDrawing = false;

                //add current rectangle into the list
                rectangles.Add(rect);
            }
            else if (moveMode && isCapture) 
            {
                isCapture = false;
                rect.ReleaseMouseCapture();
                spanLeft = 0;
                spanTop = 0;

                //update rect in the list
                rectangles[currentIndex] = rect;
                currentIndex = -1;
            }
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            moveMode = !moveMode;
        }

        private int SelectRectangle(Point position)
        {
            var x = position.X;
            var y = position.Y;
            int index = -1;

            //search the rectangles
            for (int i =0;i<rectangles.Count;i++)//search for the desire rectangles
            {
                Rectangle rectangle = rectangles[i];
                if (x <= Canvas.GetRight(rectangle) && x >= Canvas.GetLeft(rectangle))
                {
                    if (y >= Canvas.GetTop(rectangle) && y <= Canvas.GetBottom(rectangle))
                    {
                        index = i;//have to find the last one -- it's the top one
                    }
                }
            }
            return index;
        }

        private void DrawRectangle(Point start, Point end) 
        {
            //calculate the rectangle attributes
            var top = Math.Min(start.Y, end.Y);
            var bottom = Math.Max(start.Y, end.Y);
            var left = Math.Min(start.X, end.X);
            var right = Math.Max(start.X, end.X);

            rect.Width = right - left;
            rect.Height = bottom - top;

            //have to set it align to the left top -- or it will go to the middle
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.VerticalAlignment = VerticalAlignment.Top;

            //put in my canvas
            rect.Margin = new Thickness(left, top, 0, 0);
            //have to set left and top inorder to get the position of the rectangle
            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);
            Canvas.SetRight(rect, right);
            Canvas.SetBottom(rect, bottom);
        }

        
    }
}
