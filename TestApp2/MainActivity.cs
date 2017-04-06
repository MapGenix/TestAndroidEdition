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
using Android.Views;
using System;

namespace TestApp2
{
    [Activity(Label = "Test GSuite", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Map MainMap;
        private string sampleDataPath;

        private void LoadShapes()
        {
            AssetManager am = this.Assets;
            string[] files = am.List("panama");

            foreach (string file in files)
            {

                sampleDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                var filename = System.IO.Path.Combine(sampleDataPath, file);

                if (File.Exists(filename))
                    continue;

                try
                {

                    //string uri = string.Format("http://license.mapgenix.com/sampledata/vector/shapefiles/panama/{0}", file);
                    //HttpWebRequest webRequest = WebRequest.Create(uri) as HttpWebRequest;
                    //HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
                    //string statusCode = webResponse.StatusCode.ToString();
                    //if (statusCode != "OK")
                    //{

                    //}else
                    //{
                    //System.Text.Encoding enc = System.Text.Encoding.GetEncoding(1252);
                    //StreamReader responseStream = new StreamReader(webResponse.GetResponseStream(), enc);
                    //string Response = responseStream.ReadToEnd();
                    //File.WriteAllText(filename, Response);
                    //}


                    //stream = am.Open(file);
                    //fileStream = File.Create(filename, (int)am.Open(file).Length);
                    //byte[] bytes = new byte[stream.Length];
                    //stream.Read(bytes, 0, bytes.Length);
                    //fileStream.Write(bytes, 0, bytes.Length);
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
                catch (FileLoadException e)
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

        private void OpenStreetMap()
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string cachePath = System.IO.Path.Combine(basePath, "Temp");

            MainMap.MapUnit = GeographyUnit.Meter;
            OpenStreetMapOverlay osmOverlay = OverlayFactory.CreateOpenStreetMapOverlay(MainMap.Context, cachePath);
            MainMap.Overlays.Add(osmOverlay);

            MainMap.CurrentExtent = new RectangleShape(-9000000, 1300000, -8100000, 560000);
            MainMap.ZoomAnimationDuration = 200;

            MainMap.Refresh();
        }

        private void HEREMaps()
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string cachePath = System.IO.Path.Combine(basePath, "Temp");

            MainMap.MapUnit = GeographyUnit.Meter;

            HEREMapsOverlay hereMapsOverlay = OverlayFactory.CreateHEREMapsOverlay(MainMap.Context, "6bNa1RoLuPX7GnIENuUW", "WGokMTESlmY-wDZVNt3AdA");
            hereMapsOverlay.MapType = HEREMapsMapType.Normal;
            hereMapsOverlay.CacheDirectory = cachePath;

            MainMap.Overlays.Add("HereMapsOverlay", hereMapsOverlay);

            MainMap.CurrentExtent = new RectangleShape(-9000000, 1300000, -8100000, 560000);
            MainMap.Refresh();
        }

        private void BingMaps()
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string cachePath = System.IO.Path.Combine(basePath, "Temp");

            MainMap.MapUnit = GeographyUnit.Meter;

            BingMapsOverlay bingMapsOverlay = OverlayFactory.CreateBingMapsOverlay(MainMap.Context, "Ak2Auxr5HCWo1nsUGBksMW8TvuKsapJEUr869ktaQ0oQTajYUjDvIX6YAu2yOKp7");
            bingMapsOverlay.CacheDirectory = "C:\\Temp\\";
            bingMapsOverlay.MapType = BingMapsMapType.Road;

            MainMap.Overlays.Add("BingMapsOverlay", bingMapsOverlay);

            MainMap.CurrentExtent = new RectangleShape(-9000000, 1300000, -8100000, 560000);
            MainMap.Refresh();
        }

        private void GoogleMaps()
        {
            string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string cachePath = System.IO.Path.Combine(basePath, "Temp2");

            MainMap.MapUnit = GeographyUnit.Meter;

            GoogleMapsOverlay googleMapsOverlay = OverlayFactory.CreateGoogleMapsOverlay(MainMap.Context, "AIzaSyB8VXGFKgzu3CH_6lOmRze_wl1zKcneruc");
            googleMapsOverlay.CacheDirectory = cachePath;
            googleMapsOverlay.CachePictureFormat = GoogleMapsPictureFormat.Png32;
            googleMapsOverlay.MapType = GoogleMapsMapType.RoadMap;

            MainMap.Overlays.Add("GoogleOverlay", googleMapsOverlay);

            /*MainMap.CurrentExtent = new RectangleShape(-9000000, 1300000, -8100000, 560000);
            MainMap.Refresh();*/
        }

        private void PrintImageFromStream()
        {
            WebRequest request = HttpWebRequest.Create("http://tile.openstreetmap.org/6/17/32.png");
            request.Timeout = 40 * 1000;
            request.Proxy = null;

            ImageView image1 = null;// = FindViewById<ImageView>(Resource.Id.Image1);

            using (Stream imageStream = request.GetResponse().GetResponseStream())
            {
                Bitmap imageSource = BitmapFactory.DecodeStream(imageStream);
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
                LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.MainLayout);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.Main);
                MainMap = FindViewById<Map>(Resource.Id.MainMap);
                Button button1 = FindViewById<Button>(Resource.Id.setTrack);
                button1.Click += Button1_Click;
                Button button2 = FindViewById<Button>(Resource.Id.unsetTrack);
                button2.Click += Button2_Click;
                System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
                GoogleMaps();
                LoadShapes();
            }
            catch(Exception ex)
            {
                if(ex != null)
                {

                }
            }
                                  
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            MainMap.TrackOverlay.TrackMode = TrackMode.None;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            MainMap.TrackOverlay.TrackMode = TrackMode.Point;
        }

        private void InitMapShapes()
        {
            if (MainMap != null)
            {
                //MainMap.BackgroundOverlay.BackgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.LightBlue);
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

                Proj4Projection proj4Projection = new Proj4Projection();
                proj4Projection.InternalProjectionParametersString = Proj4Projection.GetWgs84ParametersString();
                proj4Projection.ExternalProjectionParametersString = Proj4Projection.GetSphericalMercatorParametersString();
                districtLayer.FeatureSource.Projection = proj4Projection;
                roadLayer.FeatureSource.Projection = proj4Projection;
                locationLayer.FeatureSource.Projection = proj4Projection;

                LayerOverlay overlay = new LayerOverlay(MainMap.Context);
                overlay.Layers.Add("District", districtLayer);
                //overlay.Layers.Add("Road", roadLayer);
                //overlay.Layers.Add("Location", locationLayer);

                //MainMap.MapUnit = GeographyUnit.DecimalDegree;

                string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string cachePath = System.IO.Path.Combine(basePath, "Temp");

                MainMap.Overlays.Add(overlay);

                districtLayer.Open();
                MainMap.CurrentExtent = districtLayer.GetBoundingBox();                
                districtLayer.Close();
                var obj = MainMap.LayoutParameters;

                MainMap.ZoomAnimationDuration = 200;

                MainMap.MapTools.ScaleLine.Enabled = true;
                MainMap.MapTools.MouseCoordinate.Enabled = true;
               // MainMap.TrackOverlay.TrackMode = TrackMode.Point;

                MainMap.Refresh();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitMapShapes();   
        }
    }
}

