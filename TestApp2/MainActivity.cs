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
using Android.Content.PM;
using Android.Runtime;
using Android;
using System.Reflection;
using NativeAndroid = Android;
using GeoAPI;
using System.Collections.Generic;

namespace TestApp2
{
    [Activity(Label = "Test GSuite", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Map MainMap;
        private string sampleDataPath;

        private void LoadData()
        {
            AssetManager am = this.Assets;
            string[] shapes = am.List("panama");

            sampleDataPath = BaseContext.CacheDir.Path;
            sampleDataPath = System.IO.Path.Combine(sampleDataPath, "Temp2/SampleData/");

            if (!Directory.Exists(sampleDataPath))
                Directory.CreateDirectory(sampleDataPath);

            foreach (string file in shapes)
            {                               
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

            string[] geojson = am.List("geojson");
            foreach(string file in geojson)
            {
                try
                {
                    var filename = System.IO.Path.Combine(sampleDataPath, file);

                    if (File.Exists(filename))
                        continue;

                    using (var br = new BinaryReader(Application.Context.Assets.Open("geojson/" + file)))
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
            string basePath = Android.OS.Environment.ExternalStorageDirectory.Path;
            string cachePath = System.IO.Path.Combine(basePath, "Temp2");

            MainMap.MapUnit = GeographyUnit.Meter;
            OpenStreetMapOverlay osmOverlay = OverlayFactory.CreateOpenStreetMapOverlay(MainMap.Context, cachePath);
            MainMap.Overlays.Add(osmOverlay);

            MainMap.CurrentExtent = new RectangleShape(-9000000, 1300000, -8100000, 560000);
            MainMap.ZoomAnimationDuration = 1000;

            MainMap.Refresh();
        }

        private void HEREMaps()
        {
            string basePath = Android.OS.Environment.ExternalStorageDirectory.Path;
            string cachePath = System.IO.Path.Combine(basePath, "Temp2");

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
            CheckSelfPermission(Manifest.Permission.AccessFineLocation);
            string basePath = Android.OS.Environment.DownloadCacheDirectory.Path;
            string cachePath = System.IO.Path.Combine(basePath, "Temp2");

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
            string basePath = BaseContext.CacheDir.Path;
            string cachePath = System.IO.Path.Combine(basePath, "Temp2");

            MainMap.MapUnit = GeographyUnit.Meter;

            GoogleMapsOverlay googleMapsOverlay = OverlayFactory.CreateGoogleMapsOverlay(MainMap.Context, "AIzaSyB8VXGFKgzu3CH_6lOmRze_wl1zKcneruc");
            googleMapsOverlay.CacheDirectory = cachePath;
            googleMapsOverlay.CachePictureFormat = GoogleMapsPictureFormat.Jpeg;
            googleMapsOverlay.MapType = GoogleMapsMapType.RoadMap;

            MainMap.Overlays.Add("GoogleOverlay", googleMapsOverlay);

            MainMap.CurrentExtent = new RectangleShape(-9000000, 1300000, -8100000, 560000);
            MainMap.Refresh();
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
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            MainMap = FindViewById<Map>(Resource.Id.MainMap);
            MainMap.ZoomAnimationDuration = 100;
            InitButtonEvents();
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
            MainMap.MapUnit = GeographyUnit.DecimalDegree;
            MainMap.ZoomAnimationDuration = 500;
            //MainMap.BackgroundOverlay.BackgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.LightBlue);
            GoogleMaps();
            //OpenStreetMap();
            LoadData();  
            //WmsOverlay();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
                return new Type[0];

            try
            {
                return assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var types = ex.Types;
                IList<Type> list = new List<Type>(types.Length);
                foreach (var t in types)
                    if (t != null && t.IsPublic)
                        list.Add(t);
                return list;
            }
            catch
            {
                return new Type[0];
            }
        }

        private void InitButtonEvents()
        {
            Button point = FindViewById<Button>(Resource.Id.point);
            point.Click += Button1_Click;

            Button line = FindViewById<Button>(Resource.Id.line);
            line.Click += Button1_Click;

            Button polygon = FindViewById<Button>(Resource.Id.polygon);
            polygon.Click += Button1_Click;

            Button square = FindViewById<Button>(Resource.Id.square);
            square.Click += Button1_Click;

            Button rectangle = FindViewById<Button>(Resource.Id.rectangle);
            rectangle.Click += Button1_Click;

            Button circle = FindViewById<Button>(Resource.Id.circle);
            circle.Click += Button1_Click;

            Button ellipse = FindViewById<Button>(Resource.Id.ellipse);
            ellipse.Click += Button1_Click;

            Button button2 = FindViewById<Button>(Resource.Id.unsetTrack);
            button2.Click += Button2_Click;

            Button edit = FindViewById<Button>(Resource.Id.edit);
            edit.Click += Button1_Click;

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            UpdateEditShapes();
            MainMap.TrackOverlay.TrackMode = TrackMode.None;
        }

        private void UpdateEditShapes()
        {
            foreach (Feature feature in MainMap.EditOverlay.EditShapesLayer.GetAll())
            {
                MainMap.TrackOverlay.TrackShapeLayer.Add(feature);
            }
            MainMap.EditOverlay.EditShapesLayer.Clear();
            MainMap.Refresh(new BaseOverlay[] { MainMap.EditOverlay, MainMap.TrackOverlay });
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Text)
            {
                case "Point":
                    MainMap.TrackOverlay.TrackMode = TrackMode.Point;
                    break;
                case "Line":
                    MainMap.TrackOverlay.TrackMode = TrackMode.Line;
                    break;
                case "Polygon":
                    MainMap.TrackOverlay.TrackMode = TrackMode.Polygon;
                    break;
                case "Square":
                    MainMap.TrackOverlay.TrackMode = TrackMode.Square;
                    break;
                case "Rectangle":
                    MainMap.TrackOverlay.TrackMode = TrackMode.Rectangle;
                    break;
                case "Circle":
                    MainMap.TrackOverlay.TrackMode = TrackMode.Circle;
                    break;
                case "Ellipse":
                    MainMap.TrackOverlay.TrackMode = TrackMode.Ellipse;
                    break;
                case "Edit":
                    MainMap.TrackOverlay.TrackMode = TrackMode.None;
                    foreach (Feature feature in MainMap.TrackOverlay.TrackShapeLayer.GetAll())
                    {
                        MainMap.EditOverlay.EditShapesLayer.Add(feature);
                    }
                    MainMap.EditOverlay.CalculateAllControlPoints();
                    MainMap.TrackOverlay.TrackShapeLayer.Clear();

                    MainMap.Refresh(new BaseOverlay[] { MainMap.EditOverlay, MainMap.TrackOverlay });
                    break;
            }
        }

        private void InitMapShapes()
        {
            if (MainMap != null)
            {

                MainMap.MapUnit = GeographyUnit.Meter;

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

                //label layer
                ShapeFileFeatureLayer labelLocationLayer = FeatureLayerFactory.CreateShapeFileFeatureLayer(sampleDataPath + "/panama_locations.shp");
                LabelStyle labelStyle = new LabelStyle("NAME", new GeoFont("Centaur", 20, DrawingFontStyles.Bold), new GeoSolidBrush(GeoColor.StandardColors.Brown));
                labelStyle.HaloPen = new GeoPen(GeoColor.StandardColors.White);
                labelStyle.PointPlacement = PointPlacement.UpperLeft;
                labelStyle.XOffsetInPixel = 5;
                labelStyle.OverlappingRule = LabelOverlappingRule.NoOverlapping;
                labelLocationLayer.ZoomLevelSet.ZoomLevel01.DefaultTextStyle = labelStyle;
                labelLocationLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

                Proj4Projection proj4Projection = new Proj4Projection();
                proj4Projection.InternalProjectionParametersString = Proj4Projection.GetWgs84ParametersString();
                proj4Projection.ExternalProjectionParametersString = Proj4Projection.GetSphericalMercatorParametersString();
                districtLayer.FeatureSource.Projection = proj4Projection;
                roadLayer.FeatureSource.Projection = proj4Projection;
                locationLayer.FeatureSource.Projection = proj4Projection;
                labelLocationLayer.FeatureSource.Projection = proj4Projection;

                LayerOverlay overlay = new LayerOverlay(MainMap.Context);
                overlay.Layers.Add("District", districtLayer);
                overlay.Layers.Add("Road", roadLayer);
                overlay.Layers.Add("Location", locationLayer);

                LayerOverlay labelOverlay = new LayerOverlay(MainMap.Context);
                labelOverlay.TileType = TileType.SingleTile;
                labelOverlay.Layers.Add("LabelLocation", labelLocationLayer);

                string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string cachePath = System.IO.Path.Combine(basePath, "Temp");

                MainMap.Overlays.Add(overlay);
                MainMap.Overlays.Add(labelOverlay);

                districtLayer.Open();
                MainMap.CurrentExtent = districtLayer.GetBoundingBox();                
                districtLayer.Close();
                var obj = MainMap.LayoutParameters;

                MainMap.MapTools.ScaleLine.Enabled = true;
                MainMap.MapTools.MouseCoordinate.Enabled = true;
                MainMap.ZoomAnimationDuration = 500;
                MainMap.Refresh();
            }
        }

        private void WmsOverlay()
        {
            WmsOverlayLite overlayWMS = OverlayFactory.CreateWmsOverlayLite(MainMap.Context, 
                new Uri("http://mrdata.usgs.gov/services/ca?request=getcapabilities&service=WMS&version=1.1.1&"));
            overlayWMS.Parameters["LAYERS"] = "California_Lithology,California_Contacts";

            MainMap.Overlays.Add(overlayWMS);
            MainMap.CurrentExtent = new RectangleShape(-123.45, 38.69, -121.41, 37.40);
            MainMap.Refresh();
        }

        private void GEOJSON()
        {
            //MapUnit set to DecimalDegrees because the data is in Longitude / Latitude.
            MainMap.MapUnit = GeographyUnit.DecimalDegree;
            MainMap.BackgroundOverlay.BackgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.White);
            string usPath = sampleDataPath + "/states.geojson";
            BaseFeatureLayer usLayer = FeatureLayerFactory.CreateVectorFeatureLayer(usPath);
            usLayer.ZoomLevelSet.ZoomLevel01.DefaultAreaStyle = AreaStyles.CreateSimpleAreaStyle(GeoColor.StandardColors.DarkCyan, GeoColor.StandardColors.Black);
            usLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            LayerOverlay staticOverlay = new LayerOverlay(MainMap.Context);
            staticOverlay.Layers.Add("VectorLayer", usLayer);
            MainMap.Overlays.Add("StaticOverlay", staticOverlay);
            MainMap.MapTools.ScaleLine.Enabled = true;
            MainMap.CurrentExtent = new RectangleShape(-130, 50, -63, 21);
            MainMap.Refresh();
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitMapShapes();   
            //GEOJSON();
        }
    }
}

