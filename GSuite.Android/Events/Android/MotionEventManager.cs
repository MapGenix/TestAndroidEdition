using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NativeAndroid = Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    internal class MotionEventManager
    {
        //public event EventHandler<GenericMotionEventArgs> Move;
        public event EventHandler<MotionEventArgs> Move;


    }
}