using System;
using System.Globalization;
using Mapgenix.Shapes;
using Mapgenix.Projection;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Mapgenix.Canvas.Properties;


using System.Collections.Generic;

namespace Mapgenix.Canvas
{
    /// <summary>Projection class using Proj4 projection library to project data from one coordinate system to another</summary>
    /// <remarks>Uses Proj4 to project data from one Srid to another. Proj4 is a well known established projection library supporting thousands of
    /// projections. It uses a special text format to feed in parameters about the from (Internal) and the To (External)
    /// projections. TIt has become a standard and many organizations have recognized it and
    /// provide their projection definition in this format. The latest EPSG (European Petroleum Survey Group) and SRID (Spatial Reference System
    /// Identifiers) list are supported.<br/><br/>
    /// The SRID for the popular WGS84 is 4326.</remarks>
    [Serializable]
    public class Proj4Projection : BaseProjection, IDisposable
    {
        private readonly RectangleShape _decimalDegreeBoundary;

        private string _externalProjectionParametersString;
        private string _internalProjectionParametersString;

        private BaseProjection _projection;

        /// <summary>Default constructor of the class.</summary>
        /// <remarks>It is necessary to set the properties InternalProjectionParametersString and ExternalProjectionParametersString.</remarks>
        public Proj4Projection()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <remarks>Use this contructor if you know the SRID numbers</remarks>
        /// <param name="internalEpsgSrid">Srid of the internal projection.</param>
        /// <param name="externalEpsgSrid">Srid of the external projection.</param>
        public Proj4Projection(int internalEpsgSrid, int externalEpsgSrid)
            : this(
                ManagedProj4Projection.GetEpsgParametersString(internalEpsgSrid),
                ManagedProj4Projection.GetEpsgParametersString(externalEpsgSrid))
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Pass in Proj4 text parameters for the From (Internal) and the To (External)
        /// projections. The parameter strings look like "+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs"</overloads>
        /// <returns>None</returns>
        /// <remarks>Use this constructor with exact projection strings.</remarks>
        /// <param name="internalProjectionParametersString">Proj4 projection string for the internal projection.</param>
        /// <param name="externalProjectionParametersString">Proj4 projection string for the external projection.</param>
        public Proj4Projection(string internalProjectionParametersString, string externalProjectionParametersString)
        {
            InternalProjectionParametersString = internalProjectionParametersString;
            ExternalProjectionParametersString = externalProjectionParametersString;

            _decimalDegreeBoundary = new RectangleShape(-179.9, 89.9, 179.9, -89.9);
        }

        /// <summary>Returns the Boundary for decimal degrees values.</summary>
        public RectangleShape DecimalDegreeBoundary
        {
            get
            {
                if (_projection != null)
                {
                    if (_projection is ManagedProj4Projection)
                    {
                        return ((ManagedProj4Projection) _projection).DecimalDegreeBoundary;
                    }
                    return ((Proj4Projection) _projection).DecimalDegreeBoundary;
                }
                return _decimalDegreeBoundary;
            }
            set
            {
                if (_projection != null)
                {
                    if (_projection is ManagedProj4Projection)
                    {
                        ((ManagedProj4Projection) _projection).DecimalDegreeBoundary = value;
                    }
                    else
                    {
                        ((Proj4Projection) _projection).DecimalDegreeBoundary = value;
                    }
                }
            }
        }

        /// <summary>Gets or sets the Proj4 text parameter for the From (internal) projection.</summary>
        ///<remarks>The string looks like "+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs".</remarks>
        public string InternalProjectionParametersString
        {
            get { return _internalProjectionParametersString; }
            set { _internalProjectionParametersString = value; }
        }

        /// <summary>Gets or sets the Proj4 text parameter for the To (external) projection.</summary>
        ///<remarks>The string looks like "+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs".</remarks>
        public string ExternalProjectionParametersString
        {
            get { return _externalProjectionParametersString; }
            set { _externalProjectionParametersString = value; }
        }

        /// <summary>Dispose method for the class.</summary>
        /// <returns>None</returns>
        /// <remarks>Releases the handles of the Proj4 library. Use Close instead of Dispose unless the instance will not be used again.</remarks>
        public void Dispose()
        {
            InitializeProjection();
        }

        /// <summary>Opens the projection.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// As the core version of the Open method it is intended to be overridden in
        /// the sub versions of the class. When overriding the developer is responsible for getting the
        /// projection class ready for doing projections.<br/><br/>
        /// In this override the Proj4 methods are called to get ready.</remarks>
        protected override void OpenCore()
        {
            InitializeProjection();
            _projection.Open();
        }

        /// <summary>Closes the projection.</summary>
        /// <returns>None</returns>
        /// <remarks>As the core version of the Close method it is intended to be overridden
        /// in sub versions of the class. When overriding the developer is responsible for freeing any
        /// state that have been maintained and getting the class ready to serialization if necessary.<br/><br/>
        /// In this override the Proj4 methods are called and the handles and memory get released.</remarks>
        protected override void CloseCore()
        {
            InitializeProjection();
            _projection.Close();
        }

