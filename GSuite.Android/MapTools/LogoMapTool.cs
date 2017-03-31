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

        private float _logoWidth;
        private float _logoHeight;
        private Bitmap _imageSource;
        private ImageView _image;

        //public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(LogoMapTool), new PropertyMetadata(new PropertyChangedCallback(SourceChanged)));
        public LogoMapTool(Context context)
            : base(context, true)
        {
            try
            {
                _logoWidth = LayoutUnitsUtil.convertDpToPixel(40, Context.Resources.DisplayMetrics.Xdpi);
                _logoHeight = LayoutUnitsUtil.convertDpToPixel(40, Context.Resources.DisplayMetrics.Ydpi);

                _imageSource = Bitmap.CreateScaledBitmap(
                    BitmapFactory.DecodeResource(Resources, global::Mapgenix.GSuite.Android.Resource.Drawable.PowerBy), (int)_logoWidth, (int)_logoHeight, true);
                _image = new ImageView(Context);
                _image.SetImageBitmap(_imageSource);

                RelativeLayout.LayoutParams layout = new RelativeLayout.LayoutParams((int)_logoWidth, (int)_logoHeight);
                layout.AddRule(LayoutRules.AlignParentEnd);
                layout.AddRule(LayoutRules.AlignParentBottom);
                layout.RightMargin = 10;
                layout.BottomMargin = 40;

                LayoutParameters = layout;

                AddView(_image);
            }
            catch (System.Exception ex)
            {

                throw;
            }            
        }

        public LogoMapTool(Context context, Bitmap imageSource)
            : base(context, true)
        {
            _imageSource = imageSource;
            _image.SetImageBitmap(_imageSource);

            _logoWidth = LayoutUnitsUtil.convertDpToPixel(60, Context.Resources.DisplayMetrics.Xdpi);
            _logoHeight = LayoutUnitsUtil.convertDpToPixel(60, Context.Resources.DisplayMetrics.Ydpi);

            RelativeLayout.LayoutParams layout = new RelativeLayout.LayoutParams((int)_logoWidth, (int)_logoHeight);
            layout.AddRule(LayoutRules.AlignParentEnd);
            layout.AddRule(LayoutRules.AlignParentBottom);
            layout.RightMargin = 10;
            layout.BottomMargin = 10;

            LayoutParameters = layout;

            AddView(_image);
        }

       
        public Bitmap Source
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                _image.SetImageBitmap(value);
            }
        }
    }
}
