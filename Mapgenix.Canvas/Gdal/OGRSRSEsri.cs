using System;

namespace Mapgenix.Canvas
{
    internal partial class OgrSpatialReference
    {
        #region UsgsEsriZones

        internal static int[] UsgsEsriZones =
        {
            101, 3101,
            102, 3126,
            201, 3151,
            202, 3176,
            203, 3201,
            301, 3226,
            302, 3251,
            401, 3276,
            402, 3301,
            403, 3326,
            404, 3351,
            405, 3376,
            406, 3401,
            407, 3426,
            501, 3451,
            502, 3476,
            503, 3501,
            600, 3526,
            700, 3551,
            901, 3601,
            902, 3626,
            903, 3576,
            1001, 3651,
            1002, 3676,
            1101, 3701,
            1102, 3726,
            1103, 3751,
            1201, 3776,
            1202, 3801,
            1301, 3826,
            1302, 3851,
            1401, 3876,
            1402, 3901,
            1501, 3926,
            1502, 3951,
            1601, 3976,
            1602, 4001,
            1701, 4026,
            1702, 4051,
            1703, 6426,
            1801, 4076,
            1802, 4101,
            1900, 4126,
            2001, 4151,
            2002, 4176,
            2101, 4201,
            2102, 4226,
            2103, 4251,
            2111, 6351,
            2112, 6376,
            2113, 6401,
            2201, 4276,
            2202, 4301,
            2203, 4326,
            2301, 4351,
            2302, 4376,
            2401, 4401,
            2402, 4426,
            2403, 4451,
            2500, 0,
            2501, 4476,
            2502, 4501,
            2503, 4526,
            2600, 0,
            2601, 4551,
            2602, 4576,
            2701, 4601,
            2702, 4626,
            2703, 4651,
            2800, 4676,
            2900, 4701,
            3001, 4726,
            3002, 4751,
            3003, 4776,
            3101, 4801,
            3102, 4826,
            3103, 4851,
            3104, 4876,
            3200, 4901,
            3301, 4926,
            3302, 4951,
            3401, 4976,
            3402, 5001,
            3501, 5026,
            3502, 5051,
            3601, 5076,
            3602, 5101,
            3701, 5126,
            3702, 5151,
            3800, 5176,
            3900, 0,
            3901, 5201,
            3902, 5226,
            4001, 5251,
            4002, 5276,
            4100, 5301,
            4201, 5326,
            4202, 5351,
            4203, 5376,
            4204, 5401,
            4205, 5426,
            4301, 5451,
            4302, 5476,
            4303, 5501,
            4400, 5526,
            4501, 5551,
            4502, 5576,
            4601, 5601,
            4602, 5626,
            4701, 5651,
            4702, 5676,
            4801, 5701,
            4802, 5726,
            4803, 5751,
            4901, 5776,
            4902, 5801,
            4903, 5826,
            4904, 5851,
            5001, 6101,
            5002, 6126,
            5003, 6151,
            5004, 6176,
            5005, 6201,
            5006, 6226,
            5007, 6251,
            5008, 6276,
            5009, 6301,
            5010, 6326,
            5101, 5876,
            5102, 5901,
            5103, 5926,
            5104, 5951,
            5105, 5976,
            5201, 6001,
            5200, 6026,
            5200, 6076,
            5201, 6051,
            5202, 6051,
            5300, 0,
            5400, 0
        };

        #endregion

        private static void MorphNameToEsri(ref string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            var nameArray = name.ToCharArray();
            for (var i = 0; i < nameArray.Length; i++)
            {
                if (!char.IsLetterOrDigit(nameArray[i]) && nameArray[i] != '+')
                {
                    nameArray[i] = '_';
                }
            }

            var j = 0;
            for (var i = 1; i < nameArray.Length; i++)
            {
                if (nameArray[j] == '_' && nameArray[i] == '_')
                    continue;
                nameArray[++j] = nameArray[i];
            }
            if (nameArray[j] == '_')
                name = new string(nameArray, 0, j).Trim('_');
            else
                name = new string(nameArray, 0, j + 1).Trim('_');
        }

        private static void InitDatumMappingTable()
        {
            if (DatumMapping != null)
                return;
            var datumMappingTable = CsvFileHelper.GetMappingTable("gdal_datum.csv",
                new[] {"DATUM_CODE", "ESRI_DATUM_NAME", "DATUM_NAME"});
            if (datumMappingTable == null || datumMappingTable.Length <= 3)
                DatumMapping = DefaultDatumMapping;
            else
            {
                DatumMapping = datumMappingTable;
                for (var i = 2; DatumMapping[i] != null; i += 3)
                {
                    if (!string.IsNullOrEmpty(DatumMapping[i - 1]) && DatumMapping[i - 1].StartsWith("\""))
                    {
                        DatumMapping[i - 1] = DatumMapping[i - 1].Trim('"');
                    }
                    if (!string.IsNullOrEmpty(DatumMapping[i]))
                    {
                        OgrepsgDatumNameMassage(ref DatumMapping[i]);
                    }
                }
            }
        }


        private static string RemapSpheroidName(string name)
        {
            if (name.CompareTo("WGS 84") == 0)
                return "WGS 1984";
            if (name.CompareTo("WGS 72") == 0)
                return "WGS 1972";
            return name;
        }