        /// <summary>Returns projected vertices based on the coordinates passed in.</summary>
        /// <returns>Projected vertices based on the coordinates passed in.</returns>
        /// <remarks>Returns a projected vertex based on the coordinates passed in. You need to override this method.</remarks>
        /// <param name="x">X values of the points to project.</param>
        /// <param name="y">Y values of the points to project.</param>
        protected override Vertex[] ConvertToExternalProjectionCore(double[] x, double[] y)
        {
            InitializeProjection();

            var multiPoints = new MultipointShape();

            for (var i = 0; i < x.Length; i++)
            {
                multiPoints.Points.Add(new PointShape(x[i], y[i]));
            }
            var convertedPoints = _projection.ConvertToExternalProjection(multiPoints) as MultipointShape;

            var verteces = new Vertex[convertedPoints.Points.Count];
            for (var i = 0; i < convertedPoints.Points.Count; i++)
            {
                var shape = convertedPoints.Points[i];
                verteces[i] = new Vertex(shape.X, shape.Y);
            }

            return verteces;
        }

        /// <summary>Returns reprojected vertices based on the coordinates passed in.</summary>
        /// <returns>Reprojected vertices based on the coordinates passed in.</returns>
        /// <remarks>Returns a projected vertex to the internal projection based on the coordinates passed in. You need to override this method.</remarks>
        /// <param name="x">X values of the points to reproject.</param>
        /// <param name="y">Y values of the points to reproject.</param>
        protected override Vertex[] ConvertToInternalProjectionCore(double[] x, double[] y)
        {
            InitializeProjection();

            var multiPoints = new MultipointShape();
            for (var i = 0; i < x.Length; i++)
            {
                multiPoints.Points.Add(new PointShape(x[i], y[i]));
            }
            var convertedPoints = _projection.ConvertToInternalProjection(multiPoints) as MultipointShape;

            var verteces = new Vertex[convertedPoints.Points.Count];
            for (var i = 0; i < convertedPoints.Points.Count; i++)
            {
                var shape = convertedPoints.Points[i];
                verteces[i] = new Vertex(shape.X, shape.Y);
            }

            return verteces;
        }

        private void InitializeProjection()
        {
            if (string.IsNullOrEmpty(InternalProjectionParametersString) ||
                string.IsNullOrEmpty(ExternalProjectionParametersString))
            {
                return;
            }

            if (_projection != null)
            {
                return;
            }

                _projection = new ManagedProj4Projection(InternalProjectionParametersString,
                    ExternalProjectionParametersString);
          
        }

        /// <summary>Static methjod to get a projection text from EPSG(European Petroleum Survey Group).</summary>
        /// <param name="srid">Srid number to get the projection text from.</param>
        /// <returns>Text corresponding to the srid.</returns>
        public static string GetEpsgParametersString(int srid)
        {
            return ManagedProj4Projection.GetEpsgParametersString(srid);
        }

        /// <summary>Static method to get a projection text for ESRI projection.</summary>
        /// <param name="srid">Srid number to get the projection ESRI text from.</param>
        /// <returns>The project text corresponding to the srid.</returns>
        public static string GetEsriParametersString(int srid)
        {
            return ManagedProj4Projection.GetEsriParametersString(srid);
        }

        /// <summary>Static method to get the Proj4 projection text for WGS84.</summary>
        /// <returns>Proj4 text corresponding to Wgs84.</returns>
        public static string GetWgs84ParametersString()
        {
            return ManagedProj4Projection.GetWgs84ParametersString();
        }

        /// <summary>Static method to get the Proj4 projection text for Spherical Mercator.</summary>
        /// <returns>Proj4 text corresponding to Spherical Mercator.</returns>
        /// <remarks>Popular map web services such as OpenStreetMaps, GoogleMaps, HEREMaps, BingMaps etc are in that projection.</remarks>
        public static string GetSphericalMercatorParametersString()
        {
            return ManagedProj4Projection.GetSphericalMercatorParametersString();
        }

        /// <summary>Static method to get the Prj string from an Epsg number.</summary>
        /// <param name="srid">Epsg number.</param>
        /// <returns>Prj string from an Epsg number.</returns>
        public static string ConvertEpsgToPrj(int srid)
        {
            var proj4ForEpsgNumber = string.Format(CultureInfo.InvariantCulture, "+init=epsg:{0}", srid);
            return ConvertProj4ToPrj(proj4ForEpsgNumber);
        }

        /// <summary>Static method to get the Prj string from a Proj4 string.</summary>
        /// <param name="srid">Proj4 string</param>
        /// <returns>Prj string from a Proj4 string.</returns>
        public static string ConvertProj4ToPrj(string proj4String)
        {
            return ManagedMiniGdal.Proj4ToWkt(proj4String);
        }

        /// <summary>Static method to get the Proj4 string from a Prj string.</summary>
        /// <param name="srid">Prj string</param>
        /// <returns>Proj4 string from a Prj string.</returns>
        public static string ConvertPrjToProj4(string prjString)
        {
            return ManagedMiniGdal.EsriWktToProj4(prjString);
        }

