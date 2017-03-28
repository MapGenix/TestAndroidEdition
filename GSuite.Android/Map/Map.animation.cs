using Android.Graphics;
using System;
using System.Windows;
using Android.Views.Animations;

namespace Mapgenix.GSuite.Android
{
    public partial class Map
    {
        /*private Storyboard _zoomStoryboard;
        private DoubleAnimation _zoomAnimationX;
        private DoubleAnimation _zoomAnimationY;
        private ScaleTransform _zoomTransform;*/

        private Animation _zoomAnimation;
        private PointF _lastZoomingCenter = new PointF(0, 0);
        private PointF _tempZoomingCenter = new PointF(0, 0);
        private long _zoomAnimationDuration;

        public long ZoomAnimationDuration
        {
            get
            {
                if (_zoomAnimationDuration == 0)
                {
                    _zoomAnimationDuration = 400;
                }

                return _zoomAnimationDuration;
            }
            set
            {
                _zoomAnimationDuration = value;
            }
        }

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

        private void ZoomAnimation_AnimationEnd(object sender, Animation.AnimationEndEventArgs e)
        {
            _zoomAnimation.Dispose();
            _zoomAnimation = new ScaleAnimation(0.0f, 1f, 0.0f, 1f);
            _zoomAnimation.Duration = ZoomAnimationDuration;
            _zoomAnimation.AnimationEnd += ZoomAnimation_AnimationEnd;

            _lastZoomingCenter = _tempZoomingCenter;

            CurrentExtent = _targetSnappedExtent;
            Refresh();

        }

        private void ExecuteZoomAnimation(double targetZoomingScale, double previousZoomingScale, PointF zoomLogicCenter)
        {
            /*_zoomTransform.CenterX = zoomLogicCenter.X - 1;
            _zoomTransform.CenterY = zoomLogicCenter.Y - 1;*/

            float fromX = Convert.ToSingle(zoomLogicCenter.X - 1);
            float fromy = Convert.ToSingle(zoomLogicCenter.Y - 1);
            float zoomFactor = Math.Max(1f, Convert.ToSingle(previousZoomingScale / targetZoomingScale));

            /* _zoomAnimationX.To = zoomFactor;
             _zoomAnimationY.To = zoomFactor;

             _zoomStoryboard.Stop();
             _zoomStoryboard.Begin();*/

            //_zoomAnimation = new ScaleAnimation(_lastZoomingCenter.X, zoomLogicCenter.X, _lastZoomingCenter.Y, _lastZoomingCenter.X);
            _zoomAnimation = new ScaleAnimation(zoomFactor - 1f, zoomFactor, zoomFactor - 1f, zoomFactor, zoomLogicCenter.X, zoomLogicCenter.Y);
            _zoomAnimation.Duration = ZoomAnimationDuration;
            _zoomAnimation.AnimationEnd += ZoomAnimation_AnimationEnd;

            _tempZoomingCenter = zoomLogicCenter;

            Matrix toMatrix = new Matrix();
            toMatrix.SetScale(zoomFactor, zoomFactor);
            foreach(var overlay in Overlays)
            {
                BaseTileOverlay baseOverlay = (BaseTileOverlay)overlay;
                //baseOverlay.SetMatrixTiles(toMatrix);
            }

            StartAnimation(_zoomAnimation);

            //CurrentExtent = _targetSnappedExtent;
            //Refresh();

        }
    }
}
