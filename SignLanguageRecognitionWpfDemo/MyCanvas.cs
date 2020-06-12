using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SignLanguageRecognitionWpfDemo
{
    public partial class MyCanvas : Canvas
    {
        public static float getDpi()
        {
            Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle);
            return currentGraphics.DpiX;
        }

        public void AddVisual(DrawingVisual visual, float dpi)
        {
            double Width = this.ActualWidth;
            double Height = this.ActualHeight;
            int dpWidth = Convert.ToInt32(Width * dpi / 96.0);
            int dpHeight = Convert.ToInt32(Height * dpi / 96.0);

            RenderTargetBitmap bitmapRender = new RenderTargetBitmap(dpWidth, dpHeight, dpi, dpi, PixelFormats.Pbgra32);
            bitmapRender.Render((Visual)visual);

            var image = new System.Windows.Controls.Image();
            image.Source = bitmapRender;
            image.Stretch = Stretch.Uniform;

            this.Children.Clear();
            this.Children.Add(image);
        }

        public void AddBitmapImage(BitmapSource source)
        {
            this.Children.Clear();
            if (source == null) return;
            System.Windows.Controls.Image BitmapImage = new System.Windows.Controls.Image();
            BitmapImage.Source = source;

            this.Children.Add(BitmapImage);
        }
    }
}