        ~Proj4Projection()
        {
            Dispose();
        }

        /// <summary>Returns the geographic unit of the internal projection.</summary>
        /// <returns>geographic unit of the internal projection.</returns>
        protected override GeographyUnit GetInternalGeographyUnitCore()
        {
            InitializeProjection();
            if (_projection != null)
            {
                return _projection.GetInternalGeographyUnit();
            }
            return GeographyUnit.Unknown;
        }

        /// <summary>Returns the geographic unit of the external projection.</summary>
        /// <returns>geographic unit of the external projection.</returns>
        protected override GeographyUnit GetExternalGeographyUnitCore()
        {
            InitializeProjection();
            if (_projection != null)
            {
                return _projection.GetExternalGeographyUnit();
            }
            return GeographyUnit.Unknown;
        }


        [Serializable]
        private class ManagedProj4Projection : BaseProjection
        {
            private readonly double _radian = Math.PI / 180;

            private readonly GisSys _system = new GisSys();
            private RectangleShape _decimalDegreeBoundary;

            [NonSerialized]
            private ProjType _destinationProj;

            private string _externalProjectionParametersString;
            private string _internalProjectionParametersString;
            private bool _isFromProjectionInDegree;
            private bool _isToProjectionInDegree;

            [NonSerialized]
            private ProjType _sourceProj;


            public ManagedProj4Projection()
                : this(string.Empty, string.Empty)
            {
            }


            public ManagedProj4Projection(string internalProjectionParameters, string externalProjectionParameters)
            {
                InternalProjectionParametersString = internalProjectionParameters;
                ExternalProjectionParametersString = externalProjectionParameters;
                _decimalDegreeBoundary = new RectangleShape(-179.9, 89.9, 179.9, -89.9);
            }



            public RectangleShape DecimalDegreeBoundary
            {
                get { return _decimalDegreeBoundary; }
                set { _decimalDegreeBoundary = value; }
            }



            public string InternalProjectionParametersString
            {
                get { return _internalProjectionParametersString; }
                set
                {
                    var parameterStrings = value.Split('+');
                    string newValue = null;
                    for (var i = 1; i < parameterStrings.Length; i++)
                    {
                        if (i == 1)
                        {
                            newValue = newValue + "+" + parameterStrings[i];
                        }
                        else
                        {
                            newValue = newValue + " +" + parameterStrings[i];
                        }
                    }
                    _internalProjectionParametersString = newValue;
                }
            }


            public string ExternalProjectionParametersString
            {
                get { return _externalProjectionParametersString; }
                set
                {
                    var parameterStrings = value.Split('+');
                    string newValue = null;
                    for (var i = 1; i < parameterStrings.Length; i++)
                    {
                        if (i == 1)
                        {
                            newValue = newValue + "+" + parameterStrings[i];
                        }
                        else
                        {
                            newValue = newValue + " +" + parameterStrings[i];
                        }
                    }
                    _externalProjectionParametersString = newValue;
                }
            }


            protected override bool CanReproject
            {
                get
                {
                    return
                        !InternalProjectionParametersString.Replace(" ", "")
                            .Equals(ExternalProjectionParametersString.Replace(" ", ""), StringComparison.Ordinal);
                }
            }


            protected override void OpenCore()
            {
                _sourceProj = _system.InitGisSystem(_internalProjectionParametersString);
                _destinationProj = _system.InitGisSystem(_externalProjectionParametersString);

                _isFromProjectionInDegree = _internalProjectionParametersString.Contains("+proj=longlat");
                _isToProjectionInDegree = _externalProjectionParametersString.Contains("+proj=longlat");
                base.OpenCore();
            }


            protected override Vertex[] ConvertToExternalProjectionCore(double[] x, double[] y)
            {
                var projUv = new ProjUv[x.Length];
                if (_isFromProjectionInDegree)
                {
                    for (var i = 0; i < x.Length; i++)
                    {
                        if (x[i] < _decimalDegreeBoundary.UpperLeftPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.UpperLeftPoint.X;
                        }
                        if (x[i] > _decimalDegreeBoundary.LowerRightPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.LowerRightPoint.X;
                        }
                        if (y[i] < _decimalDegreeBoundary.LowerRightPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.LowerRightPoint.Y;
                        }
                        if (y[i] > _decimalDegreeBoundary.UpperLeftPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.UpperLeftPoint.Y;
                        }

                        projUv[i] = new ProjUv();
                        projUv[i].U = x[i] * _radian;
                        projUv[i].V = y[i] * _radian;
                    }
                }
                else
                {
                    for (var i = 0; i < x.Length; i++)
                    {
                        projUv[i] = new ProjUv();
                        projUv[i].U = x[i];
                        projUv[i].V = y[i];
                    }
                }

                var errorNumber = _system.Pj_Transform(_sourceProj, _destinationProj, x.Length, 1, projUv, null);

                for (var i = 0; i < projUv.Length; i++)
                {
                    if (double.IsInfinity(projUv[i].U) || double.IsInfinity(projUv[i].V))
                    {
                        if (i > 0)
                        {
                            projUv[i].U = projUv[i - 1].U;
                            projUv[i].V = projUv[i - 1].V;
                        }
                        else
                        {
                            projUv[i].U = double.Epsilon;
                            projUv[i].V = double.Epsilon;
                        }
                    }
                }

                for (var k = 0; k < projUv.Length; k++)
                {
                    if (projUv[k].U == double.Epsilon || projUv[k].V == double.Epsilon)
                    {
                        for (var j = 0; j < projUv.Length; j++)
                        {
                            projUv[j].U = double.Epsilon;
                            projUv[j].V = double.Epsilon;
                        }
                        break;
                    }
                }

                var vertices = new Vertex[projUv.Length];
                if (errorNumber == 0)
                {
                    if (_isToProjectionInDegree)
                    {
                        for (var i = 0; i < projUv.Length; i++)
                        {
                            vertices[i].X = projUv[i].U / _radian;
                            vertices[i].Y = projUv[i].V / _radian;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < projUv.Length; i++)
                        {
                            vertices[i].X = projUv[i].U;
                            vertices[i].Y = projUv[i].V;
                        }
                    }
                }
                else
                {
                    ThrowException(errorNumber);
                }
                return vertices;
            }


