using Android;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using System.ComponentModel;
using System.IO;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// This class represents a logo to display on the map.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class LogoMapTool : BaseMapTool
    {


        private ImageView _imageSource;

        //public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(LogoMapTool), new PropertyMetadata(new PropertyChangedCallback(SourceChanged)));
        public LogoMapTool(Context context)
            : base(context, true)
        {
            //DefaultStyleKey = typeof(LogoMapTool);
        }

        public LogoMapTool(Context context, Bitmap imageSource)
            : base(context, true)
        {
            this.Source = new ImageView(this.CurrentMap.Context);
            Source.SetImageBitmap(imageSource);
        }

       
        public ImageView Source
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                AddView(_imageSource);
            }
        }
    }
}
