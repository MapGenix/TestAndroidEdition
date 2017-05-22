using Android.Graphics;
using System;
using System.Windows;
using Android.Views.Animations;

namespace Mapgenix.GSuite.Android
{
    public partial class Map
    {
        private Animation _zoomAnimation;
        private PointF _lastZoomingCenter = new PointF(0, 0);
        private PointF _tempZoomingCenter = new PointF(0, 0);
        private long _zoomAnimationDuration;
        private float _zoomFactor = 1;
        private float _previosZoomFactor;

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
            float fromX = Convert.ToSingle(zoomLogicCenter.X - 1);
            float fromy = Convert.ToSingle(zoomLogicCenter.Y - 1);

            _previosZoomFactor = _zoomFactor;
            _zoomFactor = Convert.ToSingle(previousZoomingScale / targetZoomingScale);

            _zoomAnimation = new ScaleAnimation(1, _zoomFactor, 1, _zoomFactor, zoomLogicCenter.X, zoomLogicCenter.Y);
            _zoomAnimation.Duration = ZoomAnimationDuration;
            _zoomAnimation.AnimationEnd += ZoomAnimation_AnimationEnd;

            _tempZoomingCenter = zoomLogicCenter;

            OverlayCanvas.StartAnimation(_zoomAnimation);
        }
    }
}