        internal void MorphToEsri()
        {
            Fixup();

            StripCTParms();

            if (Node == null)
                return;

            var projection = GetAttrValue("PROJECTION");

            if (!string.IsNullOrEmpty(projection) &&
                string.Equals(projection, OgrCoreDefinitiaons.SrsPtHotineObliqueMercator) &&
                Math.Abs(GetProjParm(OgrCoreDefinitiaons.SrsPpAzimuth, 0.0) - 90) < 0.0001 &&
                Math.Abs(GetProjParm(OgrCoreDefinitiaons.SrsPpRectifiedGridAngle, 0.0) - 90) < 0.0001)
            {
                SetNode("PROJCS|PROJECTION", "Hotine_Oblique_Mercator_Azimuth_Center");
                var projcsNode = GetAttrNode("PROJCS");

                var rgaChildIndex = FindProjParm("rectified_grid_angle", projcsNode);
                if (rgaChildIndex != -1)
                    projcsNode.ChildNodes.RemoveAt(rgaChildIndex);
                projection = GetAttrValue("PROJECTION");
            }
            if (!string.IsNullOrEmpty(projection) &&
                string.Equals(projection, OgrCoreDefinitiaons.SrsPtPolarStereographic,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (GetProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0) < 0.0)
                {
                    SetNode("PROJCS|PROJECTION", "Stereographic_South_Pole");
                    projection = GetAttrValue("PROJECTION");
                }
                else
                {
                    SetNode("PROJCS|PROJECTION", "Stereographic_North_Pole");
                    projection = GetAttrValue("PROJECTION");
                }
            }
            if (!string.IsNullOrEmpty(projection) &&
                string.Equals(projection, OgrCoreDefinitiaons.SrsPtObliqueStereographic,
                    StringComparison.OrdinalIgnoreCase))
            {
                SetNode("PROJCS|PROJECTION", "Stereographic");
            }
            Node.ApplyRemapper("PROJECTION", ProjMapping, 1, 0, 2);
            projection = GetAttrValue("PROJECTION");
            InitDatumMappingTable();
            Node.ApplyRemapper("DATUM", DatumMapping, 2, 1, 3);

            string projCsName = "", gcsName = "";
            OgrsrsNode projCsNode, projCsNodeChild = null;

            var geogCsNode = GetAttrNode("GEOGCS");
            if (geogCsNode != null)
            {
                var geogCsName = geogCsNode.ChildNodes[0].Value;
                var authName = GetAuthorityName("GEOGCS");
                var utmPrefix = string.Empty;
                var gcsCode = -1;

                if (!string.IsNullOrEmpty(authName) && string.Equals(authName, "EPSG"))
                    gcsCode = GetIntOutOfString(GetAuthorityCode("GEOGCS"));
                if (gcsCode == 4326 || string.Equals(geogCsName, "WGS84", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(geogCsName, "WGS 84", StringComparison.OrdinalIgnoreCase))
                {
                    geogCsNode.ChildNodes[0].Value = "GCS_WGS_1984";
                    utmPrefix = "WGS_1984";
                }
                else if (gcsCode == 4267 || string.Equals(geogCsName, "NAD27") || string.Equals(geogCsName, "NAD 27"))
                {
                    geogCsNode.ChildNodes[0].Value = "GCS_North_American_1927";
                    utmPrefix = "NAD_1927";
                }
                else if (gcsCode == 4269
                         || string.Equals(geogCsName, "NAD83")
                         || string.Equals(geogCsName, "NAD 83"))
                {
                    geogCsNode.ChildNodes[0].Value = "GCS_North_American_1983";
                    utmPrefix = "NAD_1983";
                }
                string[] unknownMapping = {"Unknown", "Unnamed", null, null};

                Node.ApplyRemapper("PROJCS", unknownMapping, 1, 0, 2);
                Node.ApplyRemapper("GEOGCS", unknownMapping, 1, 0, 2);
                Node.ApplyRemapper("DATUM", unknownMapping, 1, 0, 2);
                Node.ApplyRemapper("SPHEROID", unknownMapping, 1, 0, 2);
                Node.ApplyRemapper("PRIMEM", unknownMapping, 1, 0, 2);

                if ((projCsNode = GetAttrNode("PROJCS")) != null)
                    projCsNodeChild = projCsNode.ChildNodes[0];
                if (projCsNodeChild != null)
                {
                    projCsName = projCsNodeChild.Value;
                    MorphNameToEsri(ref projCsName);
                    projCsNodeChild.Value = projCsName;
                }

                if (!string.IsNullOrEmpty(projCsName) &&
                    (string.Equals(projCsName, "unnamed", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(projCsName, "unknown", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(projCsName, "", StringComparison.OrdinalIgnoreCase)))
                {
                    if (GetAttrValue("PROJECTION", 0) != null)
                    {
                        projCsName = GetAttrValue("PROJECTION", 0);
                        projCsNodeChild.Value = projCsName;
                    }
                }

                var isNorth = false;
                var zone = 0;

                if (!string.IsNullOrEmpty(projCsName) &&
                    projCsName.StartsWith("UTM Zone ", StringComparison.OrdinalIgnoreCase))
                {
                    zone = GetIntOutOfString(projCsName.Substring(9, projCsName.Length - 9));
                    if (projCsName.IndexOf("North") >= 0)
                        isNorth = true;
                }
                if (zone <= 0)
                    zone = GetUtmZone(out isNorth);

                if (zone > 0 && !string.IsNullOrEmpty(utmPrefix))
                {
                    string utmName;
                    if (isNorth)
                        utmName = string.Format("{0}_UTM_Zone_{1}N", utmPrefix, zone);
                    else
                        utmName = string.Format("{0}_UTM_Zone_{1}S", utmPrefix, zone);

                    if (projCsNodeChild != null)
                        projCsNodeChild.Value = utmName;
                }

                Node.ApplyRemapper("UNIT", UnitMapping, 1, 0, 2);

                var unitNode = GetAttrNode("GEOGCS|UNIT");
                var angularUnitsName = "";
                if (unitNode != null && unitNode.ChildNodes.Count >= 2 &&
                    Math.Abs(GetAngularUnits(ref angularUnitsName) - 0.0174532925199433) < 0.00000000001)
                {
                    unitNode.ChildNodes[0].Value = "Degree";
                    unitNode.ChildNodes[1].Value = "0.017453292519943295";
                }

                var projcsUnitNode = GetAttrNode("PROJCS|UNIT");

                if (projcsUnitNode != null && projcsUnitNode.ChildNodes.Count >= 2
                    && Math.Abs(GetLinearUnits() - 0.30480060960121924) < 0.000000000000001)
                {
                    projcsUnitNode.ChildNodes[0].Value = "Foot_US";
                    projcsUnitNode.ChildNodes[1].Value = "0.30480060960121924";
                }

                #region Remap parameters used for Albers and Mercator.

                projection = GetAttrValue("PROJECTION");
                projCsNode = GetAttrNode("PROJCS");

                if (!string.IsNullOrEmpty(projection) && string.Equals(projection, "Albers"))
                    Node.ApplyRemapper("PARAMETER", AlbersMapping, 1, 0, 2);

                if (projection != null
                    &&
                    (string.Equals(projection, OgrCoreDefinitiaons.SrsPtEquidistantConic) ||
                     string.Equals(projection, OgrCoreDefinitiaons.SrsPtLambertAzimuthalEqualArea) ||
                     string.Equals(projection, OgrCoreDefinitiaons.SrsPtAzimuthalEquidistant) ||
                     string.Equals(projection, OgrCoreDefinitiaons.SrsPtSinusoidal) ||
                     string.Equals(projection, OgrCoreDefinitiaons.SrsPtRobinson)))
                    Node.ApplyRemapper("PARAMETER", EcMapping, 1, 0, 2);

                if (projection != null && string.Equals(projection, "Mercator"))
                    Node.ApplyRemapper("PARAMETER", MercatorMapping, 1, 0, 2);

                if (projection != null && projection.StartsWith("Stereographic_", StringComparison.OrdinalIgnoreCase) &&
                    projection.EndsWith("_Pole", StringComparison.OrdinalIgnoreCase))
                    Node.ApplyRemapper("PARAMETER", PolarStereographicMapping, 1, 0, 2);

                if (projection != null && string.Equals(projection, "Plate_Carree"))
                    if (FindProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, projCsNode) < 0)
                        Node.ApplyRemapper("PARAMETER", PolarStereographicMapping, 1, 0, 2);

                #endregion

                if (!string.IsNullOrEmpty(projection) &&
                    string.Equals(projection, "Equidistant_Cylindrical", StringComparison.OrdinalIgnoreCase))
                {
                    if (GetNormProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, 0.0) != 0.0)
                    {
                    }
                    else
                    {
                        projCsNode = GetAttrNode("PROJCS");
                        var indexToRemove = FindProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin);
                        if (projCsNode != null && indexToRemove >= 0)
                            projCsNode.ChildNodes.RemoveAt(indexToRemove);
                    }
                }

                var spheroidNode = GetAttrNode("SPHEROID");
                OgrsrsNode spheroidChild = null;

                if (spheroidNode != null)
                {
                    spheroidChild = spheroidNode.ChildNodes[0];
                }
                if (spheroidChild != null)
                {
                    var newValue = RemapSpheroidName(spheroidChild.Value);
                    MorphNameToEsri(ref newValue);
                    spheroidChild.Value = newValue;
                }
                if (spheroidNode != null)
                    spheroidChild = spheroidNode.ChildNodes[1];
                if (spheroidChild != null)
                {
                    var value = spheroidChild.Value;
                    for (var i = 0; !string.IsNullOrEmpty(InvFlatteningMapping[i]); i = i + 2)
                    {
                        if (InvFlatteningMapping[i].StartsWith(value, StringComparison.OrdinalIgnoreCase))
                        {
                            spheroidChild.Value = InvFlatteningMapping[i + 1];
                            break;
                        }
                    }
                }

                var datumNode = GetAttrNode("DATUM");
                if (datumNode != null)
                    datumNode = datumNode.ChildNodes[0];

                if (datumNode != null)
                {
                    var datumName = datumNode.Value;
                    if (!datumName.StartsWith("D_", StringComparison.OrdinalIgnoreCase))
                    {
                        var newValue = "D_" + datumNode.Value;
                        datumNode.Value = newValue;
                    }
                }

                if (projCsNodeChild != null)
                {
                    projCsName = projCsNodeChild.Value;
                }
                if (!string.IsNullOrEmpty(projCsName))
                {
                    gcsName = GetAttrValue("GEOGCS");
                    if (!string.IsNullOrEmpty(gcsName) && !gcsName.StartsWith("GCS_"))
                    {
                        var newGcsName = "GCS_" + gcsName;
                        SetNewName(this, "GEOGCS", newGcsName);
                        gcsName = GetAttrValue("GEOGCS");
                    }
                    RemapGeogCsName(this, gcsName);

                    projection = GetAttrValue("PROJECTION");

                    if (!string.IsNullOrEmpty(projection))
                    {
                        if (string.Equals(projection, "Lambert_Conformal_Conic", StringComparison.OrdinalIgnoreCase))
                        {
                            if (FindProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel2, projCsNode) < 0)
                            {
                                var iChild = FindProjParm(OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, projCsNode);
                                var iChild1 = FindProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, projCsNode);
                                if (iChild >= 0 && iChild1 < 0)
                                {
                                    var parameterNode = projCsNode.ChildNodes[iChild];
                                    if (parameterNode != null)
                                    {
                                        var newParameterNode = new OgrsrsNode("PARAMETER");
                                        newParameterNode.ChildNodes.Add(new OgrsrsNode("standard_parallel_1"));
                                        newParameterNode.ChildNodes.Add(new OgrsrsNode(parameterNode.ChildNodes[1].Value));
                                        projCsNode.ChildNodes.Add(newParameterNode);
                                    }
                                }
                            }
                        }
                        if (string.Equals(projection, "Plate_Carree"))
                        {
                            var iChild = FindProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, projCsNode);
                            if (iChild < 0)
                                iChild = FindProjParm(OgrCoreDefinitiaons.SrsPpPseudoStdParallel1, projCsNode);

                            if (iChild >= 0)
                            {
                                var parameterNode = projCsNode.ChildNodes[iChild];
                                if (!string.Equals(parameterNode.ChildNodes[1].Value, "0.0") &&
                                    !string.Equals(parameterNode.ChildNodes[1].Value, "0"))
                                {
                                    SetNode("PROJCS|PROJECTION", "Equidistant_Cylindrical");
                                    projection = GetAttrValue("PROJECTION");
                                }
                            }
                        }
                        DeleteParamBasedOnPrjName(this, projection, DeleteParametersBasedOnProjection);
                        AddParamBasedOnPrjName(this, projection, AddParametersBasedOnProjection);
                        RemapPValuesBasedOnProjCsAndPName(this, projection, ParamValueMapping);
                        RemapPNamesBasedOnProjCsAndPName(this, projection, ParamNameMapping);
                    }
                }
            }
        }

        internal void ImportFromEsri(string wkt)
        {
            if (string.IsNullOrEmpty(wkt))
            {
                throw new ArgumentException("The argument wkt should not be null or empty.");
            }
            if (wkt.StartsWith("GEOGCS", StringComparison.OrdinalIgnoreCase) ||
                wkt.StartsWith("PROJCS", StringComparison.OrdinalIgnoreCase) ||
                wkt.StartsWith("LOCAL_CS", StringComparison.OrdinalIgnoreCase))
            {
                ImportFromWkt(wkt);
                MorphFromEsri();
                return;
            }

           throw new NotImplementedException("Method for importing WKT of this type is not implemented");

        }

        private void MorphFromEsri()
        {
            if (Node == null)
            {
                throw new NullReferenceException("Node is not initialized, please call ImportFromEsri method first");
            }
            InitDatumMappingTable();
            for (var i = 0; i < DatumMapping.Length; i++)
            {
                if (string.Equals(DatumMapping[i], "World_Geodetic_System_1984", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Target Found, Index is " + i);
                    break;
                }
            }

            Node.ApplyRemapper("DATUM", DatumMapping, 1, 2, 3);
            var datumNode = GetAttrNode("DATUM");
            if (datumNode != null)
                datumNode = datumNode.ChildNodes[0];
            if (datumNode != null)
            {
                if (datumNode.Value.StartsWith("D_", StringComparison.OrdinalIgnoreCase))
                {
                    datumNode.Value = datumNode.Value.Remove(0, 2);
                }
            }

            var projectionName = GetAttrValue("PROJECTION");

            if (!string.IsNullOrEmpty(projectionName) && string.Equals(projectionName, "Lambert_Conformal_Conic"))
            {
                if (GetProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel1, 1000.0) != 1000.0 &&
                    GetProjParm(OgrCoreDefinitiaons.SrsPpStandardParallel2, 1000.0) != 1000.0)
                    SetNode("PROJCS|PROJECTION", OgrCoreDefinitiaons.SrsPtLambertConformalConic_2Sp);
                else
                    SetNode("PROJCS|PROJECTION", OgrCoreDefinitiaons.SrsPtLambertConformalConic_1Sp);

                projectionName = GetAttrValue("PROJECTION");
            }

            if (!string.IsNullOrEmpty(projectionName) &&
                string.Equals(projectionName, "Hotine_Oblique_Mercator_Azimuth_Center",
                    StringComparison.OrdinalIgnoreCase))
            {
                SetProjParm(OgrCoreDefinitiaons.SrsPpRectifiedGridAngle,
                    GetProjParm(OgrCoreDefinitiaons.SrsPpAzimuth, 0.0));
                FixupOrdering();
            }
            if (!string.IsNullOrEmpty(projectionName) &&
                string.Equals(projectionName, "Albers", StringComparison.OrdinalIgnoreCase))
            {
                Node.ApplyRemapper("PARAMETER", AlbersMapping, 0, 1, 2);
            }
            if (!string.IsNullOrEmpty(projectionName) &&
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtEquidistantConic) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtLambertAzimuthalEqualArea) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtAzimuthalEquidistant) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtSinusoidal) ||
                string.Equals(projectionName, OgrCoreDefinitiaons.SrsPtRobinson))
            {
                Node.ApplyRemapper("PARAMETER", EcMapping, 0, 1, 2);
            }
            if (!string.IsNullOrEmpty(projectionName) &&
                string.Equals(projectionName, "Mercator", StringComparison.OrdinalIgnoreCase))
            {
                Node.ApplyRemapper("PARAMETER", MercatorMapping, 0, 1, 2);
            }
            if (!string.IsNullOrEmpty(projectionName) &&
                projectionName.StartsWith("Stereographic_", StringComparison.OrdinalIgnoreCase) &&
                projectionName.EndsWith("_Pole", StringComparison.OrdinalIgnoreCase))
            {
                Node.ApplyRemapper("PARAMETER", PolarStereographicMapping, 0, 1, 2);
            }
            if (!string.IsNullOrEmpty(projectionName) &&
                projectionName.StartsWith("Stereographic_", StringComparison.OrdinalIgnoreCase) &&
                projectionName.EndsWith("_Pole", StringComparison.OrdinalIgnoreCase))
            {
                SetNode("PROJCS|PROJECTION", OgrCoreDefinitiaons.SrsPtPolarStereographic);
                projectionName = GetAttrValue("PROJECTION");
            }

            Node.ApplyRemapper("PROJECTION", ProjMapping, 0, 1, 2);
            Node.ApplyRemapper("DATUM", DatumMapping, 1, 2, 3);
        }

        private static void SetNewName(OgrSpatialReference ogr, string keyName, string newName)
        {
            var node = ogr.GetAttrNode(keyName);
            OgrsrsNode childNode = null;
            if (node != null)
                childNode = node.ChildNodes[0];
            if (childNode != null)
                childNode.Value = newName;
        }

      
        private static int RemapNameBasedOnKeyName(OgrSpatialReference ogr, string name, string keyName,
            string[] mappingTable)
        {
            int i, n;
            var iIndex = -1;
            for (i = 0; mappingTable[i] != null; i += 2)
            {
                n = name.Length;

                if (name.StartsWith(mappingTable[i]) && mappingTable[i].Length >= n)
                {
                    iIndex = i;
                    break;
                }
            }
            if (iIndex >= 0)
            {
                var node = ogr.GetAttrNode(keyName);
                OgrsrsNode nodeChild = null;
                if (node != null)
                    nodeChild = node.ChildNodes[0];
                if (nodeChild != null && nodeChild.Value.Length > 0)
                    nodeChild.Value = mappingTable[iIndex + 1];
            }
            return iIndex;
        }

        private static int RemapNamesBasedOnTwo(OgrSpatialReference ogr, string name1, string name2,
            string[] mappingTable, int tableStepSize, string[] keyNames, int keys)
        {
            int i, n, n1;
            var iIndex = -1;
            for (i = 0; mappingTable[i] != null; i += tableStepSize)
            {
                n = name1.Length;
                n1 = mappingTable[i].Length;

                if (name1.StartsWith(mappingTable[i]))
                {
                    var j = i;
                    while (mappingTable[j] != null &&
                           string.Equals(mappingTable[i], mappingTable[j], StringComparison.OrdinalIgnoreCase))
                    {
                        if (name2.StartsWith(mappingTable[j + 1], StringComparison.InvariantCultureIgnoreCase))
                        {
                            iIndex = j;
                            break;
                        }
                        j += 3;
                    }
                    if (iIndex >= 0)
                        break;
                }
            }
            if (iIndex >= 0)
            {
                for (i = 0; i < keys; i++)
                {
                    var node = ogr.GetAttrNode(keyNames[i]);
                    OgrsrsNode nodeChild = null;
                    if (node != null)
                        nodeChild = node.ChildNodes[0];
                    if (nodeChild != null && nodeChild.Value.Length > 0)
                        nodeChild.Value = mappingTable[iIndex + i + 2];
                }
            }
            return iIndex;
        }

        private static int RemapPValuesBasedOnProjCsAndPName(OgrSpatialReference ogr, string projCsName,
            string[] mappingTable)
        {
            var ret = 0;
            var projcsNode = ogr.GetAttrNode("PROJCS");
            for (var i = 0; mappingTable[i] != null; i += 4)
            {
                while (mappingTable[i] != null && projCsName.StartsWith(mappingTable[i]))
                {
                    OgrsrsNode parmNode;
                    var paramName = mappingTable[i + 1];
                    var paramValue = mappingTable[i + 2];
                    for (var iChild = 0; iChild < projcsNode.ChildNodes.Count; iChild++)
                    {
                        parmNode = projcsNode.ChildNodes[iChild];

                        if (string.Equals(parmNode.Value, "PARAMETER", StringComparison.OrdinalIgnoreCase) &&
                            parmNode.ChildNodes.Count == 2 &&
                            string.Equals(parmNode.ChildNodes[0].Value, paramName, StringComparison.OrdinalIgnoreCase) &&
                            parmNode.ChildNodes[1].Value.StartsWith(paramValue))
                        {
                            parmNode.ChildNodes[1].Value = mappingTable[i + 3];
                            break;
                        }
                    }
                    ret++;
                    i += 4;
                }
                if (ret > 0)
                    break;
            }
            return ret;
        }

        private static int RemapPNamesBasedOnProjCsAndPName(OgrSpatialReference ogr, string projCsName,
            string[] mappingTable)
        {
            var ret = 0;
            var projCsNode = ogr.GetAttrNode("PROJCS");
            for (var i = 0; mappingTable[i] != null; i += 3)
            {
                while (mappingTable[i] != null && projCsName.StartsWith(mappingTable[i]))
                {
                    OgrsrsNode parmNode;
                    var paramName = mappingTable[i + 1];
                    for (var iChild = 0; iChild < projCsNode.ChildNodes.Count; iChild++)
                    {
                        parmNode = projCsNode.ChildNodes[iChild];

                        if (string.Equals(parmNode.Value, "PARAMETER", StringComparison.OrdinalIgnoreCase)
                            && parmNode.ChildNodes.Count == 2
                            &&
                            string.Equals(parmNode.ChildNodes[0].Value, paramName, StringComparison.OrdinalIgnoreCase))
                        {
                            parmNode.ChildNodes[0].Value = mappingTable[i + 2];
                            break;
                        }
                    }
                    ret++;
                    i += 3;
                }
                if (ret > 0)
                    break;
            }
            return ret;
        }

        private static int DeleteParamBasedOnPrjName(OgrSpatialReference ogr, string projectionName,
            string[] mappingTable)
        {
            int iIndex = -1, ret = -1;
            for (var i = 0; mappingTable[i] != null; i += 2)
            {
                if (projectionName.StartsWith(mappingTable[i]))
                {
                    var projcsNode = ogr.GetAttrNode("PROJCS");
                    OgrsrsNode parmNode;
                    var paramName = mappingTable[i + 1];
                    iIndex = -1;
                    for (var iChild = 0; iChild < projcsNode.ChildNodes.Count; iChild++)
                    {
                        parmNode = projcsNode.ChildNodes[iChild];

                        if (string.Equals(parmNode.Value, "PARAMETER", StringComparison.OrdinalIgnoreCase) &&
                            parmNode.ChildNodes.Count == 2 &&
                            string.Equals(parmNode.ChildNodes[0].Value, paramName, StringComparison.OrdinalIgnoreCase))
                        {
                            iIndex = iChild;
                            break;
                        }
                    }
                    if (iIndex >= 0)
                    {
                        projcsNode.ChildNodes.RemoveAt(iIndex);
                        ret++;
                    }
                }
            }
            return ret;
        }

        private static int AddParamBasedOnPrjName(OgrSpatialReference ogr, string projectionName, string[] mappingTable)
        {
            var ret = -1;
            var projcsNode = ogr.GetAttrNode("PROJCS");
            for (var i = 0; mappingTable[i] != null; i += 3)
            {
                if (projectionName.StartsWith(mappingTable[i]))
                {
                    OgrsrsNode parmNode;
                    var exist = false;
                    for (var iChild = 0; iChild < projcsNode.ChildNodes.Count; iChild++)
                    {
                        parmNode = projcsNode.ChildNodes[iChild];

                        if (string.Equals(parmNode.Value, "PARAMETER", StringComparison.OrdinalIgnoreCase) &&
                            parmNode.ChildNodes.Count == 2 &&
                            string.Equals(parmNode.ChildNodes[0].Value, mappingTable[i + 1],
                                StringComparison.OrdinalIgnoreCase))
                            exist = true;
                    }
                    if (!exist)
                    {
                        parmNode = new OgrsrsNode("PARAMETER");

                        var nameNode = new OgrsrsNode(mappingTable[i + 1]);
                        var valueNode = new OgrsrsNode(mappingTable[i + 2]);

                        parmNode.ChildNodes.Add(nameNode);
                        parmNode.ChildNodes.Add(valueNode);
                        projcsNode.ChildNodes.Add(parmNode);
                        ret++;
                    }
                }
            }
            return ret;
        }

        private static int RemapGeogCsName(OgrSpatialReference ogr, string geogcsName)
        {
            string[] keyNameG = {"GEOGCS"};

            var ret = -1;

            var unitName = ogr.GetAttrValue("GEOGCS|UNIT");
            if (!string.IsNullOrEmpty(unitName))
                ret = RemapNamesBasedOnTwo(ogr, geogcsName.Substring(4), unitName, GcsNameMappingBasedOnUnit, 3,
                    keyNameG, 1);
            if (ret < 0)
            {
                var primeName = ogr.GetAttrValue("PRIMEM");
                if (!string.IsNullOrEmpty(primeName))
                    ret = RemapNamesBasedOnTwo(ogr, geogcsName.Substring(4), primeName, GcsNameMappingBasedPrime, 3,
                        keyNameG, 1);
                if (ret < 0)
                    ret = RemapNameBasedOnKeyName(ogr, geogcsName.Substring(4), "GEOGCS", GcsNameMapping);
            }
            if (ret < 0)
            {
                var projcsName = ogr.GetAttrValue("PROJCS");
                ret = RemapNamesBasedOnTwo(ogr, projcsName, geogcsName, GcsNameMappingBasedOnProjCs, 3, keyNameG, 1);
            }
            return ret;
        }

       
        #region MappingsDefinition

        internal static string[] ProjMapping =
        {
            "Albers", OgrCoreDefinitiaons.SrsPtAlbersConicEqualArea,
            "Cassini", OgrCoreDefinitiaons.SrsPtCassiniSoldner,
            "Equidistant_Cylindrical", OgrCoreDefinitiaons.SrsPtEquirectangular,
            "Plate_Carree", OgrCoreDefinitiaons.SrsPtEquirectangular,
            "Hotine_Oblique_Mercator_Azimuth_Natural_Origin", OgrCoreDefinitiaons.SrsPtHotineObliqueMercator,
            "Hotine_Oblique_Mercator_Azimuth_Center", OgrCoreDefinitiaons.SrsPtHotineObliqueMercator,
            "Lambert_Conformal_Conic", OgrCoreDefinitiaons.SrsPtLambertConformalConic_2Sp,
            "Lambert_Conformal_Conic", OgrCoreDefinitiaons.SrsPtLambertConformalConic_1Sp,
            "Van_der_Grinten_I", OgrCoreDefinitiaons.SrsPtVandergrinten,
            OgrCoreDefinitiaons.SrsPtTransverseMercator, OgrCoreDefinitiaons.SrsPtTransverseMercator,
            "Gauss_Kruger", OgrCoreDefinitiaons.SrsPtTransverseMercator,
            "Mercator", OgrCoreDefinitiaons.SrsPtMercator_1Sp,
            null, null
        };

        internal static string[] AlbersMapping =
        {
            OgrCoreDefinitiaons.SrsPpCentralMeridian,
            OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
            OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, "Central_Parallel", OgrCoreDefinitiaons.SrsPpLatitudeOfCenter,
            null, null
        };

        internal static string[] EcMapping =
        {
            OgrCoreDefinitiaons.SrsPpCentralMeridian,
            OgrCoreDefinitiaons.SrsPpLongitudeOfCenter, OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin,
            OgrCoreDefinitiaons.SrsPpLatitudeOfCenter, null, null
        };

        internal static string[] MercatorMapping =
        {
            OgrCoreDefinitiaons.SrsPpStandardParallel1,
            OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, null, null
        };

        internal static string[] PolarStereographicMapping =
        {
            OgrCoreDefinitiaons.SrsPpStandardParallel1,
            OgrCoreDefinitiaons.SrsPpLatitudeOfOrigin, null, null
        };

        internal static string[] DatumMapping;

        internal static string[] DefaultDatumMapping =
        {
            "6267", "North_American_1927", "North_American_Datum_1927",
            "6269", "North_American_1983", "North_American_Datum_1983", null, null, null
        };

        internal static string[] UnitMapping =
        {
            "Meter", "meter", "Meter", "metre", "Foot", "foot", "Foot", "feet",
            "Foot", "international_feet", "Foot_US", OgrCoreDefinitiaons.SrsUlUsFoot, "Foot_Clarke", "clarke_feet",
            "Degree", "degree", "Degree", "degrees", "Degree", OgrCoreDefinitiaons.SrsUaDegree, "Radian",
            OgrCoreDefinitiaons.SrsUaRadian, null, null
        };

        #endregion

        #region OGRSRSEsriNamesDefinition

        internal static string[] InvFlatteningMapping =
        {
            "293.464999999", "293.465",
            "293.466020000", "293.46602",
            "294.26067636900", "294.260676369",
            "294.9786981999", "294.9786982",
            "294.978698213", "294.9786982",
            "295.9999999999", "296.0",
            "297.0000000000", "297.0",
            "298.256999999", "298.257",
            "298.2600000000", "298.26",
            "298.25722210100", "298.257222101",
            "298.25722356299", "298.257223563",
            "298.2684109950054", "298.268410995005",
            "298.299999999", "298.3",
            "299.15281280000", "299.1528128",
            "300.80169999999", "300.8017",
            "300.80170000000", "300.8017",
            null, null
        };

        internal static string[] GcsNameMappingBasedOnUnit =
        {
            "Merchich", "Degree", "GCS_Merchich_Degree",
            "Voirol_Unifie_1960", "Degree", "GCS_Voirol_Unifie_1960_Degree",
            "NTF", "Grad", "GCS_NTF_Paris",
            null, null, null
        };

        internal static string[] GcsNameMappingBasedPrime =
        {
            "S_JTSK", "Ferro", "GCS_S_JTSK_Ferro",
            "MGI", "Ferro", "GCS_MGI_Ferro",
            "Madrid_1870", "Madrid", "GCS_Madrid_1870_Madrid",
            "Monte_Mario", "Rome", "GCS_Monte_Mario_Rome",
            "NGO_1948", "Oslo", "GCS_NGO_1948_Oslo",
            "MGI", "Stockholm", "GCS_RT38_Stockholm",
            "Stockholm_1938", "Stockholm", "GCS_RT38_Stockholm",
            "Bern_1898", "Bern", "GCS_Bern_1898_Bern",
            null, null, null
        };

        internal static string[] GcsNameMapping =
        {
            "North_American_Datum_1983", "GCS_North_American_1983",
            "North_American_Datum_1927", "GCS_North_American_1927",
            "NAD27_CONUS", "GCS_North_American_1927",
            "NAD27[CONUS]", "GCS_North_American_1927",
            "Reseau_Geodesique_de_Nouvelle_Caledonie_1991-93", "GCS_RGNC_1991-93",
            "Reseau_Geodesique_de_la_Polynesie_Francaise", "GCS_RGPF",
            "Rauenberg_1983", "GCS_RD/83",
            "Phillipine_Reference_System_1992", "GCS_PRS_1992",
            "Potsdam_1983", "GCS_PD/83",
            "Datum_Geodesi_Nasional_1995", "GCS_DGN_1995",
            "Islands_Network_1993", "GCS_ISN_1993",
            "Institut_Geographique_du_Congo_Belge_1955", "GCS_IGCB_1955",
            "IGC_1962_Arc_of_the_6th_Parallel_South", "GCS_IGC_1962_6th_Parallel_South",
            "Jamaica_2001", "GCS_JAD_2001",
            "European_Libyan_1979", "GCS_European_Libyan_Datum_1979",
            "Madrid_1870", "GCS_Madrid_1870_Madrid",
            "Azores_Occidental_Islands_1939", "GCS_Azores_Occidental_1939",
            "Azores_Central_Islands_1948", "GCS_Azores_Central_1948",
            "Azores_Oriental_Islands_1940", "GCS_Azores_Oriental_1940",
            "Lithuania_1994", "GCS_LKS_1994",
            "Libyan_Geodetic_Datum_2006", "GCS_LGD2006",
            "Lisbon", "GCS_Lisbon_Lisbon",
            "Stockholm_1938", "GCS_RT38",
            "Latvia_1992", "GCS_LKS_1992",
            "Azores_Oriental_Islands_1995", "GCS_Azores_Oriental_1995",
            "Azores_Central_Islands_1948", "GCS_Azores_Central_1948",
            "Azores_Central_Islands_1995", "GCS_Azores_Central_1995",
            "ATF", "GCS_ATF_Paris",
            "ITRF_2000", "GCS_MONREF_1997",
            "Faroe_Datum_1954", "GCS_FD_1954",
            "Vietnam_2000", "GCS_VN_2000",
            "Belge_1950", "GCS_Belge_1950_Brussels",
            "Qatar_1948", "GCS_Qatar_1948",
            "Qatar", "GCS_Qatar_1974",
            "Kuwait_Utility", "GCS_KUDAMS",
            null, null
        };

        internal static string[] GcsNameMappingBasedOnProjCs =
        {
            "EUREF_FIN_TM35FIN", "GCS_ETRS_1989", "GCS_EUREF_FIN",
            null, null, null
        };

        internal static string[] DeleteParametersBasedOnProjection =
        {
            "Stereographic_South_Pole", "scale_factor",
            "Stereographic_North_Pole", "scale_factor",
            "Mercator", "scale_factor",
            "Miller_Cylindrical", "latitude_of_center",
            "Equidistant_Cylindrical", "pseudo_standard_parallel_1",
            "Plate_Carree", "latitude_of_origin",
            "Plate_Carree", "pseudo_standard_parallel_1",
            "Plate_Carree", "standard_parallel_1",
            "Hotine_Oblique_Mercator_Azimuth_Center", "rectified_grid_angle",
            "Hotine_Oblique_Mercator_Azimuth_Natural_Origin", "rectified_grid_angle",
            null, null
        };

        internal static string[] AddParametersBasedOnProjection =
        {
            "Cassini", "scale_factor", "1.0",
            "Lambert_Conformal_Conic", "scale_factor", "1.0",
            "Mercator", "standard_parallel_1", "0.0",
            null, null, null
        };

        internal static string[] ParamValueMapping =
        {
            "Cassini", "false_easting", "283799.9999", "283800.0",
            "Cassini", "false_easting", "132033.9199", "132033.92",
            "Cassini", "false_northing", "214499.9999", "214500.0",
            "Cassini", "false_northing", "62565.9599", "62565.95",
            "Transverse_Mercator", "false_easting", "499999.1331", "500000.0",
            "Transverse_Mercator", "false_easting", "299999.4798609", "300000.0",
            "Transverse_Mercator", "false_northing", "399999.30648", "400000.0",
            "Transverse_Mercator", "false_northing", "499999.1331", "500000.0",
            null, null, null, null
        };

        internal static string[] ParamNameMapping =
        {
            "Lambert_Azimuthal_Equal_Area", "longitude_of_center", "Central_Meridian",
            "Lambert_Azimuthal_Equal_Area", "Latitude_Of_Center", "Latitude_Of_Origin",
            "Miller_Cylindrical", "longitude_of_center", "Central_Meridian",
            "Gnomonic", "central_meridian", "Longitude_Of_Center",
            "Gnomonic", "latitude_of_origin", "Latitude_Of_Center",
            "Orthographic", "central_meridian", "Longitude_Of_Center",
            "Orthographic", "latitude_of_origin", "Latitude_Of_Center",
            "New_Zealand_Map_Grid", "central_meridian", "Longitude_Of_Origin",
            null, null, null
        };

        #endregion
    }
}