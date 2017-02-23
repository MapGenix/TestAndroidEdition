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
    public class MapSimpleGestureManager : GestureDetector.SimpleOnGestureListener
    {
        public override bool OnDown(MotionEvent e)
        {
            return true;
        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            return true;
        }
    }
}