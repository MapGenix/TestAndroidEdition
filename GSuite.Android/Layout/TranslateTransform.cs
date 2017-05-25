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
    internal class TranslateTransform
    {
        private float _offsetX;
        private float _offsetY;
        private MapLayout _view;


        public TranslateTransform(MapLayout view)
            : this(view, 0, 0)
        { }

        public TranslateTransform(MapLayout view, float offsetX, float offsetY)
        {
            _offsetX = offsetX;
            _offsetX = offsetY;
            _view = view;
        }

        public float X
        {
            get { return _offsetX; }
            set
            {
                _offsetX = value;
                _view.PostTranslate(_offsetX, _offsetY);
            }
        }

        public float Y
        {
            get { return _offsetY; }
            set
            {
                _offsetY = value;
                _view.PostTranslate(_offsetX, _offsetY);
            }
        }

    }
}