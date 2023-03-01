using RectanglesOnImageMVVM.ViewModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RectanglesOnImages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CanvasViewModel _canvasViewModel;
        public MainWindow()
        {
            InitializeComponent();
            _canvasViewModel = new CanvasViewModel();
            canvasImage.Source = _canvasViewModel.CanvasImage;

            colorPicker.ItemsSource = typeof(Colors).GetProperties();
            //make it default to white
            colorPicker.SelectedItem = typeof(Colors).GetProperty("White");
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            canvasImage.Source = _canvasViewModel.UploadCanvasImage();
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            //when draw button is clicked -- can only draw rectangles
            //if drawingMode is false -- can select/drag and drop/resize/delete
            _canvasViewModel.DrawingMode = !_canvasViewModel.DrawingMode;

            //style changed
            if (_canvasViewModel.DrawingMode) { drawButton.Background = Brushes.LightGreen; }
            else { drawButton.Background = Brushes.Pink; }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int toDelete = _canvasViewModel.RemoveRectangle();
            if (toDelete >= 0)
            {
                appCanvas.Children.RemoveAt(toDelete + 1);//start from 1!!!!
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            _canvasViewModel.SaveToFile(appCanvas);
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canvasViewModel.Rect != null)
            {
                Color selectedColor = (Color)(colorPicker.SelectedItem as PropertyInfo).GetValue(null, null);
                _canvasViewModel.Rect.Fill = new SolidColorBrush(selectedColor);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SolidColorBrush currentColor = new SolidColorBrush((Color)(colorPicker.SelectedItem as PropertyInfo).GetValue(null, null));

            var mouseDownPosition = e.GetPosition(appCanvas);
            _canvasViewModel.MouseDownOnCanvas(mouseDownPosition, currentColor);
            if (_canvasViewModel.DrawingMode)
            {
                appCanvas.Children.Add(_canvasViewModel.Rect);
            }

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            _canvasViewModel.MouseMoveOnCanvas(e.GetPosition(appCanvas), appCanvas);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _canvasViewModel.MouseUpOnCanvas();
        }



    }
}