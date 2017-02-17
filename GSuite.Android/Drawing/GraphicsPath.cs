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
using NativeAndroid = Android;

namespace Mapgenix.GSuite.Android
{
    public class GraphicsPath
    {
        private Path _path;

        public GraphicsPath()
        {
            _path = new Path();
        }

        #region public properties

        public Path.FillType FillMode
        {
            get
            {
                return _path.GetFillType();
            }
            set
            {
                _path.SetFillType(value);
            }
        }

        internal Path Path
        {
            get
            {
                return _path;
            }
        }

        #endregion

        #region public methods

        public void AddPolygon(Point[] points)
        {
            List<PointF> pointsF = new List<PointF>();
            foreach (Point p in points)
                pointsF.Add(new PointF(p));

            this.AddPolygon(pointsF.ToArray());
        }

        public void AddPolygon(PointF[] points)
        {
            for(int i = 0; i < points.Length; i++)
            {
                if (i == 0)
                    _path.MoveTo(points[i].X, points[i].Y);
                else
                    _path.LineTo(points[i].X, points[i].Y);
            }
            _path.Close();
        }

        public void Dispose()
        {
            _path.Dispose();
        }

        #endregion



    }
}