using Android.Graphics;
using System;
using System.Windows;

namespace Mapgenix.GSuite.Android
{
    public partial class Map
    {
        /*private Storyboard _zoomStoryboard;
        private DoubleAnimation _zoomAnimationX;
        private DoubleAnimation _zoomAnimationY;
        private ScaleTransform _zoomTransform;*/

        private void InitAnimation()
        {
            /*_zoomStoryboard = (Storyboard)_overlayCanvas.Resources["ZoomStoryboard"];
            _zoomStoryboard.FillBehavior = FillBehavior.Stop;
            _zoomStoryboard.Completed += ZoomStoryboard_Completed;
            _zoomAnimationX = (DoubleAnimation)_zoomStoryboard.Children[0];
            _zoomAnimationY = (DoubleAnimation)_zoomStoryboard.Children[1];
            _zoomAnimationX.To = 1;
            _zoomAnimationY.To = 1;
            _zoomTransform = (ScaleTransform)_overlayCanvas.RenderTransform;*/
        }

        private void ZoomStoryboard_Completed(object sender, EventArgs e)
        {
            /*_zoomTransform.ScaleX = 1;
            _zoomTransform.ScaleY = 1;
            _zoomAnimationX.To = 1;
            _zoomAnimationY.To = 1;*/

            CurrentExtent = _targetSnappedExtent;
            Refresh();
        }

        private void ExecuteZoomAnimation(double targetZoomingScale, double previousZoomingScale, PointF zoomLogicCenter)
        {
            /*_zoomTransform.CenterX = zoomLogicCenter.X - 1;
            _zoomTransform.CenterY = zoomLogicCenter.Y - 1;

            double zoomFactor = previousZoomingScale / targetZoomingScale;
            _zoomAnimationX.To = zoomFactor;
            _zoomAnimationY.To = zoomFactor;

            _zoomStoryboard.Stop();
            _zoomStoryboard.Begin();*/
        }
    }
}
