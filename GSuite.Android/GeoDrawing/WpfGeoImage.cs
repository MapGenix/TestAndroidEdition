using System;
using System.IO;
using System.Windows.Media.Imaging;
using Mapgenix.Canvas;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// This class represents an GeoImage object for WPF.
    /// It uses an Uri and Stream for the image source.
    /// </summary>
    [Serializable]
    public class WpfGeoImage : GeoImage
    {
        public WpfGeoImage(Stream sourceStream)
        {
            SourceStream = sourceStream;

            BitmapImage imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = sourceStream;
            imageSource.EndInit();

            WidthInPixel = imageSource.Width;
            HeightInPixel = imageSource.Height;
        }
        

        public WpfGeoImage(Uri imageUri)
        {
            ImageUri = imageUri;

            BitmapImage imageSource = new BitmapImage(imageUri);
            WidthInPixel = imageSource.Width;
            HeightInPixel = imageSource.Height;
        }

        public Uri ImageUri { get; set; }

        public double WidthInPixel { get; set; }

        public double HeightInPixel { get; set; }

        public Stream SourceStream { get; set; }
    }
}
