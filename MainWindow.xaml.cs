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

        public MainWindow()
        {
            InitializeComponent();
            _availableToDraw = true;
            _isDrawing = false;
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {   
            //select file from computer
            OpenFileDialog  openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri uri = new Uri(openFileDialog.FileName);
                //using image brush to set the background
                //canvasGrid.Background = new ImageBrush(new BitmapImage(uri));
                canvasImage.Source = new BitmapImage(uri);
            }
        }



        private bool _availableToDraw;//only when button is clicked can start drawing
        private bool _isDrawing;
        private Point _startPosition;
        Rectangle rect;

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            _availableToDraw = !_availableToDraw;
        }

        private void Canvas_DrawMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(_availableToDraw)
            {
                _isDrawing = true;//drawing now
                _startPosition = e.GetPosition(appCanvas); //left top position

                rect = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Fill = Brushes.Yellow,
                };

                appCanvas.Children.Add(rect);
            }
        }

        private void Canvas_DrawMouseMove(object sender, MouseEventArgs e)
        {
            if (_availableToDraw && _isDrawing)
            {
                Point endPosition = e.GetPosition(appCanvas);

                var top = Math.Min(_startPosition.Y, endPosition.Y);
                var bottom = Math.Max(_startPosition.Y, endPosition.Y);
                var left = Math.Min(_startPosition.X, endPosition.X);
                var right = Math.Max(_startPosition.X, endPosition.X);

                rect.Width = right - left;
                rect.Height = bottom - top;

                //rect.HorizontalAlignment = HorizontalAlignment.Left;
                //rect.VerticalAlignment = VerticalAlignment.Top;

                //put in my canvas
                //use canvas.setLeft at beginning -- always at middle -- change to margin
                rect.Margin = new Thickness(left, top, 0, 0);
            }
        }

        private void Canvas_DrawMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_availableToDraw)
            {
                _isDrawing = false;
            }
        }

    }
}
