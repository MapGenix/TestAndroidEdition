using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Layers.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Layers
{
    /// <summary>To generate maps outside a map control</summary>
   [Serializable]
    public class MapEngine
    {
        bool _showLogo;
        RectangleShape _currentExtent;
        BaseGeoBrush _backgroundBrush;
        GdiPlusGeoCanvas _geoCanvas;
        readonly Collection<SimpleCandidate> _labeledFeaturesInLayers;
        readonly Dictionary<string, BaseLayer> _staticLayers;
        readonly Dictionary<string, BaseLayer> _dynamicLayers;

        readonly Dictionary<string, BaseAdornmentLayer> _adornmentLayerCollection;

        public event EventHandler<LayersEventArgs> LayersDrawing;

        public event EventHandler<LayersEventArgs> LayersDrawn;

        public event EventHandler<LayerEventArgs> LayerDrawing;

        public event EventHandler<LayerEventArgs> LayerDrawn;

        public event EventHandler<AdornmentLayersEventArgs> AdornmentLayersDrawing;

        public event EventHandler<AdornmentLayersEventArgs> AdornmentLayersDrawn;

        public event EventHandler<AdornmentLayerEventArgs> AdornmentLayerDrawing;

        public event EventHandler<AdornmentLayerEventArgs> AdornmentLayerDrawn;

        public MapEngine()
        {
            _backgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.Transparent);
            _labeledFeaturesInLayers = new Collection<SimpleCandidate>();
            _staticLayers = new Dictionary<string, BaseLayer>();
            _dynamicLayers = new Dictionary<string, BaseLayer>();
            _adornmentLayerCollection = new Dictionary<string, BaseAdornmentLayer>();
            _geoCanvas = new GdiPlusGeoCanvas();
            _currentExtent = new RectangleShape();
        }

        protected virtual void OnAdornmentLayersDrawing(AdornmentLayersEventArgs e)
        {
            EventHandler<AdornmentLayersEventArgs> handler = AdornmentLayersDrawing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAdornmentLayersDrawn(AdornmentLayersEventArgs e)
        {
            EventHandler<AdornmentLayersEventArgs> handler = AdornmentLayersDrawn;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAdornmentLayerDrawing(AdornmentLayerEventArgs e)
        {
            EventHandler<AdornmentLayerEventArgs> handler = AdornmentLayerDrawing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAdornmentLayerDrawn(AdornmentLayerEventArgs e)
        {
            EventHandler<AdornmentLayerEventArgs> handler = AdornmentLayerDrawn;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLayersDrawing(LayersEventArgs e)
        {
            EventHandler<LayersEventArgs> handler = LayersDrawing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLayersDrawn(LayersEventArgs e)
        {
            EventHandler<LayersEventArgs> handler = LayersDrawn;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLayerDrawing(LayerEventArgs e)
        {
            EventHandler<LayerEventArgs> handler = LayerDrawing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnLayerDrawn(LayerEventArgs e)
        {
            EventHandler<LayerEventArgs> handler = LayerDrawn;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public GdiPlusGeoCanvas Canvas
        {
            get { return _geoCanvas; }
            set { _geoCanvas = value; }
        }

        public bool ShowLogo
        {
            get { return _showLogo; }
            set { _showLogo = value; }
        }

        public RectangleShape CurrentExtent
        {
            get { return _currentExtent; }
            set { _currentExtent = value; }
        }

        public ICollection<BaseLayer> StaticLayers()
        {
            Collection<BaseLayer> layers = new Collection<BaseLayer>();
            foreach(BaseLayer l in _staticLayers.Values)
            {
                layers.Add(l);
            }
            return layers;
        }
        
        public void AddStaticLayer(BaseLayer layer)
        {
            _staticLayers.Add(Guid.NewGuid().ToString(), layer);
        }

    

        public BaseGeoBrush BackgroundFillBrush
        {
            get { return _backgroundBrush; }
            set { _backgroundBrush = value; }
        }

        public BaseFeatureLayer FindStaticFeatureLayer(string name)
        {
            BaseFeatureLayer featureLayer = null;

            if (_staticLayers.ContainsKey(name))
            {
                featureLayer = _staticLayers[name] as BaseFeatureLayer;
            }
            return featureLayer;
        }

        /*public RasterLayer FindStaticRasterLayer(string name)
        {
            RasterLayer rasterLayer = null;

            if (_staticLayers.ContainsKey(name))
            {
                rasterLayer = _staticLayers[name] as RasterLayer;
            }

            return rasterLayer;
        }*/

        public BaseFeatureLayer FindDynamicFeatureLayer(string name)
        {
            BaseFeatureLayer featureLayer = null;

            if (_dynamicLayers.ContainsKey(name))
            {
                featureLayer = _dynamicLayers[name] as BaseFeatureLayer;
            }

            return featureLayer;
        }

        /*public RasterLayer FindDynamicRasterLayer(string name)
        {
            RasterLayer rasterLayer = null;

            if (_dynamicLayers.ContainsKey(name))
            {
                rasterLayer = _dynamicLayers[name] as RasterLayer;
            }

            return rasterLayer;
        }*/

        public RectangleShape GetDrawingExtent(float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return GetDrawingExtent(_currentExtent, screenWidth, screenHeight);
        }

        public static RectangleShape GetDrawingExtent(RectangleShape worldExtent, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.GetDrawingExtent(worldExtent, screenWidth, screenHeight);
        }

        public void OpenAllLayers()
        {
            foreach (BaseLayer layer in _staticLayers.Values)
            {
                if (!layer.IsOpen) { layer.Open(); }
            }

            foreach (BaseLayer layer in _dynamicLayers.Values)
            {
                if (!layer.IsOpen) { layer.Open(); }
            }

            foreach (BaseLayer layer in _adornmentLayerCollection.Values)
            {
                if (!layer.IsOpen) { layer.Open(); }
            }
        }

        public void CloseAllLayers()
        {
            foreach (BaseLayer layer in _staticLayers.Values)
            {
                if (layer.IsOpen) { layer.Close(); }
            }

            foreach (BaseLayer layer in _dynamicLayers.Values)
            {
                if (layer.IsOpen) { layer.Close(); }
            }

            foreach (BaseLayer layer in _adornmentLayerCollection.Values)
            {
                if (layer.IsOpen) { layer.Close(); }
            }
        }

        public static RectangleShape CenterAt(RectangleShape worldExtent, PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckShapeIsValidForOperation(worldPoint);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.CenterAt(worldExtent, worldPoint, screenWidth, screenHeight);
        }

        public void CenterAt(PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckShapeIsValidForOperation(worldPoint);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = CenterAt(_currentExtent, worldPoint, screenWidth, screenHeight);
        }

        public static RectangleShape CenterAt(RectangleShape worldExtent, Feature centerFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.CenterAt(worldExtent, centerFeature, screenWidth, screenHeight);
        }

        public void CenterAt(Feature centerFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = CenterAt(_currentExtent, centerFeature, screenWidth, screenHeight);
        }

        public static RectangleShape CenterAt(RectangleShape worldExtent, float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.CenterAt(worldExtent, screenX, screenY, screenWidth, screenHeight);
        }

        public void CenterAt(float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = CenterAt(_currentExtent, screenX, screenY, screenWidth, screenHeight);
        }

        public static float GetScreenDistanceBetweenTwoWorldPoints(RectangleShape worldExtent, PointShape worldPoint1, PointShape worldPoint2, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(worldPoint1, "worldPoint1");
            Validators.CheckParameterIsNotNull(worldPoint2, "worldPoint2");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.GetScreenDistanceBetweenTwoWorldPoints(worldExtent, worldPoint1, worldPoint2, screenWidth, screenHeight);
        }

        public float GetScreenDistanceBetweenTwoWorldPoints(PointShape worldPoint1, PointShape worldPoint2, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldPoint1, "worldPoint1");
            Validators.CheckParameterIsNotNull(worldPoint2, "worldPoint2");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return GetScreenDistanceBetweenTwoWorldPoints(_currentExtent, worldPoint1, worldPoint2, screenWidth, screenHeight);
        }

        public float GetScreenDistanceBetweenTwoWorldPoints(Feature worldPointFeature1, Feature worldPointFeature2, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return GetScreenDistanceBetweenTwoWorldPoints(_currentExtent, worldPointFeature1, worldPointFeature2, screenWidth, screenHeight);
        }

        public static float GetScreenDistanceBetweenTwoWorldPoints(RectangleShape worldExtent, Feature worldPointFeature1, Feature worldPointFeature2, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.GetScreenDistanceBetweenTwoWorldPoints(worldExtent, worldPointFeature1, worldPointFeature2, screenWidth, screenHeight);
        }

        public static double GetWorldDistanceBetweenTwoScreenPoints(RectangleShape worldExtent, ScreenPointF screenPoint1, ScreenPointF screenPoint2, float screenWidth, float screenHeight, GeographyUnit worldExtentUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");

            return ExtentHelper.GetWorldDistanceBetweenTwoScreenPoints(worldExtent, screenPoint1, screenPoint2, screenWidth, screenHeight, worldExtentUnit, distanceUnit);
        }

        public double GetWorldDistanceBetweenTwoScreenPoints(ScreenPointF screenPoint1, ScreenPointF screenPoint2, float screenWidth, float screenHeight, GeographyUnit mapUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return GetWorldDistanceBetweenTwoScreenPoints(_currentExtent, screenPoint1, screenPoint2, screenWidth, screenHeight, mapUnit, distanceUnit);
        }

        public static double GetWorldDistanceBetweenTwoScreenPoints(RectangleShape worldExtent, float screenPoint1X, float screenPoint1Y, float screenPoint2X, float screenPoint2Y, float screenWidth, float screenHeight, GeographyUnit worldExtentUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");

            return ExtentHelper.GetWorldDistanceBetweenTwoScreenPoints(worldExtent, screenPoint1X, screenPoint1Y, screenPoint2X, screenPoint2Y, screenWidth, screenHeight, worldExtentUnit, distanceUnit);
        }

        public double GetCurrentScale(float screenWidth, GeographyUnit mapUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return GetCurrentScale(_currentExtent, screenWidth, mapUnit);
        }


        public static double GetCurrentScale(RectangleShape worldExtent, float screenWidth, GeographyUnit mapUnit)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");

            return ExtentHelper.GetScale(worldExtent, screenWidth, mapUnit);
        }

        public static RectangleShape GetBoundingBoxOfItems(IEnumerable<BaseShape> shapes)
        {
            Validators.CheckParameterIsNotNull(shapes, "shapes");

            return ExtentHelper.GetBoundingBoxOfItems(shapes);
        }

        public static RectangleShape GetBoundingBoxOfItems(IEnumerable<Feature> features)
        {
            Validators.CheckParameterIsNotNull(features, "features");

            return ExtentHelper.GetBoundingBoxOfItems(features);
        }

        
        public GeoImage Draw(IEnumerable<BaseLayer> layers, GeoImage image, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(layers, "layers");
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(layers, image, mapUnit, false);
        }

        public GeoImage Draw(IEnumerable<BaseLayer> layers, int width, int height, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(layers, "layers");
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(height, "height", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(layers, width, height, mapUnit, false);
        }

        public GeoImage DrawStaticLayers(GeoImage image, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_staticLayers.Values, image, mapUnit, true);
        }

        public GeoImage DrawDynamicLayers(GeoImage image, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_dynamicLayers.Values, image, mapUnit, false);
        }

        public GeoImage DrawAdornmentLayers(GeoImage image, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckParameterIsNotNull(image, "gdiPlusBitmap");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_adornmentLayerCollection.Values, image, mapUnit);
        }

        public GeoImage DrawStaticLayers(int width, int height, GeographyUnit mapUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(height, "height", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_staticLayers.Values, width, height, mapUnit, true);
        }

        public GeoImage DrawDynamicLayers(int width, int height, GeographyUnit mapUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(height, "height", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_dynamicLayers.Values, width, height, mapUnit, false);
        }

        public GeoImage DrawAdornmentLayers(int width, int height, GeographyUnit mapUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(height, "height", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_adornmentLayerCollection.Values, width, height, mapUnit);
        }

        GeoImage Draw(IEnumerable<BaseAdornmentLayer> adornmentLayers, int width, int height, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(adornmentLayers, "adornmentLayers");
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(height, "height", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            GeoImage returningGeoImage = new GeoImage(width, height);

            Object nativeImage = _geoCanvas.ToNativeImage(returningGeoImage);

            AdornmentLayersEventArgs layersDrawingEventArgs = new AdornmentLayersEventArgs(adornmentLayers);
            OnAdornmentLayersDrawing(layersDrawingEventArgs);

            _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
            foreach (BaseAdornmentLayer adornmentLayer in adornmentLayers)
            {
                DrawOneAdornmentLayer(adornmentLayer);
            }
            _geoCanvas.EndDrawing();

            AdornmentLayersEventArgs layerslDrawnEventArgs = new AdornmentLayersEventArgs(adornmentLayers);
            OnAdornmentLayersDrawn(layerslDrawnEventArgs);

            return returningGeoImage;
        }

        GeoImage Draw(IEnumerable<BaseLayer> layers, int width, int height, GeographyUnit mapUnit, bool isToDrawBackground)
        {
            Validators.CheckParameterIsNotNull(layers, "layers");
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(height, "height", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            GeoImage returningGeoImage = null;
            using (GeoImage tempGeoImage = new GeoImage(width, height))
            {
                _labeledFeaturesInLayers.Clear();

                Object nativeImage = _geoCanvas.ToNativeImage(tempGeoImage);

                if (isToDrawBackground)
                {
                    _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
                    _geoCanvas.Clear(_backgroundBrush);
                    _geoCanvas.EndDrawing();
                }

                LayersEventArgs layersDrawingEventArgs = new LayersEventArgs(layers, _currentExtent, nativeImage);
                OnLayersDrawing(layersDrawingEventArgs);

                if (!layersDrawingEventArgs.Cancel)
                {
                    foreach (BaseLayer layer in layers)
                    {
                        _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
                        DrawOneLayer(layer, nativeImage);
                        _geoCanvas.EndDrawing();
                    }
                }

                _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
                DrawLogo();
                _geoCanvas.EndDrawing();

                LayersEventArgs layerslDrawnEventArgs = new LayersEventArgs(layers, _currentExtent, nativeImage);
                OnLayersDrawn(layerslDrawnEventArgs);

                returningGeoImage = _geoCanvas.ToGeoImage(nativeImage);

            }
            
            return returningGeoImage;
        }

        GeoImage Draw(IEnumerable<BaseAdornmentLayer> adornmentLayers, GeoImage image, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(adornmentLayers, "adornmentLayers");
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            AdornmentLayersEventArgs layersDrawingEventArgs = new AdornmentLayersEventArgs(adornmentLayers);
            OnAdornmentLayersDrawing(layersDrawingEventArgs);

            Object nativeImage = _geoCanvas.ToNativeImage(image);
            _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
            foreach (BaseAdornmentLayer adornmentLayer in adornmentLayers)
            {
                DrawOneAdornmentLayer(adornmentLayer);
            }
            _geoCanvas.EndDrawing();

            AdornmentLayersEventArgs layerslDrawnEventArgs = new AdornmentLayersEventArgs(adornmentLayers);
            OnAdornmentLayersDrawn(layerslDrawnEventArgs);

            using (GeoImage currentGeoImage = _geoCanvas.ToGeoImage(nativeImage))
            {
                StreamHelper.CopyStream(currentGeoImage.GetImageStream(_geoCanvas), image.GetImageStream(_geoCanvas));
            }
            
            return image;
        }

        GeoImage Draw(IEnumerable<BaseLayer> layers, GeoImage image, GeographyUnit mapUnit, bool isToDrawBackground)
        {
            Validators.CheckParameterIsNotNull(layers, "layers");
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _labeledFeaturesInLayers.Clear();

            Object nativeImage = _geoCanvas.ToNativeImage(image);
            if (isToDrawBackground)
            {
                _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
                _geoCanvas.Clear(_backgroundBrush);
                _geoCanvas.EndDrawing();
            }

            LayersEventArgs layersDrawingEventArgs = new LayersEventArgs(layers, _currentExtent, nativeImage);
            OnLayersDrawing(layersDrawingEventArgs);

            if (!layersDrawingEventArgs.Cancel)
            {
                foreach (BaseLayer layer in layers)
                {
                    _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
                    DrawOneLayer(layer, nativeImage);
                    _geoCanvas.EndDrawing();
                }
            }

            _geoCanvas.BeginDrawing(nativeImage, _currentExtent, mapUnit);
            DrawLogo();
            _geoCanvas.EndDrawing();

            LayersEventArgs layerslDrawnEventArgs = new LayersEventArgs(layers, _currentExtent, nativeImage);
            OnLayersDrawn(layerslDrawnEventArgs);

            GeoImage tempGeoImage = null;
            try
            {
                tempGeoImage = _geoCanvas.ToGeoImage(nativeImage);
                StreamHelper.CopyStream(tempGeoImage.GetImageStream(_geoCanvas), image.GetImageStream(_geoCanvas));
            }
            finally
            {
                if (tempGeoImage != null) { tempGeoImage.Dispose(); }
            }

            return image;
        }

        Bitmap Draw(IEnumerable<BaseAdornmentLayer> adornmentLayers, Bitmap gdiPlusBitmap, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(gdiPlusBitmap, "gdiPlusBitmap");
            Validators.CheckParameterIsNotNull(adornmentLayers, "adornmentLayers");
            Validators.CheckParameterIsNotNull(gdiPlusBitmap, "image");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            if (!(_geoCanvas is GdiPlusGeoCanvas)) { throw new ArgumentException("The GeoCanvas isn't right."); }

            AdornmentLayersEventArgs layersDrawingEventArgs = new AdornmentLayersEventArgs(adornmentLayers);
            OnAdornmentLayersDrawing(layersDrawingEventArgs);

            _geoCanvas.BeginDrawing(gdiPlusBitmap, _currentExtent, mapUnit);
            foreach (BaseAdornmentLayer adornmentLayer in adornmentLayers)
            {
                DrawOneAdornmentLayer(adornmentLayer);
            }
            _geoCanvas.EndDrawing();

            AdornmentLayersEventArgs layerslDrawnEventArgs = new AdornmentLayersEventArgs(adornmentLayers);
            OnAdornmentLayersDrawn(layerslDrawnEventArgs);

            return gdiPlusBitmap;
        }

        Bitmap Draw(IEnumerable<BaseLayer> layers, Bitmap gdiPlusBitmap, GeographyUnit mapUnit, bool isToDrawBackground)
        {
            Validators.CheckParameterIsNotNull(layers, "layers");
            Validators.CheckParameterIsNotNull(gdiPlusBitmap, "gdiPlusBitmap");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _labeledFeaturesInLayers.Clear();

            if (!(_geoCanvas is GdiPlusGeoCanvas)) { throw new ArgumentException("The GeoCanvas isn't right."); }

            if (isToDrawBackground)
            {
                _geoCanvas.BeginDrawing(gdiPlusBitmap, _currentExtent, mapUnit);
                _geoCanvas.Clear(_backgroundBrush);
                _geoCanvas.EndDrawing();
            }


            LayersEventArgs layersDrawingEventArgs = new LayersEventArgs(layers, _currentExtent, gdiPlusBitmap);
            OnLayersDrawing(layersDrawingEventArgs);

            if (!layersDrawingEventArgs.Cancel)
            {
                foreach (BaseLayer layer in layers)
                {
                    _geoCanvas.BeginDrawing(gdiPlusBitmap, _currentExtent, mapUnit);
                    DrawOneLayer(layer, gdiPlusBitmap);
                    _geoCanvas.EndDrawing();
                }
            }

            _geoCanvas.BeginDrawing(gdiPlusBitmap, _currentExtent, mapUnit);
            DrawLogo();
            _geoCanvas.EndDrawing();

            LayersEventArgs layerslDrawnEventArgs = new LayersEventArgs(layers, _currentExtent, gdiPlusBitmap);
            OnLayersDrawn(layerslDrawnEventArgs);

            return gdiPlusBitmap;
        }

        public Bitmap Draw(IEnumerable<BaseLayer> layers, Bitmap gdiPlusBitmap, GeographyUnit mapUnit)
        {
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(layers, gdiPlusBitmap, mapUnit, false);
        }

        public Bitmap DrawStaticLayers(Bitmap gdiPlusBitmap, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(gdiPlusBitmap, "gdiPlusBitmap");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_staticLayers.Values, gdiPlusBitmap, mapUnit, true);
        }

        public Bitmap DrawDynamicLayers(Bitmap gdiPlusBitmap, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(gdiPlusBitmap, "gdiPlusBitmap");
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_dynamicLayers.Values, gdiPlusBitmap, mapUnit, false);
        }

        public Bitmap DrawAdornmentLayers(Bitmap gdiPlusBitmap, GeographyUnit mapUnit)
        {
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
            Validators.CheckParameterIsNotNull(gdiPlusBitmap, "gdiPlusBitmap");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return Draw(_adornmentLayerCollection.Values, gdiPlusBitmap, mapUnit);
        }

        void DrawLogo()
        {
            if (ShowLogo)
            {
                using (Stream stream = new MemoryStream())
                {
                    Resources.MapgenixLogo.Save(stream, ImageFormat.Png);
                    float logoWidth = Resources.MapgenixLogo.Width;
                    float logoHeight = Resources.MapgenixLogo.Height;
                    using (GeoImage logoImage = new GeoImage(stream))
                    {
                        _geoCanvas.DrawWorldImage(logoImage, _currentExtent.LowerRightPoint.X,
                            _currentExtent.LowerRightPoint.Y, logoWidth, logoHeight, DrawingLevel.LevelFour,
                            -logoWidth/2 - 2, -logoHeight/2 - 2, 0);
                    }
                }
            }
        }

        public void ZoomIn(int percentage)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomIn(_currentExtent, percentage);
        }

        public static RectangleShape ZoomIn(RectangleShape worldExtent, int percentage)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomIn(worldExtent, percentage);
        }

        public static RectangleShape ZoomIntoCenter(RectangleShape worldExtent, int percentage, PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomIntoCenter(worldExtent, percentage, worldPoint, screenWidth, screenHeight);
        }

        public static RectangleShape ZoomIntoCenter(RectangleShape worldExtent, int percentage, Feature centerFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomIntoCenter(worldExtent, percentage, centerFeature, screenWidth, screenHeight);
        }

        public void ZoomIntoCenter(int percentage, PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomIntoCenter(_currentExtent, percentage, worldPoint, screenWidth, screenHeight);
        }

        public void ZoomIntoCenter(int percentage, Feature centerFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomIntoCenter(_currentExtent, percentage, centerFeature, screenWidth, screenHeight);
        }

        public static RectangleShape ZoomIntoCenter(RectangleShape worldExtent, int percentage, float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenX, "screenX", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenY, "screenY", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomIntoCenter(worldExtent, percentage, screenX, screenY, screenWidth, screenHeight);
        }

        public void ZoomIntoCenter(int percentage, float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenX, "screenX", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenY, "screenY", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomIntoCenter(_currentExtent, percentage, screenX, screenY, screenWidth, screenHeight);
        }

        public static RectangleShape ZoomOut(RectangleShape worldExtent, int percentage)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomOut(worldExtent, percentage);
        }

        public void ZoomOut(int percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomOut(_currentExtent, percentage);
        }

        public static RectangleShape ZoomOutToCenter(RectangleShape worldExtent, int percentage, PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomOutToCenter(worldExtent, percentage, worldPoint, screenWidth, screenHeight);
        }

        public static RectangleShape ZoomOutToCenter(RectangleShape worldExtent, int percentage, Feature centerFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomOutToCenter(worldExtent, percentage, centerFeature, screenWidth, screenHeight);
        }

        public void ZoomOutToCenter(int percentage, PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomOutToCenter(_currentExtent, percentage, worldPoint, screenWidth, screenHeight);
        }

        public void ZoomOutToCenter(int percentage, Feature centerFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomOutToCenter(_currentExtent, percentage, centerFeature, screenWidth, screenHeight);
        }

        public static RectangleShape ZoomOutToCenter(RectangleShape worldExtent, int percentage, float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ZoomOutToCenter(worldExtent, percentage, screenX, screenY, screenWidth, screenHeight);
        }

        public void ZoomOutToCenter(int percentage, float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomOutToCenter(_currentExtent, percentage, screenX, screenY, screenWidth, screenHeight);
        }

        public static RectangleShape Pan(RectangleShape worldExtent, PanDirection direction, int percentage)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(direction, "direction");
            Validators.CheckPanDirectionIsValid(direction, "direction");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            return ExtentHelper.Pan(worldExtent, direction, percentage);
        }

        public void Pan(PanDirection panDirection, int percentage)
        {
            Validators.CheckPanDirectionIsValid(panDirection, "direction");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = Pan(_currentExtent, panDirection, percentage);
        }

        public static RectangleShape Pan(RectangleShape worldExtent, float degree, int percentage)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsInRange(degree, "degree", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            return ExtentHelper.Pan(worldExtent, degree, percentage);
        }

        public void Pan(float degree, int percentage)
        {
            Validators.CheckIfInputValueIsInRange(degree, "degree", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = Pan(_currentExtent, degree, percentage);
        }

        public ScreenPointF ToScreenCoordinate(double worldX, double worldY, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return ToScreenCoordinate(_currentExtent, worldX, worldY, screenWidth, screenHeight);
        }

        public ScreenPointF ToScreenCoordinate(PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return ToScreenCoordinate(_currentExtent, worldPoint.X, worldPoint.Y, screenWidth, screenHeight);
        }

        public ScreenPointF ToScreenCoordinate(Feature worldPointFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return ToScreenCoordinate(_currentExtent, worldPointFeature, screenWidth, screenHeight);
        }

        public PointShape ToWorldCoordinate(float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return ToWorldCoordinate(_currentExtent, screenX, screenY, screenWidth, screenHeight);
        }

        public PointShape ToWorldCoordinate(ScreenPointF screenPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            return ToWorldCoordinate(screenPoint.X, screenPoint.Y, screenWidth, screenHeight);
        }

        public static ScreenPointF ToScreenCoordinate(RectangleShape worldExtent, double worldX, double worldY, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ToScreenCoordinate(worldExtent, worldX, worldY, screenWidth, screenHeight);
        }

        public static ScreenPointF ToScreenCoordinate(RectangleShape worldExtent, PointShape worldPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckShapeIsValidForOperation(worldPoint);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ToScreenCoordinate(worldExtent, worldPoint, screenWidth, screenHeight);
        }

        public static ScreenPointF ToScreenCoordinate(RectangleShape worldExtent, Feature worldPointFeature, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckFeatureIsValid(worldPointFeature);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ToScreenCoordinate(worldExtent, worldPointFeature, screenWidth, screenHeight);
        }

        public static PointShape ToWorldCoordinate(RectangleShape worldExtent, float screenX, float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ToWorldCoordinate(worldExtent, screenX, screenY, screenWidth, screenHeight);
        }

        public static PointShape ToWorldCoordinate(RectangleShape worldExtent, ScreenPointF screenPoint, float screenWidth, float screenHeight)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0, RangeCheckingInclusion.ExcludeValue);

            return ExtentHelper.ToWorldCoordinate(worldExtent, screenPoint, screenWidth, screenHeight);
        }

        public static RectangleShape SnapToZoomLevel(RectangleShape worldExtent, GeographyUnit worldExtentUnit, float screenWidth, float screenHeight, ZoomLevelSet zoomLevelSet)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");

            return ExtentHelper.SnapToZoomLevel(worldExtent, worldExtentUnit, screenWidth, screenHeight, zoomLevelSet);
        }

        public void SnapToZoomLevel(GeographyUnit worldExtentUnit, float screenWidth, float screenHeight, ZoomLevelSet zoomLevelSet)
        {
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = SnapToZoomLevel(_currentExtent, worldExtentUnit, screenWidth, screenHeight, zoomLevelSet);
        }

        public static RectangleShape ZoomToScale(double targetScale, RectangleShape worldExtent, GeographyUnit worldExtentUnit, float screenWidth, float screenHeight)
        {
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");

            return ExtentHelper.ZoomToScale(targetScale, worldExtent, worldExtentUnit, screenWidth, screenHeight);
        }

        public void ZoomToScale(double targetScale, GeographyUnit worldExtentUnit, float screenWidth, float screenHeight)
        {
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckMapEngineExtentIsValid(_currentExtent, "currentExtent");

            _currentExtent = ZoomToScale(targetScale, _currentExtent, worldExtentUnit, screenWidth, screenHeight);
        }

        
        void DrawOneLayer(BaseLayer layer, Object nativeImage)
        {
            LayerEventArgs layerDrawingEventArgs = new LayerEventArgs(layer, _currentExtent, nativeImage);
            OnLayerDrawing(layerDrawingEventArgs);

            if (!layerDrawingEventArgs.Cancel)
            {
                layer.Draw(_geoCanvas, _labeledFeaturesInLayers);
            }

            LayerEventArgs layerDrawnEventArgs = new LayerEventArgs(layer, _currentExtent, nativeImage);
            OnLayerDrawn(layerDrawnEventArgs);
        }

        void DrawOneAdornmentLayer(BaseAdornmentLayer adornmentLayer)
        {
            AdornmentLayerEventArgs layerDrawingEventArgs = new AdornmentLayerEventArgs(adornmentLayer);
            OnAdornmentLayerDrawing(layerDrawingEventArgs);

            if (adornmentLayer.IsVisible)
            {
                adornmentLayer.Draw(_geoCanvas, _labeledFeaturesInLayers);
            }
            AdornmentLayerEventArgs layerDrawnEventArgs = new AdornmentLayerEventArgs(adornmentLayer);
            OnAdornmentLayerDrawn(layerDrawnEventArgs);
        }

        public static DataTable LoadDataTable(Collection<Feature> features, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            DataTable dataTable = null;

            if (features != null)
            {
                dataTable = new DataTable();
                dataTable.Locale = CultureInfo.InvariantCulture;

                foreach (string columnName in returningColumnNames)
                {
                    dataTable.Columns.Add(columnName);
                }

                foreach (Feature feature in features)
                {
                    DataRow dataRow = dataTable.NewRow();

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (feature.ColumnValues.ContainsKey(dataTable.Columns[i].ColumnName))
                        {
                            dataRow[i] = feature.ColumnValues[dataTable.Columns[i].ColumnName];
                        }
                        else
                        {
                            dataRow[i] = String.Empty;
                        }
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }

    }
}