            protected override Vertex[] ConvertToInternalProjectionCore(double[] x, double[] y)
            {
                var projUv = new ProjUv[x.Length];
                if (_isToProjectionInDegree)
                {
                    for (var i = 0; i < x.Length; i++)
                    {
                        if (x[i] < _decimalDegreeBoundary.UpperLeftPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.UpperLeftPoint.X;
                        }
                        if (x[i] > _decimalDegreeBoundary.LowerRightPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.LowerRightPoint.X;
                        }
                        if (y[i] < _decimalDegreeBoundary.LowerRightPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.LowerRightPoint.Y;
                        }
                        if (y[i] > _decimalDegreeBoundary.UpperLeftPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.UpperLeftPoint.Y;
                        }
                        projUv[i] = new ProjUv();
                        projUv[i].U = x[i] * _radian;
                        projUv[i].V = y[i] * _radian;
                    }
                }
                else
                {
                    for (var i = 0; i < x.Length; i++)
                    {
                        projUv[i] = new ProjUv();
                        projUv[i].U = x[i];
                        projUv[i].V = y[i];
                    }
                }

                var errorNumber = _system.Pj_Transform(_destinationProj, _sourceProj, x.Length, 1, projUv, null);

                var vertices = new Vertex[projUv.Length];
                if (errorNumber == 0)
                {
                    for (var i = 0; i < projUv.Length; i++)
                    {
                        if (double.IsInfinity(projUv[i].U) || double.IsInfinity(projUv[i].V))
                        {

                            if (i > 0)
                            {
                                projUv[i].U = projUv[i - 1].U;
                                projUv[i].V = projUv[i - 1].V;
                            }
                            else
                            {
                                projUv[i].U = 0;
                                projUv[i].V = 0;
                            }
                        }
                    }
                    if (_isFromProjectionInDegree)
                    {
                        for (var i = 0; i < projUv.Length; i++)
                        {
                            vertices[i].X = projUv[i].U / _radian;
                            vertices[i].Y = projUv[i].V / _radian;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < projUv.Length; i++)
                        {
                            vertices[i].X = projUv[i].U;
                            vertices[i].Y = projUv[i].V;
                        }
                    }
                }
                else
                {
                    ThrowException(errorNumber);
                }
                return vertices;
            }


            public static string GetEpsgParametersString(int srid)
            {
                return UnmanagedProj4Projection.GetEpsgParametersString(srid);
            }

            public static string GetEsriParametersString(int srid)
            {
                return UnmanagedProj4Projection.GetEsriParametersString(srid);
            }



            public static string GetWgs84ParametersString()
            {
                return UnmanagedProj4Projection.GetWgs84ParametersString();
            }

           

            public static string GetSphericalMercatorParametersString()
            {
                return UnmanagedProj4Projection.GetSphericalMercatorParametersString();
            }


