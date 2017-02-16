using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Mapgenix.GSuite.Android;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using System.Collections.ObjectModel;

namespace TestApp2
{
    [Activity(Label = "Test GSuite", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        private ImageView image1;
        private Map MainMap;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.MainLayout);
            
            Bitmap bitmap = Bitmap.CreateBitmap(256, 256, Bitmap.Config.Argb8888);

            Canvas canvas = new Canvas(bitmap);

            Point[] points = { new Point(1, 1), new Point(200, 10), new Point(230, 100), new Point(235, 200), new Point(1, 1) };
            Point[] points2 = { new Point(80, 40), new Point(110, 50), new Point(140, 70), new Point(145, 30), new Point(80, 40) };

            Collection<Point[]> rings = new Collection<Point[]>();
            rings.Add(points);
            rings.Add(points2);

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.Fill);
            paint.Color = Color.Blue;
            paint.AntiAlias = true;

            canvas.DrawPaint(paint);
            
            foreach (var ringPoints in rings)
            {
                Path pathPolygon = new Path();
                for (int i = 0; i < ringPoints.Length; i++)
                {
                    if(i == 0)
                    {
                        pathPolygon.MoveTo(ringPoints[i].X, ringPoints[i].Y);
                    }else
                    {
                        pathPolygon.LineTo(ringPoints[i].X, ringPoints[i].Y);
                    }
                }

                paint.StrokeWidth = 3;
                paint.SetPathEffect(null);
                paint.Color = Color.Green;
                paint.SetStyle(Paint.Style.Fill);

                canvas.DrawPath(pathPolygon, paint);

                paint.Color = Color.Black;
                paint.SetStyle(Paint.Style.Stroke);
                canvas.DrawPath(pathPolygon, paint);

            }

            ImageView image1 = FindViewById<ImageView>(Resource.Id.Image1);
            image1.SetImageBitmap(bitmap);

            /*SetContentView(Resource.Layout.Main);
            MainMap = FindViewById<Map>(Resource.Id.MainMap);
            if (MainMap != null)
            {
                MainMap.BackgroundOverlay.BackgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.Blue);
                MainMap.CurrentExtent = new RectangleShape(-130, 50, -63, 21);
                var obj = MainMap.LayoutParameters;
                MainMap.Refresh();
            }*/
            //GsuiteMap.MeasuredWidth

            /*Bitmap imageSource = BitmapFactory.DecodeResource(Resources, Resource.Drawable.normal61628);
            if (image1 == null)
            {
                image1 = new ImageView(this);
            }

            image1.SetImageBitmap(imageSource);
            FrameLayout.LayoutParams p = new FrameLayout.LayoutParams(400, 400);
            p.LeftMargin = 0;
            p.TopMargin = 0;
            p.Height = 400;
            p.Width = 400;
            frame.AddView(image1, 0, p);*/

            // Set our view from the "main" layout resource
            // 
        }

        protected override void OnResume()
        {
            base.OnResume();
            /*GsuiteMap.BackgroundOverlay.BackgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.Blue);
            GsuiteMap.CurrentExtent = new RectangleShape(-130, 50, -63, 21);
            var obj = GsuiteMap.LayoutParameters;
            GsuiteMap.Refresh();*/

        }

    }
}

