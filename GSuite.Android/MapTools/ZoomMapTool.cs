using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
/*using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;*/
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Android.Content;
using Android.Widget;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// The panning and zooming panel of the map.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class ZoomMapTool : BaseMapTool
    {
        private const string LevelTipsFormat = "Level {0}";
        private const int ZoomBarHeightPerLevel = 6;

        private float _zoomBarHeight;
        private float _zoomBarWidth;
        private ImageButton _zoomInButton;
        private ImageButton _zoomOutButton;

        public ZoomMapTool(Context context)
            : base(context, true)
        {
            
            _zoomBarHeight = LayoutUnitsUtil.convertDpToPixel(70, Context.Resources.DisplayMetrics.Ydpi);
            _zoomBarWidth = LayoutUnitsUtil.convertDpToPixel(30, Context.Resources.DisplayMetrics.Xdpi);

            RelativeLayout.LayoutParams layout = new RelativeLayout.LayoutParams((int)_zoomBarWidth, (int)_zoomBarHeight);
            layout.AddRule(LayoutRules.AlignParentTop);
            layout.LeftMargin = 10;
            layout.TopMargin = 10;

            this.LayoutParameters = layout;

            RelativeLayout.LayoutParams zoomInLayout = new RelativeLayout.LayoutParams((int)_zoomBarWidth, (int)_zoomBarHeight / 2);
            zoomInLayout.AddRule(LayoutRules.AlignParentTop);
            _zoomInButton = new ImageButton(Context);
            int id = Resource.Drawable.plus_blue_square;
            _zoomInButton.SetImageBitmap(Bitmap.CreateScaledBitmap(
                BitmapFactory.DecodeResource(Resources, global::Mapgenix.GSuite.Android.Resource.Drawable.plus_blue_square), (int)_zoomBarWidth - 5, ((int)_zoomBarHeight / 2) - 8, true));
            _zoomInButton.LayoutParameters = zoomInLayout;
            _zoomInButton.Click += _zoomInButton_Click;

            RelativeLayout.LayoutParams zoomOutLayout = new RelativeLayout.LayoutParams((int)_zoomBarWidth, (int)_zoomBarHeight / 2);
            zoomOutLayout.TopMargin = ((int)_zoomBarHeight / 2) - 7;
            _zoomOutButton = new ImageButton(Context);
            _zoomOutButton.SetImageBitmap(Bitmap.CreateScaledBitmap(
                BitmapFactory.DecodeResource(Resources, global::Mapgenix.GSuite.Android.Resource.Drawable.minus_blue_square), (int)_zoomBarWidth - 5, ((int)_zoomBarHeight / 2) - 8, true));
            _zoomOutButton.LayoutParameters = zoomOutLayout;
            _zoomOutButton.Click += _zoomOutButton_Click;

            AddView(_zoomInButton);
            AddView(_zoomOutButton);
        }        

        private void _zoomInButton_Click(object sender, EventArgs e)
        {
            CurrentMap.ZoomIn();
        }

        private void _zoomOutButton_Click(object sender, EventArgs e)
        {
            CurrentMap.ZoomOut();
        }        
    }
}
