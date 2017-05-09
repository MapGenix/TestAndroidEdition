using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    public class MapLayout : RelativeLayout
    {

        private float _scaleFactor = 1;
        private float _pivotX;
        private float _pivotY;

        public MapLayout(Context context)
            : base(context)
        {

        }

        protected override void DispatchDraw(global::Android.Graphics.Canvas canvas)
        {
            canvas.Save(SaveFlags.Matrix);
            canvas.Scale(_scaleFactor, _scaleFactor, _pivotX, _pivotY);
            base.DispatchDraw(canvas);
            canvas.Restore();
        }

        /*protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            if (!changed)
                return;
            for (int index = 0; index < this.ChildCount; ++index)
            {
                ViewGroup.LayoutParams layoutParameters = this.GetChildAt(index).LayoutParameters;
                if (layoutParameters.Width == -1 && layoutParameters.Height == -1)
                    this.GetChildAt(index).Layout(0, 0, this.Width, this.Height);
            }
        }*/

        internal void PostScale(float scaleFactor, float pivotX, float pivotY)
        {
            _scaleFactor = scaleFactor;
            _pivotX = pivotX;
            _pivotY = pivotY;
            this.Invalidate();
        }

        internal void Restore()
        {
            _scaleFactor = 1;
            this.Invalidate();
        }

    }
}