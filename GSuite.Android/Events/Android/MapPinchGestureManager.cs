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

        internal Map Sender
        {
            get; set;
        }

        public override bool OnScale(ScaleGestureDetector detector)
        {
            Console.WriteLine("Pinch start!!!");
            Console.WriteLine("ScaleFactor: " + detector.ScaleFactor);
            Console.WriteLine("Current Span: " + detector.CurrentSpan);
            Console.WriteLine("Current Span X: " + detector.CurrentSpanX);
            Console.WriteLine("Current Span Y: " + detector.CurrentSpanY);
            Console.WriteLine("Focus X: " + detector.FocusX);
            Console.WriteLine("Focus Y: " + detector.FocusY);
            Console.WriteLine("Previous Span: " + detector.PreviousSpan);
            Console.WriteLine("Previous Span X: " + detector.PreviousSpanX);
            Console.WriteLine("Previous Span Y: " + detector.PreviousSpanY);
            Console.WriteLine("Pinch end!!!");

            if (detector.IsInProgress)
                Console.WriteLine("Is progress");
            else
                Console.WriteLine("Finished process");

            //Sender.EventManagerPich(detector);

            return true;
        }
    }
}