            private static void ThrowException(int errorNumber)
            {
                switch (errorNumber)
                {
                    case -1:
                        throw new InvalidOperationException("no arguments in initialization list");
                    case -2:
                        throw new InvalidOperationException("no options found in 'init' file");
                    case -3:
                        throw new InvalidOperationException("no colon in init= string");
                    case -4:
                        throw new InvalidOperationException("projection not named");
                    case -5:
                        throw new InvalidOperationException("unknown projection id");
                    case -6:
                        throw new InvalidOperationException("effective eccentricity = 1.");
                    case -7:
                        throw new InvalidOperationException("unknown unit conversion id");
                    case -8:
                        throw new InvalidOperationException("invalid boolean param argument");
                    case -9:
                        throw new InvalidOperationException("unknown elliptical parameter name");
                    case -10:
                        throw new InvalidOperationException("reciprocal flattening (1/f) = 0");
                    case -11:
                        throw new InvalidOperationException("|radius reference latitude| > 90");
                    case -12:
                        throw new InvalidOperationException("squared eccentricity < 0");
                    case -13:
                        throw new InvalidOperationException("major axis or radius = 0 or not given");
                    case -14:
                        throw new InvalidOperationException("latitude or longitude exceeded limits");
                    case -15:
                        throw new InvalidOperationException("invalid x or y");
                    case -16:
                        throw new InvalidOperationException("improperly formed DMS value");
                    case -17:
                        throw new InvalidOperationException("non-convergent inverse meridinal dist");
                    case -18:
                        throw new InvalidOperationException("non-convergent inverse phi2");
                    case -19:
                        throw new InvalidOperationException("acos/asin: |arg| >1.+1e-14");
                    case -20:
                        throw new InvalidOperationException("tolerance condition error");
                    case -21:
                        throw new InvalidOperationException("conic lat_1 = -lat_2");
                    case -22:
                        throw new InvalidOperationException("lat_1 >= 90");
                    case -23:
                        throw new InvalidOperationException("lat_1 = 0");
                    case -24:
                        throw new InvalidOperationException("lat_ts >= 90");
                    case -25:
                        throw new InvalidOperationException("no distance between control points");
                    case -26:
                        throw new InvalidOperationException("projection not selected to be rotated");
                    case -27:
                        throw new InvalidOperationException("W <= 0 or M <= 0");
                    case -28:
                        throw new InvalidOperationException("lsat not in 1-5 range");
                    case -29:
                        throw new InvalidOperationException("path not in range");
                    case -30:
                        throw new InvalidOperationException("h <= 0");
                    case -31:
                        throw new InvalidOperationException("k <= 0");
                    case -32:
                        throw new InvalidOperationException("lat_0 = 0 or 90 or alpha = 90");
                    case -33:
                        throw new InvalidOperationException("lat_1=lat_2 or lat_1=0 or lat_2=90");
                    case -34:
                        throw new InvalidOperationException("elliptical usage required");
                    case -35:
                        throw new InvalidOperationException("invalid UTM zone number");
                    case -36:
                        throw new InvalidOperationException("arg(s) out of range for Tcheby eval");
                    case -37:
                        throw new InvalidOperationException("failed to find projection to be rotated");
                    case -38:
                        throw new InvalidOperationException("failed to load NAD27-83 correction file");
                    case -39:
                        throw new InvalidOperationException("both n & m must be spec'd and > 0");
                    case -40:
                        throw new InvalidOperationException("n <= 0, n > 1 or not specified");
                    case -41:
                        throw new InvalidOperationException("lat_1 or lat_2 not specified");
                    case -42:
                        throw new InvalidOperationException("|lat_1| == |lat_2|");
                    case -43:
                        throw new InvalidOperationException("lat_0 is pi/2 from mean lat");
                    case -44:
                        throw new InvalidOperationException("unparseable coordinate system definition");
                    case -45:
                        throw new InvalidOperationException("geocentric transformation missing z or ellps");
                    case -46:
                        throw new InvalidOperationException("unknown prime meridian conversion id");
                    default:
                        return;
                }
            }

            protected override GeographyUnit GetInternalGeographyUnitCore()
            {
                return GetGeographyUnit(InternalProjectionParametersString);
            }

            protected override GeographyUnit GetExternalGeographyUnitCore()
            {
                return GetGeographyUnit(ExternalProjectionParametersString);
            }

            private GeographyUnit GetGeographyUnit(string projectionString)
            {
                var geographyUnit = GeographyUnit.Unknown;
                if (!string.IsNullOrEmpty(projectionString))
                {
                    if (projectionString.Contains("units=m"))
                    {
                        geographyUnit = GeographyUnit.Meter;
                    }
                    else if (projectionString.Contains("to_meter=0.304") || projectionString.Contains("units=us-ft") ||
                             projectionString.Contains("units=ft"))
                    {
                        geographyUnit = GeographyUnit.Feet;
                    }
                    else if (projectionString.Contains("proj=longlat"))
                    {
                        geographyUnit = GeographyUnit.DecimalDegree;
                    }
                }
                return geographyUnit;
            }

            public static string ConvertEpsgToPrj(int srid)
            {
                return Proj4Projection.ConvertEpsgToPrj(srid);
            }

            public static string ConvertProj4ToPrj(string proj4String)
            {
                return Proj4Projection.ConvertProj4ToPrj(proj4String);
            }

            public static string ConvertPrjToProj4(string prjString)
            {
                return Proj4Projection.ConvertPrjToProj4(prjString);
            }
        }

        [Serializable]
        private class UnmanagedProj4Projection : BaseProjection, IDisposable
        {
            private static readonly object LockObject = new object();
            private readonly double _radian = Math.PI / 180;

            [NonSerialized]
            private Assembly _assembly;

            private RectangleShape _decimalDegreeBoundary;

            private string _externalProjectionParametersString;
            private IntPtr _externalProjectionPj;
            private string _internalProjectionParametersString;
            private IntPtr _internalProjectionPj;
            private bool _isFromProjectionInDegree;
            private bool _isToProjectionInDegree;

