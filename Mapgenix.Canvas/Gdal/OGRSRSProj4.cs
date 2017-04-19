using System;
using System.Collections.Generic;
using System.Text;

namespace Mapgenix.Canvas
{
    internal partial class OgrSpatialReference
    {
        private const string Symbol = "NnEeSsWw";

        private static string[] _ogrPjEllps;

        private static List<OgrProj4Datum> _ogrPjDatums;

        internal static string[] OgrpjEllps
        {
            get
            {
                if (_ogrPjEllps == null)
                {
                    _ogrPjEllps = new[]
                    {
                        "MERIT", "a=6378137.0", "rf=298.257", "MERIT 1983",
                        "SGS85", "a=6378136.0", "rf=298.257", "Soviet Geodetic System 85",
                        "GRS80", "a=6378137.0", "rf=298.257222101", "GRS 1980(IUGG, 1980)",
                        "IAU76", "a=6378140.0", "rf=298.257", "IAU 1976",
                        "airy", "a=6377563.396", "b=6356256.910", "Airy 1830",
                        "APL4.9", "a=6378137.0.", "rf=298.25", "Appl. Physics. 1965",
                        "NWL9D", "a=6378145.0.", "rf=298.25", "Naval Weapons Lab., 1965",
                        "mod_airy", "a=6377340.189", "b=6356034.446", "Modified Airy",
                        "andrae", "a=6377104.43", "rf=300.0", "Andrae 1876 (Den., Iclnd.)",
                        "aust_SA", "a=6378160.0", "rf=298.25", "Australian Natl & S. Amer. 1969",
                        "GRS67", "a=6378160.0", "rf=298.2471674270", "GRS 67(IUGG 1967)",
                        "bessel", "a=6377397.155", "rf=299.1528128", "Bessel 1841",
                        "bess_nam", "a=6377483.865", "rf=299.1528128", "Bessel 1841 (Namibia)",
                        "clrk66", "a=6378206.4", "b=6356583.8", "Clarke 1866",
                        "clrk80", "a=6378249.145", "rf=293.4663", "Clarke 1880 mod.",
                        "CPM", "a=6375738.7", "rf=334.29", "Comm. des Poids et Mesures 1799",
                        "delmbr", "a=6376428.", "rf=311.5", "Delambre 1810 (Belgium)",
                        "engelis", "a=6378136.05", "rf=298.2566", "Engelis 1985",
                        "evrst30", "a=6377276.345", "rf=300.8017", "Everest 1830",
                        "evrst48", "a=6377304.063", "rf=300.8017", "Everest 1948",
                        "evrst56", "a=6377301.243", "rf=300.8017", "Everest 1956",
                        "evrst69", "a=6377295.664", "rf=300.8017", "Everest 1969",
                        "evrstSS", "a=6377298.556", "rf=300.8017", "Everest (Sabah & Sarawak)",
                        "fschr60", "a=6378166.", "rf=298.3", "Fischer (Mercury Datum) 1960",
                        "fschr60m", "a=6378155.", "rf=298.3", "Modified Fischer 1960",
                        "fschr68", "a=6378150.", "rf=298.3", "Fischer 1968",
                        "helmert", "a=6378200.", "rf=298.3", "Helmert 1906",
                        "hough", "a=6378270.0", "rf=297.", "Hough",
                        "intl", "a=6378388.0", "rf=297.", "International 1909 (Hayford)",
                        "krass", "a=6378245.0", "rf=298.3", "Krassovsky, 1942",
                        "kaula", "a=6378163.", "rf=298.24", "Kaula 1961",
                        "lerch", "a=6378139.", "rf=298.257", "Lerch 1979",
                        "mprts", "a=6397300.", "rf=191.", "Maupertius 1738",
                        "new_intl", "a=6378157.5", "b=6356772.2", "New International 1967",
                        "plessis", "a=6376523.", "b=6355863.", "Plessis 1817 (France)",
                        "SEasia", "a=6378155.0", "b=6356773.3205", "Southeast Asia",
                        "walbeck", "a=6376896.0", "b=6355834.8467", "Walbeck",
                        "WGS60", "a=6378165.0", "rf=298.3", "WGS 60",
                        "WGS66", "a=6378145.0", "rf=298.25", "WGS 66",
                        "WGS72", "a=6378135.0", "rf=298.26", "WGS 72",
                        "WGS84", "a=6378137.0", "rf=298.257223563", "WGS 84",
                        "sphere", "a=6370997.0", "b=6370997.0", "Normal Sphere (r=6370997)",
                        null, null, null, null
                    };
                }
                return _ogrPjEllps;
            }
        }

        internal static List<OgrProj4Datum> OgrpjDatums
        {
            get
            {
                if (_ogrPjDatums == null)
                {
                    _ogrPjDatums = new List<OgrProj4Datum>();
                    _ogrPjDatums.Add(new OgrProj4Datum
                    {
                        PjName = "GGRS87",
                        OgrName = "Greek_Geodetic_Reference_System_1987",
                        Epsg = 4121,
                        Gcs = 6121
                    });
                    _ogrPjDatums.Add(new OgrProj4Datum
                    {
                        PjName = "potsdam",
                        OgrName = "Deutsches_Hauptdreiecksnetz",
                        Epsg = 4314,
                        Gcs = 6314
                    });
                    _ogrPjDatums.Add(new OgrProj4Datum
                    {
                        PjName = "carthage",
                        OgrName = "Carthage",
                        Epsg = 4223,
                        Gcs = 6223
                    });
                    _ogrPjDatums.Add(new OgrProj4Datum
                    {
                        PjName = "hermannskogel",
                        OgrName = "Militar_Geographische_Institut",
                        Epsg = 4312,
                        Gcs = 6312
                    });
                    _ogrPjDatums.Add(new OgrProj4Datum {PjName = "ire65", OgrName = "TM65", Epsg = 4299, Gcs = 6299});
                    _ogrPjDatums.Add(new OgrProj4Datum
                    {
                        PjName = "nzgd49",
                        OgrName = "New_Zealand_Geodetic_Datum_1949",
                        Epsg = 4272,
                        Gcs = 6272
                    });
                    _ogrPjDatums.Add(new OgrProj4Datum
                    {
                        PjName = "OSGB36",
                        OgrName = "OSGB_1936",
                        Epsg = 4277,
                        Gcs = 6277
                    });
                }
                return _ogrPjDatums;
            }
        }

        private static string OgrGetProj4Datum(string datunName, int epsgDatum)
        {
            foreach (var item in OgrpjDatums)
            {
                if (item.Gcs == epsgDatum || string.Equals(item.OgrName, datunName, StringComparison.OrdinalIgnoreCase))
                    return item.PjName;
            }
            return null;
        }

