using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mapgenix.GSuite.Wpf.Properties;

namespace Mapgenix.GSuite.Wpf
{
    internal static class InstallerHelper
    {
        internal static bool isInitialized;
        internal static bool isLicensed;
        internal static bool isExpired;
        internal static int daysLeft;

        internal static void CheckInstaller(System.Windows.Controls.Canvas canvas, double canvasWidth, double canvasHeight)
        {
            if (isLicensed == false && isExpired == true)
            {
                DrawExpiredWaterMark("EVALUATION EXPIRED",canvas, canvasWidth, canvasHeight);
            }
            else if (isLicensed == false && isExpired == false)
            {
                DrawEvalWaterMark("EVALUATION \r\n" + daysLeft.ToString() + " day(s) left", canvas, canvasWidth, canvasHeight);
            }
           
        }

        private static void DrawEvalWaterMark(string message, System.Windows.Controls.Canvas canvas, double canvasWidth, double canvasHeight)
        {
            if (canvas != null)
            {
                FontFamily fontFamily = new FontFamily("Arial");
                double fontSize = 16;
                Brush foregroundBrush = new SolidColorBrush(Color.FromArgb(100, 128,128,128));
                
                canvas.Children.Clear();

                Random random = new Random(DateTime.Now.Millisecond);

                for (int height = 0; height < canvasHeight; height += 256)
                {
                    for (int width = 0; width < canvasWidth; width += 256)
                    {
                        double x = random.Next(width, width + 128);
                        double y = random.Next(height, height + 128);

                        TextBlock textBlock = new TextBlock();
                        textBlock.Width = 200;
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        textBlock.Foreground = foregroundBrush;
                        textBlock.Text = message;
                        textBlock.FontFamily = fontFamily;
                        textBlock.FontSize = fontSize;
                        textBlock.FontWeight = FontWeights.Bold;
                        canvas.Children.Add(textBlock);

                         System.Windows.Controls.Canvas.SetLeft(textBlock, x);
                         System.Windows.Controls.Canvas.SetTop(textBlock, y);
                           
                     }
                }
           }
        }

        private static void DrawExpiredWaterMark(string message, System.Windows.Controls.Canvas canvas, double canvasWidth, double canvasHeight)
        {
            if (canvas != null)
            {
                FontFamily fontFamily = new FontFamily("Arial");
                double fontSize = 16;
                Brush foregroundBrush = new SolidColorBrush(Color.FromArgb(100, 128, 128, 128));
                Brush backgroundBrush = new SolidColorBrush(Colors.White);

                canvas.Children.Clear();

                Random random = new Random(DateTime.Now.Millisecond);
                canvas.Background = backgroundBrush;
                for (int height = 0; height < canvasHeight; height += 256)
                {
                    for (int width = 0; width < canvasWidth; width += 256)
                    {
                        double x = random.Next(width, width + 128);
                        double y = random.Next(height, height + 128);

                        TextBlock textBlock = new TextBlock();
                        textBlock.Width = 120;
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        textBlock.Foreground = foregroundBrush;
                        textBlock.Text = message;
                        textBlock.FontFamily = fontFamily;
                        textBlock.FontSize = fontSize;
                        textBlock.FontWeight = FontWeights.Bold;
                        canvas.Children.Add(textBlock);

                        System.Windows.Controls.Canvas.SetLeft(textBlock, x);
                        System.Windows.Controls.Canvas.SetTop(textBlock, y);

                    }
                }
            }
        }
    }
}
