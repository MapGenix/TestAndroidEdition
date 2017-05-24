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
using System.Collections.ObjectModel;

namespace Mapgenix.GSuite.Android
{
    public class MapLayout : RelativeLayout
    {

        private float _scaleFactor = 1;
        private float _pivotX;
        private float _pivotY;
        private float _offsetX = 0;
        private float _offsetY = 0;
        private LayoutPostType _type;

        public MapLayout(Context context)
            : base(context)
        {
            
        }

        public Collection<View> GetAllViews()
        {
            Collection<View> views = new Collection<View>();
            for(int i = 0; i < this.ChildCount; i++)
            {
                views.Add(this.GetChildAt(i));
            }
            return views;
        }

        internal void PostScale(float scaleFactor, float pivotX, float pivotY)
        {
            _scaleFactor = scaleFactor;
            _pivotX = pivotX;
            _pivotY = pivotY;
            _type = LayoutPostType.Scale;
            this.Invalidate();
        }

        internal void PostTranslate(float offsetX, float offsetY)
        {
            _offsetX = offsetX;
            _offsetY = offsetY;
            _type = LayoutPostType.Translate;
            this.Invalidate();
        }

        internal void Restore()
        {
            _scaleFactor = 1;
            this.Invalidate();
        }

        protected override void DispatchDraw(global::Android.Graphics.Canvas canvas)
        {            
            canvas.Save(SaveFlags.Matrix);
            switch(_type)
            {
                case LayoutPostType.Scale:
                    canvas.Scale(_scaleFactor, _scaleFactor, _pivotX, _pivotY);
                    break;
                case LayoutPostType.Translate:
                    canvas.Translate(_offsetX, _offsetY);
                    break;
            }
            
            base.DispatchDraw(canvas);
            canvas.Restore();
        }
    }
}