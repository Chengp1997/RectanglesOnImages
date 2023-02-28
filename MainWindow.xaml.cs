using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private bool canDraw;
        private bool canMove;
        private Point startPosition;// use when drawing -- check current startPosition
        private Rectangle? rect; // current chosen rectangle
        private List<Rectangle> rectangles;
        private double spanLeft;
        private double spanTop;
        private int currentIndex;

        enum beChosen { NOTCHOSEN = 2, CHOSEN = 5 }

        public MainWindow()
        {
            InitializeComponent();
            this.drawingMode = false;
            this.canDraw = false;
            this.canMove = false;
            this.rectangles = new List<Rectangle>();
            ResetCurrentStatus();
            colorPicker.ItemsSource = typeof(Colors).GetProperties();
            //make it default to white
            colorPicker.SelectedItem = typeof(Colors).GetProperty("White");
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            //select file from computer
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Upload your canvas";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
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

            //style changed
            if (drawingMode) { drawButton.Background = Brushes.LightGreen; }
            else { drawButton.Background = Brushes.Pink; }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (rect != null)
            {
                rectangles.RemoveAt(currentIndex);
                appCanvas.Children.RemoveAt(currentIndex + 1);//start from 1!!!!
                ResetCurrentStatus();
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (saveFileDialog.ShowDialog() == true)
            {
                Uri path = new Uri(saveFileDialog.FileName);
                SaveImage(path);
            }
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rect != null)
            {
                Color selectedColor = (Color)(colorPicker.SelectedItem as PropertyInfo).GetValue(null, null);
                rect.Fill = new SolidColorBrush(selectedColor);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (drawingMode)
            {
                this.canDraw = true;//drawing now
                this.startPosition = e.GetPosition(appCanvas); //left top position

                this.rect = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = (double)beChosen.NOTCHOSEN,
                    Fill = new SolidColorBrush((Color)(colorPicker.SelectedItem as PropertyInfo).GetValue(null, null)),
                };

                appCanvas.Children.Add(rect);

            }
            else //when not drawing mode - can choose rectangles
            {
                //position of my cursor 
                var cursorPosition = e.GetPosition(appCanvas);
                //choose rectangles on the canvas
                ChooseRectangle(cursorPosition);

                //if find the rectangle, capture the rectangle
                if (rect != null)
                {
                    Mouse.Capture(rect);
                    this.canMove = true;// if chosen - can move now
                }
            }

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawingMode && canDraw)
            {
                Point endPosition = e.GetPosition(appCanvas);

                DrawRectangle(startPosition, endPosition);

            }
            else if (canMove)//if is not drawing mode -- move will change the size of the shape
            {
                Point cursorPosition = e.GetPosition(appCanvas);

                //need to calculate the position of the leftTop and right Bottom to draw the rectangle
                var left = cursorPosition.X - spanLeft;
                var top = cursorPosition.Y - spanTop;

                //regulation -- can not access the grid
                if (left < 0) { left = 0; }
                if (top < 0) { top = 0; }
                if (left + rect.Width > appCanvas.ActualWidth) { left = appCanvas.ActualWidth - rect.Width; }
                if (top + rect.Height > appCanvas.ActualHeight) { top = appCanvas.ActualHeight - rect.Height; }

                //start drawing
                Point leftTop = new(left, top);
                Point rightBottom = new(left + rect.Width, top + rect.Height);

                DrawRectangle(leftTop, rightBottom);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rect != null)
            {
                if (drawingMode && canDraw)
                {
                    canDraw = false;

                    //add current rectangle into the list
                    rectangles.Add(rect);
                    rect = null;
                }
                else if (canMove)//for choosing mode -- when mouseup, update the new rectangle
                {
                    canMove = false;
                    rect.ReleaseMouseCapture();
                    spanLeft = 0;
                    spanTop = 0;

                    //update rect in the list
                    rectangles[currentIndex] = rect;
                }
            }
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

        private void ChooseRectangle(Point cursorPosition)
        {

            var x = cursorPosition.X;
            var y = cursorPosition.Y;
            int index = -1;

            //search the rectangles
            for (int i = 0; i < rectangles.Count; i++)//search for the desire rectangles
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

            this.currentIndex = index;

            //refresh the rectangle styles
            RefreshRectangles();

            if (currentIndex == -1) {
                ResetCurrentStatus();
                return;
            }// if not found ,just return

            //if found, set the status of current rectangles
            rect = rectangles[currentIndex];

            spanLeft = cursorPosition.X - Canvas.GetLeft(rect);
            spanTop = cursorPosition.Y - Canvas.GetTop(rect);


        }

        private void RefreshRectangles()
        {
            //refresh stroke
            for (int i = 0; i < rectangles.Count; i++)
            {
                var layer = AdornerLayer.GetAdornerLayer(rectangles[i]);
                var adorner = layer.GetAdorners(rectangles[i]);
                if (adorner != null) { layer.Remove(adorner[0]); }
                if (i == currentIndex)
                {
                    layer.Add(new DraggableRectangle(rectangles[i]));
                    rectangles[i].StrokeThickness = (double)beChosen.CHOSEN;
                    continue;
                }
                rectangles[i].StrokeThickness = (double)beChosen.NOTCHOSEN;
            }



        }

        private void ResetCurrentStatus()
        {
            this.currentIndex = -1;
            this.rect = null;
            spanLeft = 0;
            spanTop = 0;
        }

        private void SaveImage(Uri destination)
        {
            Size size = new Size(appCanvas.ActualWidth, appCanvas.ActualHeight);
            if (size.IsEmpty) return;
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(appCanvas), null, new Rect(new Point(), size));
                context.Close();
            }
            bitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using(FileStream stream = new FileStream(destination.LocalPath, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(stream);
            }
        }

    }
}
