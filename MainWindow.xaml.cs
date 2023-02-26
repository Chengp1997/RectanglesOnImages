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

        private bool _startDrawing;

        public MainWindow()
        {
            InitializeComponent();
            _startDrawing = false;
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {   
            //select file from computer
            OpenFileDialog  openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri uri = new Uri(openFileDialog.FileName);
                //using image brush to set the background
                appCanvas.Background = new ImageBrush(new BitmapImage(uri));
            }
        }

        private void Canvas_DrawMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