            [NonSerialized]
            private object _loadObject;

            [NonSerialized]
            private Type _type;


            public UnmanagedProj4Projection()
                : this(string.Empty, string.Empty)
            {
            }


            public UnmanagedProj4Projection(string internalProjectionParametersString,
                string externalProjectionParametersString)
            {
                InternalProjectionParametersString = internalProjectionParametersString;
                ExternalProjectionParametersString = externalProjectionParametersString;

                _decimalDegreeBoundary = new RectangleShape(-179.9, 89.9, 179.9, -89.9);
            }

            public UnmanagedProj4Projection(int internalEpsgSrid, int externalEpsgSrid)
                : this(GetEpsgParametersString(internalEpsgSrid), GetEpsgParametersString(externalEpsgSrid))
            {
            }


            public RectangleShape DecimalDegreeBoundary
            {
                get { return _decimalDegreeBoundary; }
                set { _decimalDegreeBoundary = value; }
            }


            public string InternalProjectionParametersString
            {
                get { return _internalProjectionParametersString; }
                set { _internalProjectionParametersString = value; }
            }


            public string ExternalProjectionParametersString
            {
                get { return _externalProjectionParametersString; }
                set { _externalProjectionParametersString = value; }
            }

            public void Dispose()
            {
                Close();
            }


            protected override void OpenCore()
            {
                Validators.CheckParameterIsNotNull(GetParameters(InternalProjectionParametersString),
                    "internalProjectionParameters");
                Validators.CheckParameterIsNotNull(GetParameters(ExternalProjectionParametersString),
                    "externalProjectionParameters");

                if (_assembly == null)
                {
                    var allPossibleFolders = GetAllPossibleFolders();
                    var assemblyName = GetProj4ExtensionAssemblyName();

                    var assemblyPath = string.Empty;
                    foreach (var candidateFolder in allPossibleFolders)
                    {
                        if (File.Exists(candidateFolder + assemblyName))
                        {
                            assemblyPath = candidateFolder + assemblyName;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(assemblyPath))
                    {
                        throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                            "Could not find {0} assembly.", assemblyName));
                    }
                    _assembly = Assembly.LoadFrom(assemblyPath);
                    if (IntPtr.Size == 4)
                    {
                        _type = _assembly.GetType("Proj4Extension.Proj4Extensionx86");
                        _loadObject = _assembly.CreateInstance("Proj4Extension.Proj4Extensionx86");
                    }
                    else
                    {
                        _type = _assembly.GetType("Proj4Extension.Proj4ExtensionX64");
                        _loadObject = _assembly.CreateInstance("Proj4Extension.Proj4ExtensionX64");
                    }
                }

                _externalProjectionPj = Init(ExternalProjectionParametersString);
                if (_externalProjectionPj == IntPtr.Zero)
                {
                    throw new ArgumentException(ExceptionDescription.ProjectionInitializationError,
                        "externalProjectionParameters");
                }

                _internalProjectionPj = Init(InternalProjectionParametersString);
                if (_internalProjectionPj == IntPtr.Zero)
                {
                    throw new ArgumentException(ExceptionDescription.ProjectionInitializationError,
                        "internalProjectionParameters");
                }

                _isFromProjectionInDegree = InternalProjectionParametersString.Contains("+proj=longlat");
                _isToProjectionInDegree = ExternalProjectionParametersString.Contains("+proj=longlat");

                base.OpenCore();
            }

            private string GetProj4ExtensionAssemblyName()
            {
                var assemblyName = string.Empty;
                if (IntPtr.Size == 4)
                {
                    assemblyName = "Proj4ExtensionX86.dll";
                }
                else
                {
                    assemblyName = "Proj4ExtensionX64.dll";
                }
                return assemblyName;
            }

            private string[] GetAllPossibleFolders()
            {
                var fullName = Assembly.GetExecutingAssembly().FullName;
                var version = fullName.Split(',')[1].Trim().Substring(8);
                var majorNumber = int.Parse(version.Split('.')[0], CultureInfo.InvariantCulture);
                var secondNumber = int.Parse(version.Split('.')[1], CultureInfo.InvariantCulture);

                var possilbeVersions = new List<string>();
                var possilbeFolders = new List<string>();

                for (var i = majorNumber; i >= 4; i--)
                {
                    if (secondNumber == 5)
                    {
                        possilbeVersions.Add(i.ToString(CultureInfo.InvariantCulture) + "." + "5");
                        secondNumber = 0;
                    }
                    else
                    {
                        secondNumber = 5;
                    }
                    possilbeVersions.Add(i.ToString(CultureInfo.InvariantCulture) + "." + "0");
                }


                /*foreach (var item in possilbeVersions)
                {
                    possilbeFolders.Add(Environment.SystemDirectory +
                                        string.Format(CultureInfo.InvariantCulture, "\\..\\SysWOW64\\GSuite {0}\\", item));
                }
                possilbeFolders.Add(Environment.SystemDirectory + "\\..\\SysWOW64\\");

                foreach (var item in possilbeVersions)
                {
                    possilbeFolders.Add(Environment.SystemDirectory +
                                        string.Format(CultureInfo.InvariantCulture, "\\GSuite {0}\\", item));
                }
                possilbeFolders.Add(Environment.SystemDirectory + "\\");*/

                return possilbeFolders.ToArray();
            }


