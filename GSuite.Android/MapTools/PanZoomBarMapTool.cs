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

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// The panning and zooming panel of the map.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class PanZoomBarMapTool : BaseMapTool
    {
        private const string LevelTipsFormat = "Level {0}";
        private const int ZoomBarHeightPerLevel = 6;

        /*public static readonly DependencyProperty GlobeButtonVisibilityProperty = DependencyProperty.Register("GlobeButtonVisibility", typeof(Visibility), typeof(PanZoomBarMapTool), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty GlobeButtonImageSourceProperty = DependencyProperty.Register("GlobeButtonImageSource", typeof(BitmapSource), typeof(PanZoomBarMapTool));

        public event EventHandler<ExtentEventArgs> GlobeButtonClick;*/

        /*private Image _zoomInImage;
        private Image _zoomOutImage;
        private Image _zoomHandlerImage;
        private Image _globeImage;
        private System.Windows.Controls.Canvas _zoomHandlerTip;
        private TextBlock _zoomHandlerTipLabel;
        private System.Windows.Controls.Canvas _zoomBar;
        private Grid _panPanel;*/
        private int _zoomBarHeight;
        private bool _isDragging;
        /*private Point _originPosition;
        private BitmapSource _defaultGlobeIconSource;*/

        public PanZoomBarMapTool(Context context)
            : base(context, true)
        {
            /*DefaultStyleKey = typeof(PanZoomBarMapTool);

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Margin = new Thickness(4);

            _defaultGlobeIconSource = new BitmapImage(new Uri("/WpfDesktopEdition;component/Resources/Circle.png", UriKind.RelativeOrAbsolute));*/
        }


        /*public DisplayZoomBarText DisplayZoomBarText { get; set; }

       

        public override void OnApplyTemplate()
        {
            _zoomInImage = (Image)GetTemplateChild("ZoomInImage");
            _zoomOutImage = (Image)GetTemplateChild("ZoomOutImage");
            _zoomBar = (System.Windows.Controls.Canvas)GetTemplateChild("ZoomBar");
            _zoomHandlerImage = (Image)GetTemplateChild("ZoomHandler");
            _zoomHandlerTip = (System.Windows.Controls.Canvas)GetTemplateChild("ZoomHandlerTip");
            _zoomHandlerTipLabel = (TextBlock)GetTemplateChild("ZoomHandlerTipLabel");
            _panPanel = (Grid)GetTemplateChild("PanPanel");
            _globeImage = (Image)GetTemplateChild("GlobeImage");
            if (_globeImage.Source == null)
            {
                _globeImage.Source = _defaultGlobeIconSource;
            }

            _zoomInImage.Cursor = Cursors.Hand;
            _zoomOutImage.Cursor = Cursors.Hand;
            _zoomHandlerImage.Cursor = Cursors.Hand;

            _zoomBarHeight = ZoomBarHeightPerLevel * (CurrentMap.ZoomLevelScales.Count - 1);
            _zoomBar.Height = _zoomBarHeight;

            _globeImage.MouseDown += globeImage_MouseDown;
            _globeImage.MouseUp += globeImage_MouseUp;

            InitEvent();
            Refresh(CurrentMap.CurrentExtent);
        }*/

        /*protected virtual RectangleShape OnGlobeButtonClick(RectangleShape newExtent)
        {
            EventHandler<ExtentEventArgs> handler = GlobeButtonClick;
            if (handler != null)
            {
                ExtentEventArgs args = new ExtentEventArgs(newExtent);
                handler(this, args);
                return args.Extent;
            }
            else
            {
                return newExtent;
            }
         }*/

        /*private void globeImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }*/

        /*private void globeImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentMap != null)
            {
                Collection<BaseShape> rectangles = new Collection<BaseShape>();
                foreach (BaseOverlay overlay in CurrentMap.Overlays)
                {
                    RectangleShape rect = overlay.GetBoundingBox();
                    if (rect != null)
                    {
                        rectangles.Add(rect);
                    }
                }

                if (rectangles.Count != 0)
                {
                    RectangleShape targetExtent = OnGlobeButtonClick(ExtentHelper.GetBoundingBoxOfItems(rectangles));
                    if (targetExtent != null)
                    {
                        CurrentMap.CurrentExtent = targetExtent;
                        CurrentMap.Refresh();
                    }
                }
            }

            e.Handled = true;
        }*/

        protected override void EnabledChangedCore(bool newValue)
        {
            base.EnabledChangedCore(newValue);
            InitEvent();
        }

        private void InitEvent()
        {
            if (CurrentMap == null) { return; }

            /*_panPanel.MouseDown -= PanPanelMouseDown;
            _zoomInImage.MouseDown -= ZoomInImageMouseDown;
            _zoomOutImage.MouseDown -= ZoomOutImageMouseDown;
            _zoomHandlerImage.MouseDown -= ZoomHandlerImageMouseDown;
            _zoomHandlerImage.MouseMove -= ZoomHandlerImageMouseMove;
            _zoomHandlerImage.MouseUp -= ZoomHandlerImageMouseUp;
            CurrentMap.CurrentScaleChanged -= CurrentMapCurrentScaleChanged;

            if (IsEnabled)
            {
                _panPanel.MouseDown += PanPanelMouseDown;
                _zoomInImage.MouseDown += ZoomInImageMouseDown;
                _zoomOutImage.MouseDown += ZoomOutImageMouseDown;
                _zoomHandlerImage.MouseDown += ZoomHandlerImageMouseDown;
                _zoomHandlerImage.MouseMove += ZoomHandlerImageMouseMove;
                _zoomHandlerImage.MouseUp += ZoomHandlerImageMouseUp;
                CurrentMap.CurrentScaleChanged += CurrentMapCurrentScaleChanged;
            }*/
        }

        /*private void ZoomHandlerImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _zoomHandlerTip.Visibility = Visibility.Collapsed;
            _zoomHandlerImage.ReleaseMouseCapture();

            double targetY = e.GetPosition(_zoomBar).Y;
            if (targetY > _zoomBarHeight) { targetY = _zoomBarHeight; }
            else if (targetY < 0) { targetY = 0; }

            int targetLevelIndex = GetTargetZoomLevelIndex(targetY);
            CurrentMap.ZoomToScale(CurrentMap.ZoomLevelScales[targetLevelIndex]);
        }*/

        /*private void ZoomHandlerImageMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(_zoomBar);
                if (currentPosition.Y >= 0 && currentPosition.Y <= _zoomBarHeight)
                {
                    double offsetY = currentPosition.Y - _originPosition.Y;
                    double zoomHandlerY = (double)_zoomHandlerImage.GetValue(System.Windows.Controls.Canvas.TopProperty) + offsetY;
                    _zoomHandlerImage.SetValue(System.Windows.Controls.Canvas.TopProperty, zoomHandlerY);
                    _zoomHandlerTip.SetValue(System.Windows.Controls.Canvas.TopProperty, zoomHandlerY);

                    if (DisplayZoomBarText == DisplayZoomBarText.Display)
                    {
                        int targetLevelIndex = GetTargetZoomLevelIndex(currentPosition.Y);
                        _zoomHandlerTipLabel.Text = String.Format(CultureInfo.InvariantCulture, LevelTipsFormat, targetLevelIndex + 1);
                    }

                    _originPosition = currentPosition;
                }
            }
        }*/

        /*private void ZoomHandlerImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DisplayZoomBarText == DisplayZoomBarText.Display)
            {
                _zoomHandlerTip.Visibility = Visibility.Visible;
            }

            _originPosition = e.GetPosition(_zoomBar);
            _isDragging = true;
            _zoomHandlerImage.CaptureMouse();
        }

        private void PanPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point panPanelCenter = new Point(_panPanel.Width / 2, _panPanel.Height / 2);
            Point currentPoint = e.GetPosition(_panPanel);

            float panAngle = (float)(Math.Atan2((currentPoint.Y - panPanelCenter.Y), (currentPoint.X - panPanelCenter.X)) * 180 / Math.PI) + 90;
            if (panAngle < 0) { panAngle += 360; }
            CurrentMap.Pan(panAngle, 5);
        }

        private void ZoomOutImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentMap.ZoomOut();
        }

        private void ZoomInImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentMap.ZoomIn();
        }*/

        private void CurrentMapCurrentScaleChanged(object sender, ScaleChangedEventArgs e)
        {
            /*if (CurrentMap != null && _zoomHandlerImage != null)
            {
                Refresh(e.CurrentExtent);
            }*/
        }

        private void Refresh(RectangleShape targetExtent)
        {
            /*int levelIndex = CurrentMap.GetSnappedZoomLevelIndex(targetExtent);
            double zoomHandlerY = (double)(_zoomBarHeight - levelIndex * ZoomBarHeightPerLevel);

            _zoomHandlerImage.SetValue(System.Windows.Controls.Canvas.TopProperty, zoomHandlerY);
            _zoomHandlerTip.SetValue(System.Windows.Controls.Canvas.TopProperty, zoomHandlerY);

            string toolTip = String.Format(CultureInfo.InvariantCulture, LevelTipsFormat, levelIndex + 1);
            _zoomHandlerImage.ToolTip = toolTip;
            _zoomHandlerTipLabel.Text = toolTip;*/
        }

        private int GetTargetZoomLevelIndex(double targetTop)
        {
            return (int)(_zoomBarHeight - targetTop) / ZoomBarHeightPerLevel;
        }
    }
}