        private string[] OsrProj4Tokenize(string proj4String)
        {
            var proj4StringArray = proj4String.Split(new[] {'+', '\t', '\n', ' '}, StringSplitOptions.RemoveEmptyEntries);


            for (var i = 0; i < proj4StringArray.Length; i++)
            {
                if (proj4StringArray[i].IndexOf("=") < 0 && i != proj4StringArray.Length - 1)
                    proj4StringArray[i] += "=yes";
            }

            return proj4StringArray;
        }

        private static double OSR_GDV(string[] source, string field, double defaultValue)
        {
            var value = FetchNameValue(source, field);

            if (string.IsNullOrEmpty(value) && string.Equals(field, "k", StringComparison.OrdinalIgnoreCase))
                value = FetchNameValue(source, "k_0");
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return DmsToDec(value);
        }

        internal void ImportFromProj4(string proj4String)
        {
            Clear();
            var formattedProj4String = proj4String.Replace((char) 10, ' ')
                .Replace((char) 13, ' ')
                .Replace((char) 9, ' ');

            var startIndex = formattedProj4String.IndexOf("init=epsg:");
            if (startIndex >= 0 && formattedProj4String.IndexOf("proj=") < 0)
            {
                var proj4SubString = formattedProj4String.Substring(startIndex + 10);

                if (ImportFromEpsg(GetIntOutOfString(proj4SubString)))
                    return;
            }
            var proj4Parameters = OsrProj4Tokenize(formattedProj4String);

            #region Extract the prime meridian, if there is one set.

            var pm = FetchNameValue(proj4Parameters, "pm");
            var fromGreenwich = 0.0;

            if (!string.IsNullOrEmpty(pm))
            {
                if (string.Equals(pm, "lisbon"))
                {
                    fromGreenwich = DmsToDec("9d07'54.862\"W");
                }
                else if (string.Equals(pm, "paris"))
                {
                    fromGreenwich = DmsToDec("2d20'14.025\"E");
                }
                else if (string.Equals(pm, "bogota"))
                {
                    fromGreenwich = DmsToDec("74d04'51.3\"W");
                }
                else if (string.Equals(pm, "madrid"))
                {
                    fromGreenwich = DmsToDec("3d41'16.48\"W");
                }
                else if (string.Equals(pm, "rome"))
                {
                    fromGreenwich = DmsToDec("12d27'8.4\"E");
                }
                else if (string.Equals(pm, "bern"))
                {
                    fromGreenwich = DmsToDec("7d26'22.5\"E");
                }
                else if (string.Equals(pm, "jakarta"))
                {
                    fromGreenwich = DmsToDec("106d48'27.79\"E");
                }
                else if (string.Equals(pm, "ferro"))
                {
                    fromGreenwich = DmsToDec("17d40'W");
                }
                else if (string.Equals(pm, "brussels"))
                {
                    fromGreenwich = DmsToDec("4d22'4.71\"E");
                }
                else if (string.Equals(pm, "stockholm"))
                {
                    fromGreenwich = DmsToDec("18d3'29.8\"E");
                }
                else if (string.Equals(pm, "athens"))
                {
                    fromGreenwich = DmsToDec("23d42'58.815\"E");
                }
                else if (string.Equals(pm, "oslo"))
                {
                    fromGreenwich = DmsToDec("10d43'22.5\"E");
                }
                else
                {
                    fromGreenwich = DmsToDec(pm);
                    pm = "unnamed";
                }
            }
            else
                pm = "Greenwich";

            #endregion

            #region Set Nodes

            var proj = FetchNameValue(proj4Parameters, "proj");

            if (string.IsNullOrEmpty(proj))
            {
                return;
            }
            if (string.Equals(proj, "longlat", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(proj, "latlong", StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (string.Equals(proj, "bonne", StringComparison.OrdinalIgnoreCase))
            {
                SetBonne(OSR_GDV(proj4Parameters, "lat_1", 0.0), OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0), OSR_GDV(proj4Parameters, "y_0", 0.0));
            }
            else if (string.Equals(proj, "cass"))
            {
                SetCs(OSR_GDV(proj4Parameters, "lat_0", 0.0), OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0), OSR_GDV(proj4Parameters, "y_0", 0.0));
            }
            else if (string.Equals(proj, "nzmg"))
            {
                SetNzmg(OSR_GDV(proj4Parameters, "lat_0", -41.0),
                    OSR_GDV(proj4Parameters, "lon_0", 173.0),
                    OSR_GDV(proj4Parameters, "x_0", 2510000.0),
                    OSR_GDV(proj4Parameters, "y_0", 6023150.0));
            }

            else if (string.Equals(proj, "cea"))
            {
                SetCea(OSR_GDV(proj4Parameters, "lat_ts", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "tmerc"))
            {
                SetTm(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "utm"))
            {
                SetUtm((int) OSR_GDV(proj4Parameters, "zone", 0.0),
                    (int) OSR_GDV(proj4Parameters, "south", 1.0));
            }

            else if (string.Equals(proj, "merc")
                     && OSR_GDV(proj4Parameters, "lat_ts", 1000.0) < 999.0)
            {
                SetMercator2Sp(OSR_GDV(proj4Parameters, "lat_ts", 0.0),
                    0.0,
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "merc"))
            {
                SetMercator(0.0,
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "stere")
                     && Math.Abs(OSR_GDV(proj4Parameters, "lat_0", 0.0) - 90) < 0.001)
            {
                SetPs(OSR_GDV(proj4Parameters, "lat_ts", 90.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "stere")
                     && Math.Abs(OSR_GDV(proj4Parameters, "lat_0", 0.0) + 90) < 0.001)
            {
                SetPs(OSR_GDV(proj4Parameters, "lat_ts", -90.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (proj.StartsWith("stere", StringComparison.OrdinalIgnoreCase)
                     && FetchNameValue(proj4Parameters, "k") != null)
            {
                SetOs(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "stere"))
            {
                SetStereographic(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    1.0,
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "eqc"))
            {
                if (OSR_GDV(proj4Parameters, "lat_ts", 0.0) != 0.0)
                    SetEquirectangular2(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                        OSR_GDV(proj4Parameters, "lon_0", 0.0),
                        OSR_GDV(proj4Parameters, "lat_ts", 0.0),
                        OSR_GDV(proj4Parameters, "x_0", 0.0),
                        OSR_GDV(proj4Parameters, "y_0", 0.0));
                else
                    SetEquirectangular(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                        OSR_GDV(proj4Parameters, "lon_0", 0.0),
                        OSR_GDV(proj4Parameters, "x_0", 0.0),
                        OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "gstmerc"))
            {
                SetGaussSchreiberTMercator(OSR_GDV(proj4Parameters, "lat_0", -21.116666667),
                    OSR_GDV(proj4Parameters, "lon_0", 55.53333333309),
                    OSR_GDV(proj4Parameters, "k_0", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 160000.000),
                    OSR_GDV(proj4Parameters, "y_0", 50000.000));
            }

            else if (string.Equals(proj, "gnom"))
            {
                SetGnomonic(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "ortho"))
            {
                SetOrthographic(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "laea"))
            {
                SetLaea(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "aeqd"))
            {
                SetAe(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "eqdc"))
            {
                SetEc(OSR_GDV(proj4Parameters, "lat_1", 0.0),
                    OSR_GDV(proj4Parameters, "lat_2", 0.0),
                    OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "mill"))
            {
                SetMc(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "moll"))
            {
                SetMollweide(OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "eck1") || string.Equals(proj, "eck2") || string.Equals(proj, "eck3") ||
                     string.Equals(proj, "eck4") || string.Equals(proj, "eck5") || string.Equals(proj, "eck6"))
            {
                SetEckert(proj[3] - '0',
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "poly"))
            {
                SetPolyconic(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "aea"))
            {
                SetAcea(OSR_GDV(proj4Parameters, "lat_1", 0.0),
                    OSR_GDV(proj4Parameters, "lat_2", 0.0),
                    OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "robin"))
            {
                SetRobinson(OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "vandg"))
            {
                SetVdg(OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "sinu"))
            {
                SetSinusoidal(OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "gall"))
            {
                SetGs(OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "goode"))
            {
                SetGh(OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "geos"))
            {
                SetGeos(OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "h", 35785831.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "lcc"))
            {
                if (OSR_GDV(proj4Parameters, "lat_0", 0.0)
                    == OSR_GDV(proj4Parameters, "lat_1", 0.0))
                {
                    SetLcc1Sp(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                        OSR_GDV(proj4Parameters, "lon_0", 0.0),
                        OSR_GDV(proj4Parameters, "k_0", 1.0),
                        OSR_GDV(proj4Parameters, "x_0", 0.0),
                        OSR_GDV(proj4Parameters, "y_0", 0.0));
                }
                else
                {
                    SetLcc(OSR_GDV(proj4Parameters, "lat_1", 0.0),
                        OSR_GDV(proj4Parameters, "lat_2", 0.0),
                        OSR_GDV(proj4Parameters, "lat_0", 0.0),
                        OSR_GDV(proj4Parameters, "lon_0", 0.0),
                        OSR_GDV(proj4Parameters, "x_0", 0.0),
                        OSR_GDV(proj4Parameters, "y_0", 0.0));
                }
            }

            else if (string.Equals(proj, "omerc"))
            {
                SetHom(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lonc", 0.0),
                    OSR_GDV(proj4Parameters, "alpha", 0.0),
                    0.0,
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "somerc"))
            {
                SetHom(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    90.0, 90.0,
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "krovak"))
            {
                SetKrovak(OSR_GDV(proj4Parameters, "lat_0", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "alpha", 0.0),
                    0.0,
                    OSR_GDV(proj4Parameters, "k", 1.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "iwm_p"))
            {
                SetIwmPolyconic(OSR_GDV(proj4Parameters, "lat_1", 0.0),
                    OSR_GDV(proj4Parameters, "lat_2", 0.0),
                    OSR_GDV(proj4Parameters, "lon_0", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "wag1"))
            {
                SetWagner(1, 0.0,
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "wag2"))
            {
                SetWagner(2, 0.0,
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "wag3"))
            {
                SetWagner(3,
                    OSR_GDV(proj4Parameters, "lat_ts", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "wag4"))
            {
                SetWagner(4, 0.0,
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "wag5"))
            {
                SetWagner(5, 0.0,
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "wag6"))
            {
                SetWagner(6, 0.0,
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "wag7"))
            {
                SetWagner(7, 0.0,
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }

            else if (string.Equals(proj, "tpeqd"))
            {
                SetTped(OSR_GDV(proj4Parameters, "lat_1", 0.0),
                    OSR_GDV(proj4Parameters, "lon_1", 0.0),
                    OSR_GDV(proj4Parameters, "lat_2", 0.0),
                    OSR_GDV(proj4Parameters, "lon_2", 0.0),
                    OSR_GDV(proj4Parameters, "x_0", 0.0),
                    OSR_GDV(proj4Parameters, "y_0", 0.0));
            }
            else
            {
                throw new NotSupportedException(string.Format("OGR_PROJ4", "Unsupported projection:{0}", proj));
            }

            #endregion

            #region Try to translate the datum

            var value = FetchNameValue(proj4Parameters, "datum");
            var isFullyDefined = false;
            if (string.IsNullOrEmpty(value))
            {
            }
            else if ((string.Equals(value, "NAD27", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(value, "NAD83", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(value, "WGS84", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(value, "WGS72", StringComparison.OrdinalIgnoreCase)) && fromGreenwich == 0.0)
            {
                SetWellKnownGeogCs(value);
                isFullyDefined = true;
            }
            else
            {
                foreach (var item in OgrpjDatums)
                {
                    if (string.Equals(value, item.PjName, StringComparison.OrdinalIgnoreCase))
                    {
                        var oGcs = new OgrSpatialReference();
                        try
                        {
                            oGcs.ImportFromEpsg(item.Epsg);
                            CopyGeogCsFrom(oGcs);
                        }
                        catch
                        {
                        }

                        isFullyDefined = true;
                        break;
                    }
                }
            }

            #endregion

            #region Set the ellipsoid information.

            double semiMajor, invFlattening, semiMinor;
            value = FetchNameValue(proj4Parameters, "ellps");

            if (!string.IsNullOrEmpty(value) && !isFullyDefined)
            {
                for (var i = 0; OgrpjEllps[i] != null; i += 4)
                {
                    if (!string.Equals(_ogrPjEllps[i], value, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!_ogrPjEllps[i + 1].StartsWith("a=", StringComparison.OrdinalIgnoreCase))
                        throw new FormatException(string.Format("Bad OGRPJEllps format : {0}", _ogrPjEllps[i + 1]));

                    semiMajor = GetDoubleOutOfString(_ogrPjEllps[i + 1].Substring(2, _ogrPjEllps[i + 1].Length - 2));
                    if (_ogrPjEllps[i + 2].StartsWith("rf=", StringComparison.OrdinalIgnoreCase))
                        invFlattening =
                            GetDoubleOutOfString(_ogrPjEllps[i + 2].Substring(3, _ogrPjEllps[i + 2].Length - 3));
                    else
                    {
                        if (!_ogrPjEllps[i + 2].StartsWith("b=", StringComparison.OrdinalIgnoreCase))
                            throw new FormatException(string.Format("Bad OGRPJEllps format : {0}", _ogrPjEllps[i + 2]));
                        semiMinor = GetDoubleOutOfString(_ogrPjEllps[i + 2].Substring(2, _ogrPjEllps[i + 2].Length - 2));
                        if (Math.Abs(semiMajor/semiMinor) - 1.0 < 0.0000000000001)
                            invFlattening = 0.0;
                        else
                            invFlattening = -1.0/(semiMinor/semiMajor - 1.0);
                    }
                    SetGeogCS(_ogrPjEllps[i + 3], "unknown", _ogrPjEllps[i], semiMajor, invFlattening, pm, fromGreenwich);
                    isFullyDefined = true;
                    break;
                }
            }
            if (!isFullyDefined)
            {
                semiMajor = OSR_GDV(proj4Parameters, "a", 0.0);
                if (semiMajor == 0.0)
                {
                    semiMajor = OSR_GDV(proj4Parameters, "R", 0.0);
                    if (semiMajor != 0.0)
                    {
                        semiMinor = -1.0;
                        invFlattening = 0.0;
                    }
                    else
                    {                        semiMajor = OgrCoreDefinitiaons.SrsWgs84Semimajor;
                        invFlattening = OgrCoreDefinitiaons.SrsWgs84Invflattening;
                        semiMinor = -1.0;
                    }
                }
                else
                {
                    semiMinor = OSR_GDV(proj4Parameters, "b", -1.0);
                    invFlattening = OSR_GDV(proj4Parameters, "rf", -1.0);
                }
                if (semiMinor == -1.0 && invFlattening == -1.0)
                {
                    throw new FormatException(string.Format("OGR_PROJ4:Can't find ellipse definition in {0}", proj));
                }
                if (invFlattening == -1.0)
                {
                    if (Math.Abs(semiMajor/semiMinor) - 1.0 < 0.0000000000001)
                        invFlattening = 0.0;
                    else
                        invFlattening = -1.0/(semiMinor/semiMajor - 1.0);
                }
                SetGeogCS("unnamed ellipse", "unknown", "unnamed", semiMajor, invFlattening, pm, fromGreenwich);
                isFullyDefined = true;
            }

            #endregion

            #region Handle TOWGS84 conversion.

            value = FetchNameValue(proj4Parameters, "towgs84");
            if (!string.IsNullOrEmpty(value))
            {
                var toWgs84 = value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                if (toWgs84.Length >= 7)
                {
                    SetTOWGS84(GetDoubleOutOfString(toWgs84[0]),
                        GetDoubleOutOfString(toWgs84[1]),
                        GetDoubleOutOfString(toWgs84[2]),
                        GetDoubleOutOfString(toWgs84[3]),
                        GetDoubleOutOfString(toWgs84[4]),
                        GetDoubleOutOfString(toWgs84[5]),
                        GetDoubleOutOfString(toWgs84[6]));
                }
                else if (toWgs84.Length >= 3)
                {
                    SetTOWGS84(GetDoubleOutOfString(toWgs84[0]), GetDoubleOutOfString(toWgs84[0]),
                        GetDoubleOutOfString(toWgs84[0]));
                }
            }

            #endregion

            #region Linear units translation

            if (IsProjected() || IsLocal())
            {
                value = FetchNameValue(proj4Parameters, "to_meter");
                if (!string.IsNullOrEmpty(value) && GetDoubleOutOfString(value) > 0)
                {
                    var dValue = GetDoubleOutOfString(value);

                    if (Math.Abs(dValue - GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlUsFootConv)) < 0.00000001)
                    {
                        SetLinearUnits(OgrCoreDefinitiaons.SrsUlUsFoot,
                            GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlUsFootConv));
                    }
                    else if (Math.Abs(dValue - GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlFootConv)) < 0.00000001)
                    {
                        SetLinearUnits(OgrCoreDefinitiaons.SrsUlFoot,
                            GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlFootConv));
                    }
                    else if (dValue == 1.0)
                    {
                        SetLinearUnits(OgrCoreDefinitiaons.SrsUlMeter, 1.0);
                    }
                    else
                        SetLinearUnits("unknown", GetDoubleOutOfString(value));
                }
                else if (!string.IsNullOrEmpty((value = FetchNameValue(proj4Parameters, "units"))))
                {
                    if (string.Equals(value, "meter") || string.Equals(value, "m"))
                        SetLinearUnits(OgrCoreDefinitiaons.SrsUlMeter, 1.0);
                    else if (string.Equals(value, "us-ft"))
                        SetLinearUnits(OgrCoreDefinitiaons.SrsUlUsFoot,
                            GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlUsFootConv));
                    else if (string.Equals(value, "ft"))
                        SetLinearUnits(OgrCoreDefinitiaons.SrsUlFoot,
                            GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlFootConv));
                    else if (string.Equals(value, "yd"))
                        SetLinearUnits(value, 0.9144);
                    else if (string.Equals(value, "us-yd"))
                        SetLinearUnits(value, 0.914401828803658);
                    else 
                        SetLinearUnits(value, 1.0);
                }
            }

            #endregion

           

            string unitName = null;
            if (GetLinearUnits(ref unitName) != 1.0 && IsProjected())
            {
                var projcsNode = GetAttrNode("PROJCS");
                foreach (var child in projcsNode.ChildNodes)
                {
                    if (!string.Equals(child.Value, "PARAMETER", StringComparison.OrdinalIgnoreCase) &&
                        child.ChildNodes.Count != 2)
                        continue;

                    var parmName = child.ChildNodes[0].Value;
                    if (IsLinearParameter(parmName))
                        SetNormProjParm(parmName, GetProjParm(parmName, 0.0));
                }
            }


            if (proj.IndexOf("wktext") != -1)
            {
                SetExtension(Node.Value, "PROJ4", proj);
            }
            Node.SetParentNode();
        }

        internal string ExportToProj4()
        {
            var proj4StringBuilder = new StringBuilder();
            var proj4String = string.Empty;
            var fromGreenwich = 0.0;

            var projectionName = GetAttrValue("PROJECTION");

            if (Node == null || Node.ChildNodes.Count == 0)
            {
                throw new ArgumentNullException("Node", "Node is not initialized correctly");
            }

            proj4String = GetExtension(Node.Value, "PROJ4", string.Empty);
            if (!string.IsNullOrEmpty(proj4String))
                return proj4String;

            var primemNode = GetAttrNode("PRIMEM");
            if (primemNode != null && primemNode.ChildNodes.Count >= 2)
            {
                fromGreenwich = GetDoubleOutOfString(primemNode.ChildNodes[1].Value);
            }

            #region projection definition

            if (projectionName == null && IsGeographic())
            {
                proj4StringBuilder.Append("+proj=longlat ");
            }
            else if (projectionName == null && !IsGeographic())
            {
                return proj4String;
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtBonne, StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat("+proj=bonne +lon_0={0:g} +lat_1={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtCylindricalEqualArea,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat("+proj=cea +lon_0={0:g} +lat_ts={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtCassiniSoldner,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat("+proj=cass +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtNewZealandMapGrid,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=nzmg +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtTransverseMercator,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtTransverseMercatorMi21,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtTransverseMercatorMi22,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtTransverseMercatorMi23,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtTransverseMercatorMi24,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtTransverseMercatorMi25,
                    StringComparison.OrdinalIgnoreCase))
            {
                bool isNorth;
                var zone = GetUtmZone(out isNorth);
                if (zone != 0)
                {
                    if (isNorth)
                    {
                        proj4StringBuilder.AppendFormat("+proj=utm +zone={0} ", zone);
                    }
                    else
                    {
                        proj4StringBuilder.AppendFormat("+proj=utm +zone={0} +south ", zone);
                    }
                }
                else
                {
                    proj4StringBuilder.AppendFormat(
                        "+proj=tmerc +lat_0={0:g} +lon_0={1:g} +k={2:g} +x_0={3:g} +y_0={4:g} ",
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
                }
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtMercator_1Sp,
                StringComparison.OrdinalIgnoreCase))
            {
                if (GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0) == 0.0)
                {
                    proj4StringBuilder.AppendFormat(
                        "+proj=merc +lon_0={0:g} +k={1:g} +x_0={2:g} +y_0={3:g} ",
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
                }
                else if (GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0) == 1.0)
                {
                    proj4StringBuilder.AppendFormat(
                        "+proj=merc +lon_0={0:g} +lat_ts={1:g} +x_0={2:g} +y_0={3:g} ",
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
                }
                else
                {
                    throw new FormatException(
                        "Mercator_1SP with scale != 1.0 and latitude of origin != 0, not supported by PROJ.4!");
                }
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtMercator_2Sp,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=merc +lon_0={0:g} +lat_ts={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtObliqueStereographic,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=sterea +lat_0={0:g} +lon_0={1:g} +k={2:g} +x_0={3:g} +y_0={4:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtStereographic,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=stere +lat_0={0:g} +lon_0={1:g} +k={2:g} +x_0={3:g} +y_0={4:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons.SrsPtPolarStereographic,
                StringComparison.OrdinalIgnoreCase))
            {
                if (
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0) >=
                    0.0)
                {
                    proj4StringBuilder.AppendFormat(
                        "+proj=stere +lat_0=90 +lat_ts={0:g} +lon_0={1:g} +k={2:g} +x_0={3:g} +y_0={4:g} ",
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
                            90.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian,
                            0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
                }
                else
                {
                    proj4StringBuilder.AppendFormat(
                        "+proj=stere +lat_0=-90 +lat_ts={0:g} +lon_0={1:g} +k={2:g} +x_0={3:g} +y_0={4:g} ",
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
                            -90.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian,
                            0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor, 1.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                        GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
                }
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons.SrsPtEquirectangular,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=eqc +lat_ts={0:g} +lat_0={1:g} +lon_0={2:g} +x_0={3:g} +y_0={4:g} ",
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1,
                        0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
                        0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpCentralMeridian,
                        0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons.SrsPtGaussschreibertmercator,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=gstmerc +lat_0={0:g} +lon_0={1:g} +k_0={2:g} +x_0={3:g} +y_0={4:g} ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
                        -21.116666667),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpCentralMeridian,
                        55.53333333309),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpScaleFactor,
                        1.0),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseEasting,
                        160000.000),
                    GetNormProjParm(OgrCoreDefinitiaons.SrsPpFalseNorthing,
                        50000.000));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons.SrsPtGnomonic,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=gnom +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons.SrsPtOrthographic,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=ortho +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpCentralMeridian,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpFalseEasting, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons
                    .SrsPtLambertAzimuthalEqualArea,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=laea +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpLatitudeOfOrigin, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpCentralMeridian,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpFalseEasting,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons.SrsPpFalseNorthing,
                        0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons
                    .SrsPtAzimuthalEquidistant,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=aeqd +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpLatitudeOfOrigin, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpCentralMeridian, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseEasting, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons
                    .SrsPtEquidistantConic,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=eqdc +lat_0={0:g} +lon_0={1:g} +lat_1={2:g} +lat_2={3:g} +x_0={4:g} +y_0={5:g} ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpLatitudeOfCenter, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpLongitudeOfCenter, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpStandardParallel1, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpStandardParallel2, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseEasting, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons
                    .SrsPtMillerCylindrical,
                StringComparison.OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=mill +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} +R_A ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpLatitudeOfOrigin,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpCentralMeridian,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseEasting, 0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseNorthing, 0.0));
            }
            else if (string.Equals(projectionName,
                OgrCoreDefinitiaons
                    .SrsPtMollweide,
                StringComparison
                    .OrdinalIgnoreCase))
            {
                proj4StringBuilder.AppendFormat(
                    "+proj=moll +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpCentralMeridian,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseEasting,
                        0.0),
                    GetNormProjParm(
                        OgrCoreDefinitiaons
                            .SrsPpFalseNorthing,
                        0.0));
            }
            else if (string.Equals(
                projectionName,
                OgrCoreDefinitiaons
                    .SrsPtEckertI,
                StringComparison
                    .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat(
                        "+proj=eck1 +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm(
                            OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                            0.0),
                        GetNormProjParm(
                            OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                            0.0),
                        GetNormProjParm(
                            OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                            0.0));
            }
            else if (
                string.Equals(
                    projectionName,
                    OgrCoreDefinitiaons
                        .SrsPtEckertIi,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat(
                        "+proj=eck2 +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm(
                            OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                            0.0),
                        GetNormProjParm(
                            OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                            0.0),
                        GetNormProjParm(
                            OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                            0.0));
            }
            else if (
                string.Equals(
                    projectionName,
                    OgrCoreDefinitiaons
                        .SrsPtEckertIii,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat(
                        "+proj=eck3 +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if (
                string.Equals(
                    projectionName,
                    OgrCoreDefinitiaons
                        .SrsPtEckertIv,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=eck4 +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if (
                string
                    .Equals(
                        projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtEckertV,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=eck5 +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtEckertVi,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=eck6 +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtPolyconic,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=poly +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfOrigin,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtAlbersConicEqualArea,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=aea +lat_1={0:g} +lat_2={1:g} +lat_0={2:g} +lon_0={3:g} +x_0={4:g} +y_0={5:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpStandardParallel1,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpStandardParallel2,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfOrigin,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtRobinson,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=robin +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtVandergrinten,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=vandg +lon_0={0:g} +x_0={1:g} +y_0={2:g} +R_A ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtSinusoidal,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=sinu +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLongitudeOfCenter,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtGallStereographic,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=gall +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtGoodeHomolosine,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=goode +lon_0={0:g} +x_0={1:g} +y_0={2:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtGeostationarySatellite,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=geos +lon_0={0:g} +h={1:g} +x_0={2:g} +y_0={3:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpSatelliteHeight,
                                35785831.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtLambertConformalConic_2Sp,
                        StringComparison
                            .OrdinalIgnoreCase) ||
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtLambertConformalConic_2SpBelgium,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=lcc +lat_1={0:g} +lat_2={1:g} +lat_0={2:g} +lon_0={3:g} +x_0={4:g} +y_0={5:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpStandardParallel1,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpStandardParallel2,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfOrigin,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtLambertConformalConic_1Sp,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=lcc +lat_1={0:g} +lat_0={1:g} +lon_0={2:g} +k_0={3:g} +x_0={4:g} +y_0={5:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfOrigin,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfOrigin,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpScaleFactor,
                                1.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtHotineObliqueMercator,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                if
                    (
                    Math
                        .Abs
                        (GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpAzimuth,
                                0.0) -
                         90.0) <
                    0.0001 &&
                    Math
                        .Abs
                        (GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpRectifiedGridAngle,
                                0.0) -
                         90.0) <
                    0.0001)
                {
                    proj4StringBuilder
                        .AppendFormat
                        ("+proj=somerc +lat_0={0:g} +lon_0={1:g} +k_0={2:g} +x_0={3:g} +y_0={4:g} ",
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpLatitudeOfOrigin,
                                    0.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpCentralMeridian,
                                    0.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpScaleFactor,
                                    1.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpFalseEasting,
                                    0.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpFalseNorthing,
                                    0.0));
                }
                else
                {
                    proj4StringBuilder
                        .AppendFormat
                        ("+proj=omerc +lat_0={0:g} +lonc={1:g} +alpha={2:g} +k={3:g} +x_0={4:g} +y_0={5:g} ",
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpLatitudeOfOrigin,
                                    0.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpCentralMeridian,
                                    0.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpAzimuth,
                                    0.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpScaleFactor,
                                    1.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpFalseEasting,
                                    0.0),
                            GetNormProjParm
                                (OgrCoreDefinitiaons
                                    .SrsPpFalseNorthing,
                                    0.0));
                }
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtHotineObliqueMercatorTwoPointNaturalOrigin,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=omerc +lat_0={0:g} +lon_1={1:g} +lat_1={2:g} +lon_2={3:g} +lat_2={4:g} +k={5:g} +x_0={6:g} +y_0={7:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfOrigin,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfPoint1,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLongitudeOfPoint1,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfPoint2,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLongitudeOfPoint2,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpScaleFactor,
                                1.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtKrovak,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=krovak +lat_0={0:g} +lon_0={1:g} +alpha={2:g} +k={3:g} +x_0={4:g} +y_0={5:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfCenter,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLongitudeOfCenter,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpAzimuth,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpScaleFactor,
                                1.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtTwoPointEquidistant,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=tpeqd +lat_1={0:g} +lon_1={1:g} +lat_2={2:g} +lon_2={3:g} +x_0={4:g} +y_0={5:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOf_1StPoint,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLongitudeOf_1StPoint,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOf_2NdPoint,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLongitudeOf_2NdPoint,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtImwPolyconic,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=iwm_p +lat_1={0:g} +lat_2={1:g} +lon_0={2:g} +x_0={3:g} +y_0={4:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOf_1StPoint,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOf_2NdPoint,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtWagnerI,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=wag1 +x_0={0:g} +y_0={1:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtWagnerIi,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=wag2 +x_0={0:g} +y_0={1:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtWagnerIii,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=wag3 +x_0={0:g} +y_0={1:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtWagnerIv,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=wag4 +x_0={0:g} +y_0={1:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtWagnerV,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=wag5 +x_0={0:g} +y_0={1:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtWagnerVi,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=wag6 +x_0={0:g} +y_0={1:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtWagnerVii,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=wag7 +x_0={0:g} +y_0={1:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else if
                (
                string
                    .Equals
                    (projectionName,
                        OgrCoreDefinitiaons
                            .SrsPtSwissObliqueCylindrical,
                        StringComparison
                            .OrdinalIgnoreCase))
            {
                proj4StringBuilder
                    .AppendFormat
                    ("+proj=somerc +lat_0={0:g} +lon_0={1:g} +x_0={2:g} +y_0={3:g} ",
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpLatitudeOfOrigin,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpCentralMeridian,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseEasting,
                                0.0),
                        GetNormProjParm
                            (OgrCoreDefinitiaons
                                .SrsPpFalseNorthing,
                                0.0));
            }
            else
            {
                throw
                    new NotSupportedException
                        (string
                            .Format
                            ("No translation for {0} to PROJ.4 format is known.",
                                projectionName));
            }

            #endregion

            #region Datum, handling earth model

            var semiMajor = GetSemiMajor();
            var invFlattening = GetInvFlattening();
            var proj4Ellipse = string.Empty;

            var datumNodeName = GetAttrValue("DATUM");
            if (Math.Abs(semiMajor - 6378249.145) < 0.01 && Math.Abs(invFlattening - 293.465) < 0.0001)
            {
                proj4Ellipse = "clrk80"; /* Clark 1880 */
            }
            else if (Math.Abs(semiMajor - 6378245.0) < 0.01 && Math.Abs(invFlattening - 298.3) < 0.0001)
            {
                proj4Ellipse = "krass"; /* Krassovsky */
            }
            else if (Math.Abs(semiMajor - 6378388.0) < 0.01 && Math.Abs(invFlattening - 297.0) < 0.0001)
            {
                proj4Ellipse = "intl"; /* International 1924 */
            }
            else if (Math.Abs(semiMajor - 6378160.0) < 0.01 && Math.Abs(invFlattening - 298.25) < 0.0001)
            {
                proj4Ellipse = "aust_SA"; /* Australian */
            }
            else if (Math.Abs(semiMajor - 6377397.155) < 0.01 && Math.Abs(invFlattening - 299.1528128) < 0.0001)
            {
                proj4Ellipse = "bessel"; /* Bessel 1841 */
            }
            else if (Math.Abs(semiMajor - 6377483.865) < 0.01 && Math.Abs(invFlattening - 299.1528128) < 0.0001)
            {
                proj4Ellipse = "bess_nam"; /* Bessel 1841 (Namibia / Schwarzeck)*/
            }
            else if (Math.Abs(semiMajor - 6378160.0) < 0.01 && Math.Abs(invFlattening - 298.247167427) < 0.0001)
            {
                proj4Ellipse = "GRS67"; /* GRS 1967 */
            }
            else if (Math.Abs(semiMajor - 6378137) < 0.01 && Math.Abs(invFlattening - 298.257222101) < 0.000001)
            {
                proj4Ellipse = "GRS80"; /* GRS 1980 */
            }
            else if (Math.Abs(semiMajor - 6378206.4) < 0.01 && Math.Abs(invFlattening - 294.9786982) < 0.0001)
            {
                proj4Ellipse = "clrk66"; /* Clarke 1866 */
            }
            else if (Math.Abs(semiMajor - 6377340.189) < 0.01 && Math.Abs(invFlattening - 299.3249646) < 0.0001)
            {
                proj4Ellipse = "mod_airy"; /* Modified Airy */
            }
            else if (Math.Abs(semiMajor - 6377563.396) < 0.01 && Math.Abs(invFlattening - 299.3249646) < 0.0001)
            {
                proj4Ellipse = "airy"; /* Airy */
            }
            else if (Math.Abs(semiMajor - 6378200) < 0.01 && Math.Abs(invFlattening - 298.3) < 0.0001)
            {
                proj4Ellipse = "helmert"; /* Helmert 1906 */
            }
            else if (Math.Abs(semiMajor - 6378155) < 0.01 && Math.Abs(invFlattening - 298.3) < 0.0001)
            {
                proj4Ellipse = "fschr60m"; /* Modified Fischer 1960 */
            }
            else if (Math.Abs(semiMajor - 6377298.556) < 0.01 && Math.Abs(invFlattening - 300.8017) < 0.0001)
            {
                proj4Ellipse = "evrstSS"; /* Everest (Sabah & Sarawak) */
            }
            else if (Math.Abs(semiMajor - 6378165.0) < 0.01 && Math.Abs(invFlattening - 298.3) < 0.0001)
            {
                proj4Ellipse = "WGS60";
            }
            else if (Math.Abs(semiMajor - 6378145.0) < 0.01 && Math.Abs(invFlattening - 298.25) < 0.0001)
            {
                proj4Ellipse = "WGS66";
            }
            else if (Math.Abs(semiMajor - 6378135.0) < 0.01 && Math.Abs(invFlattening - 298.26) < 0.0001)
            {
                proj4Ellipse = "WGS72";
            }
            else if (Math.Abs(semiMajor - 6378137.0) < 0.01 && Math.Abs(invFlattening - 298.257223563) < 0.000001)
            {
                proj4Ellipse = "WGS84";
            }
            else if (string.Equals(datumNodeName, "North_American_Datum_1927", StringComparison.OrdinalIgnoreCase))
            {
                //        proj4Ellipse = "clrk66:+datum=nad27"; /* NAD 27 */
                proj4Ellipse = "clrk66";
            }
            else if (string.Equals(datumNodeName, "North_American_Datum_1983", StringComparison.OrdinalIgnoreCase))
            {
                //        proj4Ellipse = "GRS80:+datum=nad83";       /* NAD 83 */
                proj4Ellipse = "GRS80";
            }

            if (string.IsNullOrEmpty(proj4Ellipse))
                proj4StringBuilder.AppendFormat("+a={0:g} +b={1:g} ", GetSemiMajor(), GetSemiMinor());
            else
                proj4StringBuilder.AppendFormat("+ellps={0:g} ", proj4Ellipse);

            #endregion

            #region translate datum

            var proj4Datum = string.Empty;

            var toWgs84Node = GetAttrNode("TOWGS84");
            var epsgDatum = -1;
            var epsgGeogCs = -1;

            var authorityName = GetAuthorityName("DATUM");
            if (!string.IsNullOrEmpty(authorityName) &&
                string.Equals(authorityName, "EPSG", StringComparison.OrdinalIgnoreCase))
                epsgDatum = int.Parse(GetAuthorityCode("DATUM"));

            var geogCsAuthorityName = GetAuthorityName("GEOGCS");
            if (!string.IsNullOrEmpty(geogCsAuthorityName) &&
                string.Equals(geogCsAuthorityName, "EPSG", StringComparison.OrdinalIgnoreCase))
                epsgGeogCs = int.Parse(GetAuthorityCode("GEOGCS"));

            if (string.Equals(datumNodeName, OgrCoreDefinitiaons.SrsDnNad27, StringComparison.OrdinalIgnoreCase) ||
                epsgDatum == 6267)
                proj4Datum = "+datum=NAD27";
            else if (string.Equals(datumNodeName, OgrCoreDefinitiaons.SrsDnNad83, StringComparison.OrdinalIgnoreCase) ||
                     epsgDatum == 6269)
                proj4Datum = "+datum=NAD83";
            else if (
                string.Equals(datumNodeName, OgrCoreDefinitiaons.SrsDnWgs84, StringComparison.OrdinalIgnoreCase) ||
                epsgDatum == 6326)
                proj4Datum = "+datum=WGS84";
            else if (OgrGetProj4Datum(datumNodeName, epsgDatum) != null)
            {
                proj4Datum = OgrGetProj4Datum(datumNodeName, epsgDatum);
                proj4StringBuilder.Append("+datum=");
            }
            else if (toWgs84Node != null)
            {
                var overflowed = false;
                for (var i = 0; i < toWgs84Node.ChildNodes.Count && !overflowed; i++)
                {
                    if (toWgs84Node.ChildNodes[i].Value.Length > 24)
                        overflowed = true;
                }

                if (!overflowed && toWgs84Node.ChildNodes.Count > 2 &&
                    (toWgs84Node.ChildNodes.Count < 6 ||
                     (string.IsNullOrEmpty(toWgs84Node.ChildNodes[3].Value) &&
                      string.IsNullOrEmpty(toWgs84Node.ChildNodes[4].Value) &&
                      string.IsNullOrEmpty(toWgs84Node.ChildNodes[5].Value) &&
                      string.IsNullOrEmpty(toWgs84Node.ChildNodes[6].Value))))
                {
                    proj4Datum = string.Format("+towgs84={0},{1},{2}", toWgs84Node.ChildNodes[0].Value,
                        toWgs84Node.ChildNodes[1].Value, toWgs84Node.ChildNodes[2].Value);
                }
                else if (!overflowed && toWgs84Node.ChildNodes.Count > 6)
                {
                    proj4Datum = string.Format("+towgs84={0},{1},{2},{3},{4},{5},{6}",
                        toWgs84Node.ChildNodes[0].Value,
                        toWgs84Node.ChildNodes[1].Value,
                        toWgs84Node.ChildNodes[2].Value,
                        toWgs84Node.ChildNodes[3].Value,
                        toWgs84Node.ChildNodes[4].Value,
                        toWgs84Node.ChildNodes[5].Value,
                        toWgs84Node.ChildNodes[6].Value);
                }
            }
            else if (epsgGeogCs != -1)
            {
                var padTransform = new double[7];
                if (EpsgGetWgs84Transform(epsgGeogCs, padTransform))
                {
                    proj4Datum = string.Format("+towgs84={0},{1},{2},{3},{4},{5},{6}",
                        padTransform[0],
                        padTransform[1],
                        padTransform[2],
                        padTransform[3],
                        padTransform[4],
                        padTransform[5],
                        padTransform[6]);
                }
            }
            if (!string.IsNullOrEmpty(proj4Datum))
            {
                proj4StringBuilder.Append(proj4Datum + " ");
            }

            #endregion

           

            var code = 0;
            var pmValue = string.Empty;
            if (primemNode != null && primemNode.ChildNodes.Count >= 2 &&
                GetDoubleOutOfString(primemNode.ChildNodes[1].Value) != 0.0)
            {
                var authorityNameForPrimeMeridian = GetAuthorityName("PRIMEM");

                if (!string.IsNullOrEmpty(authorityNameForPrimeMeridian) &&
                    string.Equals(authorityNameForPrimeMeridian, "EPSG"))
                    code = GetIntOutOfString(GetAuthorityCode("PRIMEM"));
                switch (code)
                {
                    case 8902:
                        pmValue = "lisbon";
                        break;
                    case 8903:
                        pmValue = "paris";
                        break;
                    case 8904:
                        pmValue = "bogota";
                        break;
                    case 8905:
                        pmValue = "madrid";
                        break;

                    case 8906:
                        pmValue = "rome";
                        break;

                    case 8907:
                        pmValue = "bern";
                        break;

                    case 8908:
                        pmValue = "jakarta";
                        break;

                    case 8909:
                        pmValue = "ferro";
                        break;

                    case 8910:
                        pmValue = "brussels";
                        break;

                    case 8911:
                        pmValue = "stockholm";
                        break;

                    case 8912:
                        pmValue = "athens";
                        break;

                    case 8913:
                        pmValue = "oslo";
                        break;
                    default:
                        pmValue = fromGreenwich.ToString();
                        break;
                }
                proj4StringBuilder.AppendFormat("+pm={0} ", pmValue);
            }

           

            #region Handle linear units.

            var proj4Units = string.Empty;
            string linearUnits = null;
            var linearConv = GetLinearUnits(ref linearUnits);
            proj4String = proj4StringBuilder.ToString();

            if (proj4String.IndexOf("longlat") >= 0)
                proj4Units = null;

            else if (linearConv == 1.0)
                proj4Units = "m";

            else if (linearConv == 1000.0)
                proj4Units = "km";

            else if (linearConv == 0.0254)
                proj4Units = "in";

            else if (string.Equals(linearUnits, OgrCoreDefinitiaons.SrsUlFoot) ||
                     Math.Abs(linearConv - GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlFootConv)) < 0.000000001)
                proj4Units = "ft";

            else if (string.Equals(linearUnits, "IYARD") || linearConv == 0.9144)
                proj4Units = "yd";

            else if (linearConv == 0.001)
                proj4Units = "mm";

            else if (linearConv == 0.01)
                proj4Units = "cm";

            else if (string.Equals(linearUnits, OgrCoreDefinitiaons.SrsUlUsFoot) ||
                     Math.Abs(linearConv - GetDoubleOutOfString(OgrCoreDefinitiaons.SrsUlUsFootConv)) < 0.00000001)
                proj4Units = "us-ft";

            else if (string.Equals(linearUnits, OgrCoreDefinitiaons.SrsUlNauticalMile))
                proj4Units = "kmi";

            else if (string.Equals(linearUnits, "Mile") || string.Equals(linearUnits, "IMILE"))
                proj4Units = "mi";

            else
                proj4StringBuilder.AppendFormat("+to_meter={0:g} ", linearConv);
            if (!string.IsNullOrEmpty(proj4Units))
                proj4StringBuilder.AppendFormat("+units={0} ", proj4Units);

            #endregion

            proj4StringBuilder.Append("+no_defs ");
            proj4String = proj4StringBuilder.ToString();
            return proj4String;
        }

        private static string FetchNameValue(string[] source, string name)
        {
            foreach (var item in source)
            {
                var spliterIndex = name.Length;
                var itemCharArray = item.ToCharArray();
                if (item.StartsWith(name, StringComparison.OrdinalIgnoreCase) &&
                    (itemCharArray[spliterIndex] == '=' || itemCharArray[spliterIndex] == ':'))
                {
                    return item.Substring(spliterIndex + 1, item.Length - spliterIndex - 1);
                }
            }
            return null;
        }

        private static double DmsToDec(string value)
        {
            double returnValue;
            char sign;
            if (value[0].Equals('-') || value[0].Equals('+'))
            {
                sign = value[0];
                value = value.Substring(1);
            }
            else
                sign = '+';
            var degreeIndex = value.IndexOf("d", StringComparison.OrdinalIgnoreCase);
            var rIndex = value.IndexOf("r", StringComparison.OrdinalIgnoreCase);
            var minuteIndex = value.IndexOf('\'');
            var secondIndex = value.IndexOf('"');
            var directionIndex = secondIndex > 0 ? secondIndex + 1 : value.Length - 1;

            if (degreeIndex < 0 && rIndex < 0 && minuteIndex < 0 && secondIndex < 0)
            {
                return GetDoubleOutOfString(value)*(sign == '+' ? 1 : -1);
            }
            if (rIndex > 0 && minuteIndex < 0 && degreeIndex < 0 && secondIndex < 0)
            {
                returnValue = GetDoubleOutOfString(value.Substring(0, rIndex + 1));
                return Symbol.IndexOf(value[rIndex + 1]) > 3 ? -returnValue : returnValue;
            }
            double degrees = 0, minutes = 0, seconds = 0;
            degrees = GetDoubleOutOfString(value.Substring(0, degreeIndex));
            if (minuteIndex > 0)
                minutes = GetDoubleOutOfString(value.Substring(degreeIndex + 1, minuteIndex - degreeIndex - 1));
            if (secondIndex > 0)
                seconds = GetDoubleOutOfString(value.Substring(minuteIndex + 1, secondIndex - minuteIndex - 1));

            returnValue = degrees + minutes/60 + seconds/3600;

            if (directionIndex > 0)
                return Symbol.IndexOf(value[directionIndex]) > 3 ? -returnValue : returnValue;
            return returnValue;
        }
    }

    internal struct OgrProj4Datum
    {
        internal string PjName;

        internal string OgrName;

        internal int Epsg;

        internal int Gcs;
    }
}