            protected override void CloseCore()
            {
                if (_externalProjectionPj != IntPtr.Zero)
                {
                    Free(_externalProjectionPj);
                    _externalProjectionPj = IntPtr.Zero;
                }

                if (_internalProjectionPj != IntPtr.Zero)
                {
                    Free(_internalProjectionPj);
                    _internalProjectionPj = IntPtr.Zero;
                }

                base.CloseCore();
            }


            protected override Vertex[] ConvertToExternalProjectionCore(double[] x, double[] y)
            {
                Validators.CheckParameterIsNotNull(x, "x");
                Validators.CheckParameterIsNotNull(y, "y");

                var resultZ = new double[x.Length];
                var resultX = new double[x.Length];
                var resultY = new double[x.Length];

                if (_isFromProjectionInDegree)
                {
                    for (var i = 0; i < resultX.Length; i++)
                    {
                        if (x[i] < _decimalDegreeBoundary.UpperLeftPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.UpperLeftPoint.X;
                        }
                        if (x[i] > _decimalDegreeBoundary.LowerRightPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.LowerRightPoint.X;
                        }

                        resultX[i] = x[i] * _radian;
                    }
                    for (var i = 0; i < resultY.Length; i++)
                    {
                        if (y[i] < _decimalDegreeBoundary.LowerRightPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.LowerRightPoint.Y;
                        }
                        if (y[i] > _decimalDegreeBoundary.UpperLeftPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.UpperLeftPoint.Y;
                        }

                        resultY[i] = y[i] * _radian;
                    }
                }
                else
                {
                    for (var i = 0; i < resultX.Length; i++)
                    {
                        resultX[i] = x[i];
                    }
                    for (var i = 0; i < resultY.Length; i++)
                    {
                        resultY[i] = y[i];
                    }
                }

                Transform(_internalProjectionPj, _externalProjectionPj, resultX.Length, resultX, resultY, resultZ);

                var verteies = new Vertex[x.Length];
                if (_isToProjectionInDegree)
                {
                    for (var i = 0; i < verteies.Length; i++)
                    {
                        verteies[i] = new Vertex(resultX[i] / _radian, resultY[i] / _radian);
                    }
                }
                else
                {
                    for (var i = 0; i < verteies.Length; i++)
                    {
                        verteies[i] = new Vertex(resultX[i], resultY[i]);
                    }
                }
                return verteies;
            }


            protected override Vertex[] ConvertToInternalProjectionCore(double[] x, double[] y)
            {
                Validators.CheckParameterIsNotNull(x, "x");
                Validators.CheckParameterIsNotNull(y, "y");

                var resultZ = new double[x.Length];
                var resultX = new double[x.Length];
                var resultY = new double[x.Length];

                if (_isToProjectionInDegree)
                {
                    for (var i = 0; i < resultX.Length; i++)
                    {
                        if (x[i] < _decimalDegreeBoundary.UpperLeftPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.UpperLeftPoint.X;
                        }
                        if (x[i] > _decimalDegreeBoundary.LowerRightPoint.X)
                        {
                            x[i] = _decimalDegreeBoundary.LowerRightPoint.X;
                        }

                        resultX[i] = x[i] * _radian;
                    }
                    for (var i = 0; i < resultY.Length; i++)
                    {
                        if (y[i] < _decimalDegreeBoundary.LowerRightPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.LowerRightPoint.Y;
                        }
                        if (y[i] > _decimalDegreeBoundary.UpperLeftPoint.Y)
                        {
                            y[i] = _decimalDegreeBoundary.UpperLeftPoint.Y;
                        }

                        resultY[i] = y[i] * _radian;
                    }
                }
                else
                {
                    for (var i = 0; i < resultX.Length; i++)
                    {
                        resultX[i] = x[i];
                    }
                    for (var i = 0; i < resultY.Length; i++)
                    {
                        resultY[i] = y[i];
                    }
                }

                Transform(_externalProjectionPj, _internalProjectionPj, resultX.Length, resultX, resultY, resultZ);
                for (var i = 0; i < resultX.Length; i++)
                {
                    if (double.IsInfinity(resultX[i]) || double.IsInfinity(resultY[i]))
                    {
                        for (var j = 0; j < resultX.Length; j++)
                        {
                            resultX[j] = x[j];
                            resultY[j] = y[j];
                        }
                    }
                }

                var verteies = new Vertex[x.Length];
                if (_isFromProjectionInDegree)
                {
                    for (var i = 0; i < verteies.Length; i++)
                    {
                        verteies[i] = new Vertex(resultX[i] / _radian, resultY[i] / _radian);
                    }
                }
                else
                {
                    for (var i = 0; i < verteies.Length; i++)
                    {
                        verteies[i] = new Vertex(resultX[i], resultY[i]);
                    }
                }

                return verteies;
            }

