using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Mapgenix.GSuite.Android;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using System.Collections.ObjectModel;
using Mapgenix.Layers;
using Android.Content.Res;
using System.IO;
using System.Net;
using Mapgenix.Styles;

namespace TestApp2
{
    [Activity(Label = "Test GSuite", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        private ImageView image1;
        private Map MainMap;
        private string sampleDataPath;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            /**LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.MainLayout);
            
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
            paint.SetShader(new LinearGradient(0, 0, 256, 0, Color.White, Color.Black, Shader.TileMode.Clamp));
            
            canvas.DrawPaint(paint);
            paint.SetShader(null);
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
                //paint.SetShader(new LinearGradient(256, 0, 0, 10, Color.White, Color.Green, Shader.TileMode.Repeat));
                paint.SetStyle(Paint.Style.Fill);

                canvas.DrawPath(pathPolygon, paint);
                //paint.SetShader(null);

                paint.Color = Color.Black;
                paint.SetStyle(Paint.Style.Stroke);
                canvas.DrawPath(pathPolygon, paint);

            }

            ImageView image1 = FindViewById<ImageView>(Resource.Id.Image1);
            image1.SetImageBitmap(bitmap);**/
            /**GsuiteMap.MeasuredWidth

            Bitmap imageSource = BitmapFactory.DecodeResource(Resources, Resource.Drawable.normal61628);
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
            frame.AddView(image1, 0, p);**/


            MainMap = FindViewById<Map>(Resource.Id.MainMap);

            AssetManager am = this.Assets;
            string[] files = am.List("panama");
            Stream stream = null;
            FileStream fileStream = null;

            foreach (string file in files)
            { 

                sampleDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                //var path = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                var filename = System.IO.Path.Combine(sampleDataPath, file);

                if(File.Exists(filename))
                    continue;

                try
                {

                    /**string uri = string.Format("http://license.mapgenix.com/sampledata/vector/shapefiles/panama/{0}", file);
                    HttpWebRequest webRequest = WebRequest.Create(uri) as HttpWebRequest;
                    HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
                    string statusCode = webResponse.StatusCode.ToString();
                    if (statusCode != "OK")
                    {

                    }else
                    {
                        System.Text.Encoding enc = System.Text.Encoding.GetEncoding(1252);
                        StreamReader responseStream = new StreamReader(webResponse.GetResponseStream(), enc);
                        string Response = responseStream.ReadToEnd();
                        File.WriteAllText(filename, Response);
                    }


                    stream = am.Open(file);
                    fileStream = File.Create(filename, (int)am.Open(file).Length);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    fileStream.Write(bytes, 0, bytes.Length);*/
                    using (var br = new BinaryReader(Application.Context.Assets.Open("panama/" + file)))
                    {
                        using (var bw = new BinaryWriter(new FileStream(filename, FileMode.Create)))
                        {
                            byte[] buffer = new byte[2048];
                            int length = 0;
                            while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bw.Write(buffer, 0, length);
                            }
                        }
                    }
                    

                }
                catch(FileLoadException e)
                {
                    throw e;
                }
                finally
                {
                    //stream.Dispose();
                    //fileStream.Dispose();
                }
                
            }

        }

        protected override void OnResume()
        {
            base.OnResume();
            if (MainMap != null)
            {
                MainMap.BackgroundOverlay.BackgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.LightBlue);

                ShapeFileFeatureLayer districtLayer = FeatureLayerFactory.CreateShapeFileFeatureLayer(sampleDataPath + "/panama_districts.shp");
                districtLayer.ZoomLevelSet.ZoomLevel01.DefaultAreaStyle = AreaStyles.CreateSimpleAreaStyle(GeoColor.StandardColors.LightPink, GeoColor.StandardColors.Black);
                districtLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
                //create layer for road
                ShapeFileFeatureLayer roadLayer = FeatureLayerFactory.CreateShapeFileFeatureLayer(sampleDataPath + "/Panama_Roads.shp");
                roadLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle = LineStyles.CreateSimpleLineStyle(GeoColor.StandardColors.LightGreen, 2, true);
                roadLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
                //create layer for location
                ShapeFileFeatureLayer locationLayer = FeatureLayerFactory.CreateShapeFileFeatureLayer(sampleDataPath + "/panama_locations.shp");
                locationLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimpleCircleStyle(GeoColor.StandardColors.LightSkyBlue, 20, GeoColor.StandardColors.Black);
                locationLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

                LayerOverlay overlay = new LayerOverlay(MainMap.Context);
                overlay.Layers.Add("District", districtLayer);
                overlay.Layers.Add("Road", roadLayer);
                overlay.Layers.Add("Location", locationLayer);

                MainMap.Overlays.Add(overlay);

                districtLayer.Open();
                //MainMap.CurrentExtent = new RectangleShape(-130, 50, -63, 21);
                MainMap.CurrentExtent = districtLayer.GetBoundingBox();
                districtLayer.Close();
                var obj = MainMap.LayoutParameters;
                MainMap.Refresh();
            }
        }
    }
}

