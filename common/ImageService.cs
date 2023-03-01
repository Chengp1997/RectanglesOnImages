using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RectanglesOnImageMVVM.common
{
    internal class ImageService
    {
        private BitmapImage _defaultImage;
        public ImageService()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            //_defaultImage = new BitmapImage(new Uri(path.Substring(0, path.LastIndexOf("bin")) + "Images/defaultCanvas.jpg"));
        }

        public BitmapImage DefaultImage { get { return _defaultImage; } }

        public BitmapImage FindImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Upload your canvas";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                Uri uri = new Uri(openFileDialog.FileName);
                _defaultImage = new BitmapImage(uri);
            }
            return _defaultImage;
        }

        public void SaveToFile(FrameworkElement visual)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (saveFileDialog.ShowDialog() == true)
            {
                Uri path = new Uri(saveFileDialog.FileName);
                SaveImage(visual, path);
            }
        }

        private static void SaveImage(FrameworkElement visual, Uri destination)
        {
            Size size = new Size(visual.ActualWidth, visual.ActualHeight);
            if (size.IsEmpty) return;
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(visual), null, new Rect(new Point(), size));
                context.Close();
            }
            bitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = new FileStream(destination.LocalPath, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(stream);
            }
        }
    }
}