            private static string[] GetParameters(string projectionParameter)
            {
                if (string.IsNullOrEmpty(projectionParameter))
                {
                    return null;
                }
                var result = projectionParameter.Split(new[] { "+", " " }, StringSplitOptions.RemoveEmptyEntries);
                return result;
            }


            ~UnmanagedProj4Projection()
            {
                Close();
            }

            public static string GetEpsgParametersString(int srid)
            {
                Validators.CheckValueIsBiggerThanZero(srid, "srid");

                var bytes = Resources.ESPG;
                return GetParametersString(srid.ToString(CultureInfo.InvariantCulture), bytes);
            }


            public static string GetEsriParametersString(int srid)
            {
                Validators.CheckValueIsBiggerThanZero(srid, "srid");

                var bytes = Resources.ESRI;
                return GetParametersString(srid.ToString(CultureInfo.InvariantCulture), bytes);


            }


            public static string GetSphericalMercatorParametersString()
            {
                string result = null;

                var bytes = Resources.ESRIExtra;
                result = GetParametersString("900913", bytes);

                return result;
            }

            public static string GetWgs84ParametersString()
            {
                string result = null;

                var bytes = Resources.ESPG;
                result = GetParametersString("4326", bytes);

                return result;
            }

            public static string GetLatLongParametersString()
            {
                return GetWgs84ParametersString();
            }

            public static string GetDecimalDegreesParametersString()
            {
                return GetWgs84ParametersString();
            }

            private IntPtr Init(string parameterString)
            {
                var value = IntPtr.Zero;

                lock (LockObject)
                {
                    var projectionParameters = GetParameters(parameterString);
                    var parameterTypes = new Type[2] { Type.GetType("System.Int32"), Type.GetType("System.String[]") };
                    var methodInfo = _type.GetMethod("Init", parameterTypes);
                    var parameters = new object[2] { projectionParameters.Length, projectionParameters };
                    value = (IntPtr)(methodInfo.Invoke(_loadObject, parameters));
                }

                return value;
            }

            private string Free(IntPtr projPj)
            {
                string value = null;
                lock (LockObject)
                {
                    var parameterTypes = new Type[1] { Type.GetType("System.IntPtr") };
                    var methodInfo = _type.GetMethod("Free", parameterTypes);
                    var parameters = new object[1] { projPj };
                    value = (string)(methodInfo.Invoke(_loadObject, parameters));
                }
                return value;
            }

            private int Transform(IntPtr srcCs, IntPtr destCs, long pointCount, double[] x, double[] y, double[] z)
            {
                var result = 0;
                lock (LockObject)
                {
                    var methodInfo = _type.GetMethod("Transform");
                    var parameters = new object[6] { srcCs, destCs, (int)pointCount, x, y, z };
                    result = (int)(methodInfo.Invoke(_loadObject, parameters));
                }
                return result;
            }

            private static string GetParametersString(string srid, byte[] fileBuffer)
            {
                var parametersString = string.Empty;

                using (var stream = new MemoryStream(fileBuffer))
                {
                    using (var gZip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        using (var reader = new StreamReader(gZip))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                var index = line.IndexOf(">", StringComparison.OrdinalIgnoreCase);
                                if (srid == line.Substring(1, index - 1))
                                {
                                    parametersString = line.Substring(index + 1).Replace("<>", "");
                                    parametersString = parametersString.Replace("+", " +").Trim();
                                    break;
                                }
                            }
                        }
                    }
                }
                return parametersString;
            }

            protected override GeographyUnit GetInternalGeographyUnitCore()
            {
                return GetGeographyUnit(InternalProjectionParametersString);
            }

            protected override GeographyUnit GetExternalGeographyUnitCore()
            {
                return GetGeographyUnit(ExternalProjectionParametersString);
            }

            private GeographyUnit GetGeographyUnit(string projectionString)
            {
                var geographyUnit = GeographyUnit.Unknown;
                if (!string.IsNullOrEmpty(projectionString))
                {
                    if (projectionString.Contains("units=m"))
                    {
                        geographyUnit = GeographyUnit.Meter;
                    }
                    else if (projectionString.Contains("to_meter=0.304") || projectionString.Contains("units=us-ft") ||
                             projectionString.Contains("units=ft"))
                    {
                        geographyUnit = GeographyUnit.Feet;
                    }
                    else if (projectionString.Contains("proj=longlat"))
                    {
                        geographyUnit = GeographyUnit.DecimalDegree;
                    }
                }
                return geographyUnit;
            }


            public static string ConvertEpsgToPrj(int srid)
            {
                return Proj4Projection.ConvertEpsgToPrj(srid);
            }

            public static string ConvertProj4ToPrj(string proj4String)
            {
                return Proj4Projection.ConvertProj4ToPrj(proj4String);
            }

            public static string ConvertPrjToProj4(string prjString)
            {
                return Proj4Projection.ConvertPrjToProj4(prjString);
            }
        }
    }

   
}