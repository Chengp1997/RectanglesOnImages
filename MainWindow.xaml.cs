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
        private bool chooseMode;
        private bool isDrawing;
        private bool isMoving;
        private Point startPosition;// mouse down position
        Rectangle rect;
        private List<Rectangle> rectangles;
        double spanLeft;
        double spanTop;
        int currentIndex;

        enum beChosen { NOTCHOSEN = 2, CHOSEN=5 }

        public MainWindow()
        {
            InitializeComponent();
            drawingMode = false;
            chooseMode = false;
            isDrawing = false;
            isMoving = false;
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
                    StrokeThickness = (double) beChosen.NOTCHOSEN,
                    Fill = Brushes.Yellow,
                };

                appCanvas.Children.Add(rect);
            }
            else if(chooseMode) //when drawingMode == false -> when mouse down - the item is selected
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
                    isMoving = true;
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

            }else if (chooseMode && isMoving)//if is not drawing mode -- move will change the size of the shape
            {
                Point cursorPosition = e.GetPosition(appCanvas);

                //need to calculate the position of the leftTop and right Bottom to draw the rectangle
                var left = cursorPosition.X - spanLeft;
                var top = cursorPosition.Y - spanTop;

                //regulation -- can not access the grid
                if (left < 0) { left = 0; }
                if (top < 0) {  top = 0; }
                if (left + rect.Width > appCanvas.ActualWidth) { left = appCanvas.ActualWidth - rect.Width; }
                if(top + rect.Height > appCanvas.ActualHeight) { top = appCanvas.ActualHeight - rect.Height; }

                //start drawing
                Point leftTop = new(left,top);
                Point rightBottom = new(left+rect.Width, top+rect.Height);

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
            else if (chooseMode && isMoving) 
            {
                isMoving = false;
                rect.ReleaseMouseCapture();
                spanLeft = 0;
                spanTop = 0;

                //update rect in the list
                rectangles[currentIndex] = rect;
                currentIndex = -1;
            }
        }

        private void ChooseButton_Click(object sender, RoutedEventArgs e)
        {
            chooseMode = !chooseMode;
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

            //if find the one, made the rectangle style different, others back 
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (i == index)
                {
                    rectangles[i].StrokeThickness = (double)beChosen.CHOSEN;
                }
                else
                {
                    rectangles[i].StrokeThickness = (double)(beChosen.NOTCHOSEN);
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
