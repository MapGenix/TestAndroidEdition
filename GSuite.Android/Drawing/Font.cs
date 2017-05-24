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
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    internal class Font
    {
        private float _size;
        private Color _color;
        private string _fontName;
        private FontStyle _style;
        private FontAling _aling;

        public float Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public string FontName
        {
            get { return _fontName; }
            set { _fontName = value; }
        }

        public FontStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }    

        public FontAling Aling
        {
            get { return _aling; }
            set { _aling = value; }
        }

        private Paint.Align PaintAling
        {
            get
            {
                switch (_aling)
                {
                    case FontAling.Left:
                        return Paint.Align.Left;
                    case FontAling.Rigth:
                        return Paint.Align.Right;
                    default:
                        return Paint.Align.Center;
                }
            }
        }

        public Font()
            : this("Arial", 12f, FontStyle.Regular, Color.Black, FontAling.Center)
        { }

        public Font(string fontName, float size, FontStyle style)
            : this(fontName, size, style, Color.Black, FontAling.Center)
        { }

        public Font(string fontName, float size, FontStyle style, Color color)
            : this(fontName, size, style, color, FontAling.Center)
        { }

        public Font(string fontName, float size, FontStyle style, Color color, FontAling aling)
        {
            _fontName = fontName;
            _size = size;
            _color = color;
            _style = style;
            _aling = aling;
        }

        public Paint toPaint()
        {
            Paint p = new Paint();
            Typeface plain = Typeface.Create(_fontName, TypefaceStyle.Normal);
            Typeface tf = null;

            if ((_style | FontStyle.Bold) != 0 && (_style | FontStyle.Italic) != 0)
                tf = Typeface.Create(plain, TypefaceStyle.BoldItalic);
            else if ((_style | FontStyle.Bold) != 0)
                tf = Typeface.Create(plain, TypefaceStyle.Bold);
            else if ((_style | FontStyle.Italic) != 0)
                tf = Typeface.Create(plain, TypefaceStyle.Italic);
            else
                tf = plain;

            p.Color = _color;
            p.SetTypeface(tf);
            p.TextSize = _size;
            p.TextAlign = PaintAling;
            //p.UnderlineText = (_style | FontStyle.Underline) != 0 ? true : false;
            //p.StrikeThruText = (_style | FontStyle.Strikeout) != 0 ? true : false;
            return p;
        }        
    }
}