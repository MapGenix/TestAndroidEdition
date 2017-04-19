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

namespace TestApp2
{
    public class MyView: View
    {
        public MyView(Context context)
            :base(context)
        {

        }

        protected override void OnDraw(Canvas c)
        {
            /*Paint paint = new Paint();
            Path path = new Path();
   
            paint.SetStyle(Paint.Style.Fill);
            paint.Color = Color.Green;
            c.dra

            paint.setStyle(Paint.Style.FILL);
            paint.setColor(Color.TRANSPARENT);
            c.drawPaint(paint);
            for (int i = 50; i < 100; i++)
            {
                path.moveTo(i, i - 1);
                path.lineTo(i, i);
            }
            path.close();
            paint.setStrokeWidth(3);
            paint.setPathEffect(null);
            paint.setColor(Color.BLACK);
            paint.setStyle(Paint.Style.STROKE);
            c.drawPath(path, paint);*/
        }

    }
}