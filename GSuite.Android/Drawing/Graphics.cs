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
using NativeAndroid = Android;
using Android.Graphics;
using Android.Util;

namespace Mapgenix.GSuite.Android
{
    class Graphics
    {

        private NativeAndroid.Graphics.Canvas _canvas;
        private Paint _drawConfig;
        
        public Graphics()
        {
            _canvas = new NativeAndroid.Graphics.Canvas();
            _drawConfig = new Paint();
        }

        public Graphics(NativeAndroid.Graphics.Canvas canvas)
        {
            _canvas = canvas;
            _drawConfig = new Paint();
        }

        public Graphics(Bitmap sourceBitmap)
        {
            _canvas = new NativeAndroid.Graphics.Canvas(sourceBitmap);
            _drawConfig = new Paint();
        }

        #region public properties

        public Paint DrawConfig
        {
            get
            {
                if (_drawConfig == null)
                    _drawConfig = new Paint();

                return _drawConfig;
            }
            set
            {
                _drawConfig = value;
            }
        }

        #endregion

        #region static methods

        public static Graphics FromImage(Bitmap sourceBitmap)
        {
            Graphics graphics = new Graphics(sourceBitmap);
            return graphics;
        }

        #endregion

        #region public methods

        public void DrawImage(Bitmap tempBitmap, float left, float top)
        {
            if(_drawConfig == null)
            {
                _drawConfig = new Paint();
                _drawConfig.AntiAlias = true;
            }

            this.DrawImage(tempBitmap, left, top, _drawConfig);
        }

        public void DrawImage(Bitmap tempBitmap, float left, float top, Paint paint)
        {
            Validators.CheckParameterIsNotNull(tempBitmap, "tempBitmap");

            try
            {
                _canvas.DrawBitmap(tempBitmap, left, top, paint);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void DrawImage(Bitmap tempBitmap, RectF dest, RectF source)
        {
            try
            {
                Validators.CheckParameterIsNotNull(tempBitmap, "tempBitmap");
                Rect destRect = new Rect((int)dest.Left, (int)dest.Top, (int)dest.Right, (int)dest.Bottom);
                _canvas.DrawBitmap(tempBitmap, destRect, source, _drawConfig);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void DrawPolygon(Paint pen, Point[] points)
        {
            List<PointF> pointsF = new List<PointF>();
            foreach (var point in points)
                pointsF.Add(new PointF(point));

            this.DrawPolygon(pen, pointsF.ToArray());
        }

        public void DrawPolygon(Paint pen, PointF[] points)
        {
            Path polygon = new Path();

            try
            {                
                for (int i = 0; i < points.Length; i++)
                {
                    if (i == 0)
                        polygon.MoveTo(points[i].X, points[i].Y);
                    else
                        polygon.LineTo(points[i].X, points[i].Y);
                }

                polygon.Close();

                _canvas.DrawPath(polygon, pen);
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                polygon.Dispose();
            }
            
        }

        public void DrawLines(Paint pen, Point[] points)
        {
            List<PointF> pointsF = new List<PointF>();
            foreach (var point in points)
                pointsF.Add(new PointF(point));

            this.DrawLines(pen, pointsF.ToArray());
        }

        public void DrawLines(Paint paint, PointF[] points)
        {
            Path line = new Path();

            try
            {
                for (int i = 0; i < points.Length; i++)
                {
                    if (i == 0)
                        line.MoveTo(points[i].X, points[i].Y);
                    else
                        line.LineTo(points[i].X, points[i].Y);
                }

                _canvas.DrawPath(line, paint);
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                line.Dispose();
            }
        }

        public void DrawPath(Paint paint, GraphicsPath path)
        {
            this.DrawPath(paint, path.Path);
        }

        public void DrawPath(Paint paint, Path path)
        {
            _canvas.DrawPath(path, paint);
        }

        public void DrawEllipse(Paint paint, float screenX, float screenY, float width, float height)
        {
            RectF oval = new RectF(screenX, screenY, screenX + width, screenY + height);
            _canvas.DrawOval(oval, paint);
        }

        public void DrawImageUnscaled(Bitmap bitmap, int left, int top)
        {
            _canvas.DrawBitmap(bitmap, left, top, _drawConfig);
        }

        public void FillPolygon(Color fillColor, Point[] points)
        {
            List<PointF> pointsF = new List<PointF>();
            foreach (var point in points)
                pointsF.Add(new PointF(point));

            this.FillPolygon(fillColor, pointsF.ToArray());
        }

        public void FillPolygon(Color fillColor, PointF[] points)
        {
            Path polygon = new Path();

            try
            {

                for (int i = 0; i < points.Length; i++)
                {
                    if (i == 0)
                        polygon.MoveTo(points[i].X, points[i].Y);
                    else
                        polygon.LineTo(points[i].X, points[i].Y);
                }

                polygon.Close();

                Paint paint = new Paint(_drawConfig);
                paint.Color = fillColor;
                paint.SetStyle(Paint.Style.Fill);
                _canvas.DrawPath(polygon, paint);

            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                polygon.Dispose();   
            }            
        }

        public void FillPath(Color fillColor, GraphicsPath path)
        {
            this.FillPath(fillColor, path.Path);
        }

        public void FillPath(Color fillColor, Path path)
        {
            Paint p = new Paint(_drawConfig);
            p.Color = fillColor;
            p.SetStyle(Paint.Style.Fill);

            _canvas.DrawPath(path, p);
        }

        public void FillEllipse(Color fillColor, float screenX, float screenY, float width, float height)
        {
            Paint p = new Paint(_drawConfig);
            p.Color = fillColor;
            p.SetStyle(Paint.Style.Fill);

            RectF oval = new RectF(screenX, screenY, screenX + width, screenY + height);

            _canvas.DrawOval(oval, p);
        }

        public void FillRectangle(Color fillColor, float screenX, float screenY, float width, float height)
        {
            Paint p = new Paint(_drawConfig);
            p.Color = fillColor;
            p.SetStyle(Paint.Style.Fill);

            RectF rectangle = new RectF(screenX, screenY, width, height);

            _canvas.DrawRect(rectangle, p);
        }

        public void TranslateTransform(float dx, float dy)
        {
            _canvas.Translate(dx, dy);
        }

        public void RotateTransform(float degrees)
        {
            _canvas.Rotate(degrees);
            Paint p = new Paint();
        }

        public SizeF MeasureString(string text, Font font)
        {
            Paint p = font.toPaint();
            Rect bounds = new Rect();
            p.GetTextBounds(text, 0, text.Length, bounds);
            return new SizeF(bounds.Width(), bounds.Height());
        }

        public void DrawString(string text, Font font, Color color, PointF position)
        {
            font.Color = color;
            Paint p = font.toPaint();
            _canvas.DrawText(text, position.X, position.Y, p);
        }

        public void Dispose()
        {
            _canvas.Dispose();
            if(_drawConfig != null)
                _drawConfig.Dispose();
        }

        #endregion

    }
}