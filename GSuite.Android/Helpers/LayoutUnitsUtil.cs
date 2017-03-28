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
    public class LayoutUnitsUtil
    {
        public static float convertPixelsToDp(float px, float densityDpi)
        {
            float dp = px / (densityDpi / 160f);
            return Convert.ToSingle(Math.Round(dp));
        }

        public static float convertDpToPixel(float dp, float densityDpi)
        {
            return Convert.ToSingle(Math.Round(dp * (densityDpi / 160)));
        }
    }
}