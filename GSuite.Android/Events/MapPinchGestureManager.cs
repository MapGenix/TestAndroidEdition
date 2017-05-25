using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    internal class MapPinchGestureManager : ScaleGestureDetector.SimpleOnScaleGestureListener
    {

        private float _startingSpan;
        private float _currentScale = 1;
        private float _startFocusX;
        private float _startFocusY;

        internal Map Sender
        {
            get; set;
        }

        public override bool OnScaleBegin(ScaleGestureDetector detector)
        {
            _startingSpan = detector.CurrentSpan;
            _startFocusX = detector.FocusX;
            _startFocusY = detector.FocusY;
            return true;
        }

        public override bool OnScale(ScaleGestureDetector detector)
        {
            _currentScale = (detector.CurrentSpan / _startingSpan);
            Sender.EventManagerPinch(_currentScale, _startFocusX, _startFocusY);
            return true;
        }

        public override void OnScaleEnd(ScaleGestureDetector detector)
        {
            base.OnScaleEnd(detector);
            Sender.EventManagerPinchEnd(_currentScale, _startFocusX, _startFocusY);
        }
    }